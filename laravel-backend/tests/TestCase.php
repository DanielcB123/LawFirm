<?php

namespace Tests;

use App\Tenancy\TenantConnectionManager;
use Illuminate\Foundation\Testing\TestCase as BaseTestCase;

abstract class TestCase extends BaseTestCase
{
    protected function tearDown(): void
    {
        if ($this->app !== null) {
            $this->app->make(TenantConnectionManager::class)->clear();
        }

        foreach (glob(storage_path('framework/testing-tenants/*.sqlite')) ?: [] as $file) {
            if (is_string($file)) {
                @unlink($file);
            }
        }

        parent::tearDown();
    }
}
