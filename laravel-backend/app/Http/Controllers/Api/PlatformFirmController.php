<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Firm;
use App\Models\FirmDomain;
use App\Models\User;
use App\Tenancy\Exceptions\FirmLifecycleException;
use App\Tenancy\Exceptions\TenantProvisioningException;
use App\Tenancy\FirmOnboardingService;
use App\Tenancy\FirmRegistry;
use App\Tenancy\FirmReviewService;
use App\Tenancy\PublicBrandingValidator;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Validation\Rule;
use Illuminate\Validation\ValidationException;

class PlatformFirmController extends Controller
{
    public function index(Request $request): JsonResponse
    {
        $this->authorize('viewAny', Firm::class);

        $validated = $request->validate([
            'status' => ['nullable', Rule::in(['pending_review', 'approved', 'active', 'rejected', 'suspended'])],
            'search' => ['nullable', 'string', 'max:120'],
            'perPage' => ['nullable', 'integer', 'min:1', 'max:100'],
        ]);

        $query = Firm::query()->with(['domains', 'memberships.user', 'publicProfile'])->orderByDesc('created_at');
        if (! empty($validated['status'])) {
            $query->where('lifecycle_status', $validated['status']);
        }

        if (! empty($validated['search'])) {
            $term = trim((string) $validated['search']);
            $query->where(function ($builder) use ($term): void {
                $builder
                    ->where('display_name', 'like', "%{$term}%")
                    ->orWhere('legal_name', 'like', "%{$term}%")
                    ->orWhere('slug', 'like', "%{$term}%");
            });
        }

        $page = $query->paginate((int) ($validated['perPage'] ?? 25));

        return response()->json([
            'data' => collect($page->items())->map(fn (Firm $firm): array => $this->firmPayload($firm))->values()->all(),
            'meta' => [
                'currentPage' => $page->currentPage(),
                'lastPage' => $page->lastPage(),
                'perPage' => $page->perPage(),
                'total' => $page->total(),
            ],
        ]);
    }

    public function store(Request $request, FirmRegistry $registry): JsonResponse
    {
        $this->authorize('create', Firm::class);

        $validated = $request->validate([
            'legalName' => ['required', 'string', 'max:200', 'regex:/^[^<>]*$/'],
            'displayName' => ['required', 'string', 'max:200', 'regex:/^[^<>]*$/'],
        ]);

        $firm = $registry->register($validated['legalName'], $validated['displayName'], $request->user());

        return response()->json($this->firmPayload($firm), 201);
    }

    public function review(Request $request, Firm $firm, FirmReviewService $review): JsonResponse
    {
        $this->authorize('review', $firm);

        $validated = $request->validate([
            'decision' => ['required', Rule::in(['approve', 'reject'])],
            'reason' => ['required', 'string', 'max:1000'],
            'evidenceReferences' => ['sometimes', 'array', 'max:20'],
            'evidenceReferences.*' => ['string', 'max:200', 'regex:/^[A-Za-z0-9_.:\/-]{1,200}$/'],
        ]);

        try {
            $firm = $validated['decision'] === 'approve'
                ? $review->approve($firm, $request->user(), $validated['reason'], $validated['evidenceReferences'] ?? [])
                : $review->reject($firm, $request->user(), $validated['reason'], $validated['evidenceReferences'] ?? []);
        } catch (FirmLifecycleException $exception) {
            return response()->json(['message' => $exception->getMessage()], 422);
        }

        return response()->json($this->firmPayload($firm));
    }

    public function suspend(Request $request, Firm $firm, FirmReviewService $review): JsonResponse
    {
        $this->authorize('suspend', $firm);

        $validated = $request->validate([
            'reason' => ['required', 'string', 'max:1000'],
        ]);

        try {
            $firm = $review->suspend($firm, $request->user(), $validated['reason']);
        } catch (FirmLifecycleException $exception) {
            return response()->json(['message' => $exception->getMessage()], 422);
        }

        return response()->json($this->firmPayload($firm));
    }

    public function reactivate(Request $request, Firm $firm, FirmReviewService $review): JsonResponse
    {
        $this->authorize('suspend', $firm);

        $validated = $request->validate([
            'reason' => ['required', 'string', 'max:1000'],
        ]);

        try {
            $firm = $review->reactivate($firm, $request->user(), $validated['reason']);
        } catch (FirmLifecycleException $exception) {
            return response()->json(['message' => $exception->getMessage()], 422);
        }

        return response()->json($this->firmPayload($firm));
    }

    public function provision(Request $request, Firm $firm, FirmOnboardingService $onboarding): JsonResponse
    {
        $this->authorize('provision', $firm);

        try {
            $firm = $onboarding->provisionAndAssignOwner($firm, $request->user());
        } catch (FirmLifecycleException|TenantProvisioningException $exception) {
            return response()->json(['message' => $exception->getMessage()], 422);
        }

        return response()->json($this->firmPayload($firm));
    }

    public function storeMembership(Request $request, Firm $firm, FirmReviewService $review): JsonResponse
    {
        $this->authorize('manageMembership', $firm);

        $validated = $request->validate([
            'userId' => ['required', 'integer', 'exists:users,id'],
            'role' => ['required', 'string', Rule::in(config('tenancy.membership_roles'))],
        ]);

        $member = User::query()->findOrFail($validated['userId']);
        $membership = $review->saveMembership($firm, $member, $validated['role'], $request->user());

        return response()->json([
            'firmId' => $membership->firm_id,
            'userId' => $membership->user_id,
            'role' => $membership->role,
        ], 201);
    }

    public function storeDomain(Request $request, Firm $firm, FirmReviewService $review): JsonResponse
    {
        $this->authorize('manageDomain', $firm);

        $validated = $request->validate([
            'hostname' => ['required', 'string', 'max:253'],
            'primary' => ['sometimes', 'boolean'],
        ]);

        $result = $review->addDomain($firm, $validated['hostname'], (bool) ($validated['primary'] ?? false), $request->user());
        $domain = $result['domain'];

        return response()->json([
            'id' => $domain->id,
            'hostname' => $domain->hostname,
            'primary' => $domain->is_primary,
            'verifiedAt' => $domain->verified_at?->toIso8601String(),
            'challenge' => $result['challenge'],
        ], 201);
    }

    public function reissueDomainChallenge(Request $request, Firm $firm, FirmDomain $domain, FirmReviewService $review): JsonResponse
    {
        $this->authorize('manageDomain', $firm);
        $result = $review->reissueDomainChallenge($firm, $domain, $request->user());

        return response()->json([
            'id' => $result['domain']->id,
            'hostname' => $result['domain']->hostname,
            'challenge' => $result['challenge'],
        ]);
    }

    public function verifyDomain(Request $request, Firm $firm, FirmDomain $domain, FirmReviewService $review): JsonResponse
    {
        $this->authorize('finalizeDomain', $firm);

        $verified = $review->verifyDomain($firm, $domain, $request->user());

        return response()->json([
            'id' => $verified->id,
            'hostname' => $verified->hostname,
            'verifiedAt' => $verified->verified_at?->toIso8601String(),
            'verifiedSource' => $verified->verified_source,
        ]);
    }

    public function revokeDomain(Request $request, Firm $firm, FirmDomain $domain, FirmReviewService $review): JsonResponse
    {
        $this->authorize('finalizeDomain', $firm);

        $validated = $request->validate([
            'reason' => ['required', 'string', 'max:255'],
        ]);

        $domain = $review->revokeDomain($firm, $domain, $request->user(), $validated['reason']);

        return response()->json([
            'id' => $domain->id,
            'hostname' => $domain->hostname,
            'revokedAt' => $domain->revoked_at?->toIso8601String(),
        ]);
    }

    public function updateBranding(Request $request, Firm $firm, FirmReviewService $review): JsonResponse
    {
        $this->authorize('updateBranding', $firm);

        $validated = $request->validate(PublicBrandingValidator::rules());
        $review->updatePublicProfile($firm, PublicBrandingValidator::attributes($validated), $request->user());

        return response()->json([
            'firmId' => $firm->id,
            'message' => 'Public branding saved.',
        ]);
    }

    public function storeVerification(Request $request, Firm $firm, FirmReviewService $review): JsonResponse
    {
        $this->authorize('storeVerification', $firm);

        $validated = $request->validate([
            'verificationType' => ['required', Rule::in(config('onboarding.required_verifications'))],
            'status' => ['required', Rule::in(['pending', 'verified', 'rejected', 'unavailable'])],
            'method' => ['required', Rule::in(['manual', 'provider'])],
            'notes' => ['nullable', 'string', 'max:255'],
        ]);

        try {
            $review->saveOnboardingVerification(
                $firm,
                $request->user(),
                $validated['verificationType'],
                $validated['status'],
                $validated['method'],
                $validated['notes'] ?? null,
            );
        } catch (ValidationException $exception) {
            throw $exception;
        } catch (FirmLifecycleException $exception) {
            return response()->json(['message' => $exception->getMessage()], 422);
        }

        return response()->json($this->firmPayload($firm->refresh()));
    }

    /**
     * @return array<string, mixed>
     */
    private function firmPayload(Firm $firm): array
    {
        $firm->loadMissing(['domains', 'memberships.user', 'publicProfile']);
        $application = $firm->onboardingApplication()->first();
        $revision = $application?->revision;
        $approvalCount = 0;
        $verificationStatuses = [];

        if ($revision !== null) {
            $approvalCount = $firm->onboardingReviews()
                ->where('revision', $revision)
                ->where('decision', 'approve')
                ->distinct('reviewer_user_id')
                ->count('reviewer_user_id');

            $verificationStatuses = $firm->onboardingVerifications()
                ->where('revision', $revision)
                ->get()
                ->map(fn ($verification): array => [
                    'type' => $verification->verification_type,
                    'status' => $verification->status,
                    'method' => $verification->method,
                    'reviewedAt' => $verification->reviewed_at?->toIso8601String(),
                ])
                ->values()
                ->all();
        }

        return [
            'id' => $firm->id,
            'legalName' => $firm->legal_name,
            'displayName' => $firm->display_name,
            'slug' => $firm->slug,
            'lifecycleStatus' => $firm->lifecycle_status->value,
            'provisioningStatus' => $firm->provisioning_status->value,
            'provisioningFailureReason' => $firm->provisioning_failure_reason,
            'schemaVersion' => $firm->schema_version,
            'publicSiteKey' => $firm->publicProfile?->public_site_key ?? 'default_classic',
            'domains' => $firm->domains->map(fn (FirmDomain $domain): array => [
                'id' => $domain->id,
                'hostname' => $domain->hostname,
                'isPrimary' => $domain->is_primary,
                'verifiedAt' => $domain->verified_at?->toIso8601String(),
                'revokedAt' => $domain->revoked_at?->toIso8601String(),
                'verificationLastError' => $domain->verification_last_error,
            ])->values()->all(),
            'memberships' => $firm->memberships->map(fn ($membership): array => [
                'userId' => $membership->user_id,
                'userName' => $membership->user?->name,
                'userEmail' => $membership->user?->email,
                'role' => $membership->role,
            ])->values()->all(),
            'onboarding' => [
                'hasApplication' => $application !== null,
                'applicantUserId' => $application?->applicant_user_id,
                'revision' => $revision,
                'approvalCount' => $approvalCount,
                'requiredApprovals' => $application !== null ? 2 : 0,
                'verificationStatuses' => $verificationStatuses,
            ],
        ];
    }
}
