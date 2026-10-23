<?php

namespace App\Policies;

use App\Models\Firm;
use App\Models\User;

class FirmPolicy
{
    public function viewAny(User $user): bool
    {
        return $user->hasPlatformPermission('platform.firms.read');
    }

    public function create(User $user): bool
    {
        return $user->hasPlatformPermission('platform.firms.create');
    }

    public function review(User $user, Firm $firm): bool
    {
        return $user->hasPlatformPermission('platform.firms.review.finalize');
    }

    public function storeVerification(User $user, Firm $firm): bool
    {
        return $user->hasPlatformPermission('platform.firms.verifications.record');
    }

    public function suspend(User $user, Firm $firm): bool
    {
        return $user->hasPlatformPermission('platform.firms.lifecycle.manage');
    }

    public function provision(User $user, Firm $firm): bool
    {
        return $user->hasPlatformPermission('platform.firms.provision');
    }

    public function updateBranding(User $user, Firm $firm): bool
    {
        return $user->hasPlatformPermission('platform.firms.branding.manage');
    }

    public function manageDomain(User $user, Firm $firm): bool
    {
        return $user->hasPlatformPermission('platform.firms.domains.manage');
    }

    public function finalizeDomain(User $user, Firm $firm): bool
    {
        return $user->hasPlatformPermission('platform.firms.domains.finalize');
    }

    public function manageMembership(User $user, Firm $firm): bool
    {
        return $user->hasPlatformPermission('platform.firms.memberships.manage');
    }
}
