<?php

namespace App\Models;

use Database\Factories\FirmOnboardingApplicationFactory;
use Illuminate\Database\Eloquent\Concerns\HasUuids;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class FirmOnboardingApplication extends Model
{
    /** @use HasFactory<FirmOnboardingApplicationFactory> */
    use HasFactory, HasUuids;

    /**
     * @var list<string>
     */
    protected $fillable = [
        'firm_id',
        'applicant_user_id',
        'applicant_name',
        'applicant_email',
        'business_phone',
        'revision',
        'dev_fast_tracked',
        'submitted_at',
        'last_material_change_at',
        'fast_tracked_at',
    ];

    /**
     * @return array<string, string>
     */
    protected function casts(): array
    {
        return [
            'revision' => 'integer',
            'dev_fast_tracked' => 'boolean',
            'submitted_at' => 'datetime',
            'last_material_change_at' => 'datetime',
            'fast_tracked_at' => 'datetime',
        ];
    }

    public function firm(): BelongsTo
    {
        return $this->belongsTo(Firm::class);
    }

    public function applicant(): BelongsTo
    {
        return $this->belongsTo(User::class, 'applicant_user_id');
    }

    public function reviews(): HasMany
    {
        return $this->hasMany(FirmOnboardingReview::class, 'firm_id', 'firm_id');
    }

    public function verifications(): HasMany
    {
        return $this->hasMany(FirmOnboardingVerification::class, 'firm_id', 'firm_id');
    }
}
