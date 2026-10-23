<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Tenancy\PublicBrandingResolver;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class PublicBrandingController extends Controller
{
    public function show(Request $request, PublicBrandingResolver $resolver): JsonResponse
    {
        return response()->json($resolver->payload($request));
    }
}
