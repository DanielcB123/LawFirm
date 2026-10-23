<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Tenant\TenantWorkspaceNote;
use App\Tenancy\TenantContext;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class TenantWorkspaceController extends Controller
{
    public function context(TenantContext $context): JsonResponse
    {
        $firm = $context->firm();

        return response()->json([
            'firmId' => $firm?->id,
            'displayName' => $firm?->display_name,
            'lifecycleStatus' => $firm?->lifecycle_status?->value,
        ]);
    }

    public function notes(): JsonResponse
    {
        $notes = TenantWorkspaceNote::query()
            ->orderBy('created_at')
            ->get(['id', 'title', 'body', 'created_at']);

        return response()->json([
            'notes' => $notes,
        ]);
    }

    public function storeNote(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'title' => ['required', 'string', 'max:120', 'regex:/^[^<>]*$/'],
            'body' => ['required', 'string', 'max:2000', 'regex:/^[^<>]*$/'],
        ]);

        $note = TenantWorkspaceNote::query()->create($validated);

        return response()->json([
            'id' => $note->id,
            'title' => $note->title,
            'body' => $note->body,
        ], 201);
    }
}
