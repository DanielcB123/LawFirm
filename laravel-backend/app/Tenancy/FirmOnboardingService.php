<?php

namespace App\Tenancy;

use App\Enums\FirmLifecycleStatus;
use App\Enums\FirmProvisioningStatus;
use App\Models\Firm;
use App\Models\FirmOnboardingApplication;
use App\Models\FirmOnboardingVerification;
use App\Models\User;
use App\Tenancy\Exceptions\FirmLifecycleException;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Hash;
use Illuminate\Validation\ValidationException;

class FirmOnboardingService
{
    public function __construct(
        private FirmRegistry $registry,
        private FirmReviewService $review,
        private TenantSchemaProvisioner $provisioner,
        private OnboardingFastTrackGuard $fastTrackGuard,
        private FirmAuditRecorder $audit,
    ) {}

    /**
     * @param  array{name:string,email:string,password:string,firmName:string,businessPhone?:?string}  $attributes
     * @return array{token:string,user:User,application:FirmOnboardingApplication}
     */
    public function registerGuestAndSubmit(array $attributes): array
    {
        return DB::transaction(function () use ($attributes): array {
            $role = 'Partner';
            $permissions = config('roles.'.$role, []);
            if (! is_array($permissions)) {
                $permissions = [];
            }

            $user = User::query()->create([
                'name' => $attributes['name'],
                'email' => strtolower($attributes['email']),
                'password' => Hash::make($attributes['password']),
                'role' => $role,
                'permissions' => array_values(array_filter($permissions, 'is_string')),
                'is_platform_administrator' => false,
            ]);

            $application = $this->createApplication(
                applicant: $user,
                firmName: $attributes['firmName'],
                applicantName: $attributes['name'],
                applicantEmail: $attributes['email'],
                businessPhone: $attributes['businessPhone'] ?? null,
            );

            $token = $user->createToken('firm-onboarding');

            return [
                'token' => $token->plainTextToken,
                'user' => $user->fresh(),
                'application' => $application->fresh('firm'),
            ];
        });
    }

    /**
     * @param  array{firmName:string,applicantName?:?string,applicantEmail?:?string,businessPhone?:?string}  $attributes
     */
    public function submitForAuthenticatedUser(User $applicant, array $attributes): FirmOnboardingApplication
    {
        return DB::transaction(function () use ($applicant, $attributes): FirmOnboardingApplication {
            $existing = FirmOnboardingApplication::query()
                ->where('applicant_user_id', $applicant->id)
                ->whereHas('firm', function ($query): void {
                    $query->whereIn('lifecycle_status', [
                        FirmLifecycleStatus::PendingReview->value,
                        FirmLifecycleStatus::Approved->value,
                        FirmLifecycleStatus::Rejected->value,
                    ]);
                })
                ->latest('created_at')
                ->first();

            if ($existing instanceof FirmOnboardingApplication) {
                throw ValidationException::withMessages([
                    'firmName' => 'You already have an onboarding application in progress.',
                ]);
            }

            return $this->createApplication(
                applicant: $applicant,
                firmName: $attributes['firmName'],
                applicantName: ($attributes['applicantName'] ?? null) ?: $applicant->name,
                applicantEmail: ($attributes['applicantEmail'] ?? null) ?: $applicant->email,
                businessPhone: $attributes['businessPhone'] ?? null,
            );
        });
    }

    public function latestApplicationFor(User $applicant): ?FirmOnboardingApplication
    {
        return FirmOnboardingApplication::query()
            ->where('applicant_user_id', $applicant->id)
            ->with(['firm.domains', 'firm.memberships', 'reviews', 'verifications'])
            ->latest('created_at')
            ->first();
    }

    /**
     * @param  array{firmName:string,businessPhone?:?string}  $attributes
     */
    public function updateApplication(User $applicant, FirmOnboardingApplication $application, array $attributes): FirmOnboardingApplication
    {
        if ($application->applicant_user_id !== $applicant->id) {
            throw ValidationException::withMessages([
                'application' => 'Application is not available.',
            ]);
        }

        $firm = $application->firm()->firstOrFail();
        if (! in_array($firm->lifecycle_status, [FirmLifecycleStatus::PendingReview, FirmLifecycleStatus::Rejected], true)) {
            throw new FirmLifecycleException('The application cannot be changed from its current state.');
        }

        return DB::transaction(function () use ($application, $firm, $attributes): FirmOnboardingApplication {
            $nextRevision = $application->revision + 1;
            $firm->forceFill([
                'legal_name' => $attributes['firmName'],
                'display_name' => $attributes['firmName'],
                'onboarding_revision' => $nextRevision,
                'lifecycle_status' => FirmLifecycleStatus::PendingReview,
                'provisioning_status' => FirmProvisioningStatus::NotRequested,
                'provisioning_failure_reason' => null,
                'approved_at' => null,
                'rejected_at' => null,
            ])->save();

            $application->forceFill([
                'revision' => $nextRevision,
                'business_phone' => $attributes['businessPhone'] ?? null,
                'last_material_change_at' => now(),
            ])->save();

            $this->initializeVerificationRows($firm->refresh(), $application->refresh());
            $this->audit->record($firm->refresh(), $application->applicant()->first(), 'onboarding.application.revised', 'Onboarding application revised. Existing approvals invalidated.');

            return $application->refresh(['firm', 'reviews', 'verifications']);
        });
    }

    public function runFastTrack(FirmOnboardingApplication $application, User $actor): FirmOnboardingApplication
    {
        $this->fastTrackGuard->assertRuntimeAllowed();
        $firm = $application->firm()->firstOrFail();

        return DB::transaction(function () use ($application, $firm, $actor): FirmOnboardingApplication {
            $locked = Firm::query()->whereKey($firm->id)->lockForUpdate()->firstOrFail();
            if ($locked->isUsable()) {
                return $application->refresh(['firm', 'reviews', 'verifications']);
            }

            $application->forceFill([
                'dev_fast_tracked' => true,
                'fast_tracked_at' => now(),
            ])->save();

            foreach ($this->requiredVerificationTypes() as $type) {
                FirmOnboardingVerification::query()->updateOrCreate(
                    [
                        'firm_id' => $locked->id,
                        'revision' => $application->revision,
                        'verification_type' => $type,
                    ],
                    [
                        'status' => 'bypassed',
                        'method' => 'development_bypass',
                        'reviewer_user_id' => $actor->id,
                        'notes' => 'Local development fast-track bypass.',
                        'reviewed_at' => now(),
                    ],
                );
            }

            $locked->forceFill([
                'lifecycle_status' => FirmLifecycleStatus::Approved,
                'provisioning_status' => FirmProvisioningStatus::Requested,
                'approved_at' => now(),
                'applicant_user_id' => $application->applicant_user_id,
            ])->save();
            $this->audit->record($locked->refresh(), $actor, 'onboarding.fast_track.enabled', 'Development fast-track bypass executed.');

            $this->provisionAndAssignOwner($locked->refresh(), $actor, 'Partner');

            return $application->refresh(['firm', 'reviews', 'verifications']);
        });
    }

    public function provisionAndAssignOwner(Firm $firm, ?User $actor, string $ownerRole = 'Partner'): Firm
    {
        $provisioned = $this->provisioner->provision($firm, $actor);
        $application = $firm->onboardingApplication()->first();

        if ($application instanceof FirmOnboardingApplication) {
            $applicant = $application->applicant()->first();
            if ($applicant instanceof User) {
                $this->review->saveMembership($provisioned, $applicant, $ownerRole, $actor);
                $this->audit->record($provisioned->refresh(), $actor, 'onboarding.owner.assigned', 'Onboarding owner membership assigned after provisioning.');
            }
        }

        return $provisioned->refresh();
    }

    /**
     * @return array<string, mixed>
     */
    public function statusPayload(User $applicant): array
    {
        $application = $this->latestApplicationFor($applicant);
        if (! $application instanceof FirmOnboardingApplication) {
            return [
                'hasApplication' => false,
                'fastTrackEnabled' => $this->fastTrackGuard->isEnabled(),
                'developmentNotice' => $this->fastTrackGuard->isEnabled() ? config('onboarding.local_mode_notice') : null,
            ];
        }

        $firm = $application->firm()->firstOrFail()->loadMissing(['domains', 'memberships']);
        $revision = $application->revision;
        $approvals = $firm->onboardingReviews()
            ->where('revision', $revision)
            ->where('decision', 'approve')
            ->distinct('reviewer_user_id')
            ->count('reviewer_user_id');

        $stage = match (true) {
            $firm->lifecycle_status === FirmLifecycleStatus::Rejected => 'rejected',
            $firm->lifecycle_status === FirmLifecycleStatus::Active && $firm->provisioning_status === FirmProvisioningStatus::Ready => 'active',
            $firm->provisioning_status === FirmProvisioningStatus::Failed => 'provisioning_failed',
            in_array($firm->provisioning_status, [FirmProvisioningStatus::Requested, FirmProvisioningStatus::Provisioning], true) || $firm->lifecycle_status === FirmLifecycleStatus::Approved => 'provisioning',
            $approvals === 1 => 'awaiting_second_approval',
            default => 'pending_review',
        };

        $verificationMap = $firm->onboardingVerifications()
            ->where('revision', $revision)
            ->get()
            ->keyBy('verification_type');

        return [
            'hasApplication' => true,
            'fastTrackEnabled' => $this->fastTrackGuard->isEnabled(),
            'developmentNotice' => $this->fastTrackGuard->isEnabled() ? config('onboarding.local_mode_notice') : null,
            'application' => [
                'id' => $application->id,
                'firmId' => $firm->id,
                'firmName' => $firm->display_name,
                'revision' => $revision,
                'stage' => $stage,
                'lifecycleStatus' => $firm->lifecycle_status->value,
                'provisioningStatus' => $firm->provisioning_status->value,
                'provisioningFailureReason' => $firm->provisioning_failure_reason,
                'approvalCount' => $approvals,
                'requiredApprovals' => 2,
                'preferredHost' => $firm->preferredHostname(),
                'fallbackHost' => $firm->fallbackHostname(),
                'ownerMembershipReady' => $firm->memberships->contains(fn ($membership): bool => $membership->user_id === $application->applicant_user_id),
                'verifications' => collect($this->requiredVerificationTypes())->map(function (string $type) use ($verificationMap): array {
                    /** @var FirmOnboardingVerification|null $verification */
                    $verification = $verificationMap->get($type);

                    return [
                        'type' => $type,
                        'status' => $verification?->status ?? 'pending',
                        'method' => $verification?->method ?? 'manual',
                        'reviewedAt' => $verification?->reviewed_at?->toIso8601String(),
                    ];
                })->values()->all(),
            ],
        ];
    }

    private function createApplication(
        User $applicant,
        string $firmName,
        string $applicantName,
        string $applicantEmail,
        ?string $businessPhone,
    ): FirmOnboardingApplication {
        $firm = $this->registry->register($firmName, $firmName, $applicant);
        $firm->forceFill([
            'applicant_user_id' => $applicant->id,
            'onboarding_revision' => 1,
        ])->save();

        $application = FirmOnboardingApplication::query()->create([
            'firm_id' => $firm->id,
            'applicant_user_id' => $applicant->id,
            'applicant_name' => $applicantName,
            'applicant_email' => strtolower($applicantEmail),
            'business_phone' => $businessPhone,
            'revision' => 1,
            'submitted_at' => now(),
            'last_material_change_at' => now(),
        ]);

        $this->initializeVerificationRows($firm->refresh(), $application);
        $this->audit->record($firm->refresh(), $applicant, 'onboarding.application.submitted', 'Onboarding application submitted.');

        return $application->refresh();
    }

    private function initializeVerificationRows(Firm $firm, FirmOnboardingApplication $application): void
    {
        foreach ($this->requiredVerificationTypes() as $type) {
            $status = 'pending';
            $method = 'manual';
            $notes = null;

            if ($type === 'domain_ownership' && $firm->domains()->whereNotNull('verified_at')->whereNull('revoked_at')->exists()) {
                $status = 'verified';
                $method = 'platform_managed';
                $notes = 'Verified fallback hostname is present.';
            }

            FirmOnboardingVerification::query()->updateOrCreate(
                [
                    'firm_id' => $firm->id,
                    'revision' => $application->revision,
                    'verification_type' => $type,
                ],
                [
                    'status' => $status,
                    'method' => $method,
                    'notes' => $notes,
                    'reviewed_at' => $status === 'verified' ? now() : null,
                    'reviewer_user_id' => null,
                ],
            );
        }
    }

    /**
     * @return list<string>
     */
    private function requiredVerificationTypes(): array
    {
        $types = config('onboarding.required_verifications');
        if (! is_array($types)) {
            return ['business_existence', 'applicant_authority', 'applicant_email', 'domain_ownership'];
        }

        return array_values(array_filter($types, fn (mixed $type): bool => is_string($type) && $type !== ''));
    }
}
