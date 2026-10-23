<?php

namespace App\Tenancy;

use App\Enums\FirmLifecycleStatus;
use App\Enums\FirmProvisioningStatus;
use App\Models\Firm;
use App\Models\FirmDomain;
use App\Models\FirmPublicProfile;
use App\Models\User;
use Illuminate\Support\Str;

class FirmRegistry
{
    public function __construct(private FirmAuditRecorder $audit) {}

    public function register(string $legalName, string $displayName, ?User $actor = null): Firm
    {
        $platform = config('tenancy.platform_public');

        $firm = new Firm([
            'legal_name' => $legalName,
            'display_name' => $displayName,
            'slug' => $this->uniqueSlug($displayName),
        ]);
        $firm->forceFill([
            'lifecycle_status' => FirmLifecycleStatus::PendingReview,
            'provisioning_status' => FirmProvisioningStatus::NotRequested,
        ]);
        $firm->save();

        FirmPublicProfile::query()->create([
            'firm_id' => $firm->id,
            'color_primary' => $platform['colors']['primary'],
            'color_secondary' => $platform['colors']['secondary'],
            'color_accent' => $platform['colors']['accent'],
            'typography' => $platform['typography'],
            'public_copy' => $displayName,
            'public_site_key' => $platform['public_site_key'] ?? 'default_classic',
            'enabled_modules' => ['public_site'],
        ]);

        $this->ensureFallbackDomain($firm);

        $firm = $firm->refresh();
        $this->audit->record($firm, $actor, 'firm.registered', 'Firm registered for review.');

        return $firm;
    }

    private function uniqueSlug(string $displayName): string
    {
        $base = Str::slug($displayName);
        $base = $base !== '' ? $base : 'firm';
        $base = Str::limit($base, 40, '');

        do {
            $slug = $base.'-'.bin2hex(random_bytes(3));
        } while (Firm::query()->where('slug', $slug)->exists());

        return $slug;
    }

    private function ensureFallbackDomain(Firm $firm): void
    {
        $fallbackHost = $firm->fallbackHostname();
        if (! is_string($fallbackHost) || $fallbackHost === '') {
            return;
        }

        FirmDomain::query()->firstOrCreate(
            ['hostname' => $fallbackHost],
            [
                'firm_id' => $firm->id,
                'is_primary' => true,
                'claimed_at' => now(),
                'verification_method' => 'platform_managed',
                'verified_at' => now(),
                'verified_source' => 'platform_managed',
            ],
        );
    }
}
