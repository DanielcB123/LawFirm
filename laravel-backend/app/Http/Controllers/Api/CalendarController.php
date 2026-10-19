<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\CalendarEntry;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class CalendarController extends Controller
{
    public function index(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'fromUtc' => ['nullable', 'date'],
            'toUtc' => ['nullable', 'date'],
            'entryType' => ['nullable', 'string', 'max:20'],
            'includeCompleted' => ['nullable', 'boolean'],
        ]);

        $query = CalendarEntry::query()->orderBy('scheduled_at_utc');

        if (! empty($validated['fromUtc'])) {
            $query->where('scheduled_at_utc', '>=', $validated['fromUtc']);
        }

        if (! empty($validated['toUtc'])) {
            $query->where('scheduled_at_utc', '<=', $validated['toUtc']);
        }

        if (! empty($validated['entryType'])) {
            $query->where('entry_type', $validated['entryType']);
        }

        if (($validated['includeCompleted'] ?? false) === false) {
            $query->where('is_completed', false);
        }

        $items = $query
            ->limit(500)
            ->get()
            ->map(fn (CalendarEntry $entry): array => $this->toApiItem($entry));

        return response()->json($items);
    }

    public function store(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'title' => ['required', 'string', 'max:200'],
            'entryType' => ['required', 'in:Task,Event,Deadline'],
            'deadlineType' => ['required', 'in:OrdinaryTask,InternalTarget,CourtOrdered,LegallySignificant'],
            'scheduledAtUtc' => ['required', 'date'],
            'timeZone' => ['required', 'string', 'max:80'],
            'matterReference' => ['nullable', 'string', 'max:64'],
            'sourceReference' => ['nullable', 'string', 'max:120'],
            'reminderOffsetsMinutes' => ['nullable', 'array'],
            'reminderOffsetsMinutes.*' => ['integer', 'min:0', 'max:10080'],
        ]);

        $actor = $request->user();
        $reminders = collect($validated['reminderOffsetsMinutes'] ?? [])
            ->filter(fn (int $item): bool => $item >= 0)
            ->unique()
            ->sort()
            ->values()
            ->all();

        $entry = CalendarEntry::query()->create([
            'title' => $validated['title'],
            'entry_type' => $validated['entryType'],
            'deadline_type' => $validated['deadlineType'],
            'owner_actor_id' => 'user-'.$actor->id,
            'owner_display_name' => $actor->name,
            'scheduled_at_utc' => $validated['scheduledAtUtc'],
            'time_zone' => $validated['timeZone'],
            'matter_reference' => $validated['matterReference'] ?? null,
            'source_reference' => $validated['sourceReference'] ?? null,
            'reminder_offsets_json' => json_encode($reminders, JSON_THROW_ON_ERROR),
        ]);

        return response()->json($this->toApiItem($entry), 201);
    }

    public function complete(Request $request, CalendarEntry $entry): JsonResponse
    {
        $validated = $request->validate([
            'isCompleted' => ['nullable', 'boolean'],
        ]);

        $isCompleted = $validated['isCompleted'] ?? true;
        $entry->is_completed = $isCompleted;
        $entry->completed_at_utc = $isCompleted ? now() : null;
        $entry->save();

        return response()->json($this->toApiItem($entry));
    }

    private function toApiItem(CalendarEntry $entry): array
    {
        return [
            'id' => $entry->id,
            'title' => $entry->title,
            'entryType' => $entry->entry_type,
            'deadlineType' => $entry->deadline_type,
            'ownerActorId' => $entry->owner_actor_id,
            'ownerDisplayName' => $entry->owner_display_name,
            'scheduledAtUtc' => $entry->scheduled_at_utc?->toISOString(),
            'timeZone' => $entry->time_zone,
            'matterReference' => $entry->matter_reference,
            'sourceReference' => $entry->source_reference,
            'isCompleted' => $entry->is_completed,
            'completedAtUtc' => $entry->completed_at_utc?->toISOString(),
            'reminderOffsetsMinutes' => json_decode((string) ($entry->reminder_offsets_json ?? '[]'), true),
            'createdAtUtc' => $entry->created_at?->toISOString(),
            'updatedAtUtc' => $entry->updated_at?->toISOString(),
        ];
    }
}
