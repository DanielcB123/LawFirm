<?php

namespace Tests\Feature;

use App\Models\ConsultationRequest;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class ConsultationApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_public_user_can_create_consultation_request(): void
    {
        $payload = [
            'fullName' => 'Jamie Prospect',
            'email' => 'jamie.prospect@example.com',
            'practiceArea' => 'CivilLitigation',
            'preferredAtUtc' => now()->addDays(2)->setTime(14, 0)->toISOString(),
            'timeZone' => 'America/New_York',
        ];

        $this->postJson('/api/consultations', $payload)
            ->assertCreated()
            ->assertJsonPath('fullName', 'Jamie Prospect')
            ->assertJsonPath('status', 'Requested');
    }

    public function test_available_slots_endpoint_returns_values(): void
    {
        $this->getJson('/api/consultations/available-slots?days=3')
            ->assertOk()
            ->assertJsonStructure([
                '*' => ['startsAtUtc', 'endsAtUtc'],
            ]);
    }

    public function test_queue_requires_authentication(): void
    {
        $this->getJson('/api/consultations')->assertUnauthorized();
    }

    public function test_queue_requires_consultations_read_permission(): void
    {
        $user = User::factory()->create([
            'permissions' => ['contacts.read'],
        ]);
        Sanctum::actingAs($user);

        $this->getJson('/api/consultations')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'consultations.read');
    }

    public function test_authorized_user_can_update_consultation_status(): void
    {
        $user = User::factory()->create([
            'permissions' => ['consultations.read', 'consultations.manage'],
        ]);
        Sanctum::actingAs($user);

        $consultation = ConsultationRequest::factory()->create([
            'status' => 'Requested',
        ]);

        $this->postJson("/api/consultations/{$consultation->id}/status", [
            'status' => 'Scheduled',
            'assignedToActorId' => 'user-100',
            'assignedToDisplayName' => 'Liam Legal Assistant',
            'internalNotes' => 'Confirmed follow-up call',
        ])
            ->assertOk()
            ->assertJsonPath('status', 'Scheduled')
            ->assertJsonPath('assignedToDisplayName', 'Liam Legal Assistant');
    }
}
