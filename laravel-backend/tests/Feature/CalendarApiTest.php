<?php

namespace Tests\Feature;

use App\Models\Firm;
use Illuminate\Foundation\Testing\RefreshDatabase;
use App\Models\User;
use App\Tenancy\FirmRegistry;
use App\Tenancy\FirmReviewService;
use App\Tenancy\TenantSchemaProvisioner;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class CalendarApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_calendar_endpoints_require_authentication(): void
    {
        $this->getJson('/api/tenant/calendar/entries')->assertUnauthorized();
        $this->postJson('/api/tenant/calendar/entries', [])->assertUnauthorized();
    }

    public function test_user_without_calendar_permission_gets_forbidden(): void
    {
        [$user, $firm] = $this->memberUser(['contacts.read']);
        Sanctum::actingAs($user);

        $this->withHeader('X-Firm-Id', $firm->id)->getJson('/api/tenant/calendar/entries')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'calendar.read');
    }

    public function test_authorized_user_can_create_and_complete_calendar_entry(): void
    {
        [$user, $firm] = $this->memberUser(['calendar.read', 'calendar.manage']);
        Sanctum::actingAs($user);

        $created = $this->withHeader('X-Firm-Id', $firm->id)->postJson('/api/tenant/calendar/entries', [
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

        $this->withHeader('X-Firm-Id', $firm->id)->postJson("/api/tenant/calendar/entries/{$entryId}/complete", [
            'isCompleted' => true,
        ])
            ->assertOk()
            ->assertJsonPath('isCompleted', true);
    }

    /**
     * @param  list<string>  $permissions
     * @return array{User,Firm}
     */
    private function memberUser(array $permissions): array
    {
        $admin = User::factory()->create(['is_platform_administrator' => true]);
        $firm = app(FirmRegistry::class)->register('Calendar Test LLC', 'Calendar Test', $admin);
        $firm = app(FirmReviewService::class)->approve($firm, $admin, 'approved', ['test']);
        $firm = app(TenantSchemaProvisioner::class)->provision($firm, $admin);

        $member = User::factory()->create(['permissions' => $permissions]);
        app(FirmReviewService::class)->saveMembership($firm, $member, 'Attorney', $admin);

        return [$member, $firm];
    }
}
