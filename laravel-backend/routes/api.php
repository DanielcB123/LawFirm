<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\ConsultationController;
use App\Http\Controllers\Api\ContactController;
use App\Http\Controllers\Api\IntakeController;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Route;

Route::get('/health', function () {
    $database = 'down';

    try {
        DB::select('select 1');
        $database = 'up';
    } catch (\Throwable) {
        $database = 'down';
    }

    return response()->json([
        'status' => 'ok',
        'service' => 'laravel-backend',
        'database' => $database,
    ]);
});

Route::post('/auth/login', [AuthController::class, 'login']);
Route::get('/consultations/available-slots', [ConsultationController::class, 'availableSlots']);
Route::post('/consultations', [ConsultationController::class, 'store']);

Route::middleware('auth:sanctum')->group(function (): void {
    Route::get('/identity/me', [AuthController::class, 'me']);
    Route::get('/contacts', [ContactController::class, 'index'])->middleware('permission:contacts.read');
    Route::post('/contacts', [ContactController::class, 'store'])->middleware('permission:contacts.manage');
    Route::get('/intake', [IntakeController::class, 'index'])->middleware('permission:intake.read');
    Route::post('/intake', [IntakeController::class, 'store'])->middleware('permission:intake.manage');
    Route::get('/consultations', [ConsultationController::class, 'index'])->middleware('permission:consultations.read');
    Route::post('/consultations/{consultation}/status', [ConsultationController::class, 'updateStatus'])
        ->middleware('permission:consultations.manage');
});
