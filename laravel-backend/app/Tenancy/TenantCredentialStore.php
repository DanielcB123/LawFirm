<?php

namespace App\Tenancy;

use App\Models\Firm;
use App\Models\FirmDatabaseCredential;
use Illuminate\Support\Facades\Crypt;
use Illuminate\Support\Str;

class TenantCredentialStore
{
    /**
     * @return array{username:string,password:string,host:string}
     */
    public function issueRuntimeCredential(Firm $firm): array
    {
        $username = $this->usernameForFirm($firm);
        $password = Str::random(40).'A1!';
        $host = (string) config('tenancy.runtime_grant_host');

        $this->storeCredential($firm, $username, $password, $host);

        return [
            'username' => $username,
            'password' => $password,
            'host' => $host,
        ];
    }

    public function storeFallbackCredential(Firm $firm, string $username, string $password, string $host): void
    {
        $this->storeCredential($firm, $username, $password, $host);
    }

    private function storeCredential(Firm $firm, string $username, string $password, string $host): void
    {
        $this->revokeRuntimeCredential($firm);

        $current = FirmDatabaseCredential::query()
            ->where('firm_id', $firm->id)
            ->where('username', $username)
            ->first();

        if ($current instanceof FirmDatabaseCredential) {
            $current->forceFill([
                'password_encrypted' => Crypt::encryptString($password),
                'grant_host' => $host,
                'rotated_at' => now(),
                'revoked_at' => null,
            ])->save();
        } else {
            FirmDatabaseCredential::query()->create([
                'firm_id' => $firm->id,
                'username' => $username,
                'password_encrypted' => Crypt::encryptString($password),
                'grant_host' => $host,
                'rotated_at' => now(),
            ]);
        }

    }

    /**
     * @return array{username:string,password:string,host:string}|null
     */
    public function activeRuntimeCredential(Firm $firm): ?array
    {
        $credential = FirmDatabaseCredential::query()
            ->where('firm_id', $firm->id)
            ->whereNull('revoked_at')
            ->orderByDesc('rotated_at')
            ->first();

        if (! $credential instanceof FirmDatabaseCredential) {
            return null;
        }

        return [
            'username' => $credential->username,
            'password' => Crypt::decryptString($credential->password_encrypted),
            'host' => $credential->grant_host,
        ];
    }

    public function revokeRuntimeCredential(Firm $firm): void
    {
        FirmDatabaseCredential::query()
            ->where('firm_id', $firm->id)
            ->whereNull('revoked_at')
            ->update(['revoked_at' => now()]);
    }

    private function usernameForFirm(Firm $firm): string
    {
        $id = str_replace('-', '', $firm->id);

        return 'mf_t_'.substr($id, 0, 16);
    }
}
