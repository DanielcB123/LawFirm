<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Str;

class MatterTask extends Model
{
    /** @use HasFactory<\Database\Factories\MatterTaskFactory> */
    use HasFactory;

    public $incrementing = false;

    protected $keyType = 'string';

    protected $fillable = [
        'matter_record_id',
        'title',
        'description',
        'status',
        'priority',
        'assignee_actor_id',
        'assignee_display_name',
        'due_at_utc',
        'completed_at_utc',
        'created_by_actor_id',
        'created_by_display_name',
    ];

    protected $casts = [
        'due_at_utc' => 'datetime',
        'completed_at_utc' => 'datetime',
    ];

    protected static function booted(): void
    {
        static::creating(function (self $task): void {
            if ($task->id === null) {
                $task->id = (string) Str::uuid();
            }
        });
    }

    public function matterRecord(): BelongsTo
    {
        return $this->belongsTo(MatterRecord::class);
    }
}
