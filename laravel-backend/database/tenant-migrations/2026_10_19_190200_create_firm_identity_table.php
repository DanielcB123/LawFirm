<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('firm_identity', function (Blueprint $table): void {
            $table->id();
            $table->uuid('firm_id')->unique();
            $table->unsignedTinyInteger('singleton')->default(1)->unique();
            $table->timestamp('created_at')->nullable();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('firm_identity');
    }
};
