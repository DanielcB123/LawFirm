<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Str;

class IntakeRecord extends Model
{
    public $incrementing = false;

    protected $keyType = 'string';

    protected $fillable = [
        'prospective_client_name',
        'prospective_client_email',
        'prospective_client_phone',
        'practice_area',
        'intake_type',
        'stage',
        'owner_actor_id',
        'owner_display_name',
        'next_action',
        'next_action_due_at_utc',
        'referral_source',
        'conflict_check_id',
        'approved_conflict_decision_id',
        'decline_reason',
        'notes_summary',
    ];

    protected $casts = [
        'next_action_due_at_utc' => 'datetime',
    ];

    protected static function booted(): void
    {
        static::creating(function (self $record): void {
            if ($record->id === null) {
                $record->id = (string) Str::uuid();
            }
        });
    }
}
