# Multi-firm implementation plan (dependency-ordered)

[Home](../README.md) · [Roadmap](roadmap-and-acceptance.md) · [Gap map](implementation-gap-map.md) · [ADR-002](../database/ADR-002-multitenant-architecture.md) · [Acceptance matrix](multitenant-acceptance-matrix.md)

**Status:** plan only. Nothing below is implemented. Each step is a bounded slice: it ends with green tests, an updated ProgressPlan, and a SLICE_HISTORY entry. A step doesn't start until the steps it depends on are verified.

| # | Step | Depends on | Exit evidence (acceptance ids) |
|---|---|---|---|
| T0 | Prerequisites: commit F1; rotate credentials that were tracked; baseline the existing development database (owner action, per the [baseline runbook](../database/migration-baseline.md)) | — | F1 commit; rotation recorded without values; AT-REG-01 |
| T1 | **Minimal control-plane catalog plus trusted firm resolution plus per-firm DbContext routing.** `ControlPlaneDbContext` covers `firms`, `firm_domains`, `database_servers`, and `firm_placements` only. Host → verified domain → active firm → placement. The runtime connection string comes from configuration or user secrets in tests. A per-request firm context is immutable, and the `firm_identity` check runs on lease. Two synthetic firm databases with overlapping ids sit in one container, each with a restricted runtime account. | T0 (commit) | AT-RES-01, -02, -03, -05, -06; AT-AUTHZ-01; AT-CTX-01, -02; AT-DB-01; AT-REG-01 on the routed firm |
| T2 | Firm memberships and platform identities: the token carries the platform subject only; membership is resolved per firm, with a version and session epoch | T1 | AT-RES-02, -04; AT-REVOKE-01 (local issuer) |
| T3 | Manifest-generated table-level runtime grants; migration account separation; privileged trigger step automated in the fixture | T1 | AT-DB-02..06, AT-PROV-03 |
| T4 | Provisioner (operations table, idempotent steps, readiness gates) for synthetic firms | T1, T3 | AT-PROV-01, -02 |
| T5 | Fleet migrator (per-database lock, waves, failure budget, drift check) over the synthetic firms | T4 | AT-MIG-01..03, AT-RES-07 |
| T6 | Matter assignments, ethical walls, and `firm_users` (expand/backfill from the actor columns); switch F1's access service to Q1b | T1 | AT-AUTHZ-02, -03; AT-IDX-01 (Q1b in the app) |
| T7 | Outbox, jobs, and cache with firm-scoped keys and re-authorization | T1 | AT-JOB-01, -02; AT-CACHE-01 |
| T8 | Object storage per firm prefix plus `document_versions` (expand/contract of `StorageKey`) | T1, T5; storage provider decision | AT-FILE-01; AT-MIG-04 |
| T9 | Ledger core (journal, triggers, trust subledger) behind the posting service; no real money | T3, T5 | AT-FIN-01..06 |
| T10 | Provider webhooks plus payments in test mode | T9; payment provider decision | AT-HOOK-01, -02 |
| T11 | Restore, movement, and offboarding rehearsals on the chosen host | T4, T5; hosting decision | AT-REST-01; measured RPO/RTO |
| T12 | First-firm onboarding (copy, baseline, map, validate, rehearse) | T1–T7, T11; firm agreement | Reconciliation report; sign-off |

## Unresolved decisions that block specific steps
These are tracked in [open decisions](open-decisions.md).
- **Hosting provider:** its `max_connections`, backup and point-in-time restore granularity, TLS, and whether `log_bin_trust_function_creators` can be set. Blocks T11 and sizing; it also affects T3's trigger approach.
- **Identity provider and MFA.** Blocks the production form of T2.
- **Object storage, KMS, and malware scanning.** Blocks T8.
- **Payment provider.** Blocks T10.
- **Rounding rule, fee and trust practices, and the retention schedule** (firm attorney or accountant). Blocks the production form of T9 and offboarding.
- **Cell sizing:** firms per cell and the per-firm pool size. Needs AT-SCALE-01 measurements.
- **Branding clearance and the first firm's legal name.** Blocks T12 and any public use.
