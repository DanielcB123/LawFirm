<?php

namespace App\Http\Middleware;

use App\Tenancy\PublicTenantResolver;
use App\Tenancy\TenantConnectionManager;
use Closure;
use Illuminate\Http\Request;
use Symfony\Component\HttpFoundation\Response;

class EstablishPublicTenantContext
{
    public function __construct(
        private PublicTenantResolver $resolver,
        private TenantConnectionManager $connections,
    ) {}

    public function handle(Request $request, Closure $next): Response
    {
        $this->connections->clear();
        $firm = $this->resolver->resolve($request);

        if ($firm === null) {
            return response()->json([
                'message' => 'Resource not found.',
            ], 404);
        }

        $this->connections->establish($firm);

        return $next($request);
    }

    public function terminate(Request $request, Response $response): void
    {
        $this->connections->clear();
    }
}
