# Multi-firm acceptance-test traceability matrix

[Home](../README.md) · [Roadmap](roadmap-and-acceptance.md) · [Threat model](../security/multitenant-threat-model.md) · [Implementation plan](multitenant-implementation-plan.md) · [Validation log](../../db/design/validation/mysql-8.0.36-validation.log)

## Status vocabulary
- **Designed:** the case is written here.
- **Implemented:** test code or a script exists.
- **Executed:** it was run.
- **Passed / Failed:** the outcome of the latest run.
- **Blocked:** it can't run yet, and the blocker is named.

"DB-validated" means the case was executed as a SQL check in [`run-validation.ps1`](../../db/design/run-validation.ps1). That run used a disposable MySQL 8.0.36 container, two synthetic firm databases with **overlapping local ids**, and real restricted accounts. It is evidence about engine behavior. It is not an application test.

Every application-level multi-firm case is **Designed** only, because no multi-firm code exists.

## Fixtures for application tests
- At least two synthetic firms (A and B) on one cell, plus a third on a second cell for movement and restore.
- Deliberately overlapping local ids: the same matter GUID in A and B, and the same bigint message ids.
- Real per-firm runtime accounts with the manifest's grants. No in-memory provider. The existing `MySqlApiFixture` pattern extends to several databases in one container.

## Matrix

| Id | Case | Expected | Threat | Status | Evidence |
|---|---|---|---|---|---|
| AT-REG-01 | Existing single-firm suite | 44 pass | T20 | Implemented, executed, **passed** (2026-10-12, this session) | `dotnet test backend.IntegrationTests -c EfDesign` |
| AT-RES-01 | Unverified or unknown host | 404, no firm query | T1 | Designed | — |
| AT-RES-02 | Firm A token on firm B host | 401/404, no query to either firm | T2 | Designed | — |
| AT-RES-03 | Forged firm selector in header, body, query, or route | Ignored or 400; resolved firm unchanged | T3 | Designed | — |
| AT-RES-04 | Membership revoked or version bumped mid-session | Next request denied within the cache TTL | T4 | Designed | — |
| AT-RES-05 | Suspended firm | Maintenance response, no firm query | T5 | Designed | — |
| AT-RES-06 | Placement points to firm B's database (wrong identity) | `firm_identity` mismatch → 503, alert, no data returned | T6, T8 | Designed (DB read of identity: G12 **passed**) | Log G12 |
| AT-RES-07 | Firm schema outside the app's compatible range | Maintenance response | T7 | Designed | — |
| AT-CTX-01 | 200 interleaved concurrent requests for A and B on one instance | Every response from its own firm; zero cross-firm rows | T16, T17 | Designed | — |
| AT-CTX-02 | Pooled connection reuse after firm A, then a request for B | New lease from B's pool; identity check passes | T17 | Designed | — |
| AT-AUTHZ-01 | Request firm B's matter by an id that also exists in A | Only the routed firm's row; 404 for the other | T9 | Designed (overlap shown: G13, G13b **passed**) | Log G13 |
| AT-AUTHZ-02 | Restricted grants inside a firm | F1 rules | T20 | Implemented, executed, **passed** (part of AT-REG-01) | F1 tests |
| AT-AUTHZ-03 | Ethical wall overrides an assignment or grant | 404 for the screened actor | T20 | Designed (query shape Q1b executed) | Log Q1b |
| AT-AUTHZ-04 | Client sees only published updates for their audience; a representative sees only the represented client's matters | Unpublished and other-audience items absent | T21 | Designed | — |
| AT-AUTHZ-05 | Static gate on raw SQL and `IgnoreQueryFilters` | Build fails on unapproved use | T18 | Designed | — |
| AT-DB-01 | Firm A runtime reads or switches to firm B | ERROR 1142 / 1044 | T9 | **DB-validated, passed** | G1, G2, G3 |
| AT-DB-02 | Runtime DDL (CREATE, DROP, ALTER) | ERROR 1142 | T10 | **DB-validated, passed** | G4–G6 |
| AT-DB-03 | Runtime edits audit or `firm_identity` | ERROR 1142 | T11 | **DB-validated, passed** | G7, G8 |
| AT-DB-04 | Control-plane runtime reads a firm | ERROR 1142 | T13 | **DB-validated, passed** | G15 |
| AT-DB-05 | Firm A migration account reads B or creates a database | ERROR 1142 / 1044 | T10 | **DB-validated, passed** | G14 |
| AT-DB-06 | Table-level REVOKE carved out of a database-level grant | ERROR 1147; the update still allowed, which proves table-level grants are needed | T12 | **DB-validated, passed** (limitation confirmed) | G16, G16b |
| AT-DB-07 | Per-firm connection cap | Second session refused with 1226 | — | **DB-validated, passed** | R4 |
| AT-DB-08 | Runtime positive control (append audit, update a matter) | Succeeds | — | **DB-validated, passed** | G11 |
| AT-POOL-01 | Pooled tier: cross-firm FK, unfiltered read, FirmId re-parenting, column-grant mitigation | 1452; both firms returned; 1 row moved; 1143 | T31 | **DB-validated, passed** | P1–P4 |
| AT-JOB-01 | Job for firm A after A's grant was revoked | Job re-authorizes and skips; never touches B | T19 | Designed | — |
| AT-JOB-02 | Two dispatchers claim outbox rows | Disjoint sets | T19 | **DB-validated, passed** | R3 |
| AT-CACHE-01 | Cache entry for A not readable under B | Key prefix enforced | T19 | Designed | — |
| AT-FILE-01 | Download firm A's object key while routed to B | Denied by prefix-scoped credential and by app check | T19 | **Blocked**: no object storage exists | — |
| AT-SEARCH-01, AT-EXPORT-01, AT-NOTIFY-01 | Search, export, and notification scoped to the firm and audience | No cross-firm or cross-audience items | T19 | Designed | — |
| AT-HOOK-01 | Webhook: unsigned, replayed, other account | 3819 / 1062 / accepted as a separate event | T22 | **DB-validated, passed** (DB part); the app part is designed | L15–L17 |
| AT-HOOK-02 | Event for a merchant account mapped to another firm | Rejected | T23 | Designed | — |
| AT-SEC-01 | Credential rotation without downtime for one firm | Old account dropped; no failed requests | T14 | Designed | — |
| AT-SEC-02 | App-to-database TLS required | Plain connection refused | T15 | **Blocked**: host not chosen | — |
| AT-MFA-01, AT-REVOKE-01 | MFA required; session epoch revocation | Denied without MFA; old tokens rejected | T4, T24 | **Blocked**: identity provider not chosen | — |
| AT-SUP-01 | Support access without approval, expired, or self-approved | Denied; CHECK rejects self-approval | T25 | Designed (CHECK in DDL, not executed) | — |
| AT-LOG-01 | Logs for a matter request contain no narrative or names | Assertion over the captured log | T26 | Designed | — |
| AT-FIN-01 | Balanced posting; unbalanced, edit, delete, add-line, too few lines, replayed key | Posted; 1644 ×5 (the line update blocked by grant 1142); 1062 | T27 | **DB-validated, passed** (second run; the first run **failed** because triggers were absent, ERROR 1419) | L1–L8 |
| AT-FIN-02 | Concurrent trust withdrawals that together overdraw | One commits; one fails 3819; balance never negative | T28 | **DB-validated, passed** | L9, R2 |
| AT-FIN-03 | Post races an unbalancing line | Post rejected; entry stays Draft | T29 | **DB-validated, passed** | R1 |
| AT-FIN-04 | Approval of a changed version | 1452; content change blocked with 1451 | T30 | **DB-validated, passed** | L10–L12 |
| AT-FIN-05 | Time amount rounding; double billing | 3819; 1062 | — | **DB-validated, passed** | L13, L14 |
| AT-FIN-06 | Payment over-allocation | 3819 | — | **DB-validated, passed** | L18 |
| AT-PROV-01 | Re-run provisioning with the same idempotency key | Same firm and placement; no duplicates | — | Designed | — |
| AT-PROV-02 | Kill provisioning after step 5, then resume | Resumes at step 6; readiness gates pass | — | Designed | — |
| AT-PROV-03 | Restricted user cannot create triggers | 1419; privileged definer step succeeds | — | **DB-validated, passed** | apply 13 (both firms) |
| AT-MIG-01 | Migration fails midway | Earlier DDL persists (implicit commit); target `Failed`; resume works | — | Engine behavior **DB-validated** (E4, E5); migrator designed | E4, E5 |
| AT-MIG-02 | Canary failure halts the fleet | Later waves not started | — | Designed | — |
| AT-MIG-03 | Drift: an index dropped by hand | Detected; target quarantined | — | Designed | — |
| AT-MIG-04 | Data survives expand/backfill/contract (`StorageKey` → `document_versions`) | Counts and checksums equal | — | Designed | — |
| AT-ENG-01..05 | Partitioning with FKs; implicit commit; online DDL algorithms; collation of FKs; CHECK and generated uniqueness | As documented | — | **DB-validated, passed** | E1–E12b |
| AT-REST-01 | Restore firm A to a point while firm B is unaffected; holds and revocations kept | B checksums unchanged; A at the point; current holds present | T32 | Designed; **blocked** on host backup tooling for a realistic rehearsal | — |
| AT-IDX-01 | Representative query plans use the intended indexes | As in the [index spec](../database/query-and-index-spec.md) | — | **DB-validated, passed** for the intended shapes. Earlier runs showed table scans when a GUID was bound as a utf8mb4 variable (kept as Q0). The `Status IN (...)` invoice queue does a full scan plus sort (Q5a), so it is replaced by `OpenMarker` (Q5a2) | Q0–Q8c |
| AT-IDX-02 | SQL actually emitted by EF binds GUIDs so indexes are used | Index lookups in the captured plans | — | Designed | — |
| AT-SCALE-01 | Pool and connection behavior with many firms under load | Within budget; pool-wait p95 recorded | — | Designed; needs a load harness | — |
