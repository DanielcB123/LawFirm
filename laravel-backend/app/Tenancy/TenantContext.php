<?php

namespace App\Tenancy;

use App\Models\Firm;

class TenantContext
{
    private ?Firm $firm = null;

    public function set(Firm $firm): void
    {
        $this->firm = $firm;
    }

    public function clear(): void
    {
        $this->firm = null;
    }

    public function isEstablished(): bool
    {
        return $this->firm instanceof Firm;
    }

    public function firm(): ?Firm
    {
        return $this->firm;
    }

    public function firmId(): ?string
    {
        return $this->firm?->id;
    }
}
