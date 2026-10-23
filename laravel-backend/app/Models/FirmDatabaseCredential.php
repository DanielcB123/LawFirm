<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Concerns\HasUuids;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class FirmDatabaseCredential extends Model
{
    use HasUuids;

    /**
     * @var list<string>
     */
    protected $fillable = [
        'firm_id',
        'username',
        'password_encrypted',
        'grant_host',
        'rotated_at',
        'revoked_at',
    ];

    /**
     * @return array<string, string>
     */
    protected function casts(): array
    {
        return [
            'rotated_at' => 'datetime',
            'revoked_at' => 'datetime',
        ];
    }

    public function firm(): BelongsTo
    {
        return $this->belongsTo(Firm::class);
    }
}
