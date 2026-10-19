<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Str;

class MatterRecord extends Model
{
    /** @use HasFactory<\Database\Factories\MatterRecordFactory> */
    use HasFactory;

    public $incrementing = false;

    protected $keyType = 'string';

    protected $fillable = [
        'matter_number',
        'title',
        'practice_area',
        'stage',
        'status',
        'responsible_attorney_actor_id',
        'responsible_attorney_display_name',
        'paralegal_actor_id',
        'paralegal_display_name',
    ];

    protected static function booted(): void
    {
        static::creating(function (self $record): void {
            if ($record->id === null) {
                $record->id = (string) Str::uuid();
            }
        });
    }

    public function tasks(): HasMany
    {
        return $this->hasMany(MatterTask::class);
    }
}
