# ADR-001 — firm data isolation

[Home](../README.md) · [Research](tenancy-research.md) · [Provisioning](provisioning-and-migrations.md) · [Codebase audit](../architecture/current-codebase-audit.md)

**Refined by [ADR-002](ADR-002-multitenant-architecture.md) (2026-10-12).** ADR-002 keeps this decision and replaces the "Connections and cost" section below. This file is otherwise kept as written.

**Status:** Proposed. Revised 2026-10-12 after the repository audit; awaiting hosting validation and founder sign-off. **Decision owners:** product founder and technical owner, with first-firm workflow review. Nothing in this ADR is implemented yet.

## Context from the audit
- Engine: MySQL 8.0 via Pomelo EF Core. One connection string; **no tenant concept anywhere** in code, schema, tokens, or routing.
- The schema is therefore already a single-firm data model. It has no `tenant_id` columns, which means there is nothing to backfill.
- Schema is created by `EnsureCreated` plus raw DDL. There is no migration history, so fleet rollout of schema changes is impossible today.
- Identity is an in-memory demo store. Authorization checks permission claims but not matter assignment.
- In MySQL, `SCHEMA` and `DATABASE` are synonyms [S19]. "Schema per firm" and "database per firm" are the same option on this engine. The PostgreSQL-specific schema-namespace alternative in the research does not apply.
- MySQL 8.0 has no native row-level security comparable to PostgreSQL RLS. A shared-table design would rely entirely on application filters (for example EF Core global query filters) or view/definer tricks. That is an engineering assessment; re-check against the exact server version if a pooled tier is reconsidered.

## Decision
1. **One shared application release** (the existing ASP.NET Core API and React SPA).
2. **One control-plane MySQL database** holding: firms, verified domains, platform identities (subject references only), memberships with status and version, database placement (host reference, database name, secret reference, schema version), SaaS subscription status, and provisioning jobs. No legal narratives, client contacts, or documents.
3. **One MySQL database per law firm** with the same versioned product schema. The current `LawFirmDbContext` schema becomes the firm schema.
4. **Initial placement:** firm databases share a managed MySQL server. A dedicated server is an optional later tier for a firm that contractually or operationally needs it.
5. **Per-firm runtime credentials:** each firm database gets its own MySQL user granted DML only on `firm_db.*`. Migration credentials are separate and have DDL rights. The control-plane user cannot read firm databases.

This is a recommendation for this product's starting conditions (few firms, confidential records, single-firm export/offboarding valuable). It is not a legal mandate.

## Alternatives considered

| Option | Assessment on MySQL |
|---|---|
| Shared tables with `firm_id` | Lowest cost. Requires adding `firm_id` to every table, composite tenant-safe FKs, and a global filter on every query, with no RLS backstop. A single missed filter exposes another firm. Single-firm restore requires scoped extraction. Rejected for now; reconsider only with measured cost pressure |
| Schema per firm | Identical to database per firm in MySQL [S19] |
| **Database per firm (shared server)** | Clean grants, logical export (`mysqldump --databases firm_x`), and deletion. More pools and migration fan-out. **Selected** |
| Server per firm | Strongest resource and backup independence; highest cost and operations. Optional tier later |

## Request and context boundary
Authenticate → resolve candidate firm from hostname → require a firm claim in the token that matches → verify active membership and membership version against the control plane (short-TTL cache) → resolve placement → open the firm `DbContext` → enforce role, matter grant, and audience → serialize an allowed DTO → audit. A payload or header `firmId` is never authoritative. A missing, inconsistent, or suspended context **fails closed** with no fallback connection. That rule removes today's hard-coded fallback connection string.

Implementation shape in this codebase: a scoped `IFirmContext` populated by middleware, plus a `LawFirmDbContext` factory that builds options from the resolved placement. Never use a singleton mutable connection. Each firm database contains a one-row `firm_identity` table. On first use per pooled connection or per request, the context verifies that `firm_identity.firm_id` equals the resolved firm, so a mis-routed connection string fails closed.

## Tenant-safe keys and ownership
Inside a firm database, ordinary single-column FKs are sufficient because cross-firm rows cannot coexist. This **deviates from the blueprint suggestion** to carry `tenant_id` on every row. Under database-per-firm the column adds migration cost without adding a constraint the database can enforce. Defense in depth instead comes from the `firm_identity` self-check, per-firm credentials, and `firm_id` in every *out-of-database* artifact: outbox/event envelopes, job payloads, cache keys, object-storage keys, search index names, telemetry scope, and webhook routing records. If a pooled tier is ever introduced, it must add `firm_id`, `UNIQUE(firm_id, id)`, and composite FKs `(firm_id, x_id) → x(firm_id, id)` (InnoDB supports composite FKs) before any shared placement.

Matter numbers stay unique per database, which is per firm. Replace max+1 generation with a per-firm sequence row updated under a row lock.

## Connections and cost
MySqlConnector maintains a pool per distinct connection string, and `Maximum Pool Size` defaults to 100 [S20]. With per-firm users, each firm gets its own pool. Worst-case connections ≈ app instances × firms × max pool size, which must stay below the server's `max_connections` (MySQL default 151; managed tiers vary). Start with `Maximum Pool Size` around 10 per firm, set `Connection Idle Timeout`, and measure. With one to ten firms this is comfortable. Re-evaluate before tens of firms per server; options at that point include a shared runtime user plus `ChangeDatabase`, which weakens grants, or more servers. Cost scenarios for 1, 10, and 100 firms must use actual provider pricing; none is asserted here.

## Migrations
Prerequisite: replace `EnsureCreated` and raw DDL with EF Core migrations. Baseline: generate an initial migration matching the current schema, and mark it applied on existing dev databases. Then roll out per firm: ordered history in each firm database (`__EFMigrationsHistory`), pilot firm first, per-database lock, bounded parallelism, expand → backfill → switch → contract. The application declares its compatible schema range, and the control plane records each firm's version. A firm on an incompatible version is routed to maintenance rather than queried. The control plane has its own migration history.

## Jobs, caches, search, storage
- **Outbox:** an outbox table in each firm database, written in the same transaction as the domain change. A dispatcher iterates active firms from the control plane, opens each firm context, and processes rows. Each job carries `firm_id` and rechecks authorization and publication state at execution.
- **Caches:** keys are prefixed `firm:{id}:`. No firm data in process-wide caches without that prefix. Membership caches are invalidated on revocation.
- **Search:** current search is SQL `LIKE` inside the firm database, which is naturally scoped. Any future external index is per firm or carries a filtered `firm_id`, and is rebuildable.
- **Object storage:** a separate container or bucket prefix per firm, with credentials scoped to it where the provider supports that. Opaque keys only (no client names). Object versioning is enabled.

## Backup and restore semantics
- Managed MySQL point-in-time restore typically restores a **whole server** to a new server. Azure Database for MySQL Flexible Server, for example, documents that restore "creates a new server" [S21]. Native binary-log PITR is likewise server-wide [S22]. **A database per firm does not give native per-firm PITR.**
- Single-firm recovery procedure: restore the server to an isolated environment → `mysqldump` only the affected firm database → verify `firm_identity`, row counts, and checksums → restore into a staging database → reconcile object versions, outbox, and webhook receipts processed after the restore point, plus ledger/provider state once finance exists → switch placement under a write freeze. **Never** overwrite other firms' databases, and **never** roll back control-plane revocations, holds, or memberships.
- Add scheduled per-firm logical dumps as an independent extraction path and for offboarding exports.
- Acceptance requires a rehearsed restore of one synthetic firm while a second synthetic firm stays unaffected. Measured RPO and RTO get recorded.

## Migration strategy from today
No production data exists in this repository's scope, and no destructive migration is authorized by this ADR.
1. Baseline EF migrations; externalize secrets; add the integration test harness.
2. Add the control-plane database and persistent identity/membership, replacing `DemoUserStore` outside Development.
3. Introduce `IFirmContext` plus the per-firm `DbContext` factory. During development, one synthetic firm maps to the existing dev database.
4. Add a second synthetic firm and run cross-firm negative tests on every API surface.
5. Provision Maldanado and Benitez Law in staging through the same configuration path, with synthetic matters only.

## Reconsideration triggers
Measured connection pressure, migration fleet duration, per-firm unit cost, noisy-neighbor load, contractual isolation or residency requirements. No arbitrary firm-count threshold is asserted.

## Adoption evidence (not yet met)
Migrations baseline merged; control plane and context resolution implemented; cross-firm tests pass for API, files, jobs, caches, and exports; grants verified (a firm user cannot `SELECT` another firm's database); pilot migration fan-out tested; one-firm restore rehearsed; cost model recorded; first firm's confidentiality, retention, and publication policy configured.
