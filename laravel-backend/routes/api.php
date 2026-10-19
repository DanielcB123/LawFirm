<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\CalendarController;
use App\Http\Controllers\Api\ConsultationController;
use App\Http\Controllers\Api\ContactController;
use App\Http\Controllers\Api\IntakeController;
use App\Http\Controllers\Api\MatterController;
use App\Http\Controllers\Api\TaskController;
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
    Route::get('/calendar/entries', [CalendarController::class, 'index'])->middleware('permission:calendar.read');
    Route::post('/calendar/entries', [CalendarController::class, 'store'])->middleware('permission:calendar.manage');
    Route::post('/calendar/entries/{entry}/complete', [CalendarController::class, 'complete'])
        ->middleware('permission:calendar.manage');
    Route::get('/matters', [MatterController::class, 'index'])->middleware('permission:matters.read');
    Route::get('/tasks', [TaskController::class, 'index'])->middleware('permission:tasks.read');
    Route::post('/tasks', [TaskController::class, 'store'])->middleware('permission:tasks.manage');
    Route::post('/tasks/{task}/status', [TaskController::class, 'updateStatus'])->middleware('permission:tasks.manage');
});
