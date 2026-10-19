<?php

namespace Database\Factories;

use App\Models\CalendarEntry;
use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends Factory<CalendarEntry>
 */
class CalendarEntryFactory extends Factory
{
    /**
     * Define the model's default state.
     *
     * @return array<string, mixed>
     */
    public function definition(): array
    {
        $entryType = fake()->randomElement(['Task', 'Event', 'Deadline']);
        $scheduled = now()->addDays(fake()->numberBetween(1, 14))->setTime(fake()->randomElement([9, 11, 14, 16]), 0);

        return [
            'title' => fake()->sentence(4),
            'entry_type' => $entryType,
            'deadline_type' => fake()->randomElement(['OrdinaryTask', 'InternalTarget', 'CourtOrdered', 'LegallySignificant']),
            'owner_actor_id' => 'user-1',
            'owner_display_name' => 'Avery Attorney',
            'scheduled_at_utc' => $scheduled,
            'time_zone' => 'America/New_York',
            'matter_reference' => fake()->optional()->randomElement(['MF-2026-1001', 'MF-2026-1002']),
            'source_reference' => fake()->optional()->bothify('CAL-####'),
            'is_completed' => false,
            'completed_at_utc' => null,
            'reminder_offsets_json' => json_encode([1440, 60], JSON_THROW_ON_ERROR),
        ];
    }
}
