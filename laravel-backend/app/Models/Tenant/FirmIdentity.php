<?php

namespace App\Models\Tenant;

class FirmIdentity extends TenantModel
{
    public $timestamps = false;

    /**
     * @var list<string>
     */
    protected $fillable = [
        'firm_id',
        'singleton',
    ];

    protected $table = 'firm_identity';
}
