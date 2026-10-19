<?php

namespace Database\Factories;

use App\Models\MatterRecord;
use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends Factory<MatterRecord>
 */
class MatterRecordFactory extends Factory
{
    /**
     * Define the model's default state.
     *
     * @return array<string, mixed>
     */
    public function definition(): array
    {
        return [
            'matter_number' => sprintf(
                'MF-%s-%04d',
                now()->format('Y'),
                fake()->numberBetween(1000, 9999),
            ),
            'title' => fake()->sentence(4),
            'practice_area' => fake()->randomElement(['ConstructionLaw', 'CivilLitigation', 'PersonalInjury']),
            'stage' => fake()->randomElement(['Active', 'Discovery', 'PreSuit']),
            'status' => fake()->randomElement(['Open', 'Pending', 'AtRisk']),
            'responsible_attorney_actor_id' => 'user-1',
            'responsible_attorney_display_name' => 'Avery Attorney',
            'paralegal_actor_id' => 'user-2',
            'paralegal_display_name' => 'Paige Paralegal',
        ];
    }
}
