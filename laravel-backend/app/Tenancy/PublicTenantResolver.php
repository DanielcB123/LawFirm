<?php

namespace App\Tenancy;

use App\Enums\FirmLifecycleStatus;
use App\Enums\FirmProvisioningStatus;
use App\Models\Firm;
use App\Models\FirmDomain;
use Illuminate\Http\Request;
use Illuminate\Support\Str;

class PublicTenantResolver
{
    public function __construct(private HostnameNormalizer $hosts) {}

    public function resolve(Request $request): ?Firm
    {
        $hostFirm = $this->firmForHost($request->getHost());
        if ($hostFirm instanceof Firm) {
            return $hostFirm;
        }

        if ((bool) config('tenancy.allow_header_firm_selection', false) !== true) {
            return null;
        }

        $value = $request->headers->get((string) config('tenancy.selection_header'));
        if (! is_string($value) || ! Str::isUuid($value)) {
            return null;
        }

        $firm = Firm::query()->find($value);
        if (! $firm instanceof Firm) {
            return null;
        }

        return $this->isFirmRoutable($firm) ? $firm : null;
    }

    private function firmForHost(string $host): ?Firm
    {
        try {
            $normalized = $this->hosts->normalize($host);
        } catch (\Throwable) {
            return null;
        }

        $domain = FirmDomain::query()
            ->where('hostname', $normalized)
            ->whereNotNull('verified_at')
            ->whereNull('revoked_at')
            ->with('firm')
            ->first();

        if (! $domain instanceof FirmDomain || ! $domain->firm instanceof Firm) {
            return null;
        }

        return $this->isFirmRoutable($domain->firm) ? $domain->firm : null;
    }

    private function isFirmRoutable(Firm $firm): bool
    {
        return $firm->lifecycle_status === FirmLifecycleStatus::Active
            && $firm->provisioning_status === FirmProvisioningStatus::Ready
            && is_string($firm->schema_name)
            && $firm->schema_name !== '';
    }
}
