# Fleet provisioning and migration runbook (design)

[Home](../README.md) · [Operations](reliability-and-recovery.md) · [ADR-002](../database/ADR-002-multitenant-architecture.md) · [Provisioning (original)](../database/provisioning-and-migrations.md) · [Migration baseline](../database/migration-baseline.md) · [Recovery and offboarding](recovery-and-offboarding-runbook.md)

**Status:** design. No provisioner, fleet migrator, or control-plane code exists yet. The engine behaviors cited were executed on disposable MySQL 8.0.36 ([log](../../db/design/validation/mysql-8.0.36-validation.log)).

## Provisioning one firm
One `provisioning_operations` row is created per request, unique by `(OperationType, IdempotencyKey)`. A worker takes a lease (`LeaseOwner`, `LeaseExpiresAtUtc`) and runs the steps in order. Each step is **idempotent**: it checks whether the target state already exists before acting, then appends to `StepsCompletedJson`. On a retryable error it sets `WaitingRetry` with backoff. On a non-retryable error it sets `Failed` with an error code only, never a payload.

| # | Step | Idempotency check | Readiness gate before the next step | Partial-failure handling |
|---|---|---|---|---|
| 1 | Reserve firm row (`Provisioning`), slug, placement row (generated `mf_f_<hex>`) | Unique slug and `(ServerId, DatabaseName)` | Rows exist | Release on cancel |
| 2 | Choose a cell with spare connection budget and `MaxFirmDatabases` headroom | Placement already has a server | Server `Active` | — |
| 3 | `CREATE DATABASE IF NOT EXISTS` with an administrative credential | `information_schema.SCHEMATA` | Database exists and is empty or only partly provisioned **by this operation** (marker check) | Never adopt a non-empty database not created by this operation id |
| 4 | Create migration and runtime accounts; store passwords in the secret store; record `secret_references` | Account exists and the secret version matches | Login test for each account | Rotate if the secret was lost |
| 5 | Apply EF migrations as the migration account | `__EFMigrationsHistory` | History equals the target; schema diff equals the reference (drift check) | Resume from the last applied migration (see implicit commit below) |
| 6 | Privileged step: create triggers with `DEFINER` set to the migration account | `information_schema.TRIGGERS` | 5 triggers present | Re-create |
| 7 | Apply table-level runtime grants from the manifest | `SHOW GRANTS` comparison | Grant diff is empty; negative probes pass (other database denied, DDL denied) | Re-apply |
| 8 | Insert `firm_identity` (migration account) | Row exists with this FirmId | Runtime account reads the matching FirmId | Mismatch = stop, alert |
| 9 | File scope: bucket or container prefix, scoped credential, versioning on | Prefix exists | Write, read, and delete probe under the prefix; probe outside the prefix denied | — |
| 10 | Domain verification (DNS TXT with the token whose hash is stored) | `VerifiedAtUtc` set | Host resolves only after verification | Domain stays `Pending`; firm usable only on the platform default host |
| 11 | Seed firm configuration (no client data); first admin invitation (`firm_invitations`, token hash, expiry) | Membership exists | Invitation delivered | Re-send creates a new token and invalidates the old |
| 12 | Mark the placement and firm `Active`; set `firms.CurrentPlacementId` | Status | Health probe through the real API on the firm host | — |

Cancelling or failing before step 12 leaves the firm unroutable. Cleanup is a separate `Offboard` operation; a failed provisioning is never silently deleted.

## Fleet schema migration
- **Per-database history and lock.** Each firm database has its own `__EFMigrationsHistory`. The migrator takes `GET_LOCK('mf_migrate', 0)` on the target connection, or a lease row in `schema_migration_targets`, so two migrators can't run on one database.
- **Runs and waves.** A `schema_migration_runs` row names the target migration, the phase, `MaxConcurrency`, and `FailureBudget`. Wave 0 is the canary (synthetic firms first, then one consenting real firm). Each later wave grows. Any failure beyond the budget sets the run to `Halted` automatically.
- **Bounded concurrency.** The run limits concurrency per cell and in total, so migrations don't consume the cell connection budget or I/O.
- **Expand → backfill → verify → switch → contract:**
  - *Expand:* additive DDL only, compatible with the current release.
  - *Backfill:* in batches with throttling, idempotent.
  - *Verify:* counts and checksums against the source.
  - *Switch:* deploy the release that reads the new shape.
  - *Contract:* drop the old shape only after every firm has switched and a rollback window has passed.
- **App and schema compatibility.** Each release declares the oldest and newest migration it supports. Firms outside the range get a maintenance response, never queries against an unknown shape.
- **Drift detection.** After each migration and on a schedule, compare `information_schema` (columns, indexes, FKs, checks, triggers, grants) with the reference produced from a clean migration. Drift quarantines the target (`Quarantined`) and stops automated changes to it.
- **No forks.** One schema version per release. Per-firm configuration is data, not DDL.

### DDL and transactions on MySQL (verified)
- **DDL implicitly commits.** A `CREATE TABLE` inside an open transaction committed the earlier insert (E4). EF's `START TRANSACTION … COMMIT` around a migration doesn't make it atomic.
- **A migration failing midway leaves its earlier statements applied** (E5). Each migration therefore has to be written so it can be re-run: guarded DDL, or a small single-purpose migration, with a recorded resume point.
- **The MySQL documentation describes single DDL statements as atomic** in 8.0. That is not separately tested here.
- **Algorithms.** INSTANT works for `ADD COLUMN` (E6). `ADD INDEX` can't be INSTANT (E7, ERROR 1845) but runs INPLACE with `LOCK=NONE` (E8). Changing a column type needs COPY (E9, ERROR 1846), which means a table rebuild that blocks writes; use expand/contract instead. Migrations should state `ALGORITHM` and `LOCK` explicitly, so they fail rather than silently fall back to COPY.
- **Metadata locks.** Online DDL still waits for a metadata lock behind long transactions. Set `lock_wait_timeout` low for migration sessions and retry. This was not exercised.
- **Safe failure.** On error the target becomes `Failed`, the firm stays on its last good version (the app range must still include it), and the run halts at the failure budget. Never roll back destructively.

## Moving today's existing database to the first firm
This was not performed, because the instructions forbid modifying an existing non-test database. The procedure, for a later authorized step:
1. **Schema comparison.** Diff the existing database against `10-firm-current-schema.generated.sql` with `information_schema`, read-only.
2. **Baseline.** Follow the [migration baseline runbook](../database/migration-baseline.md) on a **copy** first.
3. **First-firm mapping.** Create the control-plane firm (`Maldanado and Benitez Law`, spelling pending confirmation), a placement pointing at the copied database, `firm_identity`, accounts, and grants.
4. **Backfill.** Seed `firm_users` from distinct `*ActorId` values; `matter_assignments` from the attorney and paralegal columns; `matter_number_sequences` from the current maximum.
5. **Validation.** Row counts and checksums per table before and after. Run the F1 suite against the routed copy. Write a reconciliation report for `calendar_entries.MatterReference`.
6. **Rehearsal.** Repeat end to end on a fresh copy, timed.
7. **Rollback.** Keep the original database untouched until sign-off. Rollback means pointing the placement back to the original (a new routing version) during a write freeze.

The development database contains demo data only. No real case data is imported by any step in this design.
