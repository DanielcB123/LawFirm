<?php

namespace App\Console\Commands;

use App\Models\Firm;
use App\Tenancy\TenantConnectionManager;
use Illuminate\Console\Command;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Str;

class BackfillTenantWorkspaceDataCommand extends Command
{
    protected $signature = 'tenants:backfill-workspace
        {--firm= : Firm UUID that owns the legacy rows}
        {--apply : Execute writes. Without this flag the command only reports.}
        {--map-all-to-firm : Explicitly map all legacy rows to --firm}';

    protected $description = 'Dry-run and optionally copy legacy shared workspace rows into a tenant schema.';

    /**
     * @var array<string, string>
     */
    private array $tables = [
        'contacts' => 'id',
        'intake_records' => 'id',
        'consultation_requests' => 'id',
        'matter_records' => 'id',
        'matter_tasks' => 'id',
        'calendar_entries' => 'id',
    ];

    public function handle(TenantConnectionManager $connections): int
    {
        $firmId = (string) $this->option('firm');
        $apply = (bool) $this->option('apply');
        $mapAll = (bool) $this->option('map-all-to-firm');

        if (! Str::isUuid($firmId)) {
            $this->error('A valid --firm UUID is required.');

            return self::FAILURE;
        }

        $firm = Firm::query()->find($firmId);
        if (! $firm instanceof Firm || ! $firm->isUsable()) {
            $this->error('The selected firm is not active and provisioned.');

            return self::FAILURE;
        }

        $controlConnection = (string) config('tenancy.control_connection');
        $rowsToCopy = [];
        $ambiguousTables = [];

        $this->line('Workspace backfill summary for firm '.$firm->id.' ('.$firm->display_name.')');
        foreach ($this->tables as $table => $pk) {
            $sourceRows = DB::connection($controlConnection)->table($table)->orderBy($pk)->get()->map(
                fn ($row): array => (array) $row,
            )->all();

            $sourceCount = count($sourceRows);
            $rowsToCopy[$table] = $sourceRows;
            if ($sourceCount > 0 && ! $mapAll) {
                $ambiguousTables[] = $table;
            }

            $this->line(sprintf('- %s: source=%d', $table, $sourceCount));
        }

        if (! empty($ambiguousTables)) {
            $this->warn('Legacy rows are not tenant-tagged. Use --map-all-to-firm to provide explicit ownership mapping.');
            $this->warn('Unmapped tables: '.implode(', ', $ambiguousTables));

            return self::FAILURE;
        }

        if (! $apply) {
            $this->info('Dry run complete. Re-run with --apply to perform idempotent upserts.');

            return self::SUCCESS;
        }

        $connections->establish($firm);

        try {
            foreach ($this->tables as $table => $pk) {
                $rows = $rowsToCopy[$table];
                if ($rows === []) {
                    continue;
                }

                foreach (array_chunk($rows, 250) as $chunk) {
                    DB::connection((string) config('tenancy.tenant_connection'))
                        ->table($table)
                        ->upsert($chunk, [$pk], array_keys($chunk[0]));
                }

                $copied = count($rows);
                $this->line(sprintf('- %s: copied=%d', $table, $copied));
            }
        } finally {
            $connections->clear();
        }

        $this->info('Backfill apply completed. Re-running is safe and idempotent.');

        return self::SUCCESS;
    }
}
