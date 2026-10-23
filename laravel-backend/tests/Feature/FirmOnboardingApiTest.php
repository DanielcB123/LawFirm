<?php

namespace Tests\Feature;

use App\Enums\FirmLifecycleStatus;
use App\Enums\FirmProvisioningStatus;
use App\Models\Firm;
use App\Models\FirmMembership;
use App\Models\FirmOnboardingApplication;
use App\Models\FirmOnboardingVerification;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class FirmOnboardingApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_guest_can_register_a_firm_onboarding_application(): void
    {
        $response = $this->postJson('/api/onboarding/register', [
            'firmName' => 'Atlas Trial Counsel',
            'name' => 'Case Owner',
            'email' => 'owner@example.test',
            'password' => 'Passw0rd!',
            'password_confirmation' => 'Passw0rd!',
            'businessPhone' => '(614) 555-0101',
        ])->assertCreated();

        $userId = (int) $response->json('user.id');
        $this->assertNotNull($response->json('access_token'));

        $firm = Firm::query()->where('display_name', 'Atlas Trial Counsel')->firstOrFail();
        $this->assertSame(FirmLifecycleStatus::PendingReview, $firm->lifecycle_status);
        $this->assertSame(FirmProvisioningStatus::NotRequested, $firm->provisioning_status);
        $this->assertSame($userId, $firm->applicant_user_id);

        $application = FirmOnboardingApplication::query()->where('firm_id', $firm->id)->firstOrFail();
        $this->assertSame('owner@example.test', $application->applicant_email);
        $this->assertSame(1, $application->revision);

        $this->assertSame(0, FirmMembership::query()->where('firm_id', $firm->id)->count());
        $this->assertGreaterThanOrEqual(1, FirmOnboardingVerification::query()->where('firm_id', $firm->id)->count());
    }

    public function test_authenticated_user_can_submit_firm_application(): void
    {
        $user = User::factory()->create([
            'role' => 'Partner',
            'permissions' => config('roles.Partner'),
        ]);
        Sanctum::actingAs($user);

        $this->postJson('/api/onboarding/applications', [
            'firmName' => 'Summit Legal Group',
            'businessPhone' => '(614) 555-0122',
        ])->assertCreated();

        $this->assertDatabaseHas('firms', [
            'display_name' => 'Summit Legal Group',
            'applicant_user_id' => $user->id,
        ]);
    }

    public function test_guest_can_check_firm_name_availability(): void
    {
        $this->postJson('/api/onboarding/register', [
            'firmName' => 'Riverstone Legal',
            'name' => 'Riverstone Owner',
            'email' => 'riverstone-owner@example.test',
            'password' => 'Passw0rd!',
            'password_confirmation' => 'Passw0rd!',
        ])->assertCreated();

        $this->getJson('/api/onboarding/firm-name-availability?firmName=Riverstone Legal')
            ->assertOk()
            ->assertJsonPath('available', false)
            ->assertJsonPath('slug', 'riverstone-legal')
            ->assertJsonCount(5, 'suggestions');
    }

    public function test_registration_rejects_firm_name_when_slug_is_unavailable(): void
    {
        $this->postJson('/api/onboarding/register', [
            'firmName' => 'Keystone Trial Group',
            'name' => 'Keystone Owner',
            'email' => 'keystone-owner@example.test',
            'password' => 'Passw0rd!',
            'password_confirmation' => 'Passw0rd!',
        ])->assertCreated();

        $this->postJson('/api/onboarding/register', [
            'firmName' => 'Keystone Trial Group',
            'name' => 'Second Applicant',
            'email' => 'keystone-second@example.test',
            'password' => 'Passw0rd!',
            'password_confirmation' => 'Passw0rd!',
        ])->assertStatus(422)
            ->assertJsonValidationErrors(['firmName']);
    }

    public function test_two_distinct_platform_reviewers_are_required(): void
    {
        $this->registerApplicantWithFirm('North Star Law');
        $firm = Firm::query()->where('display_name', 'North Star Law')->firstOrFail();

        $reviewerOne = User::factory()->create(['is_platform_administrator' => true]);
        $reviewerTwo = User::factory()->create(['is_platform_administrator' => true]);

        Sanctum::actingAs($reviewerOne);
        $this->markRequiredVerificationsVerified($firm);

        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'approve',
            'reason' => 'First reviewer approved.',
            'evidenceReferences' => ['manual:one'],
        ])->assertOk();

        $this->assertSame(FirmLifecycleStatus::PendingReview, $firm->refresh()->lifecycle_status);

        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'approve',
            'reason' => 'Duplicate reviewer attempt.',
            'evidenceReferences' => ['manual:dup'],
        ])->assertStatus(422);

        Sanctum::actingAs($reviewerTwo);
        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'approve',
            'reason' => 'Second reviewer approved.',
            'evidenceReferences' => ['manual:two'],
        ])->assertOk();

        $firm->refresh();
        $this->assertSame(FirmLifecycleStatus::Approved, $firm->lifecycle_status);
        $this->assertSame(FirmProvisioningStatus::Requested, $firm->provisioning_status);
    }

    public function test_members_of_applicant_firm_cannot_approve(): void
    {
        $this->registerApplicantWithFirm('Cedar Legal');
        $firm = Firm::query()->where('display_name', 'Cedar Legal')->firstOrFail();
        $reviewerAdmin = User::factory()->create(['is_platform_administrator' => true]);
        $conflictedReviewer = User::factory()->create(['is_platform_administrator' => true]);

        Sanctum::actingAs($reviewerAdmin);
        $this->markRequiredVerificationsVerified($firm);
        $this->postJson('/api/platform/firms/'.$firm->id.'/memberships', [
            'userId' => $conflictedReviewer->id,
            'role' => 'Partner',
        ])->assertCreated();

        Sanctum::actingAs($conflictedReviewer);
        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'approve',
            'reason' => 'Conflicted reviewer attempt.',
            'evidenceReferences' => ['manual:conflict'],
        ])->assertStatus(422);
    }

    public function test_rejected_onboarding_application_can_be_reopened_and_reapproved(): void
    {
        $this->registerApplicantWithFirm('Reopen Candidate Law');
        $firm = Firm::query()->where('display_name', 'Reopen Candidate Law')->firstOrFail();

        $reviewerOne = User::factory()->create(['is_platform_administrator' => true]);
        $reviewerTwo = User::factory()->create(['is_platform_administrator' => true]);

        Sanctum::actingAs($reviewerOne);
        $this->markRequiredVerificationsVerified($firm);
        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'reject',
            'reason' => 'Initial rejection due to missing authority proof.',
            'evidenceReferences' => ['manual:reject'],
        ])->assertOk();

        $this->assertSame(FirmLifecycleStatus::Rejected, $firm->refresh()->lifecycle_status);

        $this->postJson('/api/platform/firms/'.$firm->id.'/reopen', [
            'reason' => 'Evidence completed and ready for fresh review.',
        ])->assertOk();

        $this->assertSame(FirmLifecycleStatus::PendingReview, $firm->refresh()->lifecycle_status);

        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'approve',
            'reason' => 'First reviewer approval after reopen.',
            'evidenceReferences' => ['manual:approve-1'],
        ])->assertOk();

        Sanctum::actingAs($reviewerTwo);
        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'approve',
            'reason' => 'Second reviewer approval after reopen.',
            'evidenceReferences' => ['manual:approve-2'],
        ])->assertOk();

        $firm->refresh();
        $this->assertSame(FirmLifecycleStatus::Approved, $firm->lifecycle_status);
        $this->assertSame(FirmProvisioningStatus::Requested, $firm->provisioning_status);
    }

    public function test_simplified_dev_flow_can_provision_rejected_firm_after_verifications(): void
    {
        config(['onboarding.dev_simplified_flow' => true]);
        config(['onboarding.firm_demo_users.enabled' => true]);
        config(['onboarding.firm_demo_users.email_domain' => 'firms@localhost']);
        config(['onboarding.firm_demo_users.password' => 'Passw0rd!']);

        $this->registerApplicantWithFirm('Simplified Provision Firm');
        $firm = Firm::query()->where('display_name', 'Simplified Provision Firm')->firstOrFail();
        $admin = User::factory()->create(['is_platform_administrator' => true]);

        Sanctum::actingAs($admin);
        $this->markRequiredVerificationsVerified($firm);
        $this->postJson('/api/platform/firms/'.$firm->id.'/review', [
            'decision' => 'reject',
            'reason' => 'Rejected before final documents.',
            'evidenceReferences' => ['manual:reject'],
        ])->assertOk();

        $this->postJson('/api/platform/firms/'.$firm->id.'/provision')
            ->assertOk()
            ->assertJsonPath('lifecycleStatus', 'active')
            ->assertJsonPath('provisioningStatus', 'ready');

        $this->assertDatabaseHas('users', [
            'email' => 'attorney.simplified-provision-firm.firms@localhost',
            'role' => 'Attorney',
        ]);
    }

    public function test_fast_track_is_disabled_by_default(): void
    {
        $applicant = $this->registerApplicantWithFirm('Harbor Counsel');
        Sanctum::actingAs($applicant);

        $this->postJson('/api/onboarding/applications/current/fast-track')
            ->assertStatus(403);
    }

    public function test_fast_track_can_activate_local_firm_without_marking_email_verified(): void
    {
        config(['onboarding.dev_fast_track' => true]);
        config(['onboarding.firm_demo_users.enabled' => true]);
        config(['onboarding.firm_demo_users.email_domain' => 'firms@localhost']);
        config(['onboarding.firm_demo_users.password' => 'Passw0rd!']);

        $applicant = $this->registerApplicantWithFirm('Rapid Local Firm');
        Sanctum::actingAs($applicant);

        $this->postJson('/api/onboarding/applications/current/fast-track')
            ->assertOk()
            ->assertJsonPath('application.stage', 'active');

        $firm = Firm::query()->where('display_name', 'Rapid Local Firm')->firstOrFail();
        $this->assertSame(FirmLifecycleStatus::Active, $firm->lifecycle_status);
        $this->assertSame(FirmProvisioningStatus::Ready, $firm->provisioning_status);
        $this->assertDatabaseHas('firm_memberships', [
            'firm_id' => $firm->id,
            'user_id' => $applicant->id,
            'role' => 'Partner',
        ]);
        $this->assertDatabaseHas('users', [
            'email' => 'attorney.rapid-local-firm.firms@localhost',
            'role' => 'Attorney',
        ]);
        $demoAttorney = User::query()->where('email', 'attorney.rapid-local-firm.firms@localhost')->firstOrFail();
        $this->assertDatabaseHas('firm_memberships', [
            'firm_id' => $firm->id,
            'user_id' => $demoAttorney->id,
            'role' => 'Attorney',
        ]);
        $this->assertNull($applicant->refresh()->email_verified_at);
    }

    public function test_provider_verification_fails_closed_when_unconfigured(): void
    {
        $this->registerApplicantWithFirm('Provider Check Firm');
        $firm = Firm::query()->where('display_name', 'Provider Check Firm')->firstOrFail();
        $admin = User::factory()->create(['is_platform_administrator' => true]);
        Sanctum::actingAs($admin);

        $this->postJson('/api/platform/firms/'.$firm->id.'/verifications', [
            'verificationType' => 'business_existence',
            'status' => 'verified',
            'method' => 'provider',
            'notes' => 'Provider attempt.',
        ])->assertStatus(422);
    }

    public function test_verified_status_requires_verification_evidence_fields(): void
    {
        $this->registerApplicantWithFirm('Evidence Required Firm');
        $firm = Firm::query()->where('display_name', 'Evidence Required Firm')->firstOrFail();
        $admin = User::factory()->create(['is_platform_administrator' => true]);
        Sanctum::actingAs($admin);

        $this->postJson('/api/platform/firms/'.$firm->id.'/verifications', [
            'verificationType' => 'business_existence',
            'status' => 'verified',
            'method' => 'manual',
            'notes' => 'Trying to verify without evidence.',
            'evidence' => [
                'legal_business_name' => 'Evidence Required Firm',
            ],
        ])->assertStatus(422)
            ->assertJsonValidationErrors(['evidence']);
    }

    private function registerApplicantWithFirm(string $firmName): User
    {
        $email = strtolower(str_replace(' ', '-', $firmName)).'@example.test';

        $response = $this->postJson('/api/onboarding/register', [
            'firmName' => $firmName,
            'name' => 'Applicant '.$firmName,
            'email' => $email,
            'password' => 'Passw0rd!',
            'password_confirmation' => 'Passw0rd!',
        ])->assertCreated();

        $userId = (int) $response->json('user.id');

        return User::query()->findOrFail($userId);
    }

    private function markRequiredVerificationsVerified(Firm $firm): void
    {
        $required = config('onboarding.required_verifications', []);
        if (! is_array($required)) {
            return;
        }

        foreach ($required as $verificationType) {
            if (! is_string($verificationType) || $verificationType === 'domain_ownership') {
                continue;
            }

            $this->postJson('/api/platform/firms/'.$firm->id.'/verifications', [
                'verificationType' => $verificationType,
                'status' => 'verified',
                'method' => 'manual',
                'notes' => 'Manual verification complete.',
                'evidence' => $this->evidenceFor($verificationType, $firm),
            ])->assertOk();
        }
    }

    /**
     * @return array<string, string>
     */
    private function evidenceFor(string $verificationType, Firm $firm): array
    {
        return match ($verificationType) {
            'business_existence' => [
                'legal_business_name' => $firm->legal_name,
                'tin_ein_last4' => '1234',
                'irs_lookup_reference' => 'irs:entity-check:1234',
            ],
            'applicant_authority' => [
                'applicant_role_title' => 'Partner',
                'authority_source' => 'Firm public site profile',
                'authority_reference' => 'https://example.test/team/profile',
            ],
            'applicant_email' => [
                'verified_email' => 'owner@example.test',
                'verification_method' => 'challenge_link',
                'proof_reference' => 'email-proof-123',
            ],
            'domain_ownership' => [
                'hostname' => $firm->fallbackHostname() ?? 'fallback.firms.localhost',
                'verification_source' => 'platform_managed',
                'proof_reference' => 'domain-proof-123',
            ],
            default => [],
        };
    }
}
