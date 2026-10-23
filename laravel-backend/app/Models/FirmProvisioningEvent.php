<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Concerns\HasUuids;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class FirmProvisioningEvent extends Model
{
    use HasUuids;

    /**
     * @var list<string>
     */
    protected $fillable = [
        'firm_id',
        'actor_user_id',
        'event_type',
        'provisioning_status',
        'message',
    ];

    public function firm(): BelongsTo
    {
        return $this->belongsTo(Firm::class);
    }

    public function actor(): BelongsTo
    {
        return $this->belongsTo(User::class, 'actor_user_id');
    }
}
