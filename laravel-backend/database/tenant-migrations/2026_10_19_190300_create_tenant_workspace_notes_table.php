<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('tenant_workspace_notes', function (Blueprint $table): void {
            $table->uuid('id')->primary();
            $table->string('title', 120);
            $table->string('body', 2000);
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('tenant_workspace_notes');
    }
};
