<?php

namespace App\Http\Middleware;

use App\Models\User;
use Closure;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Symfony\Component\HttpFoundation\Response;

class RequirePlatformPermission
{
    public function handle(Request $request, Closure $next, string $permission): Response
    {
        $user = $request->user();

        if (! $user instanceof User || ! $user->hasPlatformPermission($permission)) {
            return new JsonResponse([
                'message' => 'Forbidden.',
                'required_platform_permission' => $permission,
            ], Response::HTTP_FORBIDDEN);
        }

        return $next($request);
    }
}

