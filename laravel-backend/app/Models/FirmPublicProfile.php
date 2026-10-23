<?php

namespace App\Models;

use Database\Factories\FirmPublicProfileFactory;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class FirmPublicProfile extends Model
{
    /** @use HasFactory<FirmPublicProfileFactory> */
    use HasFactory;

    public $incrementing = false;

    protected $primaryKey = 'firm_id';

    protected $keyType = 'string';

    /**
     * @var list<string>
     */
    protected $fillable = [
        'firm_id',
        'logo_path',
        'favicon_path',
        'color_primary',
        'color_secondary',
        'color_accent',
        'typography',
        'contact_email',
        'contact_phone',
        'contact_address',
        'public_copy',
        'public_site_key',
        'enabled_modules',
    ];

    /**
     * @return array<string, string>
     */
    protected function casts(): array
    {
        return [
            'enabled_modules' => 'array',
        ];
    }

    public function firm(): BelongsTo
    {
        return $this->belongsTo(Firm::class);
    }
}
