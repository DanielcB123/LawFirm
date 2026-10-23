<?php

namespace Tests\Feature;

use App\Models\Firm;
use App\Models\User;
use App\Tenancy\FirmRegistry;
use App\Tenancy\FirmReviewService;
use App\Tenancy\TenantSchemaProvisioner;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class IntakeApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_unauthenticated_intake_requests_are_rejected(): void
    {
        $this->getJson('/api/tenant/intake')->assertUnauthorized();
        $this->postJson('/api/tenant/intake', [])->assertUnauthorized();
    }

    public function test_user_without_intake_read_permission_gets_forbidden(): void
    {
        [$user, $firm] = $this->memberUser(['contacts.read']);
        Sanctum::actingAs($user);

        $this->withHeader('X-Firm-Id', $firm->id)->getJson('/api/tenant/intake')
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'intake.read');
    }

    public function test_user_with_manage_permission_can_create_intake_record(): void
    {
        [$user, $firm] = $this->memberUser(['intake.manage', 'intake.read']);
        Sanctum::actingAs($user);

        $payload = [
            'prospectiveClientName' => 'Jane Prospect',
            'practiceArea' => 'ConstructionLaw',
            'intakeType' => 'Website',
            'ownerActorId' => 'u-attorney-001',
            'ownerDisplayName' => 'Demo Attorney',
            'nextAction' => 'Schedule follow-up call',
        ];

        $this->withHeader('X-Firm-Id', $firm->id)->postJson('/api/tenant/intake', $payload)
            ->assertCreated()
            ->assertJsonPath('prospectiveClientName', 'Jane Prospect')
            ->assertJsonPath('stage', 'New');
    }

    public function test_user_without_manage_permission_cannot_create_intake_record(): void
    {
        [$user, $firm] = $this->memberUser(['intake.read']);
        Sanctum::actingAs($user);

        $this->withHeader('X-Firm-Id', $firm->id)->postJson('/api/tenant/intake', [
            'prospectiveClientName' => 'Should Not Save',
            'practiceArea' => 'ConstructionLaw',
            'intakeType' => 'Website',
            'ownerActorId' => 'u-attorney-001',
            'ownerDisplayName' => 'Demo Attorney',
        ])
            ->assertForbidden()
            ->assertJsonPath('required_permission', 'intake.manage');
    }

    /**
     * @param  list<string>  $permissions
     * @return array{User,Firm}
     */
    private function memberUser(array $permissions): array
    {
        $admin = User::factory()->create(['is_platform_administrator' => true]);
        $firm = app(FirmRegistry::class)->register('Intake Test LLC', 'Intake Test', $admin);
        $firm = app(FirmReviewService::class)->approve($firm, $admin, 'approved', ['test']);
        $firm = app(TenantSchemaProvisioner::class)->provision($firm, $admin);

        $member = User::factory()->create(['permissions' => $permissions]);
        app(FirmReviewService::class)->saveMembership($firm, $member, 'Attorney', $admin);

        return [$member, $firm];
    }
}
