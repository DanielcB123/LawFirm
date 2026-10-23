<?php

namespace Database\Factories;

use App\Models\Firm;
use App\Models\FirmOnboardingApplication;
use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends Factory<FirmOnboardingApplication>
 */
class FirmOnboardingApplicationFactory extends Factory
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
            'applicant_user_id' => User::factory(),
            'applicant_name' => fake()->name(),
            'applicant_email' => fake()->safeEmail(),
            'business_phone' => fake()->optional()->phoneNumber(),
            'revision' => 1,
            'dev_fast_tracked' => false,
            'submitted_at' => now(),
            'last_material_change_at' => now(),
            'fast_tracked_at' => null,
        ];
    }
}
