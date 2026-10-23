<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('consultation_requests', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->string('full_name', 200);
            $table->string('email', 256);
            $table->string('phone', 64)->nullable();
            $table->string('practice_area', 80);
            $table->text('message')->nullable();
            $table->dateTime('preferred_at_utc');
            $table->string('time_zone', 80);
            $table->string('status', 40)->default('Requested');
            $table->string('assigned_to_actor_id', 64)->nullable();
            $table->string('assigned_to_display_name', 120)->nullable();
            $table->text('internal_notes')->nullable();
            $table->timestamps();

            $table->index('status');
            $table->index('preferred_at_utc');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('consultation_requests');
    }
};
