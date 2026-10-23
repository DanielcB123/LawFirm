<?php

namespace App\Models\Tenant;

use App\Tenancy\Exceptions\TenantContextMissingException;
use App\Tenancy\TenantContext;
use Illuminate\Database\Eloquent\Model;

abstract class TenantModel extends Model
{
    public function getConnectionName(): ?string
    {
        if (! app(TenantContext::class)->isEstablished()) {
            throw new TenantContextMissingException('Tenant context is not established.');
        }

        return (string) config('tenancy.tenant_connection');
    }
}
