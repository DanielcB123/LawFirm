<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Concerns\HasUuids;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class FirmDomain extends Model
{
    use HasUuids;

    /**
     * @var list<string>
     */
    protected $fillable = [
        'firm_id',
        'hostname',
        'claimed_at',
        'verification_method',
        'verification_token_hash',
        'verification_expires_at',
        'verification_used_at',
        'verification_last_error',
        'verified_at',
        'verified_by_user_id',
        'verified_source',
        'revoked_at',
        'released_at',
        'is_primary',
    ];

    /**
     * @return array<string, string>
     */
    protected function casts(): array
    {
        return [
            'claimed_at' => 'datetime',
            'verified_at' => 'datetime',
            'verification_expires_at' => 'datetime',
            'verification_used_at' => 'datetime',
            'revoked_at' => 'datetime',
            'released_at' => 'datetime',
            'is_primary' => 'boolean',
        ];
    }

    public function firm(): BelongsTo
    {
        return $this->belongsTo(Firm::class);
    }
}
