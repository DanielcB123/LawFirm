<?php

namespace App\Tenancy;

use App\Tenancy\Contracts\RunsTenantMigrations;
use Illuminate\Database\Migrations\Migrator;
use RuntimeException;

class LaravelTenantMigrator implements RunsTenantMigrations
{
    public function __construct(private Migrator $migrator) {}

    public function migrate(): string
    {
        $path = (string) config('tenancy.tenant_migrations_path');
        $this->assertPathIsTenantOnly($path);
        $this->assertSelectedDatabaseIsDistinct();

        return $this->migrator->usingConnection((string) config('tenancy.tenant_connection'), function () use ($path): string {
            if (! $this->migrator->repositoryExists()) {
                $this->migrator->getRepository()->createRepository();
            }

            $this->migrator->run([$path]);
            $ran = $this->migrator->getRepository()->getRan();

            if ($ran === []) {
                return '';
            }

            $latest = end($ran);

            return is_string($latest) ? $latest : '';
        });
    }

    private function assertPathIsTenantOnly(string $path): void
    {
        $controlPath = database_path('migrations');
        if ($path === '' || realpath($path) === false || realpath($path) === realpath($controlPath)) {
            throw new RuntimeException('Tenant migration path is not configured.');
        }
    }

    private function assertSelectedDatabaseIsDistinct(): void
    {
        $tenantDatabase = (string) config('database.connections.'.config('tenancy.tenant_connection').'.database');
        $controlDatabase = (string) config('database.connections.'.config('database.default').'.database');

        if ($tenantDatabase === '' || $tenantDatabase === $controlDatabase) {
            throw new RuntimeException('Tenant database is not selected.');
        }
    }
}
