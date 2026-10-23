<?php

namespace Tests\Feature;

use Illuminate\Foundation\Testing\RefreshDatabase;
use App\Models\Firm;
use App\Models\MatterRecord;
use App\Models\MatterTask;
use App\Models\User;
use App\Tenancy\FirmRegistry;
use App\Tenancy\FirmReviewService;
use App\Tenancy\TenantConnectionManager;
use App\Tenancy\TenantSchemaProvisioner;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class MatterTaskApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_tasks_and_matters_require_authentication(): void
    {
        $this->getJson('/api/tenant/matters')->assertUnauthorized();
        $this->getJson('/api/tenant/tasks')->assertUnauthorized();
    }

    public function test_user_without_required_permissions_gets_forbidden(): void
    {
        [$user, $firm] = $this->memberUser(['contacts.read']);
        Sanctum::actingAs($user);

        $this->withHeader('X-Firm-Id', $firm->id)->getJson('/api/tenant/matters')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'matters.read');

        $this->withHeader('X-Firm-Id', $firm->id)->getJson('/api/tenant/tasks')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'tasks.read');
    }

    public function test_authorized_user_can_list_matters_and_manage_tasks(): void
    {
        [$user, $firm] = $this->memberUser(['matters.read', 'tasks.read', 'tasks.manage']);
        Sanctum::actingAs($user);

        app(TenantConnectionManager::class)->establish($firm);
        try {
            $matter = MatterRecord::factory()->create([
                'matter_number' => 'MF-2026-TEST',
                'title' => 'Test matter',
            ]);

            $task = MatterTask::factory()->create([
                'matter_record_id' => $matter->id,
                'status' => 'Open',
            ]);
        } finally {
            app(TenantConnectionManager::class)->clear();
        }

        $this->withHeader('X-Firm-Id', $firm->id)->getJson('/api/tenant/matters')
            ->assertOk()
            ->assertJsonFragment([
                'matterNumber' => 'MF-2026-TEST',
            ]);

        $created = $this->withHeader('X-Firm-Id', $firm->id)->postJson('/api/tenant/tasks', [
            'matterId' => $matter->id,
            'title' => 'File hearing notice',
            'priority' => 'High',
            'dueAtUtc' => now()->addDays(3)->setTime(10, 0)->toISOString(),
        ])
            ->assertCreated()
            ->assertJsonPath('title', 'File hearing notice');

        $createdTaskId = $created->json('id');

        $this->withHeader('X-Firm-Id', $firm->id)->postJson("/api/tenant/tasks/{$task->id}/status", [
            'status' => 'Completed',
        ])
            ->assertOk()
            ->assertJsonPath('status', 'Completed');

        $this->withHeader('X-Firm-Id', $firm->id)->getJson('/api/tenant/tasks')
            ->assertOk()
            ->assertJsonFragment([
                'id' => $createdTaskId,
                'title' => 'File hearing notice',
            ]);
    }

    /**
     * @param  list<string>  $permissions
     * @return array{User,Firm}
     */
    private function memberUser(array $permissions): array
    {
        $admin = User::factory()->create(['is_platform_administrator' => true]);
        $firm = app(FirmRegistry::class)->register('Task Test LLC', 'Task Test', $admin);
        $firm = app(FirmReviewService::class)->approve($firm, $admin, 'approved', ['test']);
        $firm = app(TenantSchemaProvisioner::class)->provision($firm, $admin);

        $member = User::factory()->create(['permissions' => $permissions]);
        app(FirmReviewService::class)->saveMembership($firm, $member, 'Attorney', $admin);

        return [$member, $firm];
    }
}
