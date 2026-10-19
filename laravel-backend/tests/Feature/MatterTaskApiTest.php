<?php

namespace Tests\Feature;

use Illuminate\Foundation\Testing\RefreshDatabase;
use App\Models\MatterRecord;
use App\Models\MatterTask;
use App\Models\User;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class MatterTaskApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_tasks_and_matters_require_authentication(): void
    {
        $this->getJson('/api/matters')->assertUnauthorized();
        $this->getJson('/api/tasks')->assertUnauthorized();
    }

    public function test_user_without_required_permissions_gets_forbidden(): void
    {
        Sanctum::actingAs(User::factory()->create([
            'permissions' => ['contacts.read'],
        ]));

        $this->getJson('/api/matters')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'matters.read');

        $this->getJson('/api/tasks')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'tasks.read');
    }

    public function test_authorized_user_can_list_matters_and_manage_tasks(): void
    {
        $user = User::factory()->create([
            'permissions' => ['matters.read', 'tasks.read', 'tasks.manage'],
        ]);
        Sanctum::actingAs($user);

        $matter = MatterRecord::factory()->create([
            'matter_number' => 'MF-2026-TEST',
            'title' => 'Test matter',
        ]);

        $task = MatterTask::factory()->create([
            'matter_record_id' => $matter->id,
            'status' => 'Open',
        ]);

        $this->getJson('/api/matters')
            ->assertOk()
            ->assertJsonFragment([
                'matterNumber' => 'MF-2026-TEST',
            ]);

        $created = $this->postJson('/api/tasks', [
            'matterId' => $matter->id,
            'title' => 'File hearing notice',
            'priority' => 'High',
            'dueAtUtc' => now()->addDays(3)->setTime(10, 0)->toISOString(),
        ])
            ->assertCreated()
            ->assertJsonPath('title', 'File hearing notice');

        $createdTaskId = $created->json('id');

        $this->postJson("/api/tasks/{$task->id}/status", [
            'status' => 'Completed',
        ])
            ->assertOk()
            ->assertJsonPath('status', 'Completed');

        $this->getJson('/api/tasks')
            ->assertOk()
            ->assertJsonFragment([
                'id' => $createdTaskId,
                'title' => 'File hearing notice',
            ]);
    }
}
