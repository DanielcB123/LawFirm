<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Str;

class ConsultationRequest extends Model
{
    /** @use HasFactory<\Database\Factories\ConsultationRequestFactory> */
    use HasFactory;

    public $incrementing = false;

    protected $keyType = 'string';

    protected $fillable = [
        'full_name',
        'email',
        'phone',
        'practice_area',
        'message',
        'preferred_at_utc',
        'time_zone',
        'status',
        'assigned_to_actor_id',
        'assigned_to_display_name',
        'internal_notes',
    ];

    protected $casts = [
        'preferred_at_utc' => 'datetime',
    ];

    protected static function booted(): void
    {
        static::creating(function (self $item): void {
            if ($item->id === null) {
                $item->id = (string) Str::uuid();
            }
        });
    }
}
