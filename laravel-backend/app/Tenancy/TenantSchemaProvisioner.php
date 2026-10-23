<?php

namespace App\Tenancy;

use App\Enums\FirmLifecycleStatus;
use App\Enums\FirmProvisioningStatus;
use App\Models\Firm;
use App\Models\Tenant\FirmIdentity;
use App\Models\User;
use App\Tenancy\Contracts\RunsTenantMigrations;
use App\Tenancy\Exceptions\FirmLifecycleException;
use App\Tenancy\Exceptions\TenantProvisioningException;
use Illuminate\Database\QueryException;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;
use Throwable;

class TenantSchemaProvisioner
{
    public function __construct(
        private TenantContext $context,
        private TenantConnectionManager $connections,
        private RunsTenantMigrations $migrator,
        private FirmAuditRecorder $audit,
        private TenantCredentialStore $credentials,
    ) {}

    public function provision(Firm $firm, ?User $actor = null): Firm
    {
        $this->assertProvisionable($firm);
        $firm = $this->reserveSchema($firm, $actor);
        $schemaName = $firm->schema_name;

        if (! is_string($schemaName) || $schemaName === '') {
            throw new TenantProvisioningException('Tenant schema database operation did not complete.');
        }

        try {
            $this->createDatabase($schemaName);
            $this->connections->useSchema($schemaName, $this->connections->provisionerCredential());
            $this->context->set($firm);
            $version = $this->migrator->migrate();
            $this->seedIdentity($firm);
            $this->grantRuntimeAccess($firm, $schemaName);

            $firm->forceFill([
                'lifecycle_status' => FirmLifecycleStatus::Active,
                'provisioning_status' => FirmProvisioningStatus::Ready,
                'schema_version' => $version,
                'provisioning_failure_reason' => null,
                'activated_at' => $firm->activated_at ?? now(),
            ])->save();

            $firm = $firm->refresh();
            $this->audit->record($firm, $actor, 'provisioning.ready', 'Tenant schema is ready.');

            return $firm;
        } catch (TenantProvisioningException $exception) {
            $this->markFailed($firm, $actor, $exception->getMessage(), $exception);

            throw $exception;
        } catch (Throwable $exception) {
            $reason = $this->sanitizedReason($exception);
            $this->markFailed($firm, $actor, $reason, $exception);

            throw new TenantProvisioningException($reason);
        } finally {
            $this->connections->clear();
        }
    }

    public function createDatabase(string $schemaName): void
    {
        SchemaName::assertValid($schemaName);
        $driver = (string) config('database.connections.'.config('tenancy.provisioner_connection').'.driver');

        if ($driver === 'sqlite') {
            $this->connections->createSqliteFile($schemaName);

            return;
        }

        if ($driver !== 'mysql') {
            throw new TenantProvisioningException('Tenant schema creation is not available for this database.');
        }

        $quoted = SchemaName::quote($schemaName);
        DB::connection((string) config('tenancy.provisioner_connection'))->statement(
            "CREATE DATABASE IF NOT EXISTS {$quoted} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
        );
    }

    private function assertProvisionable(Firm $firm): void
    {
        if ($firm->lifecycle_status === FirmLifecycleStatus::Approved) {
            return;
        }

        if ($firm->lifecycle_status === FirmLifecycleStatus::Active
            && in_array($firm->provisioning_status, [
                FirmProvisioningStatus::Ready,
                FirmProvisioningStatus::Failed,
                FirmProvisioningStatus::Provisioning,
            ], true)) {
            return;
        }

        throw new FirmLifecycleException('The firm cannot be provisioned from its current state.');
    }

    private function reserveSchema(Firm $firm, ?User $actor): Firm
    {
        if (! is_string($firm->schema_name) || $firm->schema_name === '') {
            $firm->schema_name = SchemaName::generate();
        }

        SchemaName::assertValid($firm->schema_name);
        $firm->forceFill([
            'provisioning_status' => FirmProvisioningStatus::Provisioning,
            'provisioning_failure_reason' => null,
        ])->save();

        $firm = $firm->refresh();
        $this->audit->record($firm, $actor, 'provisioning.started', 'Tenant provisioning started.');

        return $firm;
    }

    private function seedIdentity(Firm $firm): void
    {
        $existing = FirmIdentity::query()->first();

        if ($existing instanceof FirmIdentity && $existing->firm_id !== $firm->id) {
            throw new TenantProvisioningException('Tenant schema identity does not match the firm.');
        }

        if ($existing instanceof FirmIdentity) {
            return;
        }

        try {
            FirmIdentity::query()->create([
                'firm_id' => $firm->id,
                'singleton' => 1,
            ]);
        } catch (QueryException) {
            $existing = FirmIdentity::query()->first();
            if (! $existing instanceof FirmIdentity || $existing->firm_id !== $firm->id) {
                throw new TenantProvisioningException('Tenant schema identity does not match the firm.');
            }
        }
    }

    private function grantRuntimeAccess(Firm $firm, string $schemaName): void
    {
        $connection = (string) config('tenancy.provisioner_connection');
        if ((string) config("database.connections.{$connection}.driver") !== 'mysql') {
            return;
        }

        $credential = $this->credentials->issueRuntimeCredential($firm);
        $username = $credential['username'];
        $password = $credential['password'];
        $host = $credential['host'];

        if (preg_match('/^[A-Za-z0-9_]{1,32}$/', $username) !== 1) {
            throw new TenantProvisioningException('Tenant schema database operation did not complete.');
        }

        if (preg_match('/^[A-Za-z0-9_%.:-]{1,255}$/', $host) !== 1) {
            throw new TenantProvisioningException('Tenant schema database operation did not complete.');
        }

        $quotedSchema = SchemaName::quote($schemaName);
        $quotedUser = "'".$username."'@'".$host."'";
        $safePassword = str_replace("'", "''", $password);

        try {
            DB::connection($connection)->statement(
                "CREATE USER IF NOT EXISTS {$quotedUser} IDENTIFIED BY '{$safePassword}'",
            );
            DB::connection($connection)->statement(
                "ALTER USER {$quotedUser} IDENTIFIED BY '{$safePassword}'",
            );
            DB::connection($connection)->statement(
                "GRANT SELECT, INSERT, UPDATE, DELETE ON {$quotedSchema}.* TO {$quotedUser}",
            );
        } catch (QueryException $exception) {
            if ((bool) config('tenancy.allow_runtime_shared_fallback', false) !== true) {
                throw $exception;
            }

            $fallbackUser = (string) config('tenancy.runtime_username');
            $fallbackHost = (string) config('tenancy.runtime_grant_host');
            $fallbackPassword = (string) config('database.connections.'.config('tenancy.tenant_connection').'.password', '');

            if ($fallbackUser === '') {
                throw $exception;
            }

            $this->credentials->storeFallbackCredential($firm, $fallbackUser, $fallbackPassword, $fallbackHost);
        }
    }

    private function markFailed(Firm $firm, ?User $actor, string $reason, Throwable $exception): void
    {
        $firm->forceFill([
            'provisioning_status' => FirmProvisioningStatus::Failed,
            'provisioning_failure_reason' => mb_substr($reason, 0, 255),
        ])->save();

        $firm->refresh();
        $this->audit->record($firm, $actor, 'provisioning.failed', $reason);

        Log::warning('firm.provisioning.failed', [
            'firm_id' => $firm->id,
            'exception_class' => $exception::class,
            'exception_code' => (string) $exception->getCode(),
        ]);
    }

    private function sanitizedReason(Throwable $exception): string
    {
        if ($exception instanceof QueryException) {
            return 'Tenant schema database operation did not complete.';
        }

        $message = $exception->getMessage();
        if (str_contains($message, 'Tenant migration path') || str_contains($message, 'Tenant database is not selected')) {
            return 'Tenant schema migration did not complete.';
        }

        if ($exception instanceof \RuntimeException && str_contains($message, 'migration')) {
            return 'Tenant schema migration did not complete.';
        }

        return 'Tenant schema migration did not complete.';
    }
}
