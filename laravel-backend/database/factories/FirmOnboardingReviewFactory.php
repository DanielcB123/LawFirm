<?php

namespace Database\Factories;

use App\Models\Firm;
use App\Models\FirmOnboardingReview;
use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends Factory<FirmOnboardingReview>
 */
class FirmOnboardingReviewFactory extends Factory
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
            'reviewer_user_id' => User::factory(),
            'decision' => 'approve',
            'reason' => fake()->sentence(),
            'decided_at' => now(),
        ];
    }
}
