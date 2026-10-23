<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('firm_public_profiles', function (Blueprint $table): void {
            $table->string('public_site_key', 80)->default('default_classic')->after('public_copy');
        });

        Schema::table('firm_domains', function (Blueprint $table): void {
            $table->timestamp('claimed_at')->nullable()->after('hostname');
            $table->string('verification_method', 32)->default('dns_txt')->after('claimed_at');
            $table->string('verification_token_hash', 64)->nullable()->after('verification_method');
            $table->timestamp('verification_expires_at')->nullable()->after('verification_token_hash');
            $table->timestamp('verification_used_at')->nullable()->after('verification_expires_at');
            $table->string('verification_last_error', 255)->nullable()->after('verification_used_at');
            $table->foreignId('verified_by_user_id')->nullable()->after('verified_at')->constrained('users')->nullOnDelete();
            $table->string('verified_source', 64)->nullable()->after('verified_by_user_id');
            $table->timestamp('revoked_at')->nullable()->after('verified_source');
            $table->timestamp('released_at')->nullable()->after('revoked_at');

            $table->index(['firm_id', 'verified_at']);
            $table->index(['hostname', 'verified_at', 'revoked_at']);
        });
    }

    public function down(): void
    {
        Schema::table('firm_domains', function (Blueprint $table): void {
            $table->dropConstrainedForeignId('verified_by_user_id');
            $table->dropIndex(['firm_id', 'verified_at']);
            $table->dropIndex(['hostname', 'verified_at', 'revoked_at']);
            $table->dropColumn([
                'claimed_at',
                'verification_method',
                'verification_token_hash',
                'verification_expires_at',
                'verification_used_at',
                'verification_last_error',
                'verified_source',
                'revoked_at',
                'released_at',
            ]);
        });

        Schema::table('firm_public_profiles', function (Blueprint $table): void {
            $table->dropColumn('public_site_key');
        });
    }
};
