<?php

namespace App\Tenancy;

use App\Models\Firm;
use App\Models\FirmPublicProfile;

class PublicBrandingPresenter
{
    /**
     * @return array<string, mixed>
     */
    public function platform(): array
    {
        $platform = config('tenancy.platform_public');

        return $this->shape(
            source: 'platform',
            displayName: $this->plain($platform['display_name'] ?? null, 'MatterForge', 200),
            publicCopy: $this->plain($platform['public_copy'] ?? null, '', 500),
            contactEmail: $this->email($platform['contact_email'] ?? null),
            contactPhone: $this->phone($platform['contact_phone'] ?? null),
            contactAddress: $this->plain($platform['contact_address'] ?? null, '', 255),
            logoPath: null,
            faviconPath: null,
            colors: [
                'primary' => $this->color($platform['colors']['primary'] ?? null, '#2F1F1A'),
                'secondary' => $this->color($platform['colors']['secondary'] ?? null, '#4D3F36'),
                'accent' => $this->color($platform['colors']['accent'] ?? null, '#5D1F2F'),
            ],
            typography: $this->typography($platform['typography'] ?? null),
            publicSiteKey: $this->siteKey($platform['public_site_key'] ?? null),
            enabledModules: $this->modules($platform['enabled_modules'] ?? []),
        );
    }

    /**
     * @return array<string, mixed>
     */
    public function forFirm(Firm $firm): array
    {
        $profile = $firm->publicProfile;
        $fallback = $this->platform();

        if (! $profile instanceof FirmPublicProfile) {
            return $fallback;
        }

        $payload = $this->shape(
            source: 'firm',
            displayName: $this->plain($firm->display_name, $fallback['displayName'], 200),
            publicCopy: $this->plain($profile->public_copy, $fallback['publicCopy'], 500),
            contactEmail: $this->email($profile->contact_email),
            contactPhone: $this->phone($profile->contact_phone),
            contactAddress: $this->plain($profile->contact_address, '', 255),
            logoPath: $this->assetPath($profile->logo_path),
            faviconPath: $this->assetPath($profile->favicon_path),
            colors: [
                'primary' => $this->color($profile->color_primary, $fallback['colors']['primary']),
                'secondary' => $this->color($profile->color_secondary, $fallback['colors']['secondary']),
                'accent' => $this->color($profile->color_accent, $fallback['colors']['accent']),
            ],
            typography: $this->typography($profile->typography),
            publicSiteKey: $this->siteKey($profile->public_site_key),
            enabledModules: $this->modules($profile->enabled_modules ?? []),
        );

        return $payload;
    }

    /**
     * @param  array{primary: string, secondary: string, accent: string}  $colors
     * @param  list<string>  $enabledModules
     * @return array<string, mixed>
     */
    private function shape(
        string $source,
        string $displayName,
        string $publicCopy,
        ?string $contactEmail,
        ?string $contactPhone,
        string $contactAddress,
        ?string $logoPath,
        ?string $faviconPath,
        array $colors,
        string $typography,
        string $publicSiteKey,
        array $enabledModules,
    ): array {
        return [
            'source' => $source,
            'displayName' => $displayName,
            'publicCopy' => $publicCopy,
            'contactEmail' => $contactEmail,
            'contactPhone' => $contactPhone,
            'contactAddress' => $contactAddress,
            'logoPath' => $logoPath,
            'faviconPath' => $faviconPath,
            'colors' => $colors,
            'typography' => $typography,
            'publicSiteKey' => $publicSiteKey,
            'enabledModules' => $enabledModules,
        ];
    }

    private function plain(mixed $value, string $fallback, int $max): string
    {
        if (! is_string($value)) {
            return $fallback;
        }

        $stripped = trim(str_replace(['<', '>'], '', strip_tags($value)));

        if ($stripped === '') {
            return $fallback;
        }

        return mb_substr($stripped, 0, $max);
    }

    private function color(mixed $value, string $fallback): string
    {
        if (! is_string($value) || preg_match('/^#[0-9A-Fa-f]{6}$/', $value) !== 1) {
            return $fallback;
        }

        return strtoupper($value);
    }

    private function typography(mixed $value): string
    {
        $allowed = config('tenancy.typography');
        if (is_string($value) && is_array($allowed) && in_array($value, $allowed, true)) {
            return $value;
        }

        return 'system-sans';
    }

    private function siteKey(mixed $value): string
    {
        $allowed = config('tenancy.public_site_keys');
        if (is_string($value) && is_array($allowed) && in_array($value, $allowed, true)) {
            return $value;
        }

        return 'default_classic';
    }

    /**
     * @return list<string>
     */
    private function modules(mixed $value): array
    {
        $allowed = config('tenancy.public_modules');
        if (! is_array($value) || ! is_array($allowed)) {
            return [];
        }

        return array_values(array_filter(
            $value,
            fn (mixed $module): bool => is_string($module) && in_array($module, $allowed, true),
        ));
    }

    private function email(mixed $value): ?string
    {
        if (! is_string($value) || filter_var($value, FILTER_VALIDATE_EMAIL) === false) {
            return null;
        }

        return $value;
    }

    private function phone(mixed $value): ?string
    {
        if (! is_string($value) || preg_match('/^[0-9+().\\-\\s]{3,40}$/', $value) !== 1) {
            return null;
        }

        return $value;
    }

    private function assetPath(mixed $value): ?string
    {
        if (! is_string($value)) {
            return null;
        }

        if (preg_match('/^\/storage\/branding\/[a-z0-9-]{1,80}\.(png|webp|svg|ico)$/', $value) !== 1) {
            return null;
        }

        return $value;
    }
}
