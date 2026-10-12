# Multi-firm threat model, resolution flow, and control matrix

[Home](../README.md) · [Security](README.md) · [Matter access policy](matter-access-policy.md) · [ADR-002](../database/ADR-002-multitenant-architecture.md) · [Acceptance matrix](../delivery/multitenant-acceptance-matrix.md)

**Status:** design.
- **Implemented today:** only the single-firm controls from F1 (matter grants, client portal scoping, restricted-party masking, demo auth limited to Development) and their 44 integration tests.
- **Executed with synthetic data:** the database-level controls marked "validated", on disposable MySQL 8.0.36 ([log](../../db/design/validation/mysql-8.0.36-validation.log)).
- **Not implemented or tested:** every application-level multi-firm control in this document.

**Principles.** None of these is a substitute for authorization: tenant columns, UUIDs, encryption, prepared statements, or separate databases. The firm boundary narrows *where* a query can go. Membership, role, matter grant, ethical wall, and audience checks still decide *what* a person may do. EF Core global query filters are application code, not database row-level security, and MySQL has no native row-level security.

## Resolution and authorization flow

```mermaid
sequenceDiagram
  autonumber
  participant C as Browser / portal
  participant E as Edge (TLS)
  participant A as API instance
  participant CP as Control plane (cached)
  participant S as Secret store
  participant F as Firm database
  C->>E: HTTPS request (Host, bearer token)
  E->>A: forward (Host preserved, private network)
  A->>A: validate token (issuer, audience, lifetime, MFA claim, sub, firm_id, mv, se)
  A->>CP: resolve Host -> firm (verified domain only)
  A->>A: token firm_id == host firm? else 401/404, no fallback
  A->>CP: membership(firm, identity): Active and version == mv, identity SessionEpoch == se
  A->>CP: firm Status Active/ReadOnly; current placement; SchemaVersion within app range
  A->>S: runtime secret for placement (cached, rotated by version)
  A->>F: open lease from firm pool (pool key = firm connection string)
  A->>F: SELECT FirmId, PlacementRoutingVersion FROM firm_identity
  A->>A: mismatch -> close, 503, alert
  A->>F: authorized query (F1 MatterAccessService, ethical wall, audience)
  A->>F: domain write + audit + outbox in one transaction
  A-->>C: allowed DTO only (masking applied)
```

Rules:
1. The firm comes **only** from a verified host plus a matching signed token claim. Header, route, query, body, cookie, and subdomain-label inputs are never authoritative. A conflicting value is a 400 that is logged as an attack signal.
2. The database name, server, and credentials come **only** from the placement catalog, through `firm_placements`. The application never accepts a database name or connection string from input, and the name pattern is enforced by CHECK.
3. Firm context is a **scoped** object that is built once per request and is immutable. It is never held in a static or singleton, and never shared across async flows. Background jobs build their own context from the job's firm id.
4. Fail closed in every one of these cases, with no default connection string: a missing firm, an unverified domain, a suspended firm, an incompatible schema, a failed identity check, or a failure to resolve the secret.

## Threats and controls

| # | Threat | Control | Status | Test ids |
|---|---|---|---|---|
| T1 | Forged or unverified host resolves a firm | Only `firm_domains.Status = Verified` routes; host normalized (lower-case, punycode); unknown host → generic 404 | Designed | AT-RES-01 |
| T2 | Token for firm A presented on firm B's host | Token `firm_id` must equal the host's firm; audience per environment | Designed | AT-RES-02 |
| T3 | Untrusted tenant selector (header, body, query, route) | Ignored for resolution; conflicting value rejected and logged | Designed | AT-RES-03 |
| T4 | Removed member keeps access | `MembershipVersion` and `SessionEpoch` in the token, rechecked per request (short-TTL cache invalidated on change); refresh denied | Designed | AT-RES-04, AT-REVOKE-01 |
| T5 | Suspended or offboarding firm still served | `firms.Status` gate before any firm query | Designed | AT-RES-05 |
| T6 | Catalog points at the wrong database or secret | `firm_identity` check per lease, compared with the routing version | Designed (DB part validated: G8, G12) | AT-RES-06 |
| T7 | App and schema version skew | App declares a compatible migration range; out-of-range firm → maintenance response | Designed | AT-RES-07 |
| T8 | Arbitrary database name or connection string injected | Names generated (`mf_f_<hex>`), CHECK pattern, never from input; no string concatenation of names into SQL | Designed (CHECK in DDL) | AT-RES-06, AT-DB-01 |
| T9 | Cross-firm read through a bug in a query | Per-firm runtime account with table-level grants; the server denies other databases | **Validated** (G1, G2, G3) | AT-DB-01 |
| T10 | Runtime account alters or drops schema | Runtime has no DDL privileges; migration account is separate and scoped to one database | **Validated** (G4–G6, G14) | AT-DB-02, AT-DB-05 |
| T11 | Tampering with audit or immutable versions | Runtime has only SELECT, INSERT on append-only tables; no UPDATE on journal lines; triggers block posted-entry edits | **Validated** (G7, G8, L3–L6) | AT-DB-03, AT-FIN-01 |
| T12 | Database-wide grant used for convenience, then a table-level REVOKE assumed to work | Only table-level runtime grants, generated from a manifest and re-applied by every migration | **Validated** (G16: REVOKE fails with 1147 and the update still succeeds) | AT-DB-06 |
| T13 | Control-plane account used to read firm data | Control-plane runtime account has no firm grants | **Validated** (G15) | AT-DB-04 |
| T14 | Secrets leaked or stale | Secret store only; `secret_references` hold pointers and versions; rotate by creating a new account or password, deploying, then dropping the old one; never log connection strings | Designed | AT-SEC-01 |
| T15 | Eavesdropping or lateral movement | TLS from browser to edge; TLS (`SslMode=VerifyFull`) from app to MySQL over a private network; no public MySQL endpoint; storage and backups encrypted at rest with provider-managed or customer keys (hosting decision) | Designed | AT-SEC-02 |
| T16 | One shared app process holds every firm's credentials (residual) | Cell affinity limits each instance to its cell's firms; secrets fetched lazily and cached in memory with a TTL; least-privilege runtime accounts; per-firm `MAX_USER_CONNECTIONS`; anomaly alerts on cross-firm access patterns | Designed; **residual risk accepted for review** | AT-CTX-01 |
| T17 | Firm context leaks across concurrent requests or pooled connections | Scoped context; one pool per firm connection string, so a connection is never re-pointed; no `ChangeDatabase`; `DbContext` built per request from the placement | Designed | AT-CTX-01, AT-CTX-02 |
| T18 | Raw SQL or `IgnoreQueryFilters()` bypasses application filters | Under database-per-firm the server still limits access to one database. Inside the firm, all matter-scoped reads go through `IMatterAccessService`; an analyzer or grep gate on `FromSql`, `ExecuteSql`, and `IgnoreQueryFilters` | Designed | AT-AUTHZ-05 |
| T19 | Background job, cache, file, search, export, or notification crosses firms | Firm id in every envelope, key, prefix, and index name; jobs re-resolve the placement and re-authorize when they run; notifications re-check the audience before sending | Designed (outbox claim validated: R3) | AT-JOB-01, AT-JOB-02, AT-CACHE-01, AT-FILE-01, AT-SEARCH-01, AT-EXPORT-01, AT-NOTIFY-01 |
| T20 | Matter-level escalation inside a firm | F1 `MatterAccessService`; grants managed by the responsible attorney; ethical walls override roles and grants | F1 **implemented and tested**; walls designed | AT-AUTHZ-02, AT-AUTHZ-03 |
| T21 | Client or representative sees unpublished content | Explicit audiences; publication state machine with an approver; representative scope | Designed (client portal scoping implemented in F1) | AT-AUTHZ-04 |
| T22 | Forged or replayed payment webhook; event mapped to the wrong firm | Verify the signature before storing (CHECK requires verified); map provider account → firm in the control plane; dedup unique per (provider, account, event) | DB part **validated** (L15–L17) | AT-HOOK-01 |
| T23 | Merchant account confusion between firms | Each firm's provider account reference is stored in its own database and checked against the control-plane mapping; payments are unique per (provider, account, reference) | Designed | AT-HOOK-02 |
| T24 | Account takeover | MFA required for staff (`RequiresMfa`); step-up for trust disbursement and grant changes; session revocation through the epoch | Designed | AT-MFA-01 |
| T25 | Over-broad platform support access | Firm-approved, scoped, expiring `support_access_requests`; approver ≠ requester (CHECK); every support action audited in both the firm and the control plane | Designed (CHECK in DDL) | AT-SUP-01 |
| T26 | Logs or telemetry leak confidential content | Log ids, codes, and firm id only; no narratives, names, or payloads; error codes only in operation tables | Designed | AT-LOG-01 |
| T27 | Unbalanced or edited ledger | Posting trigger requires balanced lines and at least 2 lines; posted entries immutable; reversal entries; idempotency keys | **Validated** (L1–L8) | AT-FIN-01 |
| T28 | Concurrent overdraft of trust funds | Row lock plus `CHECK (Balance >= 0)` | **Validated** (L9, R2) | AT-FIN-02 |
| T29 | A line added to an entry while it is being posted | The line trigger locks the entry row; posting rechecks with a locking read | **Validated** (R1) | AT-FIN-03 |
| T30 | Approval reused after the amount or payee changes | Approval bound by FK to `(Id, ContentSha256)` | **Validated** (L10–L12) | AT-FIN-04 |
| T31 | Pooled tier: cross-firm reference or re-parenting | Composite FKs plus FirmId immutable (column grants) | **Validated** (P1–P4) | AT-POOL-01 |
| T32 | Restore rolls back revocations or holds | Restore one firm into staging; replay control-plane state; never restore the control plane over current state | Designed | AT-REST-01 |

## Residual risks (accepted only after review)
- **Shared application process (T16).** A remote-code-execution bug in the API or a dependency exposes every firm reachable from that instance. Database-per-firm limits accidental leaks, not a compromised process.
- **Privileged provisioning step.** Creating triggers needs SUPER, or `log_bin_trust_function_creators = 1`. Either the pipeline holds an administrative credential, or the server accepts less-safe routine logging. A hosting decision is needed.
- **Platform operators** with server administrator rights can read any firm. That needs contractual, procedural, and audit controls; technology alone doesn't cover it.
- **Single-firm restore** goes through an isolated server copy that briefly contains every firm on that server. That copy needs the same access controls, and must be destroyed with evidence.
- **Application-layer authorization inside a firm** remains the only control between users of the same firm.
