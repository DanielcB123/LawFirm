<?php

namespace Database\Seeders;

use App\Models\ConsultationRequest;
use App\Models\Contact;
use App\Models\IntakeRecord;
use App\Models\User;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    use WithoutModelEvents;

    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        $rolePermissions = config('roles');

        $users = [
            ['name' => 'Sandra Whitmore', 'email' => 'partner@matterforge.local', 'role' => 'Partner'],
            ['name' => 'Avery Attorney', 'email' => 'attorney@matterforge.local', 'role' => 'Attorney'],
            ['name' => 'Paige Paralegal', 'email' => 'paralegal@matterforge.local', 'role' => 'Paralegal'],
            ['name' => 'Aria Accountant', 'email' => 'accountant@matterforge.local', 'role' => 'Accountant'],
            ['name' => 'Liam Legal Assistant', 'email' => 'assistant@matterforge.local', 'role' => 'LegalAssistant'],
            ['name' => 'Ira Admin', 'email' => 'adminit@matterforge.local', 'role' => 'AdminIT'],
            ['name' => 'Casey Client', 'email' => 'client@matterforge.local', 'role' => 'Client'],
        ];

        foreach ($users as $user) {
            User::query()->updateOrCreate(
                ['email' => $user['email']],
                [
                    'name' => $user['name'],
                    'password' => 'Passw0rd!',
                    'role' => $user['role'],
                    'permissions' => $rolePermissions[$user['role']] ?? [],
                ],
            );
        }

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
    }
}
