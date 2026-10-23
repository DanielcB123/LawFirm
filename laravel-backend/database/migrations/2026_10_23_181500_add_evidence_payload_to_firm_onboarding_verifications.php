<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('firm_onboarding_verifications', function (Blueprint $table): void {
            $table->json('evidence_payload')->nullable()->after('notes');
        });
    }

    public function down(): void
    {
        Schema::table('firm_onboarding_verifications', function (Blueprint $table): void {
            $table->dropColumn('evidence_payload');
        });
    }
};
