<?php

namespace Database\Factories;

use App\Models\MatterTask;
use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends Factory<MatterTask>
 */
class MatterTaskFactory extends Factory
{
    /**
     * Define the model's default state.
     *
     * @return array<string, mixed>
     */
    public function definition(): array
    {
        $status = fake()->randomElement(['Open', 'InProgress', 'Completed']);

        return [
            'matter_record_id' => \App\Models\MatterRecord::factory(),
            'title' => fake()->sentence(5),
            'description' => fake()->optional()->sentence(8),
            'status' => $status,
            'priority' => fake()->randomElement(['Low', 'Normal', 'High']),
            'assignee_actor_id' => 'user-2',
            'assignee_display_name' => 'Paige Paralegal',
            'due_at_utc' => now()->addDays(fake()->numberBetween(1, 21))->setTime(10, 0),
            'completed_at_utc' => $status === 'Completed' ? now()->subDay() : null,
            'created_by_actor_id' => 'user-1',
            'created_by_display_name' => 'Avery Attorney',
        ];
    }
}
