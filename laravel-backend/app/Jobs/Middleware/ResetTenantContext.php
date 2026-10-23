<?php

namespace App\Jobs\Middleware;

use App\Tenancy\TenantConnectionManager;

class ResetTenantContext
{
    public function handle(object $job, callable $next): void
    {
        $connections = app(TenantConnectionManager::class);
        $connections->clear();

        try {
            $next($job);
        } finally {
            $connections->clear();
        }
    }
}
