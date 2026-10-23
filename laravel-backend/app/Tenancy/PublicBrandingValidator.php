<?php

namespace App\Tenancy;

use Illuminate\Validation\Rule;

class PublicBrandingValidator
{
    /**
     * @return array<string, mixed>
     */
    public static function rules(): array
    {
        return [
            'logoPath' => ['nullable', 'string', 'regex:/^\/storage\/branding\/[a-z0-9-]{1,80}\.(png|webp|svg|ico)$/'],
            'faviconPath' => ['nullable', 'string', 'regex:/^\/storage\/branding\/[a-z0-9-]{1,80}\.(png|webp|svg|ico)$/'],
            'colors' => ['required', 'array'],
            'colors.primary' => ['required', 'regex:/^#[0-9A-Fa-f]{6}$/'],
            'colors.secondary' => ['required', 'regex:/^#[0-9A-Fa-f]{6}$/'],
            'colors.accent' => ['required', 'regex:/^#[0-9A-Fa-f]{6}$/'],
            'typography' => ['required', 'string', Rule::in(config('tenancy.typography'))],
            'contactEmail' => ['nullable', 'email', 'max:255'],
            'contactPhone' => ['nullable', 'string', 'regex:/^[0-9+().\\-\\s]{3,40}$/'],
            'contactAddress' => ['nullable', 'string', 'max:255', 'regex:/^[^<>]*$/'],
            'publicCopy' => ['required', 'string', 'max:500', 'regex:/^[^<>]*$/'],
            'publicSiteKey' => ['required', 'string', Rule::in(config('tenancy.public_site_keys'))],
            'enabledModules' => ['required', 'array'],
            'enabledModules.*' => ['string', Rule::in(config('tenancy.public_modules'))],
        ];
    }

    /**
     * @param  array<string, mixed>  $validated
     * @return array<string, mixed>
     */
    public static function attributes(array $validated): array
    {
        return [
            'logo_path' => $validated['logoPath'] ?? null,
            'favicon_path' => $validated['faviconPath'] ?? null,
            'color_primary' => strtoupper((string) $validated['colors']['primary']),
            'color_secondary' => strtoupper((string) $validated['colors']['secondary']),
            'color_accent' => strtoupper((string) $validated['colors']['accent']),
            'typography' => $validated['typography'],
            'contact_email' => $validated['contactEmail'] ?? null,
            'contact_phone' => $validated['contactPhone'] ?? null,
            'contact_address' => $validated['contactAddress'] ?? null,
            'public_copy' => $validated['publicCopy'],
            'public_site_key' => $validated['publicSiteKey'],
            'enabled_modules' => array_values($validated['enabledModules']),
        ];
    }
}
