<?php

namespace App\Tenancy;

use App\Enums\FirmProvisioningStatus;
use App\Models\Firm;
use Illuminate\Support\Facades\Log;
use Illuminate\Support\Str;
use Throwable;

class TenantFleetMigrator
{
    public function __construct(
        private TenantContext $context,
        private TenantConnectionManager $connections,
        private Contracts\RunsTenantMigrations $migrator,
        private FirmAuditRecorder $audit,
    ) {}

    /**
     * @return list<array{firmId: string, ok: bool, message: string}>
     */
    public function migrate(?string $firmId = null): array
    {
        if ($firmId !== null && ! Str::isUuid($firmId)) {
            return [[
                'firmId' => $firmId,
                'ok' => false,
                'message' => 'The firm option must be an internal firm id.',
            ]];
        }

        $query = Firm::query()
            ->where('provisioning_status', FirmProvisioningStatus::Ready)
            ->whereNotNull('schema_name')
            ->orderBy('id');

        if ($firmId !== null) {
            $query->whereKey($firmId);
        }

        $results = [];

        foreach ($query->get() as $firm) {
            $results[] = $this->migrateFirm($firm);
        }

        if ($firmId !== null && $results === []) {
            $results[] = [
                'firmId' => $firmId,
                'ok' => false,
                'message' => 'Firm is not eligible for tenant migration.',
            ];
        }

        return $results;
    }

    /**
     * @return array{firmId: string, ok: bool, message: string}
     */
    private function migrateFirm(Firm $firm): array
    {
        try {
            if (! is_string($firm->schema_name)) {
                throw new \RuntimeException('Tenant database is not selected.');
            }

            $this->connections->useSchema($firm->schema_name);
            $this->context->set($firm);
            $version = $this->migrator->migrate();
            $firm->forceFill([
                'schema_version' => $version,
                'provisioning_failure_reason' => null,
            ])->save();
            $this->audit->record($firm->refresh(), null, 'migration.ready', 'Tenant migrations completed.');

            return [
                'firmId' => $firm->id,
                'ok' => true,
                'message' => 'Tenant migrations completed.',
            ];
        } catch (Throwable $exception) {
            Log::warning('firm.migration.failed', [
                'firm_id' => $firm->id,
                'exception_class' => $exception::class,
            ]);
            $this->audit->record($firm->refresh(), null, 'migration.failed', 'Tenant schema migration did not complete.');

            return [
                'firmId' => $firm->id,
                'ok' => false,
                'message' => 'Tenant schema migration did not complete.',
            ];
        } finally {
            $this->connections->clear();
        }
    }
}
