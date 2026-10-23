<?php

namespace App\Models;

use App\Enums\FirmLifecycleStatus;
use App\Enums\FirmProvisioningStatus;
use Database\Factories\FirmFactory;
use Illuminate\Database\Eloquent\Concerns\HasUuids;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\Relations\HasOne;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Support\Str;

class Firm extends Model
{
    /** @use HasFactory<FirmFactory> */
    use HasFactory, HasUuids;

    /**
     * @var list<string>
     */
    protected $fillable = [
        'legal_name',
        'display_name',
        'slug',
    ];

    /**
     * @var list<string>
     */
    protected $hidden = [
        'schema_name',
    ];

    /**
     * @return array<string, string>
     */
    protected function casts(): array
    {
        return [
            'lifecycle_status' => FirmLifecycleStatus::class,
            'provisioning_status' => FirmProvisioningStatus::class,
            'approved_at' => 'datetime',
            'activated_at' => 'datetime',
            'rejected_at' => 'datetime',
            'suspended_at' => 'datetime',
        ];
    }

    public function publicProfile(): HasOne
    {
        return $this->hasOne(FirmPublicProfile::class);
    }

    public function applicant(): BelongsTo
    {
        return $this->belongsTo(User::class, 'applicant_user_id');
    }

    public function domains(): HasMany
    {
        return $this->hasMany(FirmDomain::class);
    }

    public function verifications(): HasMany
    {
        return $this->hasMany(FirmVerification::class);
    }

    public function memberships(): HasMany
    {
        return $this->hasMany(FirmMembership::class);
    }

    public function provisioningEvents(): HasMany
    {
        return $this->hasMany(FirmProvisioningEvent::class);
    }

    public function onboardingApplication(): HasOne
    {
        return $this->hasOne(FirmOnboardingApplication::class);
    }

    public function onboardingReviews(): HasMany
    {
        return $this->hasMany(FirmOnboardingReview::class);
    }

    public function onboardingVerifications(): HasMany
    {
        return $this->hasMany(FirmOnboardingVerification::class);
    }

    public function databaseCredentials(): HasMany
    {
        return $this->hasMany(FirmDatabaseCredential::class);
    }

    public function isUsable(): bool
    {
        return $this->lifecycle_status === FirmLifecycleStatus::Active
            && $this->provisioning_status === FirmProvisioningStatus::Ready
            && is_string($this->schema_name)
            && $this->schema_name !== '';
    }

    public function fallbackHostname(): ?string
    {
        $suffix = (string) config('tenancy.tenant_fallback_domain_suffix', '');
        if ($suffix === '' || ! is_string($this->slug) || $this->slug === '') {
            return null;
        }

        $slug = Str::of($this->slug)->lower()->replaceMatches('/[^a-z0-9-]/', '')->trim('-')->value();
        if ($slug === '') {
            return null;
        }

        return $slug.'.'.strtolower($suffix);
    }

    public function preferredHostname(): ?string
    {
        /** @var FirmDomain|null $primary */
        $primary = $this->domains
            ->first(fn (FirmDomain $domain): bool => $domain->is_primary && $domain->verified_at !== null && $domain->revoked_at === null);

        if ($primary instanceof FirmDomain) {
            return $primary->hostname;
        }

        /** @var FirmDomain|null $verified */
        $verified = $this->domains
            ->first(fn (FirmDomain $domain): bool => $domain->verified_at !== null && $domain->revoked_at === null);

        if ($verified instanceof FirmDomain) {
            return $verified->hostname;
        }

        return $this->fallbackHostname();
    }
}
