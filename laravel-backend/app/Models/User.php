<?php

namespace App\Models;

// use Illuminate\Contracts\Auth\MustVerifyEmail;
use Database\Factories\UserFactory;
use Illuminate\Database\Eloquent\Attributes\Fillable;
use Illuminate\Database\Eloquent\Attributes\Hidden;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Foundation\Auth\User as Authenticatable;
use Illuminate\Notifications\Notifiable;
use Laravel\Sanctum\HasApiTokens;

#[Fillable(['name', 'email', 'password', 'role', 'permissions', 'platform_role'])]
#[Hidden(['password', 'remember_token'])]
class User extends Authenticatable
{
    /** @use HasFactory<UserFactory> */
    use HasApiTokens, HasFactory, Notifiable;

    /**
     * Get the attributes that should be cast.
     *
     * @return array<string, string>
     */
    protected function casts(): array
    {
        return [
            'email_verified_at' => 'datetime',
            'password' => 'hashed',
            'permissions' => 'array',
            'is_platform_administrator' => 'boolean',
        ];
    }

    public function memberships(): HasMany
    {
        return $this->hasMany(FirmMembership::class);
    }

    public function onboardingApplications(): HasMany
    {
        return $this->hasMany(FirmOnboardingApplication::class, 'applicant_user_id');
    }

    public function onboardingReviews(): HasMany
    {
        return $this->hasMany(FirmOnboardingReview::class, 'reviewer_user_id');
    }

    public function onboardingVerifications(): HasMany
    {
        return $this->hasMany(FirmOnboardingVerification::class, 'reviewer_user_id');
    }

    /**
     * @return list<array{firmId: string, displayName: string, lifecycleStatus: string, role: string, preferredHost: ?string, fallbackHost: ?string}>
     */
    public function membershipPayload(): array
    {
        $this->loadMissing('memberships.firm.domains');

        return $this->memberships
            ->filter(fn (FirmMembership $membership): bool => $membership->firm instanceof Firm)
            ->map(fn (FirmMembership $membership): array => [
                'firmId' => $membership->firm_id,
                'displayName' => $membership->firm->display_name,
                'lifecycleStatus' => $membership->firm->lifecycle_status->value,
                'role' => $membership->role,
                'preferredHost' => $membership->firm->preferredHostname(),
                'fallbackHost' => $membership->firm->fallbackHostname(),
            ])
            ->values()
            ->all();
    }

    public function hasPermission(string $permission): bool
    {
        $permissions = $this->permissions ?? [];

        return in_array($permission, $permissions, true);
    }

    public function platformRole(): ?string
    {
        $role = is_string($this->platform_role) ? trim($this->platform_role) : null;
        if (! is_string($role) || $role === '') {
            return null;
        }

        $allowed = config('platform.roles');
        if (! is_array($allowed) || ! in_array($role, $allowed, true)) {
            return null;
        }

        return $role;
    }

    /**
     * @return list<string>
     */
    public function platformPermissions(): array
    {
        $role = $this->platformRole();
        if (is_string($role)) {
            $configured = config("platform.permissions.{$role}");

            return is_array($configured) ? array_values(array_filter($configured, fn (mixed $item): bool => is_string($item))) : [];
        }

        // Compatibility path for existing rows where role has not been assigned yet.
        if ($this->is_platform_administrator === true) {
            $configured = config('platform.permissions.platform_owner');

            return is_array($configured) ? array_values(array_filter($configured, fn (mixed $item): bool => is_string($item))) : [];
        }

        return [];
    }

    public function hasPlatformAccess(): bool
    {
        return $this->platformRole() !== null || $this->is_platform_administrator === true;
    }

    public function isPlatformOwner(): bool
    {
        return $this->platformRole() === 'platform_owner'
            || ($this->platformRole() === null && $this->is_platform_administrator === true);
    }

    public function isPlatformOperator(): bool
    {
        return $this->platformRole() === 'platform_ops';
    }

    public function hasPlatformPermission(string $permission): bool
    {
        return in_array($permission, $this->platformPermissions(), true);
    }
}
