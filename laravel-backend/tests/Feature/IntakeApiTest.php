<?php

namespace Tests\Feature;

use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class IntakeApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_unauthenticated_intake_requests_are_rejected(): void
    {
        $this->getJson('/api/intake')->assertUnauthorized();
        $this->postJson('/api/intake', [])->assertUnauthorized();
    }

    public function test_user_without_intake_read_permission_gets_forbidden(): void
    {
        $user = User::factory()->create([
            'permissions' => ['contacts.read'],
        ]);
        Sanctum::actingAs($user);

        $this->getJson('/api/intake')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'intake.read');
    }

    public function test_user_with_manage_permission_can_create_intake_record(): void
    {
        $user = User::factory()->create([
            'permissions' => ['intake.manage', 'intake.read'],
        ]);
        Sanctum::actingAs($user);

        $payload = [
            'prospectiveClientName' => 'Jane Prospect',
            'practiceArea' => 'ConstructionLaw',
            'intakeType' => 'Website',
            'ownerActorId' => 'u-attorney-001',
            'ownerDisplayName' => 'Demo Attorney',
            'nextAction' => 'Schedule follow-up call',
        ];

        $this->postJson('/api/intake', $payload)
            ->assertCreated()
            ->assertJsonPath('prospectiveClientName', 'Jane Prospect')
            ->assertJsonPath('stage', 'New');
    }

    public function test_user_without_manage_permission_cannot_create_intake_record(): void
    {
        $user = User::factory()->create([
            'permissions' => ['intake.read'],
        ]);
        Sanctum::actingAs($user);

        $this->postJson('/api/intake', [
            'prospectiveClientName' => 'Should Not Save',
            'practiceArea' => 'ConstructionLaw',
            'intakeType' => 'Website',
            'ownerActorId' => 'u-attorney-001',
            'ownerDisplayName' => 'Demo Attorney',
        ])
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'intake.manage');
    }
}
