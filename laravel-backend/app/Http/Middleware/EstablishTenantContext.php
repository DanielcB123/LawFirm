<?php

namespace App\Http\Middleware;

use App\Tenancy\Exceptions\TenantAccessDeniedException;
use App\Tenancy\TenantConnectionManager;
use App\Tenancy\TenantResolver;
use Closure;
use Illuminate\Http\Request;
use Symfony\Component\HttpFoundation\Response;

class EstablishTenantContext
{
    public function __construct(
        private TenantResolver $resolver,
        private TenantConnectionManager $connections,
    ) {}

    public function handle(Request $request, Closure $next): Response
    {
        $this->connections->clear();

        try {
            $firm = $this->resolver->resolve($request);
            $this->connections->establish($firm);
        } catch (TenantAccessDeniedException $exception) {
            $this->connections->clear();

            return response()->json($this->payload($exception), $exception->status);
        }

        return $next($request);
    }

    public function terminate(Request $request, Response $response): void
    {
        $this->connections->clear();
    }

    /**
     * @return array<string, mixed>
     */
    private function payload(TenantAccessDeniedException $exception): array
    {
        $payload = [
            'message' => $exception->getMessage(),
        ];

        if ($exception->errorCode !== null) {
            $payload['code'] = $exception->errorCode;
        }

        if ($exception->firms !== []) {
            $payload['firms'] = $exception->firms;
        }

        return $payload;
    }
}
