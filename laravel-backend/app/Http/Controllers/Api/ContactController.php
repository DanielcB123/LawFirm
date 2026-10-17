<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Contact;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Validation\Rule;

class ContactController extends Controller
{
    public function index(Request $request): JsonResponse
    {
        $query = Contact::query()->orderBy('display_name');
        $search = trim((string) $request->query('q', ''));

        if ($search !== '') {
            $query->where(function ($builder) use ($search): void {
                $builder
                    ->where('display_name', 'like', "%{$search}%")
                    ->orWhere('primary_email', 'like', "%{$search}%")
                    ->orWhere('primary_phone', 'like', "%{$search}%");
            });
        }

        return response()->json($query->limit(100)->get());
    }

    public function store(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'display_name' => ['required', 'string', 'max:200'],
            'contact_type' => ['required', Rule::in(['Person', 'Organization'])],
            'primary_email' => ['nullable', 'email', 'max:256'],
            'primary_phone' => ['nullable', 'string', 'max:64'],
        ]);

        $contact = Contact::create($validated);

        return response()->json($contact, 201);
    }
}
