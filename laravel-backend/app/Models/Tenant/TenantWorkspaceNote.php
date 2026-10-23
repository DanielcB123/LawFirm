<?php

namespace App\Models\Tenant;

use Illuminate\Database\Eloquent\Concerns\HasUuids;

class TenantWorkspaceNote extends TenantModel
{
    use HasUuids;

    /**
     * @var list<string>
     */
    protected $fillable = [
        'title',
        'body',
    ];
}
