<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\MatterRecord;
use App\Models\MatterTask;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class TaskController extends Controller
{
    public function index(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'status' => ['nullable', 'string', 'max:40'],
            'matterId' => ['nullable', 'uuid'],
        ]);

        $query = MatterTask::query()
            ->with('matterRecord')
            ->orderByRaw("CASE WHEN status = 'Completed' THEN 1 ELSE 0 END")
            ->orderBy('due_at_utc')
            ->orderByDesc('created_at');

        if (! empty($validated['status'])) {
            $query->where('status', $validated['status']);
        }

        if (! empty($validated['matterId'])) {
            $query->where('matter_record_id', $validated['matterId']);
        }

        $items = $query
            ->limit(300)
            ->get()
            ->map(fn (MatterTask $task): array => $this->toApiItem($task));

        return response()->json($items);
    }

    public function store(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'matterId' => ['required', 'uuid'],
            'title' => ['required', 'string', 'max:200'],
            'description' => ['nullable', 'string'],
            'priority' => ['nullable', 'in:Low,Normal,High,Urgent'],
            'assigneeActorId' => ['nullable', 'string', 'max:64'],
            'assigneeDisplayName' => ['nullable', 'string', 'max:120'],
            'dueAtUtc' => ['nullable', 'date'],
        ]);

        $actor = $request->user();
        if (! MatterRecord::query()->whereKey($validated['matterId'])->exists()) {
            return response()->json([
                'message' => 'The selected matter id is invalid.',
                'errors' => [
                    'matterId' => ['The selected matter id is invalid.'],
                ],
            ], 422);
        }
        $task = MatterTask::query()->create([
            'matter_record_id' => $validated['matterId'],
            'title' => $validated['title'],
            'description' => $validated['description'] ?? null,
            'status' => 'Open',
            'priority' => $validated['priority'] ?? 'Normal',
            'assignee_actor_id' => $validated['assigneeActorId'] ?? null,
            'assignee_display_name' => $validated['assigneeDisplayName'] ?? null,
            'due_at_utc' => $validated['dueAtUtc'] ?? null,
            'created_by_actor_id' => 'user-'.$actor->id,
            'created_by_display_name' => $actor->name,
        ]);
        $task->load('matterRecord');

        return response()->json($this->toApiItem($task), 201);
    }

    public function updateStatus(Request $request, string $taskId): JsonResponse
    {
        $validated = $request->validate([
            'status' => ['required', 'in:Open,InProgress,Waiting,Completed'],
        ]);

        $task = MatterTask::query()->findOrFail($taskId);
        $task->status = $validated['status'];
        $task->completed_at_utc = $validated['status'] === 'Completed' ? now() : null;
        $task->save();
        $task->load('matterRecord');

        return response()->json($this->toApiItem($task));
    }

    private function toApiItem(MatterTask $task): array
    {
        return [
            'id' => $task->id,
            'matterId' => $task->matter_record_id,
            'matterNumber' => $task->matterRecord?->matter_number,
            'matterTitle' => $task->matterRecord?->title,
            'title' => $task->title,
            'description' => $task->description,
            'status' => $task->status,
            'priority' => $task->priority,
            'assigneeActorId' => $task->assignee_actor_id,
            'assigneeDisplayName' => $task->assignee_display_name,
            'dueAtUtc' => $task->due_at_utc?->toISOString(),
            'completedAtUtc' => $task->completed_at_utc?->toISOString(),
            'createdByActorId' => $task->created_by_actor_id,
            'createdByDisplayName' => $task->created_by_display_name,
            'createdAtUtc' => $task->created_at?->toISOString(),
        ];
    }
}
