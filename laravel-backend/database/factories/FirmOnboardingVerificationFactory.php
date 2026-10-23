<?php

namespace Database\Factories;

use App\Models\Firm;
use App\Models\FirmOnboardingVerification;
use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends Factory<FirmOnboardingVerification>
 */
class FirmOnboardingVerificationFactory extends Factory
{
    /**
     * Define the model's default state.
     *
     * @return array<string, mixed>
     */
    public function definition(): array
    {
        return [
            'firm_id' => Firm::factory(),
            'revision' => 1,
            'verification_type' => 'business_existence',
            'status' => 'pending',
            'method' => 'manual',
            'reviewer_user_id' => User::factory(),
            'notes' => fake()->optional()->sentence(),
            'reviewed_at' => null,
        ];
    }
}
