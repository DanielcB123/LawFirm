<?php

namespace Tests\Feature;

use App\Enums\FirmLifecycleStatus;
use App\Enums\FirmProvisioningStatus;
use App\Jobs\RecordTenantWorkspaceNote;
use App\Models\Firm;
use App\Models\FirmDomain;
use App\Models\FirmPublicProfile;
use App\Models\Tenant\TenantWorkspaceNote;
use App\Models\User;
use App\Tenancy\Contracts\RunsTenantMigrations;
use App\Tenancy\FirmRegistry;
use App\Tenancy\LaravelTenantMigrator;
use App\Tenancy\FirmReviewService;
use App\Tenancy\SchemaName;
use App\Tenancy\TenantConnectionManager;
use App\Tenancy\TenantContext;
use App\Tenancy\TenantSchemaProvisioner;
use App\Tenancy\DnsTxtLookup;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Log\Events\MessageLogged;
use Illuminate\Support\Facades\Event;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Str;
use Illuminate\Validation\ValidationException;
use InvalidArgumentException;
use Laravel\Sanctum\Sanctum;
use RuntimeException;
use Tests\TestCase;

class FirmTenancyTest extends TestCase
{
    use RefreshDatabase;

    public function test_control_plane_migration_does_not_create_tenant_tables(): void
    {
        $this->assertTrue(Schema::hasTable('firms'));
        $this->assertTrue(Schema::hasTable('users'));
        $this->assertFalse(Schema::hasTable('firm_identity'));
        $this->assertFalse(Schema::hasTable('tenant_workspace_notes'));
    }

    public function test_unusable_firms_cannot_open_tenant_services(): void
    {
        $admin = $this->administrator();
        $cases = [
            FirmLifecycleStatus::PendingReview,
            FirmLifecycleStatus::Rejected,
            FirmLifecycleStatus::Suspended,
            FirmLifecycleStatus::Approved,
        ];

        foreach ($cases as $status) {
            $firm = $this->registerFirm('Synthetic '.$status->value);
            $member = User::factory()->create();
            $this->membership($admin, $firm, $member);

            if ($status === FirmLifecycleStatus::Approved) {
                app(FirmReviewService::class)->approve($firm->refresh(), $admin, 'Synthetic review.', ['file:synthetic']);
            } elseif ($status === FirmLifecycleStatus::Rejected) {
                app(FirmReviewService::class)->reject($firm->refresh(), $admin, 'Synthetic rejection.', ['file:synthetic']);
            } elseif ($status === FirmLifecycleStatus::Suspended) {
                $this->activateFirm($firm->refresh(), $admin);
                app(FirmReviewService::class)->suspend($firm->refresh(), $admin, 'Synthetic suspension.');
            }

            Sanctum::actingAs($member);

            $this->getJson('/api/tenant/notes')
                ->assertForbidden()
                ->assertJsonPath('message', 'Tenant is not available.');
        }
    }

    public function test_only_a_platform_administrator_can_approve_or_suspend_a_firm(): void
    {
        $admin = $this->administrator();
        $member = User::factory()->create();
        $firm = $this->registerFirm('Harbor Review');

        $this->assertFalse($member->can('create', Firm::class));
        $this->assertTrue($admin->can('create', Firm::class));

        Sanctum::actingAs($member);
        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'approve',
            'reason' => 'Should not be stored from this caller.',
            'evidenceReferences' => ['file:blocked'],
        ])->assertForbidden();

        Sanctum::actingAs($admin);
        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'approve',
            'reason' => 'Reviewed the synthetic filing.',
            'evidenceReferences' => ['file:synthetic-1'],
        ])->assertOk()->assertJsonPath('lifecycleStatus', 'approved');

        $this->assertDatabaseHas('firm_verifications', [
            'firm_id' => $firm->id,
            'decision' => 'approve',
        ]);

        $active = $this->activateFirm($this->registerFirm('Harbor Active'), $admin);
        Sanctum::actingAs($member);
        $this->postJson('/api/platform/firms/'.$active->id.'/suspend', [
            'reason' => 'Not allowed.',
        ])->assertForbidden();

        Sanctum::actingAs($admin);
        $this->postJson('/api/platform/firms/'.$active->id.'/suspend', [
            'reason' => 'Synthetic suspension.',
        ])->assertOk()->assertJsonPath('lifecycleStatus', 'suspended');
    }

    public function test_a_user_cannot_read_another_firms_notes_by_tampering(): void
    {
        $admin = $this->administrator();
        $firmA = $this->activateFirm($this->registerFirm('Firm Alpha'), $admin);
        $firmB = $this->activateFirm($this->registerFirm('Firm Beta'), $admin);
        $userA = User::factory()->create();
        $userB = User::factory()->create();
        $this->membership($admin, $firmA, $userA);
        $this->membership($admin, $firmB, $userB);
        $this->domain($admin, $firmB, 'beta.example.test');

        Sanctum::actingAs($userA);
        $this->postJson('/api/tenant/notes', [
            'title' => 'Alpha note',
            'body' => 'Visible only inside alpha.',
        ])->assertCreated();

        Sanctum::actingAs($userB);
        $this->postJson('/api/tenant/notes', [
            'title' => 'Beta note',
            'body' => 'Visible only inside beta.',
        ])->assertCreated();

        Sanctum::actingAs($userA);
        $this->getJson('/api/tenant/notes')
            ->assertOk()
            ->assertJsonFragment(['title' => 'Alpha note'])
            ->assertJsonMissing(['title' => 'Beta note']);

        $this->withHeader('X-Firm-Id', $firmB->id)
            ->getJson('/api/tenant/notes')
            ->assertForbidden()
            ->assertDontSee('Visible only inside beta.', false);
        unset($this->defaultHeaders['X-Firm-Id']);

        $this->getJson('/api/tenant/notes?firm_id='.$firmB->id)
            ->assertOk()
            ->assertJsonMissing(['title' => 'Beta note']);

        $this->getJson('/api/tenant/firms/'.$firmB->id.'/notes')
            ->assertOk()
            ->assertJsonFragment(['title' => 'Alpha note'])
            ->assertJsonMissing(['title' => 'Beta note']);

        $this->getJson('http://beta.example.test/api/tenant/notes')
            ->assertForbidden();

        $sharedId = (string) Str::uuid();
        app(TenantConnectionManager::class)->establish($firmA->refresh());
        $alphaNote = new TenantWorkspaceNote([
            'title' => 'Shared id alpha',
            'body' => 'Alpha body',
        ]);
        $alphaNote->id = $sharedId;
        $alphaNote->save();
        app(TenantConnectionManager::class)->clear();
        app(TenantConnectionManager::class)->establish($firmB->refresh());
        $betaNote = new TenantWorkspaceNote([
            'title' => 'Shared id beta',
            'body' => 'Beta body',
        ]);
        $betaNote->id = $sharedId;
        $betaNote->save();
        $betaTitle = TenantWorkspaceNote::query()->whereKey($sharedId)->value('title');
        app(TenantConnectionManager::class)->clear();
        app(TenantConnectionManager::class)->establish($firmA->refresh());
        $alphaTitle = TenantWorkspaceNote::query()->whereKey($sharedId)->value('title');
        $this->assertFalse(Schema::connection('tenant')->hasTable('users'));
        app(TenantConnectionManager::class)->clear();

        $this->assertSame('Shared id beta', $betaTitle);
        $this->assertSame('Shared id alpha', $alphaTitle);
        $this->assertFalse(Schema::hasTable('firm_identity'));
    }

    public function test_missing_or_inconsistent_tenant_context_fails_closed(): void
    {
        $admin = $this->administrator();
        $firmA = $this->activateFirm($this->registerFirm('Context Alpha'), $admin);
        $firmB = $this->activateFirm($this->registerFirm('Context Beta'), $admin);
        $user = User::factory()->create();
        $this->membership($admin, $firmA, $user);
        $this->membership($admin, $firmB, $user);
        $this->domain($admin, $firmA, 'alpha.example.test');

        Sanctum::actingAs($user);
        $this->getJson('/api/tenant/notes')
            ->assertStatus(409)
            ->assertJsonPath('code', 'firm_selection_required');

        $this->withHeader('X-Firm-Id', $firmA->id)
            ->getJson('http://alpha.example.test/api/tenant/notes')
            ->assertOk();

        $this->withHeader('X-Firm-Id', $firmB->id)
            ->getJson('http://alpha.example.test/api/tenant/notes')
            ->assertForbidden()
            ->assertJsonPath('message', 'Tenant context is inconsistent.');

        Sanctum::actingAs(User::factory()->create());
        $this->getJson('/api/tenant/notes')->assertForbidden();

        $this->expectException(\App\Tenancy\Exceptions\TenantContextMissingException::class);
        TenantWorkspaceNote::query()->count();
    }

    public function test_provisioning_is_isolated_retryable_and_does_not_activate_on_failure(): void
    {
        $admin = $this->administrator();
        $firm = $this->registerFirm('Provision Example');
        app(FirmReviewService::class)->approve($firm->refresh(), $admin, 'Synthetic approval.', ['file:synthetic']);

        $this->app->instance(RunsTenantMigrations::class, new class implements RunsTenantMigrations
        {
            public function migrate(): string
            {
                throw new RuntimeException('password=secret client=Jane Doe migration failed');
            }
        });

        $logged = [];
        Event::listen(MessageLogged::class, function (MessageLogged $event) use (&$logged): void {
            $logged[] = $event;
        });
        Sanctum::actingAs($admin);
        $this->postJson('/api/platform/firms/'.$firm->id.'/provision')
            ->assertStatus(422)
            ->assertJsonPath('message', 'Tenant schema migration did not complete.')
            ->assertDontSee('secret', false)
            ->assertDontSee('Jane Doe', false);

        $firm->refresh();
        $this->assertSame(FirmLifecycleStatus::Approved, $firm->lifecycle_status);
        $this->assertSame(FirmProvisioningStatus::Failed, $firm->provisioning_status);
        $this->assertStringNotContainsString('secret', (string) $firm->provisioning_failure_reason);
        $schemaName = $firm->schema_name;
        $this->assertNotNull($schemaName);

        $failure = collect($logged)->first(fn (MessageLogged $event): bool => $event->message === 'firm.provisioning.failed');
        $this->assertInstanceOf(MessageLogged::class, $failure);
        $encoded = json_encode($failure->context);
        $this->assertIsString($encoded);
        $this->assertStringNotContainsString('secret', $encoded);
        $this->assertStringNotContainsString('Jane Doe', $encoded);

        $this->app->singleton(RunsTenantMigrations::class, LaravelTenantMigrator::class);
        $ready = app(TenantSchemaProvisioner::class)->provision($firm->refresh(), $admin);
        $this->assertSame($schemaName, $ready->schema_name);
        $this->assertSame(FirmLifecycleStatus::Active, $ready->lifecycle_status);
        $this->assertSame(FirmProvisioningStatus::Ready, $ready->provisioning_status);

        $again = app(TenantSchemaProvisioner::class)->provision($ready->refresh(), $admin);
        $this->assertSame($schemaName, $again->schema_name);
        app(TenantConnectionManager::class)->establish($again);
        $this->assertSame(1, \App\Models\Tenant\FirmIdentity::query()->count());
        $this->assertTrue(Schema::connection('tenant')->hasTable('tenant_workspace_notes'));
        $this->assertFalse(Schema::connection('tenant')->hasTable('firms'));
        app(TenantConnectionManager::class)->clear();

        $this->expectException(InvalidArgumentException::class);
        SchemaName::quote('matterforge; drop database matterforge');
    }

    public function test_request_and_queue_context_are_cleared_between_firms(): void
    {
        $admin = $this->administrator();
        $firmA = $this->activateFirm($this->registerFirm('Queue Alpha'), $admin);
        $firmB = $this->activateFirm($this->registerFirm('Queue Beta'), $admin);
        $userA = User::factory()->create();
        $this->membership($admin, $firmA, $userA);

        Sanctum::actingAs($userA);
        $this->getJson('/api/tenant/context')->assertOk()->assertJsonPath('firmId', $firmA->id);
        $this->assertFalse(app(TenantContext::class)->isEstablished());

        app(TenantConnectionManager::class)->establish($firmA);
        RecordTenantWorkspaceNote::dispatchSync($firmB->id, 'beta queue body');
        $this->assertFalse(app(TenantContext::class)->isEstablished());

        app(TenantConnectionManager::class)->establish($firmA->refresh());
        $this->assertSame(0, TenantWorkspaceNote::query()->where('body', 'beta queue body')->count());
        app(TenantConnectionManager::class)->clear();
        app(TenantConnectionManager::class)->establish($firmB->refresh());
        $this->assertSame(1, TenantWorkspaceNote::query()->where('body', 'beta queue body')->count());
        app(TenantConnectionManager::class)->clear();

        $this->artisan('tenants:migrate')->assertSuccessful();
        $this->assertFalse(app(TenantContext::class)->isEstablished());
    }

    public function test_public_branding_returns_only_safe_public_fields(): void
    {
        $admin = $this->administrator();
        $firm = $this->activateFirm($this->registerFirm('Public Brand'), $admin);
        $pending = $this->registerFirm('Hidden Pending Firm');
        $this->domain($admin, $firm, 'brand.example.test');
        $this->domain($admin, $pending, 'pending.example.test');

        Sanctum::actingAs($admin);
        $this->putJson('/api/platform/firms/'.$firm->id.'/branding', [
            'logoPath' => 'https://evil.example/logo.svg',
            'faviconPath' => '/storage/branding/mark.ico',
            'colors' => [
                'primary' => '#112233',
                'secondary' => 'red;}</style><script>',
                'accent' => '#ABCDEF',
            ],
            'typography' => 'system-serif',
            'contactEmail' => 'desk@example.test',
            'contactPhone' => '(614) 555-0100',
            'contactAddress' => '1 Synthetic Street',
            'publicCopy' => '<script>alert(1)</script>',
            'publicSiteKey' => 'default_classic',
            'enabledModules' => ['public_site'],
        ])->assertStatus(422);

        $this->putJson('/api/platform/firms/'.$firm->id.'/branding', [
            'logoPath' => '/storage/branding/mark.png',
            'faviconPath' => '/storage/branding/mark.ico',
            'colors' => [
                'primary' => '#112233',
                'secondary' => '#445566',
                'accent' => '#ABCDEF',
            ],
            'typography' => 'system-serif',
            'contactEmail' => 'desk@example.test',
            'contactPhone' => '(614) 555-0100',
            'contactAddress' => '1 Synthetic Street',
            'publicCopy' => 'Synthetic public copy.',
            'publicSiteKey' => 'default_classic',
            'enabledModules' => ['public_site'],
            'schemaName' => 'matterforge',
        ])->assertOk();

        $this->assertSame(FirmLifecycleStatus::Active, $firm->refresh()->lifecycle_status);

        FirmPublicProfile::query()->where('firm_id', $firm->id)->update([
            'public_copy' => '<script>alert(1)</script>',
            'color_primary' => 'red;background:url(javascript:alert(1))',
            'enabled_modules' => json_encode(['public_site', 'billing', '<script>']),
        ]);

        $this->getJson('http://brand.example.test/api/public/branding')
            ->assertOk()
            ->assertJsonPath('source', 'firm')
            ->assertJsonPath('displayName', 'Public Brand')
            ->assertJsonPath('publicCopy', 'alert(1)')
            ->assertJsonPath('colors.primary', '#2F1F1A')
            ->assertJsonPath('colors.accent', '#ABCDEF')
            ->assertJsonMissingPath('id')
            ->assertDontSee('<script>', false)
            ->assertDontSee('schema_name', false)
            ->assertDontSee('billing', false);

        $this->getJson('http://pending.example.test/api/public/branding')
            ->assertOk()
            ->assertJsonPath('source', 'platform')
            ->assertJsonPath('displayName', 'MatterForge')
            ->assertDontSee('Hidden Pending Firm', false);

        $this->getJson('http://localhost/api/public/branding?firm_id='.$firm->id)
            ->assertOk()
            ->assertJsonPath('source', 'platform');

        $this->assertFalse(app(TenantContext::class)->isEstablished());
    }

    public function test_login_does_not_select_a_tenant_schema_or_reveal_it(): void
    {
        $admin = $this->administrator();
        $firm = $this->activateFirm($this->registerFirm('Login Firm'), $admin);
        $user = User::factory()->create();
        $this->membership($admin, $firm, $user);
        $schemaName = (string) $firm->refresh()->schema_name;

        $this->postJson('/api/auth/login', [
            'email' => $user->email,
            'password' => 'password',
        ])->assertOk()
            ->assertJsonPath('user.platformRole', null)
            ->assertJsonPath('user.platformPermissions', [])
            ->assertDontSee($schemaName, false)
            ->assertDontSee('mf_f_', false)
            ->assertJsonPath('user.memberships.0.fallbackHost', $firm->fallbackHostname());

        $this->assertFalse(app(TenantContext::class)->isEstablished());
    }

    public function test_client_supplied_schema_name_is_ignored_when_registering_a_firm(): void
    {
        Sanctum::actingAs($this->administrator());

        $this->postJson('/api/platform/firms', [
            'legalName' => 'Example Counsel LLP',
            'displayName' => 'Example Counsel',
            'schemaName' => 'matterforge',
            'schema_name' => 'mysql',
        ])->assertCreated()
            ->assertJsonPath('lifecycleStatus', 'pending_review')
            ->assertJsonMissingPath('schemaName');

        $this->assertNull(Firm::query()->firstOrFail()->schema_name);
    }

    public function test_firm_registration_creates_verified_platform_fallback_domain(): void
    {
        $firm = $this->registerFirm('Fallback Domain Firm');
        $expectedHost = $firm->fallbackHostname();
        $this->assertIsString($expectedHost);

        $domain = FirmDomain::query()
            ->where('firm_id', $firm->id)
            ->where('hostname', $expectedHost)
            ->first();

        $this->assertInstanceOf(FirmDomain::class, $domain);
        $this->assertNotNull($domain->verified_at);
        $this->assertSame('platform_managed', $domain->verified_source);
    }

    public function test_reserved_platform_hosts_cannot_be_claimed_as_tenant_domains(): void
    {
        $admin = $this->administrator();
        $firm = $this->activateFirm($this->registerFirm('Reserved Host Firm'), $admin);

        $this->expectException(ValidationException::class);
        app(FirmReviewService::class)->addDomain($firm, 'app.localhost', true, $admin);
    }

    public function test_domain_claim_verification_and_public_route_enforcement(): void
    {
        $admin = $this->administrator();
        $firm = $this->activateFirm($this->registerFirm('Domain Flow Firm'), $admin);
        Sanctum::actingAs($admin);

        $claim = $this->postJson('/api/platform/firms/'.$firm->id.'/domains', [
            'hostname' => 'Example.TEST.',
            'primary' => true,
        ])->assertCreated()->json();

        $domainId = (string) $claim['id'];
        $token = (string) ($claim['challenge']['token'] ?? '');
        $this->assertSame('example.test', FirmDomain::query()->findOrFail($domainId)->hostname);

        $this->postJson('http://example.test/api/consultations', [
            'fullName' => 'Prospect Person',
            'email' => 'prospect@example.test',
            'practiceArea' => 'CivilLitigation',
            'preferredAtUtc' => now()->addDay()->setTime(14, 0)->toIso8601String(),
            'timeZone' => 'America/New_York',
        ])->assertNotFound();

        $this->app->instance(DnsTxtLookup::class, new class extends DnsTxtLookup
        {
            public function records(string $recordName): array
            {
                return ['wrong-token'];
            }
        });

        $this->postJson('/api/platform/firms/'.$firm->id.'/domains/'.$domainId.'/verify')
            ->assertStatus(422);

        $this->app->instance(DnsTxtLookup::class, new class($token) extends DnsTxtLookup
        {
            public function __construct(private string $token) {}

            public function records(string $recordName): array
            {
                return [$this->token];
            }
        });

        $this->postJson('/api/platform/firms/'.$firm->id.'/domains/'.$domainId.'/verify')
            ->assertOk()
            ->assertJsonPath('verifiedSource', 'dns_txt');

        $this->postJson('http://example.test/api/consultations', [
            'fullName' => 'Prospect Person',
            'email' => 'prospect@example.test',
            'practiceArea' => 'CivilLitigation',
            'preferredAtUtc' => now()->addDay()->setTime(14, 0)->toIso8601String(),
            'timeZone' => 'America/New_York',
        ])->assertCreated();
    }

    public function test_header_firm_selector_is_blocked_when_disabled(): void
    {
        config(['tenancy.allow_header_firm_selection' => false]);

        $admin = $this->administrator();
        $firmA = $this->activateFirm($this->registerFirm('Header Alpha'), $admin);
        $firmB = $this->activateFirm($this->registerFirm('Header Beta'), $admin);
        $user = User::factory()->create();
        $this->membership($admin, $firmA, $user);
        $this->membership($admin, $firmB, $user);

        Sanctum::actingAs($user);
        $this->withHeader('X-Firm-Id', $firmA->id)
            ->getJson('/api/tenant/notes')
            ->assertForbidden()
            ->assertJsonPath('message', 'Tenant is not available.');
    }

    public function test_platform_administrator_without_membership_cannot_open_tenant_data(): void
    {
        $admin = $this->administrator();
        $this->activateFirm($this->registerFirm('No Implicit Access'), $admin);

        Sanctum::actingAs($admin);
        $this->getJson('/api/tenant/notes')->assertForbidden();
    }

    public function test_firm_partner_can_manage_users_within_their_firm_only(): void
    {
        $admin = $this->administrator();
        $firmA = $this->activateFirm($this->registerFirm('Firm Users Alpha'), $admin);
        $firmB = $this->activateFirm($this->registerFirm('Firm Users Beta'), $admin);
        $partner = User::factory()->create([
            'permissions' => config('roles.Partner'),
            'role' => 'Partner',
        ]);
        app(FirmReviewService::class)->saveMembership($firmA, $partner, 'Partner', $admin);
        app(FirmReviewService::class)->saveMembership($firmB, $partner, 'Partner', $admin);

        Sanctum::actingAs($partner);
        $created = $this->withHeader('X-Firm-Id', $firmA->id)->postJson('/api/tenant/users', [
            'name' => 'Taylor New Hire',
            'email' => 'taylor-'.Str::lower(Str::random(6)).'@example.test',
            'password' => 'Passw0rd!',
            'role' => 'Paralegal',
        ])->assertCreated();

        $this->withHeader('X-Firm-Id', $firmA->id)->getJson('/api/tenant/users')
            ->assertOk()
            ->assertJsonFragment(['name' => 'Taylor New Hire']);

        $newUserId = (int) $created->json('userId');
        $this->withHeader('X-Firm-Id', $firmA->id)->putJson('/api/tenant/users/'.$newUserId.'/membership', [
            'role' => 'Attorney',
        ])->assertOk()->assertJsonPath('membershipRole', 'Attorney');

        $this->withHeader('X-Firm-Id', $firmB->id)->putJson('/api/tenant/users/'.$newUserId.'/membership', [
            'role' => 'Partner',
        ])->assertNotFound();
    }

    public function test_non_partner_cannot_manage_firm_users_even_with_manage_permission(): void
    {
        $admin = $this->administrator();
        $firm = $this->activateFirm($this->registerFirm('Firm Users Restricted'), $admin);
        $attorney = User::factory()->create([
            'permissions' => array_values(array_unique(array_merge(
                config('roles.Attorney') ?? [],
                ['users.manage'],
            ))),
            'role' => 'Attorney',
        ]);
        $this->membership($admin, $firm, $attorney);

        Sanctum::actingAs($attorney);
        $this->withHeader('X-Firm-Id', $firm->id)->getJson('/api/tenant/users')
            ->assertForbidden()
            ->assertJsonPath('message', 'Only firm partners can manage users.');
    }

    public function test_platform_ops_cannot_execute_owner_only_transitions(): void
    {
        $owner = $this->administrator();
        $ops = $this->platformOperator();
        $firm = $this->registerFirm('Ops Boundary Firm');
        app(FirmReviewService::class)->approve($firm->refresh(), $owner, 'Synthetic approval.', ['file:synthetic']);

        Sanctum::actingAs($ops);
        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'reject',
            'reason' => 'Ops should not finalize this review.',
            'evidenceReferences' => ['file:blocked'],
        ])->assertForbidden()->assertJsonPath('required_platform_permission', 'platform.firms.review.finalize');

        $this->postJson('/api/platform/firms/'.$firm->id.'/provision')
            ->assertForbidden()
            ->assertJsonPath('required_platform_permission', 'platform.firms.provision');

        $active = $this->activateFirm($this->registerFirm('Ops Boundary Active'), $owner);
        $domainClaim = app(FirmReviewService::class)->addDomain($active, 'ops-boundary.example.test', true, $owner);
        $domainId = (string) $domainClaim['domain']->id;

        $this->postJson('/api/platform/firms/'.$active->id.'/suspend', [
            'reason' => 'Ops should not suspend firms.',
        ])->assertForbidden()->assertJsonPath('required_platform_permission', 'platform.firms.lifecycle.manage');

        $this->postJson('/api/platform/firms/'.$active->id.'/domains/'.$domainId.'/verify')
            ->assertForbidden()
            ->assertJsonPath('required_platform_permission', 'platform.firms.domains.finalize');

        $this->postJson('/api/platform/firms/'.$active->id.'/domains/'.$domainId.'/revoke')
            ->assertForbidden()
            ->assertJsonPath('required_platform_permission', 'platform.firms.domains.finalize');
    }

    public function test_platform_ops_can_execute_allowed_platform_operations(): void
    {
        $owner = $this->administrator();
        $ops = $this->platformOperator();
        $firm = $this->activateFirm($this->registerFirm('Ops Allowed Firm'), $owner);
        $member = User::factory()->create();

        Sanctum::actingAs($ops);
        $this->getJson('/api/platform/firms')->assertOk();

        $created = $this->postJson('/api/platform/firms', [
            'legalName' => 'Ops Created LLP',
            'displayName' => 'Ops Created',
        ])->assertCreated();
        $createdFirmId = (string) $created->json('id');

        $this->postJson('/api/onboarding/register', [
            'firmName' => 'Ops Verification Firm',
            'name' => 'Ops Verification User',
            'email' => 'ops-verification-'.Str::lower(Str::random(6)).'@example.test',
            'password' => 'Passw0rd!',
            'password_confirmation' => 'Passw0rd!',
            'businessPhone' => '(614) 555-0133',
        ])->assertCreated();
        $verificationFirmId = (string) Firm::query()->where('display_name', 'Ops Verification Firm')->value('id');

        $this->postJson('/api/platform/firms/'.$firm->id.'/memberships', [
            'userId' => $member->id,
            'role' => 'Attorney',
        ])->assertCreated();

        $domainResponse = $this->postJson('/api/platform/firms/'.$firm->id.'/domains', [
            'hostname' => 'ops-managed.example.test',
            'primary' => false,
        ])->assertCreated();
        $domainId = (string) $domainResponse->json('id');

        $this->postJson('/api/platform/firms/'.$firm->id.'/domains/'.$domainId.'/reissue-challenge')
            ->assertOk();

        $this->putJson('/api/platform/firms/'.$firm->id.'/branding', [
            'logoPath' => '/storage/branding/ops-mark.png',
            'faviconPath' => '/storage/branding/ops-mark.ico',
            'colors' => [
                'primary' => '#112233',
                'secondary' => '#445566',
                'accent' => '#778899',
            ],
            'typography' => 'system-sans',
            'contactEmail' => 'ops@example.test',
            'contactPhone' => '(614) 555-0199',
            'contactAddress' => '99 Ops Avenue',
            'publicCopy' => 'Ops-managed public profile.',
            'publicSiteKey' => 'default_classic',
            'enabledModules' => ['public_site'],
        ])->assertOk();

        $this->postJson('/api/platform/firms/'.$verificationFirmId.'/verifications', [
            'verificationType' => 'business_existence',
            'status' => 'verified',
            'method' => 'manual',
            'notes' => 'Ops confirmed formation records.',
            'evidenceReferences' => ['file:ops-verification'],
        ])->assertOk();

        $this->assertDatabaseHas('firms', [
            'id' => $createdFirmId,
            'display_name' => 'Ops Created',
        ]);
    }

    private function administrator(): User
    {
        return User::factory()->create([
            'is_platform_administrator' => true,
            'platform_role' => 'platform_owner',
        ]);
    }

    private function platformOperator(): User
    {
        return User::factory()->create([
            'is_platform_administrator' => true,
            'platform_role' => 'platform_ops',
        ]);
    }

    private function registerFirm(string $displayName): Firm
    {
        return app(FirmRegistry::class)->register($displayName.' LLP', $displayName);
    }

    private function activateFirm(Firm $firm, User $admin): Firm
    {
        if ($firm->lifecycle_status === FirmLifecycleStatus::PendingReview) {
            app(FirmReviewService::class)->approve($firm, $admin, 'Synthetic approval.', ['file:synthetic']);
        }

        return app(TenantSchemaProvisioner::class)->provision($firm->refresh(), $admin);
    }

    private function membership(User $admin, Firm $firm, User $member): void
    {
        app(FirmReviewService::class)->saveMembership($firm, $member, 'Attorney', $admin);
    }

    private function domain(User $admin, Firm $firm, string $hostname): FirmDomain
    {
        $review = app(FirmReviewService::class);
        $result = $review->addDomain($firm, $hostname, true, $admin);
        $token = $result['challenge']['token'];

        $this->app->instance(DnsTxtLookup::class, new class($token) extends DnsTxtLookup
        {
            public function __construct(private string $token) {}

            public function records(string $recordName): array
            {
                return [$this->token];
            }
        });

        return app(FirmReviewService::class)->verifyDomain($firm, $result['domain'], $admin);
    }
}
