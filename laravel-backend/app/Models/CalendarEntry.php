<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Str;

class CalendarEntry extends Model
{
    /** @use HasFactory<\Database\Factories\CalendarEntryFactory> */
    use HasFactory;

    public $incrementing = false;

    protected $keyType = 'string';

    protected $fillable = [
        'title',
        'entry_type',
        'deadline_type',
        'owner_actor_id',
        'owner_display_name',
        'scheduled_at_utc',
        'time_zone',
        'matter_reference',
        'source_reference',
        'is_completed',
        'completed_at_utc',
        'reminder_offsets_json',
    ];

    protected $casts = [
        'scheduled_at_utc' => 'datetime',
        'completed_at_utc' => 'datetime',
        'is_completed' => 'boolean',
    ];

    protected static function booted(): void
    {
        static::creating(function (self $entry): void {
            if ($entry->id === null) {
                $entry->id = (string) Str::uuid();
            }
        });
    }
}
