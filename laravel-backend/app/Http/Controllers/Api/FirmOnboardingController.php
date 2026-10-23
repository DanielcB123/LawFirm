<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\FirmOnboardingApplication;
use App\Models\User;
use App\Tenancy\FirmOnboardingService;
use App\Tenancy\OnboardingFastTrackGuard;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Validation\ValidationException;
use RuntimeException;
use Symfony\Component\HttpFoundation\Response;

class FirmOnboardingController extends Controller
{
    public function register(Request $request, FirmOnboardingService $onboarding): JsonResponse
    {
        $validated = $request->validate([
            'firmName' => ['required', 'string', 'max:200', 'regex:/^[^<>]*$/'],
            'name' => ['required', 'string', 'max:200', 'regex:/^[^<>]*$/'],
            'email' => ['required', 'string', 'email', 'max:255', 'unique:users,email'],
            'password' => ['required', 'string', 'confirmed', 'min:8'],
            'businessPhone' => ['nullable', 'string', 'max:40', 'regex:/^[0-9+().\\-\\s]{3,40}$/'],
        ]);

        $result = $onboarding->registerGuestAndSubmit($validated);
        /** @var User $user */
        $user = $result['user'];

        return response()->json([
            'access_token' => $result['token'],
            'token_type' => 'Bearer',
            'user' => $this->actorPayload($user->refresh()),
            'applicationId' => $result['application']->id,
        ], Response::HTTP_CREATED);
    }

    public function submit(Request $request, FirmOnboardingService $onboarding): JsonResponse
    {
        /** @var User $user */
        $user = $request->user();
        $validated = $request->validate([
            'firmName' => ['required', 'string', 'max:200', 'regex:/^[^<>]*$/'],
            'applicantName' => ['nullable', 'string', 'max:200', 'regex:/^[^<>]*$/'],
            'applicantEmail' => ['nullable', 'string', 'email', 'max:255'],
            'businessPhone' => ['nullable', 'string', 'max:40', 'regex:/^[0-9+().\\-\\s]{3,40}$/'],
        ]);

        $application = $onboarding->submitForAuthenticatedUser($user, $validated);

        return response()->json([
            'applicationId' => $application->id,
            'firmId' => $application->firm_id,
        ], Response::HTTP_CREATED);
    }

    public function current(Request $request, FirmOnboardingService $onboarding): JsonResponse
    {
        /** @var User $user */
        $user = $request->user();

        return response()->json($onboarding->statusPayload($user));
    }

    public function update(Request $request, FirmOnboardingService $onboarding): JsonResponse
    {
        /** @var User $user */
        $user = $request->user();
        $application = $onboarding->latestApplicationFor($user);
        if (! $application instanceof FirmOnboardingApplication) {
            throw ValidationException::withMessages([
                'application' => 'No onboarding application is available.',
            ]);
        }

        $validated = $request->validate([
            'firmName' => ['required', 'string', 'max:200', 'regex:/^[^<>]*$/'],
            'businessPhone' => ['nullable', 'string', 'max:40', 'regex:/^[0-9+().\\-\\s]{3,40}$/'],
        ]);

        $onboarding->updateApplication($user, $application, $validated);

        return response()->json($onboarding->statusPayload($user));
    }

    public function fastTrack(Request $request, FirmOnboardingService $onboarding, OnboardingFastTrackGuard $guard): JsonResponse
    {
        /** @var User $user */
        $user = $request->user();
        $application = $onboarding->latestApplicationFor($user);
        if (! $application instanceof FirmOnboardingApplication) {
            throw ValidationException::withMessages([
                'application' => 'No onboarding application is available.',
            ]);
        }

        try {
            $guard->assertRuntimeAllowed();
            $onboarding->runFastTrack($application, $user);
        } catch (RuntimeException $exception) {
            return response()->json(['message' => $exception->getMessage()], 403);
        }

        return response()->json($onboarding->statusPayload($user));
    }

    /**
     * @return array<string, mixed>
     */
    private function actorPayload(User $user): array
    {
        return [
            'id' => $user->id,
            'name' => $user->name,
            'email' => $user->email,
            'role' => $user->role,
            'permissions' => $user->permissions ?? [],
            'platformRole' => $user->platformRole(),
            'platformPermissions' => $user->platformPermissions(),
            'isPlatformAdministrator' => $user->hasPlatformAccess(),
            'memberships' => $user->membershipPayload(),
        ];
    }
}
