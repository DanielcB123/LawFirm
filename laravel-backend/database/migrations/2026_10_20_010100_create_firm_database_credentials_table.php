<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('firm_database_credentials', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->foreignUuid('firm_id')->constrained('firms')->cascadeOnDelete();
            $table->string('username', 64);
            $table->text('password_encrypted');
            $table->string('grant_host', 255)->default('%');
            $table->timestamp('rotated_at')->nullable();
            $table->timestamp('revoked_at')->nullable();
            $table->timestamps();

            $table->unique(['firm_id', 'username']);
            $table->index(['firm_id', 'revoked_at']);
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('firm_database_credentials');
    }
};
