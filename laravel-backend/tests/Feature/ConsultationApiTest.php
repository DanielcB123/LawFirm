<?php

namespace Tests\Feature;

use App\Models\ConsultationRequest;
use App\Models\Firm;
use App\Models\User;
use App\Tenancy\DnsTxtLookup;
use App\Tenancy\FirmRegistry;
use App\Tenancy\FirmReviewService;
use App\Tenancy\TenantConnectionManager;
use App\Tenancy\TenantSchemaProvisioner;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class ConsultationApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_public_user_can_create_consultation_request(): void
    {
        $firm = $this->verifiedPublicFirm('consult.example.test');
        $payload = [
            'fullName' => 'Jamie Prospect',
            'email' => 'jamie.prospect@example.com',
            'practiceArea' => 'CivilLitigation',
            'preferredAtUtc' => now()->addDays(2)->setTime(14, 0)->toISOString(),
            'timeZone' => 'America/New_York',
        ];

        $this->postJson('http://consult.example.test/api/consultations', $payload)
            ->assertCreated()
            ->assertJsonPath('fullName', 'Jamie Prospect')
            ->assertJsonPath('status', 'Requested');
    }

    public function test_public_submission_is_visible_in_authenticated_queue(): void
    {
        $firm = $this->verifiedPublicFirm('queue.example.test');
        [$user] = $this->memberUser($firm, ['consultations.read']);

        $this->postJson('http://queue.example.test/api/consultations', [
            'fullName' => 'Queue Prospect',
            'email' => 'queue.prospect@example.com',
            'practiceArea' => 'ConstructionLaw',
            'preferredAtUtc' => now()->addDays(3)->setTime(10, 0)->toISOString(),
            'timeZone' => 'America/New_York',
        ])->assertCreated();

        Sanctum::actingAs($user);

        $this->withHeader('X-Firm-Id', $firm->id)->getJson('/api/tenant/consultations')
            ->assertOk()
            ->assertJsonFragment([
                'fullName' => 'Queue Prospect',
                'email' => 'queue.prospect@example.com',
            ]);
    }

    public function test_available_slots_endpoint_returns_values(): void
    {
        $this->verifiedPublicFirm('slots.example.test');
        $this->getJson('http://slots.example.test/api/consultations/available-slots?days=3')
            ->assertOk()
            ->assertJsonStructure([
                '*' => ['startsAtUtc', 'endsAtUtc'],
            ]);
    }

    public function test_queue_requires_authentication(): void
    {
        $this->getJson('/api/tenant/consultations')->assertUnauthorized();
    }

    public function test_queue_requires_consultations_read_permission(): void
    {
        $firm = $this->verifiedPublicFirm('forbidden.example.test');
        [$user] = $this->memberUser($firm, ['contacts.read']);
        Sanctum::actingAs($user);

        $this->withHeader('X-Firm-Id', $firm->id)->getJson('/api/tenant/consultations')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'consultations.read');
    }

    public function test_authorized_user_can_update_consultation_status(): void
    {
        $firm = $this->verifiedPublicFirm('update.example.test');
        [$user] = $this->memberUser($firm, ['consultations.read', 'consultations.manage']);
        Sanctum::actingAs($user);

        app(TenantConnectionManager::class)->establish($firm);
        try {
            $consultation = ConsultationRequest::factory()->create([
                'status' => 'Requested',
            ]);
        } finally {
            app(TenantConnectionManager::class)->clear();
        }

        $this->withHeader('X-Firm-Id', $firm->id)->postJson("/api/tenant/consultations/{$consultation->id}/status", [
            'status' => 'Scheduled',
            'assignedToActorId' => 'user-100',
            'assignedToDisplayName' => 'Liam Legal Assistant',
            'internalNotes' => 'Confirmed follow-up call',
        ])
            ->assertOk()
            ->assertJsonPath('status', 'Scheduled')
            ->assertJsonPath('assignedToDisplayName', 'Liam Legal Assistant');
    }

    private function verifiedPublicFirm(string $hostname): Firm
    {
        $admin = User::factory()->create(['is_platform_administrator' => true]);
        $firm = app(FirmRegistry::class)->register($hostname.' LLC', $hostname, $admin);
        $firm = app(FirmReviewService::class)->approve($firm, $admin, 'approved', ['test']);
        $firm = app(TenantSchemaProvisioner::class)->provision($firm, $admin);

        $result = app(FirmReviewService::class)->addDomain($firm, $hostname, true, $admin);
        $token = $result['challenge']['token'];
        $this->app->instance(DnsTxtLookup::class, new class($token) extends DnsTxtLookup
        {
            public function __construct(private string $token) {}

            public function records(string $recordName): array
            {
                return [$this->token];
            }
        });
        app(FirmReviewService::class)->verifyDomain($firm, $result['domain'], $admin);

        return $firm->refresh();
    }

    /**
     * @param  list<string>  $permissions
     * @return array{User}
     */
    private function memberUser(Firm $firm, array $permissions): array
    {
        $admin = User::query()->where('is_platform_administrator', true)->firstOrFail();
        $member = User::factory()->create(['permissions' => $permissions]);
        app(FirmReviewService::class)->saveMembership($firm, $member, 'Attorney', $admin);

        return [$member];
    }
}
