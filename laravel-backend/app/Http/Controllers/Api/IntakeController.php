<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\IntakeRecord;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class IntakeController extends Controller
{
    public function index(): JsonResponse
    {
        $items = IntakeRecord::query()
            ->orderByDesc('created_at')
            ->limit(200)
            ->get()
            ->map(fn (IntakeRecord $record): array => $this->toListItem($record));

        return response()->json($items);
    }

    public function store(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'prospectiveClientName' => ['required', 'string', 'max:200'],
            'prospectiveClientEmail' => ['nullable', 'email', 'max:256'],
            'prospectiveClientPhone' => ['nullable', 'string', 'max:64'],
            'practiceArea' => ['required', 'string', 'max:80'],
            'intakeType' => ['required', 'string', 'max:40'],
            'ownerActorId' => ['required', 'string', 'max:64'],
            'ownerDisplayName' => ['required', 'string', 'max:120'],
            'nextAction' => ['nullable', 'string', 'max:200'],
            'nextActionDueAtUtc' => ['nullable', 'date'],
            'referralSource' => ['nullable', 'string', 'max:80'],
            'conflictCheckId' => ['nullable', 'uuid'],
            'notesSummary' => ['nullable', 'string'],
        ]);

        $record = IntakeRecord::query()->create([
            'prospective_client_name' => $validated['prospectiveClientName'],
            'prospective_client_email' => $validated['prospectiveClientEmail'] ?? null,
            'prospective_client_phone' => $validated['prospectiveClientPhone'] ?? null,
            'practice_area' => $validated['practiceArea'],
            'intake_type' => $validated['intakeType'],
            'stage' => 'New',
            'owner_actor_id' => $validated['ownerActorId'],
            'owner_display_name' => $validated['ownerDisplayName'],
            'next_action' => $validated['nextAction'] ?? null,
            'next_action_due_at_utc' => $validated['nextActionDueAtUtc'] ?? null,
            'referral_source' => $validated['referralSource'] ?? null,
            'conflict_check_id' => $validated['conflictCheckId'] ?? null,
            'notes_summary' => $validated['notesSummary'] ?? null,
        ]);

        return response()->json($this->toListItem($record), 201);
    }

    private function toListItem(IntakeRecord $record): array
    {
        return [
            'id' => $record->id,
            'prospectiveClientName' => $record->prospective_client_name,
            'practiceArea' => $record->practice_area,
            'intakeType' => $record->intake_type,
            'stage' => $record->stage,
            'ownerActorId' => $record->owner_actor_id,
            'ownerDisplayName' => $record->owner_display_name,
            'nextAction' => $record->next_action,
            'nextActionDueAtUtc' => $record->next_action_due_at_utc?->toISOString(),
            'referralSource' => $record->referral_source,
            'conflictCheckId' => $record->conflict_check_id,
            'approvedConflictDecisionId' => $record->approved_conflict_decision_id,
            'createdAtUtc' => $record->created_at?->toISOString(),
        ];
    }
}
