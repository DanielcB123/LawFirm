<?php

namespace App\Tenancy;

use RuntimeException;

class OnboardingFastTrackGuard
{
    public function assertConfigurationIsSafe(): void
    {
        if (! $this->isEnabled()) {
            return;
        }

        if (! app()->environment(['local', 'testing'])) {
            throw new RuntimeException('FIRM_ONBOARDING_DEV_FAST_TRACK can only be enabled in local or testing environments.');
        }
    }

    public function assertRuntimeAllowed(): void
    {
        if (! $this->isEnabled()) {
            throw new RuntimeException('Local fast-track onboarding is disabled.');
        }

        if (! app()->environment(['local', 'testing'])) {
            throw new RuntimeException('Local fast-track onboarding is not allowed in this environment.');
        }
    }

    public function isEnabled(): bool
    {
        return (bool) config('onboarding.dev_fast_track', false);
    }
}
