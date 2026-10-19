<?php

namespace Tests\Feature;

use Illuminate\Foundation\Testing\RefreshDatabase;
use App\Models\CalendarEntry;
use App\Models\User;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class CalendarApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_calendar_endpoints_require_authentication(): void
    {
        $this->getJson('/api/calendar/entries')->assertUnauthorized();
        $this->postJson('/api/calendar/entries', [])->assertUnauthorized();
    }

    public function test_user_without_calendar_permission_gets_forbidden(): void
    {
        $user = User::factory()->create([
            'permissions' => ['contacts.read'],
        ]);
        Sanctum::actingAs($user);

        $this->getJson('/api/calendar/entries')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'calendar.read');
    }

    public function test_authorized_user_can_create_and_complete_calendar_entry(): void
    {
        $user = User::factory()->create([
            'permissions' => ['calendar.read', 'calendar.manage'],
        ]);
        Sanctum::actingAs($user);

        $created = $this->postJson('/api/calendar/entries', [
            'title' => 'Prepare mediation packet',
            'entryType' => 'Task',
            'deadlineType' => 'InternalTarget',
            'scheduledAtUtc' => now()->addDays(2)->setTime(14, 0)->toISOString(),
            'timeZone' => 'America/New_York',
            'matterReference' => 'MF-2026-1001',
            'sourceReference' => 'TASKS',
            'reminderOffsetsMinutes' => [1440, 60],
        ])
            ->assertCreated()
            ->assertJsonPath('title', 'Prepare mediation packet')
            ->assertJsonPath('entryType', 'Task');

        $entryId = $created->json('id');

        $this->postJson("/api/calendar/entries/{$entryId}/complete", [
            'isCompleted' => true,
        ])
            ->assertOk()
            ->assertJsonPath('isCompleted', true);

        $this->assertTrue(CalendarEntry::query()->findOrFail($entryId)->is_completed);
    }
}
