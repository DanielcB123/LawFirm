# Data Migration Plan (Working Draft)

## Current source state

- Active application data model is in `LawFirmDbContext` (single-firm schema).
- Startup and tests target MySQL.
- Unknown at this stage whether the operator has important local data in:
  - host MySQL service,
  - XAMPP-managed MySQL instance,
  - disposable test databases only.

## Safety rules

- Do not run destructive commands against unidentified databases.
- Do not assume dev seed data is the only data needing preservation.
- If existing XAMPP data is relevant, export and verify before cutover.

## Required discovery before conversion

1. Identify all current MySQL databases used by this project locally.
2. Determine whether non-synthetic records must be retained.
3. Confirm character set/collation and timezone handling.
4. Confirm ID and FK semantics for all migrated tables.

## Initial source-to-target mapping approach

- Map EF entities/tables to Laravel migrations and Eloquent models:
  - parties/aliases/relationships
  - conflicts/decisions
  - calendar entries
  - intake records/history/notes
  - matters/stage history/tasks/documents/timeline
  - matter access grants
  - consultation requests
  - audit events
- Preserve:
  - key identifiers,
  - relationship integrity,
  - timestamps,
  - status/stage values,
  - matter-access semantics.

## Conversion workflow (planned)

1. Snapshot source DB metadata and counts.
2. Build Laravel schema migrations (non-destructive, additive in migration environment).
3. Create import scripts with idempotency guards.
4. Run imports on sanitized copy.
5. Validate row counts, FK integrity, and sampled business-critical records.
6. Re-run import to verify duplicate safety behavior.

## Validation checklist

- Table-by-table row counts match expected source subset.
- FK references valid after import.
- Matter access grants preserved.
- Date/time values preserved with intended timezone semantics.
- No sensitive fields leaked to logs during migration.

## Recovery and rollback notes

- Maintain pre-migration DB dump before any conversion.
- Keep source database read-only during final cutover window when applicable.
- If conversion fails validation, discard target and retry from preserved source snapshot.
