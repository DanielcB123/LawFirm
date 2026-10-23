<?php

namespace App\Tenancy;

use App\Models\Firm;
use App\Models\FirmDomain;
use Illuminate\Http\Request;

class PublicBrandingResolver
{
    public function __construct(
        private PublicBrandingPresenter $presenter,
        private HostnameNormalizer $hosts,
    ) {}

    /**
     * @return array<string, mixed>
     */
    public function payload(Request $request): array
    {
        $firm = $this->firmForHost($request->getHost());

        if (! $firm instanceof Firm && $this->isPlatformHost($request->getHost())) {
            $firm = $this->firmForHost($this->originHost($request));
        }

        if (! $firm instanceof Firm || ! $firm->isUsable()) {
            return $this->presenter->platform();
        }

        return $this->presenter->forFirm($firm);
    }

    private function firmForHost(?string $host): ?Firm
    {
        if (! is_string($host) || $host === '') {
            return null;
        }

        try {
            $normalized = $this->hosts->normalize($host);
        } catch (\Throwable) {
            return null;
        }

        $domain = FirmDomain::query()
            ->where('hostname', $normalized)
            ->whereNotNull('verified_at')
            ->whereNull('revoked_at')
            ->with('firm.publicProfile')
            ->first();

        return $domain?->firm;
    }

    private function isPlatformHost(string $host): bool
    {
        $platformHosts = config('tenancy.platform_hosts');

        return is_array($platformHosts) && in_array(strtolower($host), $platformHosts, true);
    }

    private function originHost(Request $request): ?string
    {
        $origin = $request->headers->get('Origin');
        if (! is_string($origin) || $origin === '') {
            return null;
        }

        $host = parse_url($origin, PHP_URL_HOST);

        return is_string($host) ? $host : null;
    }
}
