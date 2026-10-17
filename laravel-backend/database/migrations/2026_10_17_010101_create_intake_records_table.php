<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('intake_records', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->string('prospective_client_name', 200);
            $table->string('prospective_client_email', 256)->nullable();
            $table->string('prospective_client_phone', 64)->nullable();
            $table->string('practice_area', 80);
            $table->string('intake_type', 40);
            $table->string('stage', 40)->default('New');
            $table->string('owner_actor_id', 64);
            $table->string('owner_display_name', 120);
            $table->string('next_action', 200)->nullable();
            $table->dateTimeTz('next_action_due_at_utc')->nullable();
            $table->string('referral_source', 80)->nullable();
            $table->uuid('conflict_check_id')->nullable();
            $table->uuid('approved_conflict_decision_id')->nullable();
            $table->string('decline_reason', 200)->nullable();
            $table->text('notes_summary')->nullable();
            $table->timestamps();

            $table->index('stage');
            $table->index('owner_actor_id');
            $table->index('created_at');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('intake_records');
    }
};
