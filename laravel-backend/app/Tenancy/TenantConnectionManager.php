<?php

namespace App\Tenancy;

use App\Models\Firm;
use App\Tenancy\Exceptions\TenantAccessDeniedException;
use Illuminate\Support\Facades\DB;
use InvalidArgumentException;

class TenantConnectionManager
{
    private string $baseTenantUsername;

    private string $baseTenantPassword;

    public function __construct(
        private TenantContext $context,
        private TenantCredentialStore $credentials,
    ) {
        $connection = (string) config('tenancy.tenant_connection');
        $this->baseTenantUsername = (string) config("database.connections.{$connection}.username", '');
        $this->baseTenantPassword = (string) config("database.connections.{$connection}.password", '');
    }

    /**
     * @param  array{username:string,password:string,host:string}|null  $credential
     */
    public function useSchema(string $schemaName, ?array $credential = null): void
    {
        SchemaName::assertValid($schemaName);
        $this->assertDistinctFromControlPlane($schemaName);

        $connection = (string) config('tenancy.tenant_connection');
        $driver = (string) config("database.connections.{$connection}.driver");
        $database = $driver === 'sqlite'
            ? $this->sqlitePath($schemaName)
            : $schemaName;

        config(["database.connections.{$connection}.database" => $database]);
        if ($credential !== null && $driver === 'mysql') {
            config([
                "database.connections.{$connection}.username" => $credential['username'],
                "database.connections.{$connection}.password" => $credential['password'],
            ]);
        }
        DB::purge($connection);
        DB::disconnect($connection);
    }

    public function establish(Firm $firm): void
    {
        if (! $firm->isUsable() || ! is_string($firm->schema_name)) {
            throw new TenantAccessDeniedException('Tenant is not available.');
        }

        $connection = (string) config('tenancy.tenant_connection');
        $driver = (string) config("database.connections.{$connection}.driver");
        $credential = null;
        if ($driver === 'mysql') {
            $credential = $this->credentials->activeRuntimeCredential($firm);
            if ($credential === null) {
                throw new TenantAccessDeniedException('Tenant is not available.');
            }
        }

        $this->useSchema($firm->schema_name, $credential);
        $this->context->set($firm);
        $this->assertIdentity($firm);
    }

    public function clear(): void
    {
        $connection = (string) config('tenancy.tenant_connection');
        $driver = (string) config("database.connections.{$connection}.driver");
        $placeholder = $driver === 'sqlite' ? ':memory:' : '';

        config([
            "database.connections.{$connection}.database" => $placeholder,
            "database.connections.{$connection}.username" => $this->baseTenantUsername,
            "database.connections.{$connection}.password" => $this->baseTenantPassword,
        ]);
        DB::purge($connection);
        DB::disconnect($connection);
        $this->context->clear();
    }

    /**
     * @return array{username:string,password:string,host:string}
     */
    public function provisionerCredential(): array
    {
        $connection = (string) config('tenancy.provisioner_connection');

        return [
            'username' => (string) config("database.connections.{$connection}.username", ''),
            'password' => (string) config("database.connections.{$connection}.password", ''),
            'host' => (string) config("database.connections.{$connection}.host", ''),
        ];
    }

    public function sqlitePath(string $schemaName): string
    {
        SchemaName::assertValid($schemaName);

        return rtrim((string) config('tenancy.sqlite_schema_directory'), '\\/').DIRECTORY_SEPARATOR.$schemaName.'.sqlite';
    }

    public function createSqliteFile(string $schemaName): void
    {
        $directory = (string) config('tenancy.sqlite_schema_directory');
        if (! is_dir($directory) && ! mkdir($directory, 0777, true) && ! is_dir($directory)) {
            throw new InvalidArgumentException('Tenant schema directory could not be created.');
        }

        $path = $this->sqlitePath($schemaName);
        if (! is_file($path) && file_put_contents($path, '') === false) {
            throw new InvalidArgumentException('Tenant schema file could not be created.');
        }
    }

    private function assertDistinctFromControlPlane(string $schemaName): void
    {
        $control = (string) config('database.connections.'.config('database.default').'.database');
        if ($schemaName === $control || $this->sqlitePath($schemaName) === $control) {
            throw new InvalidArgumentException('Tenant schema must be distinct from the control plane.');
        }
    }

    private function assertIdentity(Firm $firm): void
    {
        $identity = DB::connection((string) config('tenancy.tenant_connection'))
            ->table('firm_identity')
            ->first();

        if ($identity === null || (string) $identity->firm_id !== (string) $firm->id) {
            $this->clear();

            throw new TenantAccessDeniedException('Tenant is not available.');
        }
    }
}
