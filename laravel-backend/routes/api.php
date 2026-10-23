<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\CalendarController;
use App\Http\Controllers\Api\ConsultationController;
use App\Http\Controllers\Api\ContactController;
use App\Http\Controllers\Api\FirmOnboardingController;
use App\Http\Controllers\Api\IntakeController;
use App\Http\Controllers\Api\MatterController;
use App\Http\Controllers\Api\PlatformFirmController;
use App\Http\Controllers\Api\PublicBrandingController;
use App\Http\Controllers\Api\TaskController;
use App\Http\Controllers\Api\TenantUserController;
use App\Http\Controllers\Api\TenantWorkspaceController;
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
Route::post('/onboarding/register', [FirmOnboardingController::class, 'register'])->middleware('throttle:12,1');
Route::get('/public/branding', [PublicBrandingController::class, 'show']);
Route::middleware('public-tenant')->group(function (): void {
    Route::get('/consultations/available-slots', [ConsultationController::class, 'availableSlots']);
    Route::post('/consultations', [ConsultationController::class, 'store']);
});

Route::middleware(['auth:sanctum', 'platform-admin'])->prefix('platform')->group(function (): void {
    Route::get('/firms', [PlatformFirmController::class, 'index'])->middleware('platform-permission:platform.firms.read');
    Route::post('/firms', [PlatformFirmController::class, 'store'])->middleware('platform-permission:platform.firms.create');
    Route::post('/firms/{firm}/review', [PlatformFirmController::class, 'review'])->middleware('platform-permission:platform.firms.review.finalize');
    Route::post('/firms/{firm}/suspend', [PlatformFirmController::class, 'suspend'])->middleware('platform-permission:platform.firms.lifecycle.manage');
    Route::post('/firms/{firm}/reactivate', [PlatformFirmController::class, 'reactivate'])->middleware('platform-permission:platform.firms.lifecycle.manage');
    Route::post('/firms/{firm}/provision', [PlatformFirmController::class, 'provision'])->middleware('platform-permission:platform.firms.provision');
    Route::post('/firms/{firm}/memberships', [PlatformFirmController::class, 'storeMembership'])->middleware('platform-permission:platform.firms.memberships.manage');
    Route::post('/firms/{firm}/domains', [PlatformFirmController::class, 'storeDomain'])->middleware('platform-permission:platform.firms.domains.manage');
    Route::post('/firms/{firm}/domains/{domain}/reissue-challenge', [PlatformFirmController::class, 'reissueDomainChallenge'])->middleware('platform-permission:platform.firms.domains.manage');
    Route::post('/firms/{firm}/domains/{domain}/verify', [PlatformFirmController::class, 'verifyDomain'])->middleware('platform-permission:platform.firms.domains.finalize');
    Route::post('/firms/{firm}/domains/{domain}/revoke', [PlatformFirmController::class, 'revokeDomain'])->middleware('platform-permission:platform.firms.domains.finalize');
    Route::put('/firms/{firm}/branding', [PlatformFirmController::class, 'updateBranding'])->middleware('platform-permission:platform.firms.branding.manage');
    Route::post('/firms/{firm}/verifications', [PlatformFirmController::class, 'storeVerification'])->middleware('platform-permission:platform.firms.verifications.record');
});

Route::middleware(['auth:sanctum', 'tenant'])->prefix('tenant')->group(function (): void {
    Route::get('/context', [TenantWorkspaceController::class, 'context']);
    Route::get('/notes', [TenantWorkspaceController::class, 'notes']);
    Route::post('/notes', [TenantWorkspaceController::class, 'storeNote']);
    Route::get('/firms/{firm}/notes', [TenantWorkspaceController::class, 'notes']);
    Route::get('/contacts', [ContactController::class, 'index'])->middleware('permission:contacts.read');
    Route::post('/contacts', [ContactController::class, 'store'])->middleware('permission:contacts.manage');
    Route::get('/intake', [IntakeController::class, 'index'])->middleware('permission:intake.read');
    Route::post('/intake', [IntakeController::class, 'store'])->middleware('permission:intake.manage');
    Route::get('/consultations', [ConsultationController::class, 'index'])->middleware('permission:consultations.read');
    Route::post('/consultations/{consultationId}/status', [ConsultationController::class, 'updateStatus'])
        ->middleware('permission:consultations.manage');
    Route::get('/calendar/entries', [CalendarController::class, 'index'])->middleware('permission:calendar.read');
    Route::post('/calendar/entries', [CalendarController::class, 'store'])->middleware('permission:calendar.manage');
    Route::post('/calendar/entries/{entryId}/complete', [CalendarController::class, 'complete'])
        ->middleware('permission:calendar.manage');
    Route::get('/matters', [MatterController::class, 'index'])->middleware('permission:matters.read');
    Route::get('/tasks', [TaskController::class, 'index'])->middleware('permission:tasks.read');
    Route::post('/tasks', [TaskController::class, 'store'])->middleware('permission:tasks.manage');
    Route::post('/tasks/{taskId}/status', [TaskController::class, 'updateStatus'])->middleware('permission:tasks.manage');
    Route::get('/users', [TenantUserController::class, 'index'])->middleware('permission:users.manage');
    Route::post('/users', [TenantUserController::class, 'store'])->middleware('permission:users.manage');
    Route::put('/users/{user}/membership', [TenantUserController::class, 'updateMembership'])->middleware('permission:users.manage');
});

Route::middleware('auth:sanctum')->group(function (): void {
    Route::get('/identity/me', [AuthController::class, 'me']);
    Route::post('/onboarding/applications', [FirmOnboardingController::class, 'submit'])->middleware('throttle:20,1');
    Route::get('/onboarding/applications/current', [FirmOnboardingController::class, 'current']);
    Route::put('/onboarding/applications/current', [FirmOnboardingController::class, 'update'])->middleware('throttle:20,1');
    Route::post('/onboarding/applications/current/fast-track', [FirmOnboardingController::class, 'fastTrack'])->middleware('throttle:10,1');
});
