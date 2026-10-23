<?php

namespace App\Tenancy;

use App\Enums\FirmLifecycleStatus;
use App\Enums\FirmProvisioningStatus;
use App\Models\Firm;
use App\Models\FirmDomain;
use App\Models\FirmOnboardingApplication;
use App\Models\FirmOnboardingReview;
use App\Models\FirmOnboardingVerification;
use App\Models\FirmMembership;
use App\Models\FirmVerification;
use App\Models\User;
use App\Tenancy\Exceptions\FirmLifecycleException;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Str;
use Illuminate\Validation\ValidationException;

class FirmReviewService
{
    public function __construct(
        private FirmAuditRecorder $audit,
        private HostnameNormalizer $hosts,
        private DomainOwnershipProofService $domainProofs,
    ) {}

    /**
     * @param  list<string>  $evidenceReferences
     */
    public function approve(Firm $firm, User $reviewer, string $reason, array $evidenceReferences): Firm
    {
        $application = $firm->onboardingApplication()->first();
        if ($application instanceof FirmOnboardingApplication) {
            return $this->approveOnboardingApplication($firm, $application, $reviewer, $reason, $evidenceReferences);
        }

        return DB::transaction(function () use ($firm, $reviewer, $reason, $evidenceReferences): Firm {
            $this->storeVerification($firm, $reviewer, 'approve', $reason, $evidenceReferences);
            $firm->forceFill([
                'lifecycle_status' => FirmLifecycleStatus::Approved,
                'provisioning_status' => FirmProvisioningStatus::Requested,
                'approved_at' => now(),
            ])->save();
            $firm = $firm->refresh();
            $this->audit->record($firm, $reviewer, 'firm.approved', 'Firm review approved. Provisioning has not run.');

            return $firm;
        });
    }

    /**
     * @param  list<string>  $evidenceReferences
     */
    public function reject(Firm $firm, User $reviewer, string $reason, array $evidenceReferences): Firm
    {
        $application = $firm->onboardingApplication()->first();
        if ($application instanceof FirmOnboardingApplication) {
            return $this->rejectOnboardingApplication($firm, $application, $reviewer, $reason, $evidenceReferences);
        }

        return DB::transaction(function () use ($firm, $reviewer, $reason, $evidenceReferences): Firm {
            $this->storeVerification($firm, $reviewer, 'reject', $reason, $evidenceReferences);
            $firm->forceFill([
                'lifecycle_status' => FirmLifecycleStatus::Rejected,
                'rejected_at' => now(),
            ])->save();
            $firm = $firm->refresh();
            $this->audit->record($firm, $reviewer, 'firm.rejected', 'Firm review rejected.');

            return $firm;
        });
    }

    public function suspend(Firm $firm, User $reviewer, string $reason): Firm
    {
        if ($firm->lifecycle_status !== FirmLifecycleStatus::Active) {
            throw new FirmLifecycleException('The firm cannot be suspended from its current state.');
        }

        return DB::transaction(function () use ($firm, $reviewer, $reason): Firm {
            $this->storeVerification($firm, $reviewer, 'suspend', $reason, []);
            $firm->forceFill([
                'lifecycle_status' => FirmLifecycleStatus::Suspended,
                'suspended_at' => now(),
            ])->save();
            $firm = $firm->refresh();
            $this->audit->record($firm, $reviewer, 'firm.suspended', 'Firm suspended.');

            return $firm;
        });
    }

    public function reactivate(Firm $firm, User $reviewer, string $reason): Firm
    {
        if ($firm->lifecycle_status !== FirmLifecycleStatus::Suspended) {
            throw new FirmLifecycleException('The firm cannot be reactivated from its current state.');
        }

        if ($firm->provisioning_status !== FirmProvisioningStatus::Ready) {
            throw new FirmLifecycleException('The firm cannot be reactivated until provisioning is ready.');
        }

        return DB::transaction(function () use ($firm, $reviewer, $reason): Firm {
            $this->storeVerification($firm, $reviewer, 'reactivate', $reason, []);
            $firm->forceFill([
                'lifecycle_status' => FirmLifecycleStatus::Active,
                'suspended_at' => null,
                'activated_at' => now(),
            ])->save();

            $firm = $firm->refresh();
            $this->audit->record($firm, $reviewer, 'firm.reactivated', 'Firm reactivated.');

            return $firm;
        });
    }

    public function reopenOnboardingApplication(Firm $firm, User $reviewer, string $reason): Firm
    {
        $application = $firm->onboardingApplication()->first();
        if (! $application instanceof FirmOnboardingApplication) {
            throw new FirmLifecycleException('Onboarding application is not available.');
        }

        if ($firm->lifecycle_status !== FirmLifecycleStatus::Rejected) {
            throw new FirmLifecycleException('Only rejected onboarding applications can be reopened.');
        }

        return DB::transaction(function () use ($firm, $application, $reviewer, $reason): Firm {
            $lockedFirm = Firm::query()->whereKey($firm->id)->lockForUpdate()->firstOrFail();
            if ($lockedFirm->lifecycle_status !== FirmLifecycleStatus::Rejected) {
                throw new FirmLifecycleException('Only rejected onboarding applications can be reopened.');
            }

            FirmOnboardingReview::query()
                ->where('firm_id', $lockedFirm->id)
                ->where('revision', $application->revision)
                ->delete();

            $this->storeVerification($lockedFirm, $reviewer, 'reopen', $reason, ['manual:reopen']);

            $lockedFirm->forceFill([
                'lifecycle_status' => FirmLifecycleStatus::PendingReview,
                'provisioning_status' => FirmProvisioningStatus::NotRequested,
                'provisioning_failure_reason' => null,
                'rejected_at' => null,
                'approved_at' => null,
            ])->save();

            $this->audit->record(
                $lockedFirm->refresh(),
                $reviewer,
                'onboarding.reopened',
                'Rejected onboarding application reopened for review.',
            );

            return $lockedFirm->refresh();
        });
    }

    public function saveMembership(Firm $firm, User $member, string $role, ?User $actor): FirmMembership
    {
        $roles = config('tenancy.membership_roles');
        if (! is_array($roles) || ! in_array($role, $roles, true)) {
            throw ValidationException::withMessages([
                'role' => 'The selected role is not available.',
            ]);
        }

        $membership = FirmMembership::query()->updateOrCreate(
            [
                'firm_id' => $firm->id,
                'user_id' => $member->id,
            ],
            [
                'role' => $role,
            ],
        );

        $this->audit->record($firm, $actor, 'membership.saved', 'Firm membership saved.');

        return $membership;
    }

    /**
     * @return array{domain: FirmDomain, challenge: array{token:string,recordName:string,expiresAt:string}}
     */
    public function addDomain(Firm $firm, string $hostname, bool $primary, ?User $actor): array
    {
        $normalized = $this->hosts->normalize($hostname);
        $existing = FirmDomain::query()->where('hostname', $normalized)->first();

        if ($existing instanceof FirmDomain && $existing->firm_id !== $firm->id) {
            throw ValidationException::withMessages([
                'hostname' => 'Hostname is already registered.',
            ]);
        }

        if ($primary) {
            FirmDomain::query()->where('firm_id', $firm->id)->update(['is_primary' => false]);
        }

        $domain = FirmDomain::query()->updateOrCreate(
            ['hostname' => $normalized],
            [
                'firm_id' => $firm->id,
                'is_primary' => $primary,
                'revoked_at' => null,
                'released_at' => null,
            ],
        );
        $challenge = $this->domainProofs->issueChallenge($domain->refresh());

        $this->audit->record($firm, $actor, 'domain.claimed', 'Firm hostname claimed and DNS challenge issued.');

        return [
            'domain' => $domain->refresh(),
            'challenge' => $challenge,
        ];
    }

    /**
     * @return array{domain: FirmDomain, challenge: array{token:string,recordName:string,expiresAt:string}}
     */
    public function reissueDomainChallenge(Firm $firm, FirmDomain $domain, ?User $actor): array
    {
        $this->assertDomainOwnership($firm, $domain);
        $challenge = $this->domainProofs->issueChallenge($domain);
        $this->audit->record($firm, $actor, 'domain.challenge.reissued', 'DNS challenge reissued for firm hostname.');

        return [
            'domain' => $domain->refresh(),
            'challenge' => $challenge,
        ];
    }

    public function verifyDomain(Firm $firm, FirmDomain $domain, ?User $actor): FirmDomain
    {
        $this->assertDomainOwnership($firm, $domain);

        if ($this->domainProofs->verifyChallenge($domain) !== true) {
            throw ValidationException::withMessages([
                'hostname' => 'DNS TXT challenge does not prove domain ownership.',
            ]);
        }

        $domain->forceFill([
            'verified_at' => now(),
            'verified_by_user_id' => $actor?->id,
            'verified_source' => 'dns_txt',
            'verification_used_at' => now(),
            'verification_last_error' => null,
            'verification_token_hash' => null,
            'verification_expires_at' => null,
            'revoked_at' => null,
            'released_at' => null,
        ])->save();

        $this->audit->record($firm, $actor, 'domain.verified', 'Firm hostname ownership verified by DNS TXT.');

        return $domain->refresh();
    }

    public function revokeDomain(Firm $firm, FirmDomain $domain, ?User $actor, string $reason): FirmDomain
    {
        $this->assertDomainOwnership($firm, $domain);

        $domain->forceFill([
            'verified_at' => null,
            'verified_by_user_id' => null,
            'verified_source' => null,
            'verification_used_at' => null,
            'verification_last_error' => $reason,
            'verification_token_hash' => null,
            'verification_expires_at' => null,
            'revoked_at' => now(),
            'released_at' => now(),
            'is_primary' => false,
        ])->save();

        $this->audit->record($firm, $actor, 'domain.revoked', 'Firm hostname verification revoked.');

        return $domain->refresh();
    }

    /**
     * @param  array<string, mixed>  $attributes
     */
    public function updatePublicProfile(Firm $firm, array $attributes, ?User $actor): Firm
    {
        $profile = $firm->publicProfile()->firstOrFail();
        $profile->fill($attributes)->save();
        $this->audit->record($firm, $actor, 'branding.updated', 'Public branding updated.');

        return $firm->refresh();
    }

    public function saveOnboardingVerification(
        Firm $firm,
        User $reviewer,
        string $verificationType,
        string $status,
        string $method,
        ?string $notes,
        ?array $evidencePayload = null,
    ): FirmOnboardingVerification {
        $application = $firm->onboardingApplication()->first();
        if (! $application instanceof FirmOnboardingApplication) {
            throw new FirmLifecycleException('Onboarding application is not available.');
        }

        $allowedTypes = config('onboarding.required_verifications');
        if (! is_array($allowedTypes) || ! in_array($verificationType, $allowedTypes, true)) {
            throw ValidationException::withMessages([
                'verificationType' => 'The selected verification type is not available.',
            ]);
        }

        if ($method === 'provider') {
            $providerConfig = config('onboarding.verification_providers.'.$verificationType);
            if (! is_string($providerConfig) || trim($providerConfig) === '') {
                throw ValidationException::withMessages([
                    'method' => 'Verification provider is unavailable for this check.',
                ]);
            }
        }

        $statusIsPending = in_array($status, ['pending', 'in_review'], true);
        $normalizedEvidence = $this->normalizedEvidencePayload($verificationType, $status, $evidencePayload ?? []);

        $verification = FirmOnboardingVerification::query()->updateOrCreate(
            [
                'firm_id' => $firm->id,
                'revision' => $application->revision,
                'verification_type' => $verificationType,
            ],
            [
                'status' => $status,
                'method' => $method,
                'notes' => $notes ? mb_substr($notes, 0, 255) : null,
                'evidence_payload' => $normalizedEvidence === [] ? null : $normalizedEvidence,
                'reviewer_user_id' => $statusIsPending ? null : $reviewer->id,
                'reviewed_at' => $statusIsPending ? null : now(),
            ],
        );

        $this->audit->record($firm, $reviewer, 'onboarding.verification.saved', 'Onboarding verification status saved.');

        return $verification->refresh();
    }

    /**
     * @param  list<string>  $evidenceReferences
     */
    private function storeVerification(Firm $firm, User $reviewer, string $decision, string $reason, array $evidenceReferences): void
    {
        FirmVerification::query()->create([
            'firm_id' => $firm->id,
            'reviewer_user_id' => $reviewer->id,
            'decision' => $decision,
            'reason' => $reason,
            'evidence_references' => $evidenceReferences,
            'decided_at' => now(),
        ]);
    }

    private function assertDomainOwnership(Firm $firm, FirmDomain $domain): void
    {
        if ($domain->firm_id !== $firm->id) {
            throw ValidationException::withMessages([
                'hostname' => 'Hostname is not attached to this firm.',
            ]);
        }
    }

    /**
     * @param  list<string>  $evidenceReferences
     */
    private function approveOnboardingApplication(
        Firm $firm,
        FirmOnboardingApplication $application,
        User $reviewer,
        string $reason,
        array $evidenceReferences,
    ): Firm {
        if (! in_array($firm->lifecycle_status, [FirmLifecycleStatus::PendingReview, FirmLifecycleStatus::Approved], true)) {
            throw new FirmLifecycleException('The firm cannot be approved from its current state.');
        }

        return DB::transaction(function () use ($firm, $application, $reviewer, $reason, $evidenceReferences): Firm {
            $lockedFirm = Firm::query()->whereKey($firm->id)->lockForUpdate()->firstOrFail();
            $lockedApplication = FirmOnboardingApplication::query()
                ->where('firm_id', $lockedFirm->id)
                ->lockForUpdate()
                ->firstOrFail();
            $this->assertReviewerEligibleForOnboarding($lockedFirm, $lockedApplication, $reviewer);
            $this->assertVerificationComplete($lockedFirm, $lockedApplication->revision);

            $existing = FirmOnboardingReview::query()
                ->where('firm_id', $lockedFirm->id)
                ->where('revision', $lockedApplication->revision)
                ->where('reviewer_user_id', $reviewer->id)
                ->first();

            if ($existing instanceof FirmOnboardingReview) {
                throw new FirmLifecycleException('A reviewer can only submit one decision per application revision.');
            }

            $rejected = FirmOnboardingReview::query()
                ->where('firm_id', $lockedFirm->id)
                ->where('revision', $lockedApplication->revision)
                ->where('decision', 'reject')
                ->exists();
            if ($rejected) {
                throw new FirmLifecycleException('The application is rejected and cannot be approved without a revision update.');
            }

            FirmOnboardingReview::query()->create([
                'firm_id' => $lockedFirm->id,
                'revision' => $lockedApplication->revision,
                'reviewer_user_id' => $reviewer->id,
                'decision' => 'approve',
                'reason' => $reason,
                'decided_at' => now(),
            ]);
            $this->storeVerification($lockedFirm, $reviewer, 'approve', $reason, $evidenceReferences);

            $approvalCount = FirmOnboardingReview::query()
                ->where('firm_id', $lockedFirm->id)
                ->where('revision', $lockedApplication->revision)
                ->where('decision', 'approve')
                ->distinct('reviewer_user_id')
                ->count('reviewer_user_id');

            if ($approvalCount >= 2) {
                $lockedFirm->forceFill([
                    'lifecycle_status' => FirmLifecycleStatus::Approved,
                    'provisioning_status' => FirmProvisioningStatus::Requested,
                    'approved_at' => now(),
                    'rejected_at' => null,
                ])->save();
                $this->audit->record($lockedFirm->refresh(), $reviewer, 'firm.approved', 'Two-person approval complete. Provisioning is requested.');
            } else {
                $lockedFirm->forceFill([
                    'lifecycle_status' => FirmLifecycleStatus::PendingReview,
                ])->save();
                $this->audit->record($lockedFirm->refresh(), $reviewer, 'firm.approval.pending_second_reviewer', 'First approval saved. Awaiting a distinct second reviewer.');
            }

            return $lockedFirm->refresh();
        });
    }

    /**
     * @param  list<string>  $evidenceReferences
     */
    private function rejectOnboardingApplication(
        Firm $firm,
        FirmOnboardingApplication $application,
        User $reviewer,
        string $reason,
        array $evidenceReferences,
    ): Firm {
        if (! in_array($firm->lifecycle_status, [FirmLifecycleStatus::PendingReview, FirmLifecycleStatus::Approved], true)) {
            throw new FirmLifecycleException('The firm cannot be rejected from its current state.');
        }

        return DB::transaction(function () use ($firm, $application, $reviewer, $reason, $evidenceReferences): Firm {
            $lockedFirm = Firm::query()->whereKey($firm->id)->lockForUpdate()->firstOrFail();
            $lockedApplication = FirmOnboardingApplication::query()
                ->where('firm_id', $lockedFirm->id)
                ->lockForUpdate()
                ->firstOrFail();
            $this->assertReviewerEligibleForOnboarding($lockedFirm, $lockedApplication, $reviewer);

            $existing = FirmOnboardingReview::query()
                ->where('firm_id', $lockedFirm->id)
                ->where('revision', $lockedApplication->revision)
                ->where('reviewer_user_id', $reviewer->id)
                ->first();
            if ($existing instanceof FirmOnboardingReview) {
                throw new FirmLifecycleException('A reviewer can only submit one decision per application revision.');
            }

            FirmOnboardingReview::query()->create([
                'firm_id' => $lockedFirm->id,
                'revision' => $lockedApplication->revision,
                'reviewer_user_id' => $reviewer->id,
                'decision' => 'reject',
                'reason' => $reason,
                'decided_at' => now(),
            ]);
            $this->storeVerification($lockedFirm, $reviewer, 'reject', $reason, $evidenceReferences);

            $lockedFirm->forceFill([
                'lifecycle_status' => FirmLifecycleStatus::Rejected,
                'provisioning_status' => FirmProvisioningStatus::NotRequested,
                'rejected_at' => now(),
            ])->save();

            $this->audit->record($lockedFirm->refresh(), $reviewer, 'firm.rejected', 'Onboarding application rejected.');

            return $lockedFirm->refresh();
        });
    }

    private function assertReviewerEligibleForOnboarding(Firm $firm, FirmOnboardingApplication $application, User $reviewer): void
    {
        if ($application->applicant_user_id === $reviewer->id) {
            throw new FirmLifecycleException('Applicants cannot review their own firm application.');
        }

        $isMember = FirmMembership::query()
            ->where('firm_id', $firm->id)
            ->where('user_id', $reviewer->id)
            ->exists();
        if ($isMember) {
            throw new FirmLifecycleException('Firm members cannot approve or reject their own firm application.');
        }
    }

    private function assertVerificationComplete(Firm $firm, int $revision): void
    {
        $required = config('onboarding.required_verifications');
        if (! is_array($required)) {
            $required = [];
        }

        $verifications = FirmOnboardingVerification::query()
            ->where('firm_id', $firm->id)
            ->where('revision', $revision)
            ->get()
            ->keyBy('verification_type');

        foreach ($required as $type) {
            if (! is_string($type) || $type === '') {
                continue;
            }

            if ($type === 'domain_ownership' && $firm->domains()->whereNotNull('verified_at')->whereNull('revoked_at')->exists()) {
                continue;
            }

            /** @var FirmOnboardingVerification|null $verification */
            $verification = $verifications->get($type);
            if (! $verification instanceof FirmOnboardingVerification || ! in_array($verification->status, ['verified', 'bypassed'], true)) {
                throw new FirmLifecycleException('Verification requirements are not complete for this application revision.');
            }
        }
    }

    /**
     * @param  array<string, mixed>  $evidencePayload
     * @return array<string, string>
     */
    private function normalizedEvidencePayload(string $verificationType, string $status, array $evidencePayload): array
    {
        $workflow = config('onboarding.verification_workflows.'.$verificationType);
        if (! is_array($workflow)) {
            return [];
        }

        $fields = $workflow['required_fields'] ?? [];
        if (! is_array($fields)) {
            return [];
        }

        $normalized = [];
        $missing = [];
        foreach ($fields as $field) {
            if (! is_array($field) || ! is_string($field['key'] ?? null) || $field['key'] === '') {
                continue;
            }

            $key = $field['key'];
            $raw = $evidencePayload[$key] ?? null;
            $value = is_string($raw) ? trim($raw) : '';

            if ($status === 'verified' && $value === '') {
                $missing[] = $key;
            }

            if ($value !== '') {
                $normalized[$key] = Str::limit($value, 255, '');
            }
        }

        if ($status === 'verified' && $missing !== []) {
            throw ValidationException::withMessages([
                'evidence' => 'Verification evidence is incomplete: '.implode(', ', $missing).'.',
            ]);
        }

        return $normalized;
    }
}
