<?php

namespace Tests\Feature;

use App\Models\Firm;
use App\Models\Tenant\TenantWorkspaceNote;
use App\Models\User;
use App\Tenancy\FirmRegistry;
use App\Tenancy\FirmReviewService;
use App\Tenancy\SchemaName;
use App\Tenancy\TenantCredentialStore;
use App\Tenancy\TenantConnectionManager;
use App\Tenancy\TenantSchemaProvisioner;
use Illuminate\Support\Facades\Artisan;
use Illuminate\Support\Facades\DB;
use PDO;
use Tests\TestCase;
use Throwable;

class TenantMysqlProvisioningTest extends TestCase
{
    private ?PDO $root = null;

    private string $controlDatabase = '';

    /** @var list<string> */
    private array $tenantSchemas = [];

    /** @var list<string> */
    private array $runtimeUsers = [];

    protected function setUp(): void
    {
        parent::setUp();

        try {
            $this->root = new PDO('mysql:host=127.0.0.1;port=3307', 'root', 'root', [
                PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            ]);
        } catch (Throwable $exception) {
            $this->markTestSkipped('MySQL is not reachable on 127.0.0.1:3307.');
        }

        $this->controlDatabase = 'mf_ctl_'.bin2hex(random_bytes(8));

        $this->root->exec(
            'CREATE DATABASE `'.$this->controlDatabase.'` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci',
        );

        $mysql = [
            'driver' => 'mysql',
            'host' => '127.0.0.1',
            'port' => '3307',
            'database' => $this->controlDatabase,
            'username' => 'root',
            'password' => 'root',
            'charset' => 'utf8mb4',
            'collation' => 'utf8mb4_unicode_ci',
            'prefix' => '',
            'prefix_indexes' => true,
            'strict' => true,
            'engine' => null,
            'options' => [],
        ];

        config([
            'database.default' => 'mysql',
            'database.connections.mysql' => $mysql,
            'database.connections.provisioner' => $mysql,
            'database.connections.tenant' => array_merge($mysql, ['database' => '']),
            'tenancy.runtime_grant_host' => '%',
        ]);

        DB::purge('mysql');
        DB::purge('provisioner');
        DB::purge('tenant');

        if (! str_starts_with((string) config('database.connections.mysql.database'), 'mf_ctl_')) {
            $this->fail('Refusing to migrate an unexpected control database.');
        }

        Artisan::call('migrate', ['--force' => true]);
    }

    protected function tearDown(): void
    {
        foreach ($this->tenantSchemas as $schema) {
            try {
                SchemaName::assertValid($schema);
                $this->root?->exec('DROP DATABASE IF EXISTS '.SchemaName::quote($schema));
            } catch (Throwable) {
                // Cleanup must not hide the test result.
            }
        }

        foreach ($this->runtimeUsers as $username) {
            if (preg_match('/^mf_t_[a-f0-9]{16}$/', $username) === 1) {
                $this->root?->exec("DROP USER IF EXISTS '{$username}'@'%'");
            }
        }

        if ($this->controlDatabase !== '' && preg_match('/^mf_ctl_[a-f0-9]{16}$/', $this->controlDatabase) === 1) {
            $this->root?->exec('DROP DATABASE IF EXISTS `'.$this->controlDatabase.'`');
        }

        DB::disconnect('mysql');
        DB::disconnect('provisioner');
        DB::disconnect('tenant');

        parent::tearDown();
    }

    public function test_mysql_provisioning_creates_isolated_schemas_and_limited_grants(): void
    {
        $admin = User::factory()->create(['is_platform_administrator' => true]);
        $registry = app(FirmRegistry::class);
        $review = app(FirmReviewService::class);
        $provisioner = app(TenantSchemaProvisioner::class);

        $firmA = $registry->register('Alpha LLP', 'Alpha Counsel');
        $firmB = $registry->register('Beta LLP', 'Beta Counsel');
        $review->approve($firmA, $admin, 'Synthetic approval.', ['file:synthetic']);
        $review->approve($firmB, $admin, 'Synthetic approval.', ['file:synthetic']);
        $firmA = $provisioner->provision($firmA->refresh(), $admin);
        $firmB = $provisioner->provision($firmB->refresh(), $admin);
        $this->tenantSchemas = [(string) $firmA->schema_name, (string) $firmB->schema_name];

        $this->assertNotSame($firmA->schema_name, $firmB->schema_name);
        $this->assertSame('active', $firmA->lifecycle_status->value);
        $this->assertFalse($this->controlHasTable('firm_identity'));

        $connections = app(TenantConnectionManager::class);
        $connections->establish($firmA);
        TenantWorkspaceNote::query()->create([
            'title' => 'Alpha',
            'body' => 'Alpha only',
        ]);
        $connections->clear();
        $connections->establish($firmB);
        $this->assertSame(0, TenantWorkspaceNote::query()->count());
        $this->assertFalse(\Illuminate\Support\Facades\Schema::connection('tenant')->hasTable('firms'));
        $connections->clear();

        $credentials = app(TenantCredentialStore::class)->activeRuntimeCredential($firmA);
        $this->assertIsArray($credentials);
        $this->runtimeUsers[] = $credentials['username'];

        $betaCredentials = app(TenantCredentialStore::class)->activeRuntimeCredential($firmB);
        if (is_array($betaCredentials)) {
            $this->runtimeUsers[] = $betaCredentials['username'];
        }

        $alpha = new PDO(
            'mysql:host=127.0.0.1;port=3307;dbname='.$firmA->schema_name,
            $credentials['username'],
            $credentials['password'],
            [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION],
        );
        $this->assertSame('Alpha', $alpha->query('select title from tenant_workspace_notes')->fetchColumn());

        if ($credentials['username'] === (string) config('tenancy.runtime_username')) {
            $this->assertTrue((bool) config('tenancy.allow_runtime_shared_fallback'));

            return;
        }

        $deniedControlPlane = false;
        try {
            $control = new PDO(
                'mysql:host=127.0.0.1;port=3307;dbname='.$this->controlDatabase,
                $credentials['username'],
                $credentials['password'],
                [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION],
            );
            $control->query('select id from firms');
        } catch (Throwable) {
            $deniedControlPlane = true;
        }

        $this->assertTrue($deniedControlPlane);

        $deniedOtherTenant = false;
        try {
            $beta = new PDO(
                'mysql:host=127.0.0.1;port=3307;dbname='.$firmB->schema_name,
                $credentials['username'],
                $credentials['password'],
                [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION],
            );
            $beta->query('select id from tenant_workspace_notes');
        } catch (Throwable) {
            $deniedOtherTenant = true;
        }

        $this->assertTrue($deniedOtherTenant);
        $this->assertNotSame($firmA->schema_name, $firmB->schema_name);
    }

    private function controlHasTable(string $table): bool
    {
        $statement = $this->root?->prepare(
            'select count(*) from information_schema.tables where table_schema = ? and table_name = ?',
        );
        $statement?->execute([$this->controlDatabase, $table]);

        return (int) $statement?->fetchColumn() > 0;
    }
}
