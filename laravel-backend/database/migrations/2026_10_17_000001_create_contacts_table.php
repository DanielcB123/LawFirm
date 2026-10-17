<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('contacts', function (Blueprint $table) {
            $table->id();
            $table->string('display_name', 200);
            $table->enum('contact_type', ['Person', 'Organization']);
            $table->string('primary_email', 256)->nullable();
            $table->string('primary_phone', 64)->nullable();
            $table->timestamps();
            $table->index('display_name');
            $table->index('contact_type');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('contacts');
    }
};
