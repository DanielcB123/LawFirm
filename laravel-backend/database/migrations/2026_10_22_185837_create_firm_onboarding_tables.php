<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('firms', function (Blueprint $table): void {
            $table->foreignId('applicant_user_id')->nullable()->after('slug')->constrained('users')->nullOnDelete();
            $table->unsignedInteger('onboarding_revision')->default(1)->after('applicant_user_id');
        });

        Schema::create('firm_onboarding_applications', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->foreignUuid('firm_id')->unique()->constrained('firms')->cascadeOnDelete();
            $table->foreignId('applicant_user_id')->constrained('users')->cascadeOnDelete();
            $table->string('applicant_name', 200);
            $table->string('applicant_email', 255);
            $table->string('business_phone', 40)->nullable();
            $table->unsignedInteger('revision')->default(1);
            $table->boolean('dev_fast_tracked')->default(false);
            $table->timestamp('submitted_at')->nullable();
            $table->timestamp('last_material_change_at')->nullable();
            $table->timestamp('fast_tracked_at')->nullable();
            $table->timestamps();

            $table->index(['applicant_user_id', 'created_at']);
        });

        Schema::create('firm_onboarding_verifications', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->foreignUuid('firm_id')->constrained('firms')->cascadeOnDelete();
            $table->unsignedInteger('revision');
            $table->string('verification_type', 64);
            $table->string('status', 32)->default('pending');
            $table->string('method', 64)->default('manual');
            $table->foreignId('reviewer_user_id')->nullable()->constrained('users')->nullOnDelete();
            $table->string('notes', 255)->nullable();
            $table->timestamp('reviewed_at')->nullable();
            $table->timestamps();

            $table->unique(['firm_id', 'revision', 'verification_type'], 'fov_firm_rev_type_uq');
            $table->index(['firm_id', 'revision', 'status'], 'fov_firm_rev_status_idx');
        });

        Schema::create('firm_onboarding_reviews', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->foreignUuid('firm_id')->constrained('firms')->cascadeOnDelete();
            $table->unsignedInteger('revision');
            $table->foreignId('reviewer_user_id')->constrained('users')->cascadeOnDelete();
            $table->string('decision', 32);
            $table->string('reason', 1000);
            $table->timestamp('decided_at');
            $table->timestamps();

            $table->unique(['firm_id', 'revision', 'reviewer_user_id'], 'for_firm_rev_reviewer_uq');
            $table->index(['firm_id', 'revision', 'decision'], 'for_firm_rev_decision_idx');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('firm_onboarding_reviews');
        Schema::dropIfExists('firm_onboarding_verifications');
        Schema::dropIfExists('firm_onboarding_applications');

        Schema::table('firms', function (Blueprint $table): void {
            $table->dropConstrainedForeignId('applicant_user_id');
            $table->dropColumn('onboarding_revision');
        });
    }
};
