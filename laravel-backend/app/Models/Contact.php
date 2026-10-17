<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Contact extends Model
{
    protected $fillable = [
        'display_name',
        'contact_type',
        'primary_email',
        'primary_phone',
    ];
}
