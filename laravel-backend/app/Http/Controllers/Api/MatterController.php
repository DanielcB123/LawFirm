<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\MatterRecord;
use Illuminate\Http\JsonResponse;

class MatterController extends Controller
{
    public function index(): JsonResponse
    {
        $items = MatterRecord::query()
            ->withCount([
                'tasks as open_task_count' => fn ($query) => $query->where('status', '!=', 'Completed'),
            ])
            ->orderByDesc('created_at')
            ->limit(200)
            ->get()
            ->map(fn (MatterRecord $record): array => $this->toListItem($record));

        return response()->json($items);
    }

    private function toListItem(MatterRecord $record): array
    {
        return [
            'id' => $record->id,
            'matterNumber' => $record->matter_number,
            'title' => $record->title,
            'practiceArea' => $record->practice_area,
            'stage' => $record->stage,
            'status' => $record->status,
            'responsibleAttorneyActorId' => $record->responsible_attorney_actor_id,
            'responsibleAttorneyDisplayName' => $record->responsible_attorney_display_name,
            'paralegalActorId' => $record->paralegal_actor_id,
            'paralegalDisplayName' => $record->paralegal_display_name,
            'openTaskCount' => $record->open_task_count ?? 0,
            'createdAtUtc' => $record->created_at?->toISOString(),
        ];
    }
}
