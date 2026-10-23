<?php

namespace App\Tenancy;

use App\Enums\FirmLifecycleStatus;
use App\Enums\FirmProvisioningStatus;
use App\Models\Firm;
use App\Models\FirmDomain;
use App\Models\FirmPublicProfile;
use App\Models\User;
use Illuminate\Support\Str;
use Illuminate\Validation\ValidationException;

class FirmRegistry
{
    public function __construct(private FirmAuditRecorder $audit) {}

    public function register(string $legalName, string $displayName, ?User $actor = null): Firm
    {
        $platform = config('tenancy.platform_public');
        $slug = $this->slugCandidate($displayName);
        if (! $this->isSlugAvailable($slug)) {
            $suggestions = $this->availableSlugSuggestions($slug);
            throw ValidationException::withMessages([
                'firmName' => $this->unavailableSlugMessage($slug, $suggestions),
            ]);
        }

        $firm = new Firm([
            'legal_name' => $legalName,
            'display_name' => $displayName,
            'slug' => $slug,
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

    public function slugCandidate(string $displayName): string
    {
        $base = Str::slug($displayName);
        if ($base === '') {
            return 'firm';
        }

        return Str::limit($base, 80, '');
    }

    public function isSlugAvailable(string $slug): bool
    {
        return ! Firm::query()->where('slug', $slug)->exists();
    }

    /**
     * @return list<string>
     */
    public function availableSlugSuggestions(string $slug, int $limit = 5): array
    {
        $base = Str::limit(Str::of($slug)->lower()->replaceMatches('/[^a-z0-9-]/', '')->trim('-')->value(), 70, '');
        if ($base === '') {
            $base = 'firm';
        }

        $suggestions = [];
        foreach ($this->configuredSuffixes() as $suffix) {
            $candidate = Str::limit($base.'-'.$suffix, 80, '');
            if ($this->isSlugAvailable($candidate) && ! in_array($candidate, $suggestions, true)) {
                $suggestions[] = $candidate;
            }
            if (count($suggestions) >= $limit) {
                return $suggestions;
            }
        }

        $counter = 2;
        while (count($suggestions) < $limit && $counter <= 200) {
            $candidate = Str::limit($base.'-'.$counter, 80, '');
            if ($this->isSlugAvailable($candidate) && ! in_array($candidate, $suggestions, true)) {
                $suggestions[] = $candidate;
            }
            $counter++;
        }

        return $suggestions;
    }

    /**
     * @return array{slug:string,available:bool,suggestions:list<string>}
     */
    public function slugAvailability(string $displayName): array
    {
        $slug = $this->slugCandidate($displayName);
        $available = $this->isSlugAvailable($slug);

        return [
            'slug' => $slug,
            'available' => $available,
            'suggestions' => $available ? [] : $this->availableSlugSuggestions($slug),
        ];
    }

    /**
     * @param  list<string>  $suggestions
     */
    private function unavailableSlugMessage(string $slug, array $suggestions): string
    {
        if ($suggestions === []) {
            return "The firm URL slug '{$slug}' is unavailable. Choose a different firm name.";
        }

        return "The firm URL slug '{$slug}' is unavailable. Try: ".implode(', ', $suggestions).'.';
    }

    /**
     * @return list<string>
     */
    private function configuredSuffixes(): array
    {
        $configured = config('onboarding.name_suggestion_suffixes', []);
        if (! is_array($configured)) {
            return [];
        }

        return array_values(array_filter(
            array_map(static fn (mixed $value): string => Str::of((string) $value)->slug('-')->value(), $configured),
            static fn (string $suffix): bool => $suffix !== ''
        ));
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
