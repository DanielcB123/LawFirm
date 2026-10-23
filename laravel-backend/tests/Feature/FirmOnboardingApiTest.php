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
            ])->assertOk();
        }
    }
}
