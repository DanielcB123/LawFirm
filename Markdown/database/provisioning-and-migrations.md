# Firm provisioning, schema migrations, and offboarding

[Home](../README.md) · [ADR](ADR-001-tenancy.md) · [Recovery](../operations/reliability-and-recovery.md)

> **Current implementation (audited 2026-10-12):** No provisioning exists, and schema is created with `EnsureCreated` plus raw `CREATE TABLE IF NOT EXISTS`. EF Core migrations are a prerequisite for everything below (G06 in the [gap map](../delivery/implementation-gap-map.md)).
>
> **Update 2026-10-12 (F1, uncommitted):** EF Core migrations now own the schema (`Baseline`, `MatterAccessGrants`). Startup refuses databases without migration history and applies migrations only when `Database:ApplyMigrationsOnStartup` is true. Existing databases follow the manual [migration baseline](migration-baseline.md). There is still no provisioning, no per-firm runner, and no runtime/migration account split.

## Provisioning state machine
Requested → provisioning → validation → ready, with failed/retryable branches. A unique operation key prevents duplicate databases or memberships. Create catalog entry, database/role, schema, private file scope, verified domain, administrative invitation, and health evidence. Publish ready only when all required components pass validation. Compensate or explicitly mark partially completed operations; never route users to half-created resources.

Authenticate bootstrap administrator invitations, expire them, and record who authorized provisioning. Do not hard-code Maldanado and Benitez into source code; provision it through the same configuration-driven path future firms use.

## Request and connection rules
Resolve placement from an authorized tenant catalog. Use scoped credentials where supported and a secrets manager. Separate migration privileges from runtime privileges. Bound connection pools and evict idle pools as needed. Never use a singleton mutable tenant connection shared among concurrent requests. Workers reset context after every job. Connection reuse tests must cover exceptions as well as successful requests.

## Fleet schema rollout
Maintain an ordered migration history per database. Use expand → backfill → verify → switch reads/writes → contract after compatibility window. Run pilot/canary firms first, use per-database locks, record duration/results, limit parallelism, and retry idempotently. Application versions declare compatible schema versions. Route outdated tenants safely rather than attempting queries against unknown schemas.

Do not silently edit already deployed migrations. Backups alone do not make destructive migrations safe. Rollback must consider data written after deployment and schema compatibility. No custom schema forks for the first firm; use configured form/template versions.

## Moving a firm
Inventory relational data, objects, search projections, secrets, outbox jobs, provider routing, and webhooks. Rehearse on synthetic data. Use a planned write freeze for the early product; any live replication approach needs separate correctness evidence. Copy, verify checksums and counts, reconcile money, update placement atomically, invalidate routing caches, and then resume. Prevent delayed jobs or webhooks from writing to the old placement. Retain rollback material under approved policy.

## Offboarding
Deliver an authorized export with metadata and exact file versions; revoke access and provider credentials; define retention/hold obligations; suppress new work; execute approved deletion after applicable holds and deadlines. Record how backups age out. Distinguish firm subscription termination from a represented client's request. A payment failure must not erase legal records or silently strand clients.
