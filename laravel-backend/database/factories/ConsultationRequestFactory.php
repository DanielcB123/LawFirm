<?php

namespace Database\Factories;

use App\Models\ConsultationRequest;
use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends Factory<ConsultationRequest>
 */
class ConsultationRequestFactory extends Factory
{
    /**
     * Define the model's default state.
     *
     * @return array<string, mixed>
     */
    public function definition(): array
    {
        return [
            'full_name' => fake()->name(),
            'email' => fake()->safeEmail(),
            'phone' => fake()->optional()->phoneNumber(),
            'practice_area' => fake()->randomElement(['ConstructionLaw', 'CivilLitigation', 'EstatePlanning']),
            'message' => fake()->optional()->sentence(14),
            'preferred_at_utc' => now()->addDays(fake()->numberBetween(1, 10))->setTime(fake()->randomElement([9, 11, 13, 15]), 0),
            'time_zone' => 'America/New_York',
            'status' => fake()->randomElement(['Requested', 'Scheduled', 'InReview']),
            'assigned_to_actor_id' => null,
            'assigned_to_display_name' => null,
            'internal_notes' => null,
        ];
    }
}
