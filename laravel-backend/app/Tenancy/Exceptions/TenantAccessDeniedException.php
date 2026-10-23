<?php

namespace App\Tenancy\Exceptions;

use RuntimeException;

class TenantAccessDeniedException extends RuntimeException
{
    /**
     * @param  list<array{firmId: string, displayName: string, lifecycleStatus: string, role: string}>  $firms
     */
    public function __construct(
        string $message,
        public readonly int $status = 403,
        public readonly ?string $errorCode = null,
        public readonly array $firms = [],
    ) {
        parent::__construct($message);
    }
}
