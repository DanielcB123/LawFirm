# Implementation gap map

[Home](../README.md) · [Codebase audit](../architecture/current-codebase-audit.md) · [Roadmap](roadmap-and-acceptance.md) · [ADR-001](../database/ADR-001-tenancy.md)

Traceability from requirement to current code, as of commit `61e5134`. "Current" cites inspected paths (backend paths relative to `backend/`, frontend relative to `frontend/src/`). **Owner is "Unassigned" until the founder assigns people.** Status values: **Absent**, **Partial**, **Unsafe** (exists but violates a requirement), **Present**. Detailed requirements live in the linked domain documents.

## Foundation: identity, tenancy, platform

| ID | Requirement | Current | Needed change | Depends on | Risk | Acceptance evidence | Owner / status |
|---|---|---|---|---|---|---|---|
| G01 | Firm tenant + control plane ([ADR-001](../database/ADR-001-tenancy.md)) | Absent | Control-plane DB: firm, domain, placement, membership, subscription status | G06 | Critical | Firm catalog resolves placement; unknown firm fails closed | Unassigned / Absent |
| G02 | Trusted per-request firm context | Absent; single `ConnectionStrings:LawFirm` plus hard-coded fallback in `Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs` | `IFirmContext` middleware, per-firm `DbContext` factory, `firm_identity` self-check, remove fallback | G01, G03 | Critical | Firm A token cannot reach Firm B on any route; mismatched host/claim → 403 | Unassigned / Absent |
| G03 | Persistent identity, membership, revocation | `Infrastructure/Auth/DemoUserStore.cs`, registered in all environments | Persistent identities; membership with version; gate demo store to Development; login rate limiting | G01 | Critical | Revoked membership denied within cache TTL; demo login fails outside Development | Unassigned / Unsafe |
| G04 | Staff MFA, managed IdP, session handling ([security](../security/README.md)) | JWT in `localStorage` (`features/auth/AuthContext.tsx`); no refresh/revocation | OIDC IdP with MFA; short-lived tokens; consider BFF cookie (HttpOnly) for SPA | G03 | High | MFA enforced for staff; logout/revocation invalidates sessions | Unassigned / Absent |
| G05 | Secrets out of source control | Signing key and DB credentials in tracked `appsettings*.json` | User-secrets/secrets manager; example config; rotate | — | High | Secret scan clean; startup fails without configured key | Unassigned / Unsafe |
| G06 | Versioned schema migrations | `EnsureCreated` + raw DDL | EF Core migrations baseline; fleet runner per firm DB | — | High | Fresh DB and existing dev DB both reach the same migration head | Unassigned / Absent |
| G07 | Automated tests on the real engine | Absent | xUnit integration project with MySQL (e.g. Testcontainers) + `WebApplicationFactory` | — | High | CI runs authorization, isolation, and concurrency tests | Unassigned / Absent |
| G08 | CI with build/lint/test gates | Absent; `npm run lint` currently fails (7 errors) | Fix lint; add pipeline | G07 | Medium | Green pipeline on PR | Unassigned / Absent |

## Confidentiality and authorization ([confidentiality](../security/confidentiality-and-access.md))

| ID | Requirement | Current | Needed change | Depends on | Risk | Acceptance evidence | Owner / status |
|---|---|---|---|---|---|---|---|
| G10 | Matter access by assignment/grant | `Features/Matters/MattersEndpoints.cs` checks permission claim only; lists all matters | `MatterAccessGrant`/team model; query-level filter in one shared helper for list, detail, child routes | G07 | Critical | Unassigned paralegal gets 404 on list/detail/tasks/docs/timeline | Unassigned / Unsafe |
| G11 | Admin policy consistent with permissions | Admins lack `matters.read.assigned`; no route uses `MatterReadAll` | Resolve via G10 policy: "assigned OR all" evaluated server-side | G10 | Medium | Admin access matches documented role matrix | Unassigned / Unsafe |
| G12 | Clients cannot write to unrelated matters | Client holds `documents.upload`; `CreateDocument` checks only existence | Require matter grant; separate client upload route | G10, G32 | Critical | Client POST to ungranted matter → 404, no row written | Unassigned / Unsafe |
| G13 | Restricted parties masked on every surface | `Features/Parties/PartiesEndpoints.cs` returns restricted names unmasked | Shared redaction serializer used by parties and conflicts | G07 | High | Paralegal search never returns restricted display name/email/aliases | Unassigned / Unsafe |
| G14 | Prospective-client records restricted | `GET /api/intake` lists all to all staff | Owner/team/role scoping; restricted intake flag | G10 | High | Non-team staff cannot list or count restricted intakes | Unassigned / Partial |
| G15 | Ethical walls | Absent | Wall membership + deny precedence over roles | G10 | High | Walled user denied in list, search, conflicts, exports | Unassigned / Absent |
| G16 | Audit atomic, append-only, minimal PII | `Infrastructure/Audit/AuditTrailService.cs` saves separately; mutable table; display names in metadata | Same-transaction audit; DB grants deny UPDATE/DELETE to runtime user; bounded correlation ID; review metadata | G06 | Medium | Failed audit rolls back change; runtime user cannot alter audit rows | Unassigned / Partial |

## Intake, consultations, engagement ([intake](../modules/intake-and-consultations.md))

| ID | Requirement | Current | Needed change | Depends on | Risk | Acceptance evidence | Owner / status |
|---|---|---|---|---|---|---|---|
| G20 | No double booking | `Features/Consultations/ConsultationsEndpoints.cs` check-then-insert | Slot reservation table with unique `(resource, slot_start)` or locked transaction | G06, G07 | High | 20 parallel requests for one slot → exactly one success | Unassigned / Unsafe |
| G21 | Firm-local availability, appointment types | UTC 09:00–16:30 weekday grid | Firm time zone + staff availability + buffers; store zone with instant | G01 | Medium | Slots render in firm hours across DST boundary | Unassigned / Unsafe |
| G22 | Public endpoint abuse controls | No rate limit/CAPTCHA | ASP.NET Core rate limiter; bot protection; minimal response body | — | Medium | Burst beyond limit → 429 | Unassigned / Absent |
| G23 | Conflict decision bound to reviewed intake/party set | Any `Approved` decision ID accepts any intake (`Features/Intake/IntakeEndpoints.cs`) | Link check to intake and party set; invalidate on party change | G10 | High | Decision from another check is rejected; party change reopens review | Unassigned / Unsafe |
| G24 | Engagement before representation | Absent; `Accepted` → convert to matter | Engagement entity with fee-agreement version, signer evidence, attorney acceptance; matter `Active` only after engagement (introduces `FeeAgreementVersion`, later consumed by G50) | G23, e-signature provider decision | High | Unsigned engagement cannot activate matter; payment/booking never activates | Unassigned / Absent |
| G25 | Concurrency-safe matter numbering | max+1 read in `GenerateMatterNumberAsync` | Per-firm sequence row under lock | G06 | Low | Parallel conversions produce unique numbers, no 500 | Unassigned / Unsafe |

## Matters, deadlines, documents ([matters](../modules/matter-management.md), [documents](../modules/documents-and-evidence.md))

| ID | Requirement | Current | Needed change | Depends on | Risk | Acceptance evidence | Owner / status |
|---|---|---|---|---|---|---|---|
| G30 | Deadline provenance | `CalendarEntry` has source/verified fields, but verifier comes from request body (`Features/Calendar/CalendarEndpoints.cs`) | Verifier = authenticated attorney only; add jurisdiction, trigger fact/date, rule version, computation inputs; FK to matter | G10 | High | Spoofed verifier ignored; trigger change flags dependents | Unassigned / Partial |
| G31 | Date-only deadline semantics | Instants only (`datetime(6)`) | `DateOnly` + zone fields for date-based deadlines | G06 | High | Date-only deadline never shifts across zones/DST | Unassigned / Absent |
| G32 | Document binary pipeline | Metadata only; caller-supplied `StorageKey` | Private object store, opaque keys, quarantine → scan → available, short-lived reauthorized download | G02, G10 | Critical | Quarantined file not downloadable; wrong-firm key denied | Unassigned / Absent |
| G33 | Immutable versions + preservation holds | Hard deletes on documents, tasks, calendar | `DocumentVersion`; soft-retire with hold check; hold blocks delete/lifecycle | G32 | High | Delete under hold → 409; prior versions retrievable | Unassigned / Absent |
| G34 | Practice extensions | `PracticeArea` string only | Construction/PI typed extensions ([construction](../practice-areas/construction-law.md), [PI](../practice-areas/personal-injury.md)) | G10, firm discovery | Medium | Attorney-approved templates applied by version | Unassigned / Absent |

## Portal, communications, jobs ([portal](../modules/client-portal.md), [communications](../modules/communications.md))

| ID | Requirement | Current | Needed change | Depends on | Risk | Acceptance evidence | Owner / status |
|---|---|---|---|---|---|---|---|
| G40 | Client portal with client-only serializers | Static `/api/portal/client`; `RequireAuth` is token-only | Client routes/API; separate client DTOs; role-aware frontend routing | G10, G42 | Critical | Client sees only granted, published records; guessed IDs fail | Unassigned / Absent |
| G41 | Publication workflow | Absent | Draft → approved → published; audience + approver; version references | G10 | High | Unpublished content absent from client list/search/notifications | Unassigned / Absent |
| G42 | Secure messaging | Absent | Matter-scoped conversations with explicit participants | G10 | High | No cross-client message visibility | Unassigned / Absent |
| G43 | Outbox, background jobs, notifications | Absent; `RemindersJson` never dispatched | Per-firm outbox; dispatcher; minimal-content notifications with send-time recheck | G02, G06 | High | Revoked recipient not notified; duplicate event → one send | Unassigned / Absent |

## Billing and money ([billing](../billing/README.md), [ledger](../billing/payments-and-ledger.md), [trust](../billing/trust-and-settlement.md))

| ID | Requirement | Current | Needed change | Depends on | Risk | Acceptance evidence | Owner / status |
|---|---|---|---|---|---|---|---|
| G50 | Legal billing (fee agreements, time, invoices) | Absent; demo `/api/billing/*` | Versioned fee agreements; time/expense; invoice snapshots; `DECIMAL` money + currency | G10, G24 | High | Recomputed invoice equals issued snapshot exactly | Unassigned / Absent |
| G51 | Payments + verified, deduplicated webhooks | Absent | PSP adapter; payment intents; signed webhook receipt with unique provider event ID | G43, G01 | Critical | Replayed/out-of-order events → one effect; redirect ≠ paid | Unassigned / Absent |
| G52 | Trust/settlement accounting | Absent | Prefer reviewed integration; native ledger only with accountant review, balanced atomic postings, overspend locks, version-bound approvals | G50, G51 | Critical | Concurrent withdrawals cannot overspend; changed settlement voids approval; three-way reconciliation | Unassigned / Absent |
| G53 | SaaS subscription billing separate from legal billing | Absent | Control-plane subscription via provider; never shares merchant accounts with firm funds | G01 | Medium | Firm subscription lapse never deletes legal records | Unassigned / Absent |

## Operations and integrations ([recovery](../operations/reliability-and-recovery.md), [integrations](../integrations/README.md))

| ID | Requirement | Current | Needed change | Depends on | Risk | Acceptance evidence | Owner / status |
|---|---|---|---|---|---|---|---|
| G60 | One-firm restore with revocations preserved | Absent | Procedure per ADR-001; per-firm logical dumps; rehearsal | G01, G32 | High | Firm A restored, Firm B untouched, current revocations intact | Unassigned / Absent |
| G61 | Observability | Logger only; correlation ID echo | Structured logs with firm scope (no narratives); metrics; alerts | G02 | Medium | Dashboards show routing errors, outbox lag | Unassigned / Partial |
| G62 | Hosting/deployment | Absent | Select host; environments; IaC | G05 | Medium | Reproducible staging deploy | Unassigned / Absent |
| G63 | Legal research integration | Absent | Do not build until licensing/API verified; internal citation workspace only | Procurement | Low | Provider decision record exists | Unassigned / Absent |
| G64 | First firm provisioned through configuration | Fictional seed (`parkerreedlaw.com`) only | Provision Maldanado and Benitez Law in staging via the G01 path; synthetic matters only | G01–G03 | Medium | No firm name hard-coded in source | Unassigned / Absent |
