<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('matter_tasks', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->foreignUuid('matter_record_id')->constrained('matter_records')->cascadeOnDelete();
            $table->string('title', 200);
            $table->text('description')->nullable();
            $table->string('status', 40)->default('Open');
            $table->string('priority', 40)->default('Normal');
            $table->string('assignee_actor_id', 64)->nullable();
            $table->string('assignee_display_name', 120)->nullable();
            $table->dateTime('due_at_utc')->nullable();
            $table->dateTime('completed_at_utc')->nullable();
            $table->string('created_by_actor_id', 64);
            $table->string('created_by_display_name', 120);
            $table->timestamps();

            $table->index('status');
            $table->index('due_at_utc');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('matter_tasks');
    }
};
