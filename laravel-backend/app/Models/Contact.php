<?php

namespace App\Models;

use App\Models\Tenant\TenantModel;

class Contact extends TenantModel
{
    protected $fillable = [
        'display_name',
        'contact_type',
        'primary_email',
        'primary_phone',
    ];
}
