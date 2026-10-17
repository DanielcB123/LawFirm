<?php

namespace Database\Seeders;

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
        User::query()->updateOrCreate([
            'email' => 'attorney@matterforge.local',
        ], [
            'name' => 'Demo Attorney',
            'password' => 'Passw0rd!',
            'role' => 'Attorney',
            'permissions' => [
                'contacts.read',
                'contacts.manage',
                'intake.read',
                'intake.manage',
            ],
        ]);

        User::query()->updateOrCreate([
            'email' => 'readonly@matterforge.local',
        ], [
            'name' => 'Read Only User',
            'password' => 'Passw0rd!',
            'role' => 'Paralegal',
            'permissions' => [
                'contacts.read',
                'intake.read',
            ],
        ]);
    }
}
