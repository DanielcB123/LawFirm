# Migration baseline for existing databases

[Home](../README.md) · [Database](README.md) · [Provisioning and migrations](provisioning-and-migrations.md) · [ADR-001](ADR-001-tenancy.md)

Status: **Runbook written, not executed against any existing database.** No existing application database was inspected, modified, or migrated while writing it.

## What changed

Before 2026-10-12 the API created its schema at startup with `EnsureCreated` plus raw `CREATE TABLE IF NOT EXISTS` statements. The schema is now owned by EF Core migrations in `backend/Infrastructure/Persistence/Migrations/`:

| Migration | Contents |
|---|---|
| `20261012050714_Baseline` | The 16 tables that existed before (parties, conflicts, audit, calendar, intake, matters and children, consultations) |
| `20261012050741_MatterAccessGrants` | `matter_access_grants` with a foreign key to `matters`, a check constraint on grant type, and a stored generated column used by a unique index so only one unrevoked grant exists per matter and actor |

Startup behavior (`InitializeLawFirmPersistenceAsync` in `backend/Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`):

1. If the database has application tables but no `__EFMigrationsHistory`, startup **stops with an error** and changes nothing. This is the state of every database created by the old code.
2. If migrations are pending and `Database:ApplyMigrationsOnStartup` is false (the default in `appsettings.json`), startup stops with an error.
3. If it is true (set in `appsettings.Development.json` and in tests), pending migrations are applied.

Both refusal paths are covered by `SchemaSafetyTests` against MySQL 8.0.36.

## Option A (recommended for development): start a new empty database

Point `ConnectionStrings:LawFirm` (user-secrets) at a new, empty schema. Development startup applies both migrations and seeds the fictional demo data. The old database is left as it was and can be dropped by its owner later.

## Option B: baseline an existing database in place

Use this only for a database whose data must be kept. Every step is manual, done by a person with a migration-privileged account, and starts with a backup.

1. **Back up.** `mysqldump --single-transaction --routines --triggers <db> > backup.sql`, and confirm the dump restores into a scratch server.
2. **Get the expected schema.** From the repository root, with a placeholder connection string so the design-time tools can start (nothing is opened):
   `$env:ConnectionStrings__LawFirm="Server=design-time-only;Database=design_time_only"` then
   `dotnet tool run dotnet-ef migrations script 0 20261012050714_Baseline --project backend -o baseline.sql`.
3. **Compare.** For each of the 16 tables, compare `SHOW CREATE TABLE` on the existing database with `baseline.sql`: columns, types, nullability, indexes, foreign keys, and **column collations**. Tables created by the old raw DDL (`intake_*`, `matters`, `matter_*`, `consultation_requests`) declared GUID columns as `char(36)` with no collation, so they take the database default (typically `utf8mb4_0900_ai_ci`). The migrations use `ascii_general_ci`.
4. **Preflight the collation for the second migration.** Run:
   `SELECT COLLATION_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'matters' AND COLUMN_NAME = 'Id';`
   If the result is not `ascii_general_ci`, `20261012050741_MatterAccessGrants` **will fail** with MySQL error 3780 (incompatible foreign-key columns). This was reproduced on a throwaway MySQL 8.0.36 container with a synthetic `matters` table, not on a real database. Either align the collation of `matters.Id` and every column that references it (on a restored copy first, inside a maintenance window), or use Option A.
5. **Resolve any other drift** found in step 3 on a restored copy first. Do not proceed while differences remain.
6. **Record the baseline as applied** (this claims the existing tables match Baseline; it does not change them):
   `CREATE TABLE __EFMigrationsHistory (MigrationId varchar(150) NOT NULL, ProductVersion varchar(32) NOT NULL, PRIMARY KEY (MigrationId)) CHARACTER SET utf8mb4;`
   `INSERT INTO __EFMigrationsHistory VALUES ('20261012050714_Baseline', '8.0.13');`
7. **Apply the remaining migration** with the migration account, either by starting the API once with `Database:ApplyMigrationsOnStartup=true` or with `dotnet tool run dotnet-ef database update --project backend` and the real connection string supplied through an environment variable that is cleared afterwards.
8. **Verify.** `__EFMigrationsHistory` lists both migrations; `matter_access_grants` exists; the API starts with `ApplyMigrationsOnStartup=false`.

## After baselining: existing access

Existing matters have no grants. Assigned attorneys and paralegals keep access through the assignment columns, attorneys keep firm-wide access, and **admins and clients lose matter access until grants are created**. The development seeder only adds its client grant when `matter_access_grants` is empty.

## Unknowns

- Whether any existing development database has drift beyond the collation difference above has not been checked.
- Production migration privileges (separate migration account versus runtime account) are a decision for the multitenancy design; see [provisioning and migrations](provisioning-and-migrations.md).
