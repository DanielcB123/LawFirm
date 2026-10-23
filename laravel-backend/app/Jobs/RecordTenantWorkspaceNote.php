<?php

namespace App\Jobs;

use App\Jobs\Middleware\ResetTenantContext;
use App\Models\Firm;
use App\Models\Tenant\TenantWorkspaceNote;
use App\Tenancy\Exceptions\TenantAccessDeniedException;
use App\Tenancy\TenantConnectionManager;
use App\Tenancy\TenantContext;
use Illuminate\Contracts\Queue\ShouldQueue;
use Illuminate\Foundation\Queue\Queueable;

class RecordTenantWorkspaceNote implements ShouldQueue
{
    use Queueable;

    public function __construct(
        public string $firmId,
        public string $body,
    ) {}

    /**
     * @return list<object>
     */
    public function middleware(): array
    {
        return [new ResetTenantContext];
    }

    public function handle(TenantConnectionManager $connections, TenantContext $context): void
    {
        try {
            $firm = Firm::query()->find($this->firmId);
            if (! $firm instanceof Firm) {
                throw new TenantAccessDeniedException('Tenant is not available.');
            }

            $connections->establish($firm);
            TenantWorkspaceNote::query()->create([
                'title' => 'Queued note',
                'body' => mb_substr($this->body, 0, 2000),
            ]);
        } finally {
            $context->clear();
            $connections->clear();
        }
    }
}
