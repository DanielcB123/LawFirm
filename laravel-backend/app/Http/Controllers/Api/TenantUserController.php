<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\FirmMembership;
use App\Models\User;
use App\Tenancy\TenantContext;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Hash;
use Illuminate\Validation\Rule;
use Symfony\Component\HttpFoundation\Response;

class TenantUserController extends Controller
{
    public function index(Request $request, TenantContext $context): JsonResponse
    {
        $firmId = $context->firmId();
        if (! is_string($firmId) || trim($firmId) === '') {
            return response()->json(['message' => 'Tenant is not available.'], Response::HTTP_FORBIDDEN);
        }

        if (! $this->isFirmPartner($request, $firmId)) {
            return response()->json(['message' => 'Only firm partners can manage users.'], Response::HTTP_FORBIDDEN);
        }

        $memberships = FirmMembership::query()
            ->where('firm_id', $firmId)
            ->with('user:id,name,email,role,permissions')
            ->orderBy('created_at')
            ->get();

        return response()->json([
            'roles' => config('tenancy.membership_roles'),
            'rolePermissions' => config('roles'),
            'members' => $memberships
                ->filter(fn (FirmMembership $membership): bool => $membership->user instanceof User)
                ->map(function (FirmMembership $membership): array {
                    $user = $membership->user;

                    return [
                        'membershipId' => $membership->id,
                        'userId' => $user->id,
                        'name' => $user->name,
                        'email' => $user->email,
                        'membershipRole' => $membership->role,
                        'permissions' => $user->permissions ?? [],
                    ];
                })
                ->values()
                ->all(),
        ]);
    }

    public function store(Request $request, TenantContext $context): JsonResponse
    {
        $firmId = $context->firmId();
        if (! is_string($firmId) || trim($firmId) === '') {
            return response()->json(['message' => 'Tenant is not available.'], Response::HTTP_FORBIDDEN);
        }

        if (! $this->isFirmPartner($request, $firmId)) {
            return response()->json(['message' => 'Only firm partners can manage users.'], Response::HTTP_FORBIDDEN);
        }

        $validated = $request->validate([
            'name' => ['required', 'string', 'max:200', 'regex:/^[^<>]*$/'],
            'email' => ['required', 'email', 'max:255', 'unique:users,email'],
            'password' => ['required', 'string', 'min:8'],
            'role' => ['required', 'string', Rule::in(config('tenancy.membership_roles'))],
        ]);

        $role = $validated['role'];
        $permissions = config("roles.{$role}");
        if (! is_array($permissions)) {
            $permissions = [];
        }

        $user = User::query()->create([
            'name' => $validated['name'],
            'email' => $validated['email'],
            'password' => Hash::make($validated['password']),
            'role' => $role,
            'permissions' => array_values(array_filter($permissions, fn (mixed $item): bool => is_string($item))),
            'platform_role' => null,
            'is_platform_administrator' => false,
        ]);

        FirmMembership::query()->create([
            'firm_id' => $firmId,
            'user_id' => $user->id,
            'role' => $role,
        ]);

        return response()->json([
            'userId' => $user->id,
            'name' => $user->name,
            'email' => $user->email,
            'membershipRole' => $role,
            'permissions' => $user->permissions ?? [],
        ], Response::HTTP_CREATED);
    }

    public function updateMembership(Request $request, TenantContext $context, User $user): JsonResponse
    {
        $firmId = $context->firmId();
        if (! is_string($firmId) || trim($firmId) === '') {
            return response()->json(['message' => 'Tenant is not available.'], Response::HTTP_FORBIDDEN);
        }

        if (! $this->isFirmPartner($request, $firmId)) {
            return response()->json(['message' => 'Only firm partners can manage users.'], Response::HTTP_FORBIDDEN);
        }

        $validated = $request->validate([
            'role' => ['required', 'string', Rule::in(config('tenancy.membership_roles'))],
        ]);

        $membership = FirmMembership::query()
            ->where('firm_id', $firmId)
            ->where('user_id', $user->id)
            ->first();

        if (! $membership instanceof FirmMembership) {
            return response()->json(['message' => 'Membership not found for this firm.'], Response::HTTP_NOT_FOUND);
        }

        $membership->role = $validated['role'];
        $membership->save();

        $permissions = config('roles.'.$validated['role']);
        if (! is_array($permissions)) {
            $permissions = [];
        }

        $user->role = $validated['role'];
        $user->permissions = array_values(array_filter($permissions, fn (mixed $item): bool => is_string($item)));
        $user->save();

        return response()->json([
            'membershipId' => $membership->id,
            'userId' => $user->id,
            'membershipRole' => $membership->role,
            'permissions' => $user->permissions ?? [],
        ]);
    }

    private function isFirmPartner(Request $request, string $firmId): bool
    {
        $actorId = $request->user()?->id;
        if (! is_int($actorId)) {
            return false;
        }

        return FirmMembership::query()
            ->where('firm_id', $firmId)
            ->where('user_id', $actorId)
            ->where('role', 'Partner')
            ->exists();
    }
}

