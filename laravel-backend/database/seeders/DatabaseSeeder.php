<?php

namespace Database\Seeders;

use App\Models\CalendarEntry;
use App\Models\ConsultationRequest;
use App\Models\Contact;
use App\Models\Firm;
use App\Models\IntakeRecord;
use App\Models\MatterRecord;
use App\Models\MatterTask;
use App\Models\User;
use App\Tenancy\FirmRegistry;
use App\Tenancy\FirmReviewService;
use App\Tenancy\TenantConnectionManager;
use App\Tenancy\TenantSchemaProvisioner;
use App\Enums\FirmLifecycleStatus;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    // use WithoutModelEvents;

    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        $rolePermissions = config('roles');

        $users = [
            ['name' => 'Sandra Whitmore', 'email' => 'partner@matterforge.local', 'role' => 'Partner', 'platformRole' => 'platform_owner'],
            ['name' => 'Avery Attorney', 'email' => 'attorney@matterforge.local', 'role' => 'Attorney', 'platformRole' => null],
            ['name' => 'Paige Paralegal', 'email' => 'paralegal@matterforge.local', 'role' => 'Paralegal', 'platformRole' => null],
            ['name' => 'Aria Accountant', 'email' => 'accountant@matterforge.local', 'role' => 'Accountant', 'platformRole' => null],
            ['name' => 'Liam Legal Assistant', 'email' => 'assistant@matterforge.local', 'role' => 'LegalAssistant', 'platformRole' => null],
            ['name' => 'Ira Admin', 'email' => 'adminit@matterforge.local', 'role' => 'AdminIT', 'platformRole' => null],
            ['name' => 'Brian and Sandra Firm Partner', 'email' => 'lawfirmbrianandsandra@matterforge.com', 'role' => 'Partner', 'platformRole' => null],
            ['name' => 'Casey Client', 'email' => 'client@matterforge.local', 'role' => 'Client', 'platformRole' => null],
        ];

        foreach ($users as $user) {
            User::query()->updateOrCreate(
                ['email' => $user['email']],
                [
                    'name' => $user['name'],
                    'password' => 'Passw0rd!',
                    'role' => $user['role'],
                    'permissions' => $rolePermissions[$user['role']] ?? [],
                    'platform_role' => $user['platformRole'],
                    'is_platform_administrator' => $user['platformRole'] !== null,
                ],
            );
        }

        $admin = User::query()->where('email', 'partner@matterforge.local')->firstOrFail();
        $attorney = User::query()->where('email', 'attorney@matterforge.local')->firstOrFail();
        $paralegal = User::query()->where('email', 'paralegal@matterforge.local')->firstOrFail();
        $assistant = User::query()->where('email', 'assistant@matterforge.local')->firstOrFail();
        $firmPartner = User::query()->where('email', 'lawfirmbrianandsandra@matterforge.com')->firstOrFail();

        $registry = app(FirmRegistry::class);
        $review = app(FirmReviewService::class);
        $provisioner = app(TenantSchemaProvisioner::class);
        $connections = app(TenantConnectionManager::class);

        $firm = Firm::query()->where('display_name', 'Brian & Sandra Law')->first();
        if (! $firm instanceof Firm) {
            $firm = $registry->register('Brian and Sandra Law LLC', 'Brian & Sandra Law', $admin);
        }

        if ($firm->lifecycle_status === FirmLifecycleStatus::PendingReview) {
            $firm = $review->approve($firm, $admin, 'Seeded development review approval.', ['seed:automated']);
        }

        if (! $firm->isUsable()) {
            $firm = $provisioner->provision($firm->refresh(), $admin);
        }
        $review->saveMembership($firm, $admin, 'Partner', $admin);
        $review->saveMembership($firm, $attorney, 'Attorney', $admin);
        $review->saveMembership($firm, $paralegal, 'Paralegal', $admin);
        $review->saveMembership($firm, $assistant, 'LegalAssistant', $admin);
        $review->saveMembership($firm, $firmPartner, 'Partner', $admin);

        $connections->establish($firm);

        try {
            Contact::query()->updateOrCreate(
                ['display_name' => 'Alicia Warren'],
                [
                    'contact_type' => 'Person',
                    'primary_email' => 'alicia.warren@example.com',
                    'primary_phone' => '(614) 555-0141',
                ],
            );

            Contact::query()->updateOrCreate(
                ['display_name' => 'Northgate Builders LLC'],
                [
                    'contact_type' => 'Organization',
                    'primary_email' => 'ops@northgatebuilders.example',
                    'primary_phone' => '(614) 555-0199',
                ],
            );

            IntakeRecord::query()->updateOrCreate(
                ['prospective_client_name' => 'Mia Carter'],
                [
                    'prospective_client_email' => 'mia.carter@example.com',
                    'prospective_client_phone' => '(614) 555-0201',
                    'practice_area' => 'ConstructionLaw',
                    'intake_type' => 'Website',
                    'stage' => 'New',
                    'owner_actor_id' => 'user-assistant',
                    'owner_display_name' => 'Liam Legal Assistant',
                    'next_action' => 'Schedule follow-up consultation',
                ],
            );

            ConsultationRequest::query()->updateOrCreate(
                ['email' => 'jamie.prospect@example.com'],
                [
                    'full_name' => 'Jamie Prospect',
                    'phone' => '(614) 555-0202',
                    'practice_area' => 'CivilLitigation',
                    'message' => 'Need help with contract dispute timeline.',
                    'preferred_at_utc' => now()->addDays(2)->setTime(14, 0),
                    'time_zone' => 'America/New_York',
                    'status' => 'Requested',
                ],
            );

            $matterOne = MatterRecord::query()->updateOrCreate(
                ['matter_number' => 'MF-2026-1001'],
                [
                    'title' => 'Northgate Builders Contract Claim',
                    'practice_area' => 'CivilLitigation',
                    'stage' => 'Discovery',
                    'status' => 'Open',
                    'responsible_attorney_actor_id' => 'user-2',
                    'responsible_attorney_display_name' => 'Avery Attorney',
                    'paralegal_actor_id' => 'user-3',
                    'paralegal_display_name' => 'Paige Paralegal',
                ],
            );

            $matterTwo = MatterRecord::query()->updateOrCreate(
                ['matter_number' => 'MF-2026-1002'],
                [
                    'title' => 'Carter Retainer Review',
                    'practice_area' => 'ConstructionLaw',
                    'stage' => 'PreSuit',
                    'status' => 'Open',
                    'responsible_attorney_actor_id' => 'user-2',
                    'responsible_attorney_display_name' => 'Avery Attorney',
                    'paralegal_actor_id' => 'user-5',
                    'paralegal_display_name' => 'Liam Legal Assistant',
                ],
            );

            MatterTask::query()->updateOrCreate(
                ['matter_record_id' => $matterOne->id, 'title' => 'Draft demand package'],
                [
                    'description' => 'Compile exhibits and draft demand narrative for review.',
                    'status' => 'InProgress',
                    'priority' => 'High',
                    'assignee_actor_id' => 'user-3',
                    'assignee_display_name' => 'Paige Paralegal',
                    'due_at_utc' => now()->addDays(3)->setTime(15, 0),
                    'completed_at_utc' => null,
                    'created_by_actor_id' => 'user-2',
                    'created_by_display_name' => 'Avery Attorney',
                ],
            );

            MatterTask::query()->updateOrCreate(
                ['matter_record_id' => $matterTwo->id, 'title' => 'Client follow-up call'],
                [
                    'description' => 'Confirm scope and timeline before formal engagement.',
                    'status' => 'Open',
                    'priority' => 'Normal',
                    'assignee_actor_id' => 'user-5',
                    'assignee_display_name' => 'Liam Legal Assistant',
                    'due_at_utc' => now()->addDays(1)->setTime(11, 30),
                    'completed_at_utc' => null,
                    'created_by_actor_id' => 'user-2',
                    'created_by_display_name' => 'Avery Attorney',
                ],
            );

            CalendarEntry::query()->updateOrCreate(
                ['title' => 'Case strategy review', 'owner_actor_id' => 'user-2'],
                [
                    'entry_type' => 'Event',
                    'deadline_type' => 'InternalTarget',
                    'owner_display_name' => 'Avery Attorney',
                    'scheduled_at_utc' => now()->addDays(2)->setTime(13, 0),
                    'time_zone' => 'America/New_York',
                    'matter_reference' => 'MF-2026-1001',
                    'source_reference' => 'INTAKE',
                    'is_completed' => false,
                    'completed_at_utc' => null,
                    'reminder_offsets_json' => json_encode([1440, 60], JSON_THROW_ON_ERROR),
                ],
            );

            CalendarEntry::query()->updateOrCreate(
                ['title' => 'Retainer signed deadline', 'owner_actor_id' => 'user-2'],
                [
                    'entry_type' => 'Deadline',
                    'deadline_type' => 'LegallySignificant',
                    'owner_display_name' => 'Avery Attorney',
                    'scheduled_at_utc' => now()->addDays(4)->setTime(16, 0),
                    'time_zone' => 'America/New_York',
                    'matter_reference' => 'MF-2026-1002',
                    'source_reference' => 'MATTER',
                    'is_completed' => false,
                    'completed_at_utc' => null,
                    'reminder_offsets_json' => json_encode([2880, 1440, 120], JSON_THROW_ON_ERROR),
                ],
            );
        } finally {
            $connections->clear();
        }
    }
}
