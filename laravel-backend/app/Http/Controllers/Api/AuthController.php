<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\Log;
use Symfony\Component\HttpFoundation\Response;

class AuthController extends Controller
{
    public function login(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'email' => ['required', 'email'],
            'password' => ['required', 'string'],
        ]);
        $email = strtolower((string) $validated['email']);

        $user = User::query()->where('email', $email)->first();
        if (! $user instanceof User) {
            $this->logLoginFailure($request, $email, 'user_not_found');

            return response()->json([
                'message' => 'Invalid email or password.',
            ], Response::HTTP_UNAUTHORIZED);
        }

        if (! Hash::check($validated['password'], $user->password)) {
            $this->logLoginFailure($request, $email, 'invalid_password', $user);

            return response()->json([
                'message' => 'Invalid email or password.',
            ], Response::HTTP_UNAUTHORIZED);
        }

        $memberships = $user->membershipPayload();
        $activeMemberships = collect($memberships)
            ->filter(fn (array $membership): bool => ($membership['lifecycleStatus'] ?? null) === 'active')
            ->count();
        $token = $user->createToken('migration-slice');
        $this->logLoginSuccess($request, $user, $activeMemberships, count($memberships));

        return response()->json([
            'access_token' => $token->plainTextToken,
            'token_type' => 'Bearer',
            'user' => [
                'id' => $user->id,
                'name' => $user->name,
                'email' => $user->email,
                'role' => $user->role,
                'permissions' => $user->permissions ?? [],
                'platformRole' => $user->platformRole(),
                'platformPermissions' => $user->platformPermissions(),
                'isPlatformAdministrator' => $user->hasPlatformAccess(),
                'memberships' => $memberships,
            ],
        ]);
    }

    public function me(Request $request): JsonResponse
    {
        /** @var User $user */
        $user = $request->user();

        return response()->json([
            'id' => $user->id,
            'name' => $user->name,
            'email' => $user->email,
            'role' => $user->role,
            'permissions' => $user->permissions ?? [],
            'platformRole' => $user->platformRole(),
            'platformPermissions' => $user->platformPermissions(),
            'isPlatformAdministrator' => $user->hasPlatformAccess(),
            'memberships' => $user->membershipPayload(),
        ]);
    }

    private function logLoginFailure(Request $request, string $email, string $reason, ?User $user = null): void
    {
        if (! $this->shouldLogAuthDiagnostics()) {
            return;
        }

        $membershipCount = null;
        $activeMembershipCount = null;
        if ($user instanceof User) {
            $memberships = $user->membershipPayload();
            $membershipCount = count($memberships);
            $activeMembershipCount = collect($memberships)
                ->filter(fn (array $membership): bool => ($membership['lifecycleStatus'] ?? null) === 'active')
                ->count();
        }

        Log::warning('auth.login.failed', [
            'reason' => $reason,
            'email' => $email,
            'host' => strtolower($request->getHost()),
            'ip' => $request->ip(),
            'user_id' => $user?->id,
            'platform_role' => $user?->platformRole(),
            'membership_count' => $membershipCount,
            'active_membership_count' => $activeMembershipCount,
        ]);
    }

    private function logLoginSuccess(Request $request, User $user, int $activeMemberships, int $membershipCount): void
    {
        if (! $this->shouldLogAuthDiagnostics()) {
            return;
        }

        Log::info('auth.login.succeeded', [
            'email' => strtolower((string) $user->email),
            'user_id' => $user->id,
            'host' => strtolower($request->getHost()),
            'ip' => $request->ip(),
            'platform_role' => $user->platformRole(),
            'membership_count' => $membershipCount,
            'active_membership_count' => $activeMemberships,
        ]);
    }

    private function shouldLogAuthDiagnostics(): bool
    {
        return in_array((string) config('app.env'), ['local', 'testing'], true)
            || (bool) config('app.debug', false);
    }
}
