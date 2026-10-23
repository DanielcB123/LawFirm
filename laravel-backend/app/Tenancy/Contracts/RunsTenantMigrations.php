<?php

namespace App\Tenancy\Contracts;

interface RunsTenantMigrations
{
    /**
     * Run pending tenant-schema migrations and return the latest migration name.
     */
    public function migrate(): string;
}
