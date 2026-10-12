# Operational safeguards, recovery, and offboarding (design)

[Home](../README.md) · [Operations](reliability-and-recovery.md) · [ADR-002](../database/ADR-002-multitenant-architecture.md) · [Fleet runbook](fleet-provisioning-and-migration-runbook.md) · [Threat model](../security/multitenant-threat-model.md)

**Status:** design. **No RPO or RTO has been measured or agreed.** Every target below is a placeholder marked *unverified* until a rehearsal records actual numbers on the chosen host.

## Connections and pools
- **Per-firm pool**, keyed by the firm runtime connection string: `Maximum Pool Size` 3–5, `Minimum Pool Size` 0, `Connection Idle Timeout` short (for example 60 s) so idle firms release their connections, and `Connection Lifetime` bounded so rotated credentials take effect. These defaults come from MySqlConnector's connection options [S20] and need re-checking against the version in use.
- **Per-cell budget.** Each app instance holds a semaphore per cell, sized to its share of `database_servers.ConnectionBudget`. A request waits briefly for a lease, then gets 503 with `Retry-After`. That protects the server's `max_connections` (151 on the default container, measured; the managed value must be measured too).
- **Per-firm cap on the server:** `MAX_USER_CONNECTIONS` on each runtime account (validated: error 1226 when exceeded, R4).
- **Idle-pool lifecycle.** Pools for firms with no traffic drain to zero. Pools for retired placements are cleared explicitly (`ClearPool`) when the routing version changes.

## Noisy-neighbor controls
- Per-firm request rate limits at the API.
- Per-firm connection caps (above).
- Per-firm job concurrency in the dispatcher: one firm's backlog can't monopolize workers (round-robin across firms).
- Statement timeouts for interactive queries (`MAX_EXECUTION_TIME` hint or a session setting for SELECT).
- Heavy exports and reports run on background workers, with lower concurrency.
- Move a firm with sustained heavy usage to its own cell (see firm movement below).

## Monitoring, with the firm dimension wherever possible
Per cell:
- slow query log (with digests);
- `performance_schema` statement digests;
- lock waits (`sys.innodb_lock_waits`) and deadlocks (`SHOW ENGINE INNODB STATUS` sampling, or the error log with `innodb_print_all_deadlocks`);
- connections used against the budget, and pool-wait time;
- replication or backup status, and disk.

Per firm:
- schema version against the release range, and drift status;
- outbox pending count and oldest age;
- dead-letter count;
- webhook unprocessed age;
- storage growth.

Fleet:
- migration run status, failures per wave, quarantined targets.

None of the metric labels may contain names or narratives; firm id and codes only.

## Backup and recovery semantics
- **Server-level:** the managed provider's automated backups and point-in-time restore. These restore a whole server to a new server [S21][S22], so they protect against total loss, but **a database per firm doesn't give per-firm point-in-time restore**.
- **Per-firm logical dumps:** a scheduled `mysqldump --single-transaction --routines --triggers mf_f_<hex>` (or the provider equivalent), encrypted, stored per firm. These serve as an independent path and as offboarding exports. Triggers have to be included and re-created with the correct `DEFINER`.
- **Object storage:** versioning enabled per firm prefix; deletes are versioned (soft) until retention allows a purge.
- **Search indexes** are rebuildable from SQL, so they are not a backup source.

## Restoring one firm (procedure)
1. Freeze writes for the affected firm: placement `ReadOnly`, outbox dispatch paused for that firm only.
2. Restore the server backup to an **isolated** server at the chosen point. Access is limited to the operators running the restore, and the copy is destroyed afterwards with evidence.
3. Extract **only** the affected firm database. Verify `firm_identity.FirmId`, row counts, and checksums against expectations.
4. Load it into a staging database on the target cell (new name, new routing version). Re-apply grants from the manifest and re-create triggers through the privileged step.
5. Reconcile external state after the restore point:
   - object versions: restore object versions as of the point; keep objects under a legal hold;
   - the outbox: events sent after the point must not be re-sent; re-derive from webhook receipts;
   - the provider ledger: payments and refunds that happened at the provider after the point are re-imported from provider records, not invented;
   - search: rebuild.
6. **Keep current revocations and holds.** Control-plane memberships, session epochs, support-access revocations, domain revocations, and the firm's preservation holds are **not** rolled back. Re-apply any hold created after the restore point to the restored data.
7. Switch the placement under the write freeze (the routing version increments). Verify through the API. Unfreeze.
8. Record the measured RPO (data lost) and RTO (time to restore) for this rehearsal. **Targets until then: unverified.**

Acceptance requires restoring synthetic firm A while synthetic firm B on the same cell stays unaffected (AT-REST-01: designed, not executed).

## Replay safeguards
- Webhook receipts are unique per provider account, so a replay is a no-op (L15).
- Journal entries are unique by idempotency key (L8).
- Outbox consumers are idempotent by `EventId`.
- After a restore, the dispatcher starts from the restored outbox but skips `EventId`s recorded as delivered by the downstream systems.

## Firm movement (cell to cell)
1. Create a new placement row (`RoutingVersion + 1`, `Status = Provisioning`) on the target cell. Provision the database, accounts, grants, and triggers.
2. Copy: a logical dump and load, or provider replication, while the firm is in `ReadOnly`. Write coordination is a freeze; no dual-writes.
3. Verify counts, checksums, `firm_identity` (update `PlacementRoutingVersion` with the migration account), and API health.
4. Switch `firms.CurrentPlacementId`, which invalidates the resolver caches. Clear the old pools.
5. **Delayed jobs.** Jobs created before the move carry the firm id, not a placement, and re-resolve the placement when they run. Jobs in flight during the freeze retry after the switch.
6. **Rollback:** point back to the old placement (another routing version) as long as no writes have reached the new one. Keep the old database read-only until the rollback window ends, then retire it.

The freeze length is not yet measured, so **no zero-downtime claim is made**.

## Exports, retention, offboarding, and deletion evidence
- **Export:** a per-firm logical dump plus an object-prefix archive plus a manifest (tables, row counts, checksums, object list with hashes), delivered through a secure channel. It is a firm-admin action requiring MFA, and it is audited.
- **Retention:** the firm's schedule plus preservation holds. Holds block purge. An offboarding with active holds waits for a documented release by the firm.
- **Offboarding states:** `Offboarding` (read-only; export offered) → contractual retention period → `Retired`.
- **Deletion:**
  - `DROP DATABASE`;
  - drop the accounts;
  - delete the object prefix and its versions;
  - delete the search indexes;
  - revoke the secrets;
  - remove the domains;
  - let backups expire per the provider's retention.

  The deletion evidence record (control-plane audit) lists each artifact, its timestamp, and the operator. It notes plainly that **server backups keep the data until they expire**; that limit is stated to the firm, not hidden.
