<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('matter_records', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->string('matter_number', 40)->unique();
            $table->string('title', 200);
            $table->string('practice_area', 80);
            $table->string('stage', 40)->default('Active');
            $table->string('status', 40)->default('Open');
            $table->string('responsible_attorney_actor_id', 64)->nullable();
            $table->string('responsible_attorney_display_name', 120)->nullable();
            $table->string('paralegal_actor_id', 64)->nullable();
            $table->string('paralegal_display_name', 120)->nullable();
            $table->timestamps();

            $table->index('practice_area');
            $table->index('stage');
            $table->index('status');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('matter_records');
    }
};
