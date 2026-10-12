# Delivery sequence and acceptance

[Home](../README.md) · [Gap map](implementation-gap-map.md) · [Open decisions](open-decisions.md) · [Multi-firm plan](multitenant-implementation-plan.md) · [Cursor prompt](../../Markldown/CURSOR_PROMPT.md)

No time estimates are promised before validating staff capacity. Phase 0 (discovery) documentation is complete as of 2026-10-12; first-firm discovery sessions are not.

| Phase | Concrete deliverable | Gate |
|---|---|---|
| 0: Discover | Existing architecture audit, linked Markdown, first-firm discovery, gap map | Actual code paths/evidence; known decisions separated from assumptions |
| 1: Foundation | Tenant resolution, identity, roles, matter grants, private storage, auditing | Cross-firm/matter tests, production configuration, recovery evidence |
| 2: Intake | Inquiry, consultations, conflicts review, engagement | Booking concurrency, conflicts decisions, signature evidence |
| 3: Matter/portal | Matter workspace, tasks/deadlines, documents, approved updates, messages | Complete approved client journey; publication boundary tests |
| 4: Finance | Legal billing, tokenized payments, reliable allocations | Exact calculations, webhook dedupe, refund/dispute reconciliation |
| 5: Practice depth | Construction and PI templates, evidence, settlement workflow | Attorney-approved templates and versioned decisions |
| 6: Trust production gate | Native validated trust ledger or reviewed integrated workflow | Accountant/firm policy review, three-way reconciliation, concurrency and restore evidence |
| 7: Expand | Additional firms, cost/scale validation, integrations, optional research/AI | Repeatable onboarding, isolation, provider terms, proven operations |

Real matters must not be opened on a demo-only deadline, security, or financial system. If a necessary function remains in an existing validated tool during pilot, document the handoff and keep its authoritative state clear in the application. Do not pretend it has been replaced.

## Dependency-ordered plan from the audit
Gap IDs refer to the [gap map](implementation-gap-map.md). Each slice is end to end (schema, API, UI where relevant, tests, and doc update) and leaves the app runnable.

| Order | Slice | Gaps closed | Why here |
|---|---|---|---|
| F1 | **Test harness + matter-level authorization** — *Verified within stated scope 2026-10-12 (uncommitted)* | G07, G10, G11, G12, G13 | Largest confidentiality defects in current code; independent of tenancy; creates the test pattern every later slice needs |
| F2 | Migrations baseline + secrets externalization + demo-identity gating + same-transaction audit with append-only grants — *migrations, secrets, and demo gating done with F1; audit (G16) Planned; existing-DB baseline and credential rotation pending* | G06, G05, G16, part of G03 | Tenancy and every schema change depend on versioned migrations |
| F3 | Lint fix + CI pipeline | G08 | Gates F1/F2 tests on every change |
| F4 | Control plane + persistent identity/membership | G01, G03 | Required before routing to firm databases |
| F5 | Firm context + per-firm `DbContext` + second synthetic firm + isolation tests | G02 | Implements ADR-001 core; proves cross-firm denial |
| F6 | Booking concurrency, firm-local slots, rate limiting, conflict binding, matter numbering | G20–G23, G25 | Public intake correctness |
| F7 | Engagement + deadline provenance + date-only deadlines | G24, G30, G31 | Representation must not start without engagement; legal-date safety |
| F8 | Private document pipeline + versions + holds | G32, G33 | Prerequisite for client uploads and portal |
| F9 | Outbox/dispatcher + minimal notifications | G43 | Prerequisite for portal notifications and payments |
| F10 | Client portal + publication + messaging | G40–G42, G14, G15 | Client journey core |
| F11 | Staff MFA / managed IdP | G04 | Before any real case data |
| F12 | Provision first firm in staging (synthetic data) + restore rehearsal | G64, G60 | Pilot readiness evidence |
| F13+ | Legal billing → payments/webhooks → trust (integration preferred) → practice extensions | G50–G53, G34 | Financial correctness after foundations |

**Multi-firm architecture (2026-10-12, design only):** [ADR-002](../database/ADR-002-multitenant-architecture.md) refines F4 and F5. Its [implementation plan](multitenant-implementation-plan.md) orders the multi-firm work as T0–T12, and the [acceptance matrix](multitenant-acceptance-matrix.md) holds the cases. The next bounded step is **T1**: a minimal control-plane catalog, trusted firm resolution, and per-firm DbContext routing with two synthetic firm databases and cross-firm tests. T1 covers the routing half of F4 and F5. Persistent identity and membership (T2) follow. T1 may start before F3 (CI), but it then relies on manual test runs. That choice belongs to the owner.

## First slice in detail (F1)
**Goal:** a staff member can reach only matters they are assigned or explicitly granted; clients cannot write to matters they are not on; restricted parties are masked everywhere. Each rule has a failing-then-passing integration test.

1. Add `backend.Tests` (xUnit + `WebApplicationFactory<Program>`). Run against MySQL 8.0 in a container so constraints match production semantics. Program-level hooks are needed for test configuration only.
2. Add `matter_access_grants (matter_id, actor_id, grant_type, granted_by, granted_at, revoked_at)`. Treat responsible attorney and paralegal as implicit grants.
3. Add one query helper (for example `AccessibleMatters(ClaimsPrincipal)`) used by every `/matters-v2` route, including child resources (tasks, documents, timeline). Inaccessible resources return 404, not 403, to avoid confirming existence. `matters.read.all`/`update.all` bypass the grant check; `read.assigned` does not.
4. Restrict `CreateDocument` for clients to matters where they hold a client grant.
5. Move restricted-party masking into a shared serializer used by `/parties` and `/conflicts`.
6. Tests: unassigned paralegal (list, detail, each child route, each write); admin access; client document write to an ungranted matter; restricted-party masking for a paralegal; positive cases for assigned users.
7. Update the gap map statuses, [audit](../architecture/current-codebase-audit.md), and [ProgressPlan.md](../../ProgressPlan.md) security notes.

Schema note: F1 adds a table before migrations exist (F2). Either do F2 first, or add the table through the existing raw-DDL path and fold it into the F2 baseline. The second option is recommended so the security fix is not delayed.

**Outcome (2026-10-12):** the migrations baseline was done first after all, because the tests needed a migration-built schema. Grants arrived as a second migration. Tests live in `backend.IntegrationTests/` rather than `backend.Tests`. Fail-before: 26 of 43 failed; pass-after: 44 of 44 passed on MySQL 8.0.36. Policy and evidence: [matter access policy](../security/matter-access-policy.md); history: [SLICE_HISTORY.md](../../SLICE_HISTORY.md).

## Cross-cutting acceptance suite
1. Firm A users cannot obtain Firm B records through APIs, URLs, files, search, cache, jobs, exports, analytics, or callbacks.
2. Same-firm client A cannot access client B's restricted matter or joint-client private content.
3. A blocked/conflicted prospect cannot bypass engagement review.
4. Unpublished/internal material never reaches a client serializer, preview, search, or notification.
5. Two simultaneous consultation requests do not double book.
6. Legal dates preserve source, reviewer, trigger, and time semantics.
7. Duplicate payment/signature events produce one valid effect.
8. Posted ledger entries balance; withdrawals cannot overspend; changed settlement invalidates approval.
9. Held evidence cannot expire/delete; originals and exact versions are exportable.
10. Recovery of one firm restores coherent SQL/object/financial state and honors current revocations/holds.
11. Keyboard/mobile clients can complete intake, read updates, upload documents, and pay through accessible flows.
12. Synthetic first-firm walkthrough demonstrates consultation to closure and honestly identifies unfinished capabilities.

Test domain invariants and failure boundaries, not trivial mirrors of implementation. CI should run appropriate build/static checks and meaningful integration tests using the actual database engine where its constraints matter. Keep a traceability table: requirement → implementation path → test/evidence → owner → status.
