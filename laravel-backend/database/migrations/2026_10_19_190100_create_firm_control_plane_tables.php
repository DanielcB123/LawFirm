<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('firms', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->string('legal_name', 200);
            $table->string('display_name', 200);
            $table->string('slug', 80)->unique();
            $table->string('lifecycle_status', 32)->index();
            $table->string('provisioning_status', 32)->index();
            $table->string('schema_name', 40)->nullable()->unique();
            $table->string('schema_version', 255)->nullable();
            $table->string('provisioning_failure_reason', 255)->nullable();
            $table->timestamp('approved_at')->nullable();
            $table->timestamp('activated_at')->nullable();
            $table->timestamp('rejected_at')->nullable();
            $table->timestamp('suspended_at')->nullable();
            $table->timestamps();
        });

        Schema::create('firm_public_profiles', function (Blueprint $table): void {
            $table->uuid('firm_id')->primary();
            $table->string('logo_path', 255)->nullable();
            $table->string('favicon_path', 255)->nullable();
            $table->char('color_primary', 7);
            $table->char('color_secondary', 7);
            $table->char('color_accent', 7);
            $table->string('typography', 32);
            $table->string('contact_email', 255)->nullable();
            $table->string('contact_phone', 40)->nullable();
            $table->string('contact_address', 255)->nullable();
            $table->string('public_copy', 500);
            $table->json('enabled_modules')->nullable();
            $table->timestamps();

            $table->foreign('firm_id')->references('id')->on('firms')->cascadeOnDelete();
        });

        Schema::create('firm_domains', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->foreignUuid('firm_id')->constrained('firms')->cascadeOnDelete();
            $table->string('hostname', 253)->unique();
            $table->timestamp('verified_at')->nullable();
            $table->boolean('is_primary')->default(false);
            $table->timestamps();
        });

        Schema::create('firm_verifications', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->foreignUuid('firm_id')->constrained('firms')->cascadeOnDelete();
            $table->foreignId('reviewer_user_id')->nullable()->constrained('users')->nullOnDelete();
            $table->string('decision', 32);
            $table->string('reason', 1000);
            $table->json('evidence_references')->nullable();
            $table->timestamp('decided_at');
            $table->timestamps();
        });

        Schema::create('firm_memberships', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->foreignUuid('firm_id')->constrained('firms')->cascadeOnDelete();
            $table->foreignId('user_id')->constrained('users')->cascadeOnDelete();
            $table->string('role', 64);
            $table->timestamps();

            $table->unique(['firm_id', 'user_id']);
        });

        Schema::create('firm_provisioning_events', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->foreignUuid('firm_id')->constrained('firms')->cascadeOnDelete();
            $table->foreignId('actor_user_id')->nullable()->constrained('users')->nullOnDelete();
            $table->string('event_type', 64);
            $table->string('provisioning_status', 32);
            $table->string('message', 255);
            $table->timestamps();

            $table->index(['firm_id', 'created_at']);
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('firm_provisioning_events');
        Schema::dropIfExists('firm_memberships');
        Schema::dropIfExists('firm_verifications');
        Schema::dropIfExists('firm_domains');
        Schema::dropIfExists('firm_public_profiles');
        Schema::dropIfExists('firms');
    }
};
