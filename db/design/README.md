# Database design artifacts (review only)

[Docs home](../../Markdown/README.md) · [ADR-002](../../Markdown/database/ADR-002-multitenant-architecture.md) · [Data dictionary](../../Markdown/database/data-dictionary.md) · [Query and index spec](../../Markdown/database/query-and-index-spec.md) · [Acceptance matrix](../../Markdown/delivery/multitenant-acceptance-matrix.md)

These files are **reviewable design SQL**, not migrations. The application doesn't load them, EF doesn't apply them, and they must never be run against an existing or non-disposable database. The production schema will come from EF Core migrations written in later slices.

## Files

| File | Content |
|---|---|
| `01-control-plane.sql` | Shared control-plane catalog (15 tables): firms, domains, identities, memberships, invitations, secret references, servers or cells, placements, provisioning operations, migration runs and targets, subscriptions, platform webhooks, support access, control audit |
| `10-firm-current-schema.generated.sql` | Current firm schema, **generated** from the EF migrations (`dotnet tool run dotnet-ef migrations script --project backend --configuration EfDesign`). Do not edit by hand |
| `11-firm-schema-required.sql` | Additions required for the first multi-firm slices: `firm_identity`, `firm_users`, assignments, ethical walls, outbox, webhook receipts, indexes for real queries |
| `12-firm-schema-future.sql` | Later structures: parties, deadlines, messaging, documents and evidence, billing and ledger, trust, settlements, imports, construction, personal injury |
| `13-firm-ledger-triggers.sql` | Ledger triggers. Need a privileged `DEFINER` step (see header; ERROR 1419 for a non-SUPER migration user when binary logging is on) |
| `20-pooled-alternative.sql` | Shared-table (pooled) comparison schema with composite firm keys. Used only to demonstrate its limits |
| `40-synthetic-data.sql` | Deterministic synthetic data generator (`@scale`, `@firm`). Contains no real people or cases |
| `50-representative-queries.sql` | `EXPLAIN ANALYZE` for the app's real and planned query shapes |
| `60-engine-behavior-checks.sql` | MySQL engine facts E1–E12b (partitioning, implicit commit, online DDL, collation, CHECK) |
| `70-money-integrity-checks.sql` | Ledger, trust, approval, billing, and webhook checks L1–L18 |
| `run-validation.ps1` | Orchestrates everything in a disposable container |
| `validation/mysql-8.0.36-validation.log` | Output of the latest run |

## How to run
Requires Docker. From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File db/design/run-validation.ps1
```

What the script does:
- starts a **new, uniquely named** `mysql:8.0.36` container with a random root password, which is passed by environment variable and never printed;
- creates two synthetic firm databases (A at scale 1.0, B at scale 0.1) with overlapping ids, per-firm migration and runtime accounts, and manifest-generated table-level grants;
- runs the grant (G), pooled (P), money (L), race (R), and plan (Q) checks;
- writes the log;
- removes the container in a `finally` block (`-KeepContainer` keeps it).

A full run took about 1 min 45 s on the development machine (2026-10-12). The firm A synthetic load alone took about 35 s.

## Evidence summary (run 2026-10-12, 06:01:44–06:03:29 UTC)
- All checks produced their expected results. The log contains 51 expected `ERROR` lines, each matching a check labelled `EXPECT ERROR`. Client output buffering can print an error line just before its label.
- Key engine findings that changed the design:
  - **Partitioning can't be combined with foreign keys.** Errors 1506 and 1503 rule out partitioning for FK-bearing tables.
  - **DDL implicitly commits**, and a failed migration leaves its earlier DDL applied.
  - **A restricted migration account can't create triggers** (error 1419).
  - **A table-level REVOKE does not override a database-level grant** (error 1147, and the write still worked), so runtime accounts get table-level grants only.
  - **A GUID bound as a utf8mb4 variable disables index use** on `ascii_general_ci` columns.
- The log section label "apply 12 (future structures, includes triggers)" was written before the triggers moved to file 13. The triggers are applied separately in the next log sections.

## Limitations
- One machine, one container, default server settings. **These are not managed-hosting limits.**
- Synthetic data. Firm A is about 506 MB, which is not a large firm, and row counts are modest. Plans show **index choice and rows examined, not production latency**. Nothing here supports a scale claim.
- Concurrency checks (R1–R4) use a few sessions. They demonstrate correctness under the tested interleavings, not throughput.
- No application code ran against these schemas. Application-level isolation is untested (see the acceptance matrix).
