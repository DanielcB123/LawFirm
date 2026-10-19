<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('calendar_entries', function (Blueprint $table) {
            $table->uuid('id')->primary();
            $table->string('title', 200);
            $table->string('entry_type', 20);
            $table->string('deadline_type', 40)->default('InternalTarget');
            $table->string('owner_actor_id', 64);
            $table->string('owner_display_name', 120);
            $table->dateTime('scheduled_at_utc');
            $table->string('time_zone', 80);
            $table->string('matter_reference', 64)->nullable();
            $table->string('source_reference', 120)->nullable();
            $table->boolean('is_completed')->default(false);
            $table->dateTime('completed_at_utc')->nullable();
            $table->text('reminder_offsets_json')->nullable();
            $table->timestamps();

            $table->index('entry_type');
            $table->index('scheduled_at_utc');
            $table->index('is_completed');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('calendar_entries');
    }
};
