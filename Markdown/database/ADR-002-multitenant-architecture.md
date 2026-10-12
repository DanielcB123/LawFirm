# ADR-002: Multi-firm architecture for MatterForge

[Home](../README.md) · [Database](README.md) · [ADR-001](ADR-001-tenancy.md) · [Data dictionary](data-dictionary.md) · [Query and index spec](query-and-index-spec.md) · [Threat model](../security/multitenant-threat-model.md) · [SQL artifacts](../../db/design/README.md)

**Status:** Proposed, 2026-10-12. It refines [ADR-001](ADR-001-tenancy.md) and keeps ADR-001's decision: a database per law firm. It replaces ADR-001's "Connections and cost" section and adds placement cells, connection budgets, and the privileged trigger step. ADR-001 is not edited except for a pointer to this ADR. **Implementation status: design only.** Nothing here is implemented in application code. The SQL was executed only against a disposable MySQL 8.0.36 container with synthetic data ([validation log](../../db/design/validation/mysql-8.0.36-validation.log)).

**Decision owners:** the product founder and the technical owner. Hosting-dependent parts wait on a provider choice (see [open decisions](../delivery/open-decisions.md)).

## Vocabulary on MySQL
- **Schema and database are synonyms.** `CREATE SCHEMA` is `CREATE DATABASE` [S19]. "Schema per firm" and "database per firm" are the same option, so this ADR says *database per firm*.
- **Database separation** is a namespace boundary on one server. Rows of two firms never share a table, and grants can name one database.
- **Permissions** decide which account can touch which database, table, or column. Separation only becomes isolation when each runtime account is granted exactly one firm's database. The [validation](../../db/design/validation/mysql-8.0.36-validation.log) checked this as G1–G15.
- **Shared server resources** are still shared between separated databases: CPU, buffer pool, redo log, `max_connections`, I/O, the binary log, and backups. A noisy firm on a shared server affects the others.
- **Dedicated instances** separate those resources too, and give the firm independent backup, restore, maintenance windows, and version timing. They cost the most.

## Options compared

| | A. Shared tables (`FirmId` column) | B. Database per firm, common schema | C. Hybrid (pooled + dedicated) |
|---|---|---|---|
| Isolation mechanism | Application filters only. MySQL has no row-level security. EF Core global query filters are application code that `IgnoreQueryFilters()` and raw SQL bypass | Account grants per database, checked by the server | Both, depending on tier |
| Cross-firm reference safety | Composite FKs `(FirmId, x)` reject cross-firm references (validated, P1). FirmId must also be immutable, or an update silently re-parents rows when local ids overlap (validated, P3) | No cross-firm rows can exist | Both |
| Read authorization | Every query must filter. A composite FK doesn't restrict reads (validated, P2) | Wrong-firm reads fail with error 1142 (validated, G1) | Both |
| Schema migration | One database | N databases, fleet rollout | Both paths, one schema version |
| Single-firm export, restore, delete | Filtered extraction across every table; deletion evidence is harder | `mysqldump` of one database; `DROP DATABASE` | Per tier |
| Connections | One pool per app instance | One pool per firm per instance (per-firm credentials) | Both |
| Changes to today's code | Add FirmId to 17 tables, composite keys and FKs everywhere, and filters on every query | None to existing tables; add routing and a firm context | Both, and the pooled tier needs FirmId everywhere |
| Unit cost at many small firms | Lowest | Higher: per-database overhead and operations | Medium |

## Challenge to the starting recommendation
The starting recommendation was shared code, a minimal control plane, a database per firm with automated migrations, a trusted placement catalog, and scoped files, jobs, caches, and search. Arguments against it:

1. **Operational load scales with firm count.** Every migration, backup check, drift scan, and credential rotation runs N times. At 1–10 firms it can be handled by hand. Past that it needs the fleet automation in the [provisioning and migration runbook](../operations/fleet-provisioning-and-migration-runbook.md). **Database per firm without automated fleet operations is not viable beyond a handful of firms.**
2. **Connections grow multiplicatively.** MySqlConnector keeps one pool per distinct connection string [S20]. Per-firm credentials mean one pool per firm on each app instance. The disposable server had `max_connections = 151` (measured; a managed tier will differ).
3. **Point-in-time restore is server-wide.** Managed point-in-time restore usually restores a whole server [S21][S22]. Database-per-firm makes extracting one firm easy, but not restoring one firm natively.
4. **It's expensive to reverse into a pooled tier.** Today's tables have no FirmId. Adding a pooled tier later means adding FirmId and composite keys to every table, which is a large migration. A schema with FirmId everywhere from the start would keep the hybrid option cheap, but it would add a constraint the database can't enforce inside a single-firm database, plus migration cost now.
5. **Triggers need privilege.** The money invariants rely on triggers. With binary logging on (the default), a migration account without SUPER can't create triggers (validated, ERROR 1419). Provisioning needs a privileged step, or the server parameter `log_bin_trust_function_creators = 1`.

Arguments for it, from repository evidence:
- The current schema is already a single-firm model ([audit](../architecture/current-codebase-audit.md)). Database per firm needs **no change to existing tables**. Model A would touch every table, query, and test.
- MySQL has no row-level security. Under model A, one missed filter, raw query, or `IgnoreQueryFilters()` call exposes another firm's privileged legal data. Under model B the server refuses the read (G1, G2), and the application authorization added in F1 remains the second layer.
- Legal-hold, export, and offboarding duties are scoped by firm, and a database boundary keeps that evidence simple.
- The expected near-term fleet is small: one real firm and synthetic test firms.

**Result:** the recommendation stands, with the refinements below. Database per firm is the right model while the fleet is small and the data is highly confidential. The strongest alternative is **C (hybrid)**, with a pooled tier for very small firms, if the measured triggers listed at the end fire.

## Decision
1. **One application release** serves all firms (ASP.NET Core API plus React SPA). No per-firm code forks or schema forks.
2. **Control-plane database** (`mf_control`) holds firms, verified domains, identity references, memberships, servers, placements with routing versions, secret references, provisioning operations, migration runs, subscriptions, support-access requests, and the control audit log ([DDL](../../db/design/01-control-plane.sql)). It holds no legal narratives, client contacts, documents, medical records, or client money.
3. **One database per firm**, named `mf_f_<32 hex>`. The platform generates the name and a CHECK constraint enforces the pattern. Names are never accepted from a request. Every firm database has the same schema: the existing EF schema plus the [required additions](../../db/design/11-firm-schema-required.sql), and later the [future structures](../../db/design/12-firm-schema-future.sql).
4. **Placement cells.** A *cell* is one MySQL server (`database_servers`) with a measured `MaxConnections`, a reserved `ConnectionBudget`, and a placement-policy `MaxFirmDatabases`. Firms are spread across cells by the trusted placement catalog. Spreading firms across servers is distinct from MySQL table partitioning (see the [index spec](query-and-index-spec.md#partitioning)). A dedicated server is a cell holding one firm (`Tier = Dedicated`).
5. **Accounts per firm database:**
   - a runtime account with **table-level** grants generated from a manifest;
   - a migration account with DDL rights on that database only;
   - triggers created by a privileged provisioning step with `DEFINER` set to the migration account.

   Database-level grants are not used for runtime accounts, because a table-level `REVOKE` can't carve exceptions out of them (validated, G16: ERROR 1147).
6. **Firm identity self-check.** Each firm database has a one-row `firm_identity` that the runtime account can only read (G8). After opening each connection lease, the application compares `FirmId` and `PlacementRoutingVersion` with the resolved placement, and fails closed on a mismatch.
7. **Connection budgets** (see capacity):
   - per-firm pools stay small (`Maximum Pool Size` 3–5, `Minimum Pool Size` 0, short idle timeout);
   - a per-cell semaphore in each app instance caps that instance's total leases to its share of `ConnectionBudget`;
   - `MAX_USER_CONNECTIONS` per runtime account bounds one firm's share on the server (validated, R4: error 1226).
8. **Cell affinity at scale.** Beyond the first few cells, app instances are grouped per cell so that each instance holds pools only for firms in its cell. This is a routing decision at the edge and needs no code fork.
9. **Out-of-database scoping.** Every artifact outside a firm database carries the firm id: object-storage prefix (`firm_placements.StoragePrefix`), job and outbox envelopes, cache keys (`firm:{id}:`), search index names, telemetry dimensions, and webhook account mappings. Jobs re-resolve the placement and re-check authorization when they run.
10. **No FirmId column on firm-owned rows** in firm databases (ADR-001's decision, kept). The cost of reversing this is recorded under reconsideration triggers.

## Capacity assumptions and scenarios
These are **assumptions to be replaced by measurements**, not results. The only numbers measured here come from a synthetic fixture: about 506 MB of data plus indexes for the "medium" synthetic firm (5,000 matters, 100,000 tasks, 300,000 messages, 60,000 document metadata rows with 90,000 versions, no file binaries), `max_connections = 151` on the default container, and schema creation of 73 tables in 4–6 s on an empty database. Real narratives, attachments, and audit volume will differ, and tiny fixtures do not show scale.

Connection formula, per cell:

\[ C_{cell} \approx \sum_{i \in instances} \min\Big(\text{budget}_i,\ \sum_{f \in active(i)} \text{open}_f\Big) + C_{admin} + C_{migration} \]

Per-firm pools idle down to zero, so only firms with recent traffic hold connections.

| Scenario | Assumed firms and users | Placement | Connections (assumption) | Records and storage (assumption) | Migration fleet duration (formula) |
|---|---|---|---|---|---|
| 1 firm | First firm; staff and portal-user counts unconfirmed (synthetic profile: 40 staff, 3,000 portal users) | 1 shared cell | 2 instances × pool 10 = 20 | One medium firm; SQL ≈ fixture order, files dominate | One database: \(T_{db}\) |
| 100 firms | Mostly small or medium firms | 2–4 cells at a placement policy of 25–50 firms each | Bounded by the per-cell budget; per-firm max 5; the default 151 is too small for any cell with many active firms, so the server setting must be raised and measured | Sum of per-firm sizes; growth monitored per firm | \(\lceil N / k \rceil \times T_{db}\) with concurrency \(k\) per cell, all cells in parallel |
| 1,000 firms | Long tail of small firms | 10–40 cells; cell affinity required | Per-cell budget enforced in the app; per-account `MAX_USER_CONNECTIONS` | Per-cell storage alarms | The same formula; canary wave first; at \(T_{db}\) = 30 s and \(k\) = 8 per cell, 1,000 firms over 20 cells take about \(\lceil 50/8 \rceil \times 30\) s ≈ 3.5 min per wave after the canary (\(T_{db}\) assumed, not measured) |
| 10,000 firms | Mostly very small firms | 100+ cells, or the hybrid pooled tier | Credential count, secret rotation, and table-level grant rows (about 73 × firms per server) become operating concerns; **not measured** | — | Fleet automation is mandatory; reconsider the hybrid model before reaching this |

\(T_{db}\) depends on the migration type. INSTANT column adds were near-instant; index builds and table rebuilds scale with table size. See the [engine checks](../../db/design/validation/mysql-8.0.36-validation.log), E6–E9. Measure \(T_{db}\) on representative firm sizes before promising a maintenance window. **No zero-downtime claim is made.**

## Shared-table constraints, if a pooled tier is ever added
These are taken from the [pooled DDL](../../db/design/20-pooled-alternative.sql) and validated:
- `PRIMARY KEY (FirmId, Id)` on every firm-owned table, so firm rows cluster together and every tenant-prefixed index is a left prefix.
- Composite FKs `(FirmId, ParentId) → parent(FirmId, Id)`. A cross-firm reference fails with error 1452 (P1).
- **FirmId is immutable.** Use column-level UPDATE grants that exclude `FirmId` (P4: error 1143) or a trigger, because an update re-parents rows when local ids overlap (P3).
- Uniqueness scoped by firm, for example `UNIQUE (FirmId, MatterNumber)`.
- These constraints don't authorize reads (P2). Every query still needs the firm predicate and the F1 matter authorization.

## Reconsideration triggers (measured, not firm-count thresholds)
- Sustained connection use above an agreed share of the cell budget, or pool-wait p95 above target, after pool tuning.
- Fleet migration wall time for one release exceeding the agreed maintenance window, measured over canary and waves.
- Per-firm infrastructure unit cost (from real provider bills) exceeding the price point for small-firm plans.
- More than an agreed fraction of firms below a size where per-database overhead dominates.
- Operational incidents caused by fleet drift or partial migrations.
- A contractual requirement for a dedicated instance or for residency (this moves a firm to a dedicated cell, not to a different model).

When any of these fires, write ADR-003 assessing the hybrid model. Its first decision would be whether to add FirmId to the common schema everywhere, so that one schema serves both tiers with no fork.

## Consequences
- **Easier:** server-enforced cross-firm denial; simple single-firm export, offboarding, and deletion; no change to existing tables; the F1 authorization layer is reused unchanged.
- **Harder:** fleet migrations, monitoring, and credential rotation; connection budgeting; single-firm restore through an isolated server; a privileged trigger-creation step.
- **Residual risk:** the shared application process holds credentials for every firm in the cells it serves. A compromised process or a routing bug in the trusted resolver could reach any of them. Mitigations are in the [threat model](../security/multitenant-threat-model.md).

## Adoption evidence still required
Everything under ADR-001's "Adoption evidence" still applies. In addition:
- a measured \(T_{db}\) for representative migrations;
- measured pool behavior with two or more firms under load;
- a decision on trigger creation (a privileged step or the server parameter) on the chosen host;
- a rehearsed restore of one firm through an isolated server, with RPO and RTO recorded.
