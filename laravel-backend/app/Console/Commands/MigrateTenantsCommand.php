<?php

namespace App\Console\Commands;

use App\Tenancy\TenantFleetMigrator;
use Illuminate\Console\Command;

class MigrateTenantsCommand extends Command
{
    protected $signature = 'tenants:migrate {--firm= : Internal firm id to migrate}';

    protected $description = 'Run tenant-schema migrations for firms whose schemas are already provisioned';

    public function handle(TenantFleetMigrator $migrator): int
    {
        $firm = $this->option('firm');
        $firmId = is_string($firm) && $firm !== '' ? $firm : null;
        $results = $migrator->migrate($firmId);
        $failed = false;

        if ($results === []) {
            $this->info('No eligible firms.');

            return self::SUCCESS;
        }

        foreach ($results as $result) {
            $status = $result['ok'] ? 'ok' : 'failed';
            $this->line($result['firmId'].' '.$status.' '.$result['message']);
            $failed = $failed || ! $result['ok'];
        }

        return $failed ? self::FAILURE : self::SUCCESS;
    }
}
