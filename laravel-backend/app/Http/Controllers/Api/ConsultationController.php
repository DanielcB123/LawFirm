<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\ConsultationRequest;
use Carbon\CarbonImmutable;
use Illuminate\Http\Request;
use Illuminate\Http\JsonResponse;

class ConsultationController extends Controller
{
    public function availableSlots(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'startDate' => ['nullable', 'date_format:Y-m-d'],
            'days' => ['nullable', 'integer', 'min:1', 'max:60'],
        ]);

        $days = (int) ($validated['days'] ?? 42);
        $start = isset($validated['startDate'])
            ? CarbonImmutable::createFromFormat('Y-m-d', $validated['startDate'], 'UTC')->startOfDay()
            : CarbonImmutable::now('UTC')->startOfDay();
        $end = $start->addDays($days);

        $booked = ConsultationRequest::query()
            ->whereIn('status', ['Requested', 'Scheduled'])
            ->whereBetween('preferred_at_utc', [$start, $end])
            ->pluck('preferred_at_utc')
            ->map(fn ($value) => CarbonImmutable::parse((string) $value, 'UTC')->format('Y-m-d H:i'))
            ->all();

        $bookedLookup = array_flip($booked);
        $slots = collect();

        foreach (range(0, $days - 1) as $offset) {
            $day = $start->addDays($offset);
            if ($day->isWeekend()) {
                continue;
            }

            foreach ([9, 10, 11, 13, 14, 15, 16] as $hour) {
                $slotStart = $day->setTime($hour, 0);
                $slotKey = $slotStart->format('Y-m-d H:i');
                if (isset($bookedLookup[$slotKey])) {
                    continue;
                }

                $slots->push([
                    'startsAtUtc' => $slotStart->toISOString(),
                    'endsAtUtc' => $slotStart->addMinutes(30)->toISOString(),
                ]);
            }
        }

        return response()->json($slots->values()->all());
    }

    public function store(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'fullName' => ['required', 'string', 'max:200'],
            'email' => ['required', 'email', 'max:256'],
            'phone' => ['nullable', 'string', 'max:64'],
            'practiceArea' => ['required', 'string', 'max:80'],
            'message' => ['nullable', 'string'],
            'preferredAtUtc' => ['required', 'date'],
            'timeZone' => ['required', 'string', 'max:80'],
        ]);

        $item = ConsultationRequest::query()->create([
            'full_name' => $validated['fullName'],
            'email' => $validated['email'],
            'phone' => $validated['phone'] ?? null,
            'practice_area' => $validated['practiceArea'],
            'message' => $validated['message'] ?? null,
            'preferred_at_utc' => $validated['preferredAtUtc'],
            'time_zone' => $validated['timeZone'],
            'status' => 'Requested',
        ]);

        return response()->json($this->toApiItem($item), 201);
    }

    public function index(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'status' => ['nullable', 'string', 'max:40'],
        ]);

        $query = ConsultationRequest::query()->orderByDesc('created_at');

        if (! empty($validated['status'])) {
            $query->where('status', $validated['status']);
        }

        $items = $query->limit(200)->get()->map(fn (ConsultationRequest $item): array => $this->toApiItem($item));

        return response()->json($items->values()->all());
    }

    public function updateStatus(Request $request, ConsultationRequest $consultation): JsonResponse
    {
        $validated = $request->validate([
            'status' => ['required', 'string', 'max:40'],
            'assignedToActorId' => ['nullable', 'string', 'max:64'],
            'assignedToDisplayName' => ['nullable', 'string', 'max:120'],
            'internalNotes' => ['nullable', 'string'],
        ]);

        $consultation->fill([
            'status' => $validated['status'],
            'assigned_to_actor_id' => $validated['assignedToActorId'] ?? null,
            'assigned_to_display_name' => $validated['assignedToDisplayName'] ?? null,
            'internal_notes' => $validated['internalNotes'] ?? null,
        ]);
        $consultation->save();

        return response()->json($this->toApiItem($consultation));
    }

    private function toApiItem(ConsultationRequest $item): array
    {
        return [
            'id' => $item->id,
            'fullName' => $item->full_name,
            'email' => $item->email,
            'phone' => $item->phone,
            'practiceArea' => $item->practice_area,
            'message' => $item->message,
            'preferredAtUtc' => $item->preferred_at_utc?->toISOString(),
            'timeZone' => $item->time_zone,
            'status' => $item->status,
            'assignedToActorId' => $item->assigned_to_actor_id,
            'assignedToDisplayName' => $item->assigned_to_display_name,
            'internalNotes' => $item->internal_notes,
            'createdAtUtc' => $item->created_at?->toISOString(),
            'updatedAtUtc' => $item->updated_at?->toISOString(),
        ];
    }
}
