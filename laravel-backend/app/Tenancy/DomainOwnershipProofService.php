<?php

namespace App\Tenancy;

use App\Models\FirmDomain;
use Carbon\CarbonImmutable;
use Illuminate\Support\Str;

class DomainOwnershipProofService
{
    public function __construct(private DnsTxtLookup $dns) {}

    /**
     * @return array{token:string,recordName:string,expiresAt:string}
     */
    public function issueChallenge(FirmDomain $domain): array
    {
        $token = 'mfv1_'.Str::random(48);
        $expiresAt = CarbonImmutable::now('UTC')->addHours(24);

        $domain->forceFill([
            'claimed_at' => now(),
            'verification_method' => 'dns_txt',
            'verification_token_hash' => hash('sha256', $token),
            'verification_expires_at' => $expiresAt,
            'verification_used_at' => null,
            'verification_last_error' => null,
            'verified_at' => null,
            'verified_by_user_id' => null,
            'verified_source' => null,
            'revoked_at' => null,
            'released_at' => null,
        ])->save();

        return [
            'token' => $token,
            'recordName' => $this->recordName($domain->hostname),
            'expiresAt' => $expiresAt->toIso8601String(),
        ];
    }

    public function verifyChallenge(FirmDomain $domain): bool
    {
        if (! is_string($domain->verification_token_hash) || $domain->verification_token_hash === '') {
            $domain->forceFill([
                'verification_last_error' => 'No active verification challenge is available.',
            ])->save();

            return false;
        }

        if ($domain->verification_expires_at === null || now()->greaterThan($domain->verification_expires_at)) {
            $domain->forceFill([
                'verification_last_error' => 'DNS TXT challenge has expired.',
            ])->save();

            return false;
        }

        $recordName = $this->recordName($domain->hostname);
        $records = $this->dns->records($recordName);
        $verified = collect($records)->contains(
            fn (string $txt): bool => hash_equals(hash('sha256', trim($txt)), $domain->verification_token_hash),
        );

        if (! $verified) {
            $domain->forceFill([
                'verification_last_error' => 'DNS TXT challenge does not match the active token.',
            ])->save();
        }

        return $verified;
    }

    public function recordName(string $hostname): string
    {
        return '_mf-domain-challenge.'.$hostname;
    }
}
