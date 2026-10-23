<?php

namespace App\Tenancy;

use App\Models\Firm;
use App\Models\FirmMembership;
use App\Models\User;
use App\Tenancy\Exceptions\TenantAccessDeniedException;
use Illuminate\Http\Request;
use Illuminate\Support\Collection;
use Illuminate\Support\Str;

class TenantResolver
{
    public function __construct(private HostnameNormalizer $hosts) {}

    public function resolve(Request $request): Firm
    {
        $user = $request->user();
        if (! $user instanceof User) {
            throw new TenantAccessDeniedException('Authentication is required.', 401);
        }

        /** @var Collection<int, FirmMembership> $memberships */
        $memberships = FirmMembership::query()
            ->where('user_id', $user->id)
            ->with('firm')
            ->get();

        $hostFirm = $this->firmForHost($request->getHost());
        $headerFirm = $this->firmForHeader($request);

        if ($hostFirm instanceof Firm && $headerFirm instanceof Firm && $hostFirm->id !== $headerFirm->id) {
            throw new TenantAccessDeniedException('Tenant context is inconsistent.');
        }

        $selected = $hostFirm ?? $headerFirm;
        if ($selected instanceof Firm) {
            $this->assertMember($memberships, $selected);
            $this->assertUsable($selected);

            return $selected;
        }

        if ($memberships->count() === 1) {
            $firm = $memberships->first()?->firm;
            if (! $firm instanceof Firm) {
                throw new TenantAccessDeniedException('Tenant is not available.');
            }

            $this->assertUsable($firm);

            return $firm;
        }

        if ($memberships->isEmpty()) {
            throw new TenantAccessDeniedException('Tenant is not available.');
        }

        throw new TenantAccessDeniedException(
            'Select a firm you belong to.',
            409,
            'firm_selection_required',
            $memberships
                ->filter(fn (FirmMembership $membership): bool => $membership->firm instanceof Firm)
                ->map(fn (FirmMembership $membership): array => [
                    'firmId' => $membership->firm_id,
                    'displayName' => $membership->firm->display_name,
                    'lifecycleStatus' => $membership->firm->lifecycle_status->value,
                    'role' => $membership->role,
                ])
                ->values()
                ->all(),
        );
    }

    private function firmForHost(string $host): ?Firm
    {
        try {
            $normalized = $this->hosts->normalize($host);
        } catch (\Throwable) {
            return null;
        }

        $platformHosts = config('tenancy.platform_hosts');
        if (is_array($platformHosts) && in_array($normalized, $platformHosts, true)) {
            return null;
        }

        return Firm::query()
            ->whereHas('domains', function ($query) use ($normalized): void {
                $query
                    ->where('hostname', $normalized)
                    ->whereNotNull('verified_at')
                    ->whereNull('revoked_at');
            })
            ->first();
    }

    private function firmForHeader(Request $request): ?Firm
    {
        $headerName = (string) config('tenancy.selection_header');
        $value = $request->headers->get($headerName);

        if (! is_string($value) || trim($value) === '') {
            return null;
        }

        if ((bool) config('tenancy.allow_header_firm_selection', false) !== true) {
            throw new TenantAccessDeniedException('Tenant is not available.');
        }

        if (! Str::isUuid($value)) {
            throw new TenantAccessDeniedException('Tenant is not available.');
        }

        $firm = Firm::query()->find($value);
        if (! $firm instanceof Firm) {
            throw new TenantAccessDeniedException('Tenant is not available.');
        }

        return $firm;
    }

    /**
     * @param  Collection<int, FirmMembership>  $memberships
     */
    private function assertMember(Collection $memberships, Firm $firm): void
    {
        $belongs = $memberships->contains(
            fn (FirmMembership $membership): bool => $membership->firm_id === $firm->id,
        );

        if (! $belongs) {
            throw new TenantAccessDeniedException('Tenant is not available.');
        }
    }

    private function assertUsable(Firm $firm): void
    {
        if (! $firm->isUsable()) {
            throw new TenantAccessDeniedException('Tenant is not available.');
        }
    }
}
