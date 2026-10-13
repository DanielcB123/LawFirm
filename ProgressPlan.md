# ProgressPlan

**Last updated:** 2026-10-12 21:45 EDT (UTC−04:00). **Primary handoff document.** Read this first, then the latest [SLICE_HISTORY.md](SLICE_HISTORY.md) entries, then [Markdown/README.md](Markdown/README.md). Agent rules: [AGENTS.md](AGENTS.md).

Status vocabulary: **Planned**, **In progress**, **Blocked**, **Implemented—verification pending**, **Verified within stated scope**. "Verified" always names what was exercised. Code existing, code building, behavior verified by integration tests, and production readiness are separate claims. Nothing in this repository is production ready. Owner is **Unassigned** for everything unless stated.

## Mission and first firm

One application that replaces a firm's separate website, intake/CRM, and practice-management products, later offered as multitenant SaaS where **the tenant is a law firm**. First firm: **Maldanado and Benitez Law** (construction law, litigation, personal injury). Details: [first firm](Markdown/product/first-firm.md), [vision and scope](Markdown/product/vision-and-scope.md). No real firm or client data is in the repository; seed data is fictional (`parkerreedlaw.com`).

## Stack

.NET 8 minimal API (`backend/`), EF Core 8.0.13 with Pomelo 8.0.3 on MySQL 8 (server version pinned to 8.0.36), React + TypeScript + Vite (`frontend/`), integration tests in `backend.IntegrationTests/` (xUnit, `WebApplicationFactory`, Testcontainers 3.10.0), unit tests in `backend.UnitTests/`, frontend tests with Vitest. `MatterForge.sln` with Central Package Management at the root. Details: [stack and runtime](Markdown/architecture/stack-and-runtime.md), [solution structure](Markdown/architecture/solution-structure.md), [codebase audit](Markdown/architecture/current-codebase-audit.md).

## Naming status (two separate facts)

- **Branding:** "MatterForge" is a **working product name**, not cleared public branding. No trademark or domain clearance has been done. It is applied to the README titles, the browser title, and the Swagger title only.
- **Repository folder:** **not renamed.** It is still `D:\Documents\Projects\LearnCSharpDotNet`. The rename was unsafe because the API and the Vite dev server were running from the folder and the Cursor workspace was open. The four manual steps and the identifier inventory are in the [naming and rename record](Markdown/product/naming-and-rename.md). Namespaces, JWT issuer and audience, connection-string key, user-secrets id, localStorage key, Git remote, and database names are unchanged on purpose.

## Tenancy status

**Single firm only in code.** One connection string, one database, no firm identity anywhere in the schema or tokens. Cross-firm isolation in the application does not exist and has not been tested.

**Architecture: designed, not implemented.** [ADR-002](Markdown/database/ADR-002-multitenant-architecture.md) refines ADR-001: database per firm, plus placement cells, connection budgets, table-level runtime grants, a `firm_identity` check, and a privileged trigger step. The strongest alternative is a hybrid. Reviewable SQL and a disposable-MySQL validation run are in [db/design](db/design/README.md). These validate engine behavior only, not the application.

## Capabilities and verification level

| Capability | Code | Verification | Notes |
|---|---|---|---|
| Matter-level authorization (staff assignment/grant, client grant, write checks, uniform 404) | Present | **Verified within stated scope**: integration tests on MySQL 8.0.36, single firm | [Policy and matrix](Markdown/security/matter-access-policy.md) |
| Client portal API (`/api/portal/matters`) | Present | **Verified within stated scope** (API only) | No portal frontend |
| Restricted-party masking (directory, aliases, relationships, conflicts) | Present | **Verified within stated scope** | Audit metadata still holds names (G16) |
| Calendar entries scoped by linked matter | Present | **Verified within stated scope** | Link is matter-number text, not a foreign key |
| EF Core migrations, refusal to touch legacy/unmigrated databases | Present | **Verified within stated scope** on throwaway databases | Legacy baseline runbook not executed on a real database |
| Demo sign-in gated to Development opt-in; no tracked secrets in current files | Present | Login 503 when disabled is tested; Development opt-in path not exercised | Historical secrets remain in Git history; rotation not done |
| Parties, conflicts, intake, matters workspace, calendar, consultations, public site | Present (earlier slices) | Build only, plus the authorization tests above | See parity matrix below |
| Billing, trust, payments, documents binary pipeline, MFA, multitenancy | Absent | — | See [gap map](Markdown/delivery/implementation-gap-map.md) |

## Completed work

| ID | Work | Status |
|---|---|---|
| A0 | Repository audit and linked `Markdown/` documentation (commit `91a9ded`) | Verified within stated scope (documentation; links checked; app code unchanged; DB/API not exercised) |
| F1 | Real-MySQL test harness + matter-level authorization + restricted-party masking (committed `0fc6831`) | Verified within stated scope |
| F2 (part) | Migrations baseline, startup refusal for unmanaged DBs, secrets removed from tracked config, demo identity gated | Implemented—verification pending for existing-DB baselining and Development opt-in; remaining F2 item (same-transaction append-only audit, G16) **Planned** |
| R1 | Solution structure: root build files, backend entity/contract split, not-implemented module and provider boilerplate, unit test project, feature-based frontend, Vitest, `Markldown/` archived under `Markdown/` (uncommitted) | **Verified within stated scope**: solution build, 44 integration + 10 unit + 4 frontend tests, no EF model change, frontend build; lint at baseline. No runtime behavior change intended or observed |
| M0 | MatterForge working-name branding plus multi-firm architecture: ADR-002, data dictionary, query and index spec, threat model, runbooks, acceptance matrix, implementation plan, `db/design` SQL and validation (committed `0fc6831`) | **Verified within stated scope**: design SQL applied and checked on disposable MySQL 8.0.36 with synthetic data (log in `db/design/validation/`); docs link-checked. No application multi-firm code; nothing deployed; no existing database touched |

## Current step: R1 solution structure, boilerplate, and documentation folder cleanup

**Status: Verified within stated scope.** Not committed (working tree on top of `0fc6831`). Scope: file and folder organization, root build files, not-implemented boilerplate, test scaffolding, and documentation. No endpoint behavior, schema, or authorization rule changed. Full detail: [SLICE_HISTORY.md](SLICE_HISTORY.md) entry `2026-10-12 - Slice R1`, and [solution structure](Markdown/architecture/solution-structure.md).

| Criterion | Result |
|---|---|
| One documentation folder | Done: `Markldown/` moved with `git mv` to `Markdown/archive/original-prompt/`; live links updated; AGENTS.md rule updated |
| Backend root build files | Done: `MatterForge.sln`, `Directory.Build.props`, `Directory.Packages.props` (same package versions as before), `global.json`, `.editorconfig` |
| Backend split without behavior change | Done: entities into `Infrastructure/Persistence/Entities/`, contracts into `Features/<Module>/Contracts/` (namespaces unchanged), Swagger setup into `Infrastructure/OpenApi/`; template `Features/WeatherForecast` deleted. 44 of 44 integration tests pass; no EF model changes |
| Backend boilerplate | Done, **not implemented**: `Common/Paging`; `IFirmContext`, `IOutbox` + unregistered `OutboxDispatcher`, `IDocumentStorage`, `IEmailSender`, `IPaymentGateway`, `ISignatureProvider`; empty authenticated route groups for Documents, Engagements, Billing, Payments, Trust, Communications; `TimeProvider.System` registered |
| Unit test project | Done: `backend.UnitTests` (10 tests, paging) |
| Frontend feature layout | Done: `app/` (providers, routes, shell), `features/<x>/` with `index.ts`, `lib/apiClient.ts`, `config/env.ts`, `components/layout/`, `pages/public/` and `pages/workspace/`; template `App.tsx`/`App.css`/assets deleted; lint rule blocks deep feature imports |
| Frontend boilerplate | Done: `features/portal` (calls the existing `/api/portal/matters` routes; no pages yet), `features/billing` draft types, `types/money.ts`, `.env.example`, Vitest (4 tests) |
| Lint | Unchanged at the baseline: 7 errors, 3 warnings, all pre-existing |

## Previous step: M0 multi-firm architecture (design) and working-name branding

**Status: Verified within stated scope.** Committed in `0fc6831`. The scope is architecture documentation, reviewable SQL, and validation on a disposable database. It does not implement tenancy or move any records.

| Criterion | Result |
|---|---|
| Rename inventory classified; branding changed only where user-visible | Done ([naming record](Markdown/product/naming-and-rename.md)) |
| Folder rename | **Not done: unsafe while processes were running.** The owner performs the manual steps |
| Tenancy options compared at 1, 100, 1,000, and 10,000 firms, with reconsideration triggers | Done ([ADR-002](Markdown/database/ADR-002-multitenant-architecture.md)). No hosting costs or limits invented |
| Control-plane and firm schemas, required versus future | Applied cleanly to disposable MySQL 8.0.36 as a restricted migration user, except triggers: ERROR 1419 led to the privileged DEFINER step |
| Isolation grants, pooled-model limits, money and approval invariants, races | All checks matched expectations (G1–G16b, P1–P4b, L1–L18, R1–R4) |
| Representative query plans | Recorded ([query and index spec](Markdown/database/query-and-index-spec.md)). Synthetic data, so no latency or scale claim |
| Threat model, runbooks, acceptance matrix, plan | Written. Application-level cases are **Designed** only |

## Earlier step: F1 matter authorization (+ F2 schema/secrets prerequisites)

**Status: Verified within stated scope.** Committed in `0fc6831`. Scope: single firm, API level, synthetic data, disposable MySQL 8.0.36 containers.

Acceptance criteria and result:

| Criterion | Result |
|---|---|
| Unauthenticated requests rejected | Pass (10 routes, 401) |
| Assigned staff can read; unassigned staff denied with 404 identical to nonexistent | Pass |
| Authorized client sees only granted matters, minimal fields | Pass |
| Client guessing another client's matter id | Pass (404 identical to nonexistent) |
| Client creating a document on another client's matter | Pass (403 on staff route, 404 on portal route, no row written) |
| Direct child-resource ids on inaccessible matters | Pass (404, rows unchanged) |
| Child reached through a different, accessible parent route | Pass (404, row stays on original matter) |
| Restricted-party exposure | Pass (directory, alias, relationship, conflicts incl. snapshot) |
| Revoked access | Pass (staff and client grants) |
| Lists and calendar filtered | Pass; no search/export/count endpoints exist to test |
| Admin access documented and tested | Pass (no implicit access; grant gives access) |
| Grant constraints (one active grant, valid type, matter exists) | Pass (enforced by MySQL) |
| Migration-backed test schema; unmanaged DB refused | Pass |
| Fail-before / pass-after | Executed: 26 of 43 failed before enforcement; 44 of 44 pass after |

## Latest check results (2026-10-12, slice R1, about 21:20–21:45 EDT)

| Check | Command | Result |
|---|---|---|
| Solution build | `dotnet build MatterForge.sln -c Release` | Success, 0 warnings, 0 errors |
| Backend build (alternate output) | `dotnet build -o ./.build-out` in `backend/` | Success, 0 warnings (run after the entity/contract split, before boilerplate; the solution build above covers the final tree) |
| Integration tests | `dotnet test backend.IntegrationTests -c EfDesign` | 44 passed, 0 failed (MySQL 8.0.36 container), on the final backend tree |
| Unit tests | `dotnet test backend.UnitTests -c Release --no-build` | 10 passed |
| Migration drift | `dotnet tool run dotnet-ef migrations has-pending-model-changes --project backend --configuration EfDesign` with a dummy `ConnectionStrings__LawFirm` environment variable (never opened) | "No changes have been made to the model since the last migration." Without the variable the command fails at design time with "Connection string 'LawFirm' is not configured"; the same failure happens on an untouched checkout of `0fc6831`, so it predates R1 |
| Frontend build | `npm run build` in `frontend/` | Success |
| Frontend tests | `npm test` in `frontend/` | 4 passed (Vitest 5.0.4) |
| Frontend lint | `npm run lint` in `frontend/` | Fails: 7 errors, 3 warnings, the same rules and counts as the audit baseline (files now under `pages/workspace/`); the new boundary rule fires on a deep-import probe and reports nothing in the tree |
| npm audit (production deps) | `npm audit --omit=dev` | 2 moderate advisories in `react-router` 6.x; `react-router-dom` `^6.30.1` is unchanged by R1. Not fixed |
| Design validation | `db/design/run-validation.ps1` | Not re-run (no `db/design` changes); last run passed during M0 |
| Markdown relative links | PowerShell check over all 52 `.md` files, anchors included | 407 relative links, 0 broken |

## Not tested

- Starting the API in Development with real user-secrets, demo login, and seeding against a new database (only the seeder itself is tested, against a throwaway database).
- Baselining any existing `EnsureCreated` database (runbook only).
- The frontend against the new API behavior (calendar free-text matter references now get 400; admins now see an empty matter list until granted; conflict match ids for masked matches change every search).
- Concurrency of grant creation beyond the unique index; performance of the `EXISTS` grant subqueries at scale.
- Anything cross-firm.

## Prioritized remaining work

| Order | Work | Depends on | Status |
|---|---|---|---|
| 1 | Review and commit the R1 working tree; rotate the credentials listed below | — | Planned (owner action) |
| 2 | Decide multitenant database architecture and controls | F1 | **Design done (M0)**; owner review of ADR-002 and of the [open decisions](Markdown/delivery/open-decisions.md) pending |
| 2a | T1: minimal control plane, trusted firm resolution, per-firm DbContext routing, two synthetic firm databases, cross-firm tests ([plan](Markdown/delivery/multitenant-implementation-plan.md)) | Commit (1) | Planned |
| 3 | F2 remainder: same-transaction, append-only audit (G16) | Migrations (done) | Planned |
| 4 | F3: lint fixes + CI running `dotnet build MatterForge.sln`, both test projects, frontend build, lint, and `npm test` (G08). Decide TanStack Query there (it removes the `set-state-in-effect` pattern). Upgrade `react-router-dom` for the audit advisories | Tests (done) | Planned |
| 5 | Frontend: client portal pages (use `features/portal`, add `pages/portal/`); handle 400 on calendar matter reference; show grant management | Portal API (done) | Planned |
| 6 | F4–F5: control plane, persistent identity/membership, firm context, per-firm DbContext, cross-firm tests | Decision 2 | Planned |
| 7 | Remaining order F6–F13 in [roadmap](Markdown/delivery/roadmap-and-acceptance.md) | — | Planned |

## Defects, risks, blockers, decisions

- **Blocker for the user's existing local database:** the API now refuses to start against a database created by the old `EnsureCreated` path. Use a new empty database or follow [migration baseline](Markdown/database/migration-baseline.md). If `matters.Id` uses a utf8mb4 collation, the access-grant migration fails with MySQL error 3780 (reproduced on a synthetic table).
- **Credentials needing rotation (not rotated):** the JWT signing keys formerly in `backend/appsettings.json` and `backend/appsettings.Development.json`, and the MySQL password(s) formerly in those files and in the hard-coded fallback connection string (including the `lawfirm_app` development account). They remain in Git history; history was not rewritten. Demo account passwords in `backend/DEV_AUTH_CREDENTIALS.md` are public by design and must never exist outside local Development.
- **Decision recorded:** admins have no implicit matter access. **Pending firm confirmation:** attorneys keep firm-wide access until ethical walls exist.
- **Decision recorded:** clients use separate `/api/portal/matters` routes and DTOs; staff routes reject clients.
- **Risk:** grant actor ids are free text (no identity directory).
- **Risk:** the user's running API process holds `backend/bin/Debug`; builds use `-o ./.build-out`, `-c EfDesign` (project level only), or `-c Release` (solution level).
- **Note (R1):** the running `dotnet run` process was started before R1 and still serves the old build until restarted. After restart it builds the new tree; behavior should be identical except that `/api/weather/forecast` and `/api/weatherforecast` (template routes) no longer exist.
- **Note (R1):** boilerplate interfaces and empty route groups are placeholders. Nothing registers or calls them; don't count them as features.

## Exact next action and resume instructions

1. Optionally rename the folder by hand: stop project terminals and app processes, close the Cursor workspace, rename `LearnCSharpDotNet` to `MatterForge` in File Explorer, and reopen `D:\Documents\Projects\MatterForge` in Cursor. Update the paths in any prompt afterward.
2. Review and commit the R1 working tree (SLICE_HISTORY entry `2026-10-12 - Slice R1`). F1 and M0 are already in `0fc6831`. `.gitignore` excludes `backend.UnitTests/bin/` and `obj/` as well as the integration test outputs.
3. Rotate the credentials above, then set local secrets as in [README.md](README.md#running-locally).
4. Start T1 with the prompt in the M0 history entry. Inputs: [ADR-002](Markdown/database/ADR-002-multitenant-architecture.md), [threat model](Markdown/security/multitenant-threat-model.md), [acceptance matrix](Markdown/delivery/multitenant-acceptance-matrix.md), [plan](Markdown/delivery/multitenant-implementation-plan.md).
5. To re-verify: start Docker, then from the repository root run `dotnet build MatterForge.sln -c Release`, `dotnet test backend.UnitTests -c Release`, `dotnet test backend.IntegrationTests -c EfDesign`, and `powershell -ExecutionPolicy Bypass -File db/design/run-validation.ps1`; in `frontend/` run `npm run build` and `npm test`.

---

# Clio-parity tracker

## Purpose

This document is the working, detailed parity plan for building this law-firm platform to the point where the firm can operate without subscribing to Clio.

Goals:

1. Track what Clio offers at a capability level.
2. Track what this repository currently has.
3. Quantify each module as:
   - `Implemented`
   - `Partial`
   - `Not Implemented`
4. Define exactly what it takes (product + technical work) to close gaps.
5. Keep this file current during development.

---

## Benchmark references

Primary references used for parity scope (public docs/pages):

- [Clio Manage](https://www.clio.com/manage/)
- [Clio Features](https://www.clio.com/features/)
- [Clio Pricing / plans](https://www.clio.com/pricing/)
- [Clio Case Management](https://www.clio.com/features/case-management/)
- [Clio Online Intake Forms](https://www.clio.com/features/online-intake-forms/)
- [Clio Legal CRM](https://www.clio.com/features/legal-crm-software/)
- [Clio Payments](https://www.clio.com/features/payments/)
- [Clio Integrations](https://www.clio.com/features/integrations/)
- [Clio + QuickBooks integration](https://www.clio.com/app-directory/quickbooks-online/)
- [Clio + Microsoft 365 integration](https://www.clio.com/app-directory/microsoft-office-365/)
- [Clio + Google Drive integration](https://www.clio.com/app-directory/google-drive/)
- [Clio + DocuSign integration](https://www.clio.com/app-directory/sign-with-docusign/)

---

## How scoring works

- `Implemented`: production-relevant workflow exists in both backend and frontend for day-to-day use.
- `Partial`: foundational objects/endpoints/UI exist but significant workflow depth, controls, or integrations are missing.
- `Not Implemented`: no meaningful workflow exists yet.

Capability score bands:

- `100%`: parity-ready for that feature set.
- `40-80%`: partial with viable foundation.
- `0-30%`: mostly missing.

These scores are directional, not legal/compliance guarantees.

---

## Current architecture inventory (repo evidence)

### Backend modules present

- Auth and identity:
  - `backend/Features/Auth/AuthEndpoints.cs`
  - `backend/Features/Identity/IdentityEndpoints.cs`
  - `backend/Infrastructure/Auth/*`
- Parties/contacts:
  - `backend/Features/Parties/PartiesEndpoints.cs`
- Conflicts:
  - `backend/Features/Conflicts/ConflictEndpoints.cs`
- Calendar/tasks/deadlines:
  - `backend/Features/Calendar/CalendarEndpoints.cs`
- Intake:
  - `backend/Features/Intake/IntakeEndpoints.cs`
- Matters:
  - `backend/Features/Matters/MattersEndpoints.cs`
- Consultations:
  - `backend/Features/Consultations/ConsultationsEndpoints.cs`
- Persistence model:
  - `backend/Infrastructure/Persistence/LawFirmDbContext.cs`
- Audit trail:
  - `backend/Infrastructure/Audit/AuditTrailService.cs`

### Backend scaffolding (not implemented; added 2026-10-12, slice R1)

These score nothing toward parity. They fix where future code goes ([solution structure](Markdown/architecture/solution-structure.md)):

- Empty, authenticated route groups: `backend/Features/{Documents,Engagements,Billing,Payments,Trust,Communications}/`
- Provider interfaces: `backend/Infrastructure/{Tenancy,Outbox,Storage,Messaging,Payments,Signatures}/`
- Paging primitives: `backend/Common/Paging/`
- Frontend: `frontend/src/features/portal` (wired to the existing portal API, no pages), `frontend/src/features/billing` (draft types)

### Frontend modules present

- Public site and contact funnel:
  - `frontend/src/pages/public/HomePage.tsx`
  - `frontend/src/pages/public/AboutPage.tsx`
  - `frontend/src/pages/public/PracticeAreasPage.tsx`
  - `frontend/src/pages/public/AttorneysPage.tsx`
  - `frontend/src/pages/public/ContactPage.tsx`
- Authenticated workspace:
  - `frontend/src/pages/workspace/WorkspaceHomePage.tsx`
  - `frontend/src/pages/workspace/WorkspaceWorklistsPage.tsx`
  - `frontend/src/pages/workspace/TasksCalendarPage.tsx`
  - `frontend/src/pages/workspace/PartiesPage.tsx`
  - `frontend/src/pages/workspace/ConflictChecksPage.tsx`
  - `frontend/src/pages/workspace/IntakeBoardPage.tsx`
  - `frontend/src/pages/workspace/IntakeDetailPage.tsx`
  - `frontend/src/pages/workspace/MattersPage.tsx`
  - `frontend/src/pages/workspace/MatterWorkspacePage.tsx`
  - `frontend/src/pages/workspace/MatterWorkspaceTabs.tsx`
  - `frontend/src/pages/workspace/WorkspaceConsultationsPage.tsx`

---

## Clio parity matrix

## 1) Marketing website and lead capture

### What Clio-aligned firms usually need

- Modern public site, attorney bios, practice pages, contact CTA, consultation booking.
- Intake path from website into CRM/case lifecycle.

### Current status

- **Status:** `Implemented` for baseline website, `Partial` for advanced lead tooling.
- **Score:** `70%`

### Evidence

- Multi-page public site with modern UI:
  - `frontend/src/pages/public/HomePage.tsx`, `AboutPage.tsx`, `PracticeAreasPage.tsx`, `AttorneysPage.tsx`
- Contact + appointment request modal with available slots:
  - `frontend/src/pages/public/ContactPage.tsx`
  - `backend/Features/Consultations/ConsultationsEndpoints.cs`

### Missing for parity

- Campaign attribution and conversion tracking.
- SEO technical layer (structured data, metadata strategy, sitemap automation).
- Multi-step lead qualification and dynamic form logic.

---

## 2) Legal CRM / intake pipeline (Clio Grow-type)

### Expected Clio-class capability

- Online intake forms (public link/embed), custom fields, conditional questions.
- Pipeline stages with automation and follow-up reminders.
- Appointment scheduling tied to intake status.
- Lead-to-matter conversion with no duplicate entry.

### Current status

- **Status:** `Partial`
- **Score:** `55%`

### Evidence

- Intake records with stages and transition rules:
  - `backend/Features/Intake/IntakeEndpoints.cs`
- Stage history + notes:
  - `LawFirmDbContext` entities `IntakeStageHistory`, `IntakeNote`
- Kanban board and detail workflow:
  - `frontend/src/pages/workspace/IntakeBoardPage.tsx`
  - `frontend/src/pages/workspace/IntakeDetailPage.tsx`
- Convert accepted intake to matter:
  - `POST /intake/{id}/convert-to-matter`

### Missing for parity

- Public configurable intake form builder.
- Conditional logic, multilingual intake templates.
- Automated workflow engine (emails/reminders/stage triggers).
- Lead scoring and conversion analytics.
- Duplicate detection/merge tools for prospects.

### What it takes to close

1. Create `IntakeFormTemplate`, `IntakeSubmission`, `IntakeFieldDefinition` schema.
2. Build public form renderer + secure form submission endpoint.
3. Add rule engine for triggers:
   - "form submitted -> create intake -> assign owner -> send confirmation -> schedule reminder."
4. Add communications template system for intake emails/SMS.

---

## 3) Contacts and relationship graph

### Expected capability

- Person/org records, aliases, relationship mapping, restricted records.

### Current status

- **Status:** `Partial to strong`
- **Score:** `75%`

### Evidence

- Party creation/search/aliases/relationships:
  - `backend/Features/Parties/PartiesEndpoints.cs`
- UI create/search:
  - `frontend/src/pages/workspace/PartiesPage.tsx`

### Missing for parity

- Rich contact timelines and communication history.
- ~~Conflict-safe redaction controls beyond current role checks.~~ Done 2026-10-12 (F1): `contacts.read.restricted` permission and shared `PartyVisibility` for directory, aliases, relationships, and conflicts.
- Bulk import/export and dedupe workflows.

---

## 4) Conflict checking and review

### Expected capability

- Search across parties/matters, snapshot evidence, legal review decision logs.

### Current status

- **Status:** `Partial to strong`
- **Score:** `80%`

### Evidence

- Conflict checks + stored snapshots + attorney decision + rationale:
  - `backend/Features/Conflicts/ConflictEndpoints.cs`
- Restricted match masking for unauthorized roles.
- UI workflow:
  - `frontend/src/pages/workspace/ConflictChecksPage.tsx`

### Missing for parity

- Cross-matter entity graph conflict depth (beyond current we connect and use the API for rect matching).
- Waiver templates and approval chains.
- Conflict packet generation and audit export.

---

## 5) Matter management and lifecycle

### Expected capability

- Matter shell, responsible team, stage/status, task/doc/timeline linkage.

### Current status

- **Status:** `Partial to strong`
- **Score:** `78%`

### Evidence

- Matter list/detail/history/tasks/doc metadata/timeline:
  - `backend/Features/Matters/MattersEndpoints.cs`
  - `frontend/src/pages/workspace/MattersPage.tsx`
  - `frontend/src/pages/workspace/MatterWorkspacePage.tsx`
  - `frontend/src/pages/workspace/MatterWorkspaceTabs.tsx`

### Missing for parity

- Matter budget vs actuals.
- Linked communications stream by matter.
- Rule-driven matter templates (auto-task packs by matter type).
- ~~Deeper permissions at matter-level assignment boundaries.~~ Done 2026-10-12 (F1): assignment/grant enforcement on every `/matters-v2` route; see [matter access policy](Markdown/security/matter-access-policy.md). Ethical walls still missing.

---

## 6) Calendar, deadlines, and tasking

### Expected capability

- Legal calendaring, deadlines, ownership, reminders, completion/acknowledgment.

### Current status

- **Status:** `Partial to strong`
- **Score:** `82%`

### Evidence

- Calendar CRUD with reminders, owner/backup, ack/complete:
  - `backend/Features/Calendar/CalendarEndpoints.cs`
- Month/week/day UI with creation/edit/delete:
  - `frontend/src/pages/workspace/TasksCalendarPage.tsx`
- Workspace worklists tied to task state:
  - `frontend/src/pages/workspace/WorkspaceWorklistsPage.tsx`

### Missing for parity

- Court rule/deadline automation per jurisdiction.
- Two-way external calendar sync (Outlook/Google).
- Recurrence engine and dependency chains.

---

## 7) Consultations queue and intake operations

### Expected capability

- Public appointment booking + internal triage queue.

### Current status

- **Status:** `Implemented baseline`
- **Score:** `72%`

### Evidence

- Available slot generation and booking conflict checks:
  - `backend/Features/Consultations/ConsultationsEndpoints.cs`
- Public contact modal scheduler:
  - `frontend/src/pages/public/ContactPage.tsx`
- Internal queue with status/assignment/notes:
  - `frontend/src/pages/workspace/WorkspaceConsultationsPage.tsx`

### Missing for parity

- Appointment type definitions and service durations.
- Staff calendar conflict negotiation and reschedule workflows.
- Automated reminders/confirmations/no-show handling.

---

## 8) Document management and e-sign

### Expected capability

- Matter-linked document storage, versioning, review, generation from templates, e-sign.

### Current status

- **Status:** `Partial (metadata-heavy, file-light)`
- **Score:** `38%`

### Evidence

- Matter document metadata CRUD + review status:
  - `backend/Features/Matters/MattersEndpoints.cs`
  - `MatterDocuments` tab in `frontend/src/pages/workspace/MatterWorkspaceTabs.tsx`

### Missing for parity

- Actual binary upload/download pipeline and secure storage.
- Versioning and compare.
- Template-based document generation.
- Native e-sign flow and signer workflow.
- Document retention/hold policy controls.

### What it takes to close

1. Add document file pipeline:
   - object storage adapter (S3/Azure Blob/etc),
   - signed URL upload/download,
   - checksum and antivirus hooks.
2. Add `DocumentVersion` model.
3. Add template merge service (matter/contact fields -> document output).
4. Integrate e-sign provider API (DocuSign/Dropbox Sign equivalent).

---

## 9) Time tracking

### Expected capability

- Start/stop timers, manual time entries, billable flags, activity codes.

### Current status

- **Status:** `Not Implemented`
- **Score:** `0%`

### Evidence

- No time-entry entities/endpoints/UI in current code.

### What it takes to close

1. Add `TimeEntry` model linked to matter/contact/user.
2. Add timer state handling (active timer constraints per user).
3. Add approvals/edit locks by billing period.
4. Expose in matter UI and billing pipelines.

---

## 10) Billing, invoicing, accounting, trust

### Expected capability

- Bills, ledger accounts, trust vs operating separation, reconciliation, collections, financial reporting.

### Current status

- **Status:** `Not Implemented` (outside auth demo placeholders)
- **Score:** `5%`

### Evidence

- Permission constants include billing scopes:
  - `backend/Infrastructure/Auth/AuthPermissions.cs`
- Demo policy endpoints only:
  - `backend/Features/Authorization/AuthorizationDemoEndpoints.cs`
- No invoice/accounting/trust domain models in `LawFirmDbContext`.

### Missing for parity

- Billing entities and invoice lifecycle.
- Trust ledger + operating ledger architecture.
- Three-way reconciliation workflows.
- Payment application against invoices/trust requests.
- Financial statements and aging reports.

### What it takes to close

1. Accounting domain schema:
   - `Account`, `LedgerEntry`, `Invoice`, `InvoiceLine`, `Payment`, `TrustTransaction`, `ReconciliationBatch`.
2. Immutable ledger posting engine.
3. Billing workflow:
   - draft -> review -> approval -> issue -> payment -> write-off/refund.
4. Trust compliance controls:
   - no fee debits from trust,
   - trust/operating segregation,
   - audit reports.
5. Financial reporting APIs + exports.

---

## 11) Payments (Clio Payments equivalent)

### Expected capability

- Card/ACH/wallet acceptance, secure links, trust-safe routing, payment plans.

### Current status

- **Status:** `Not Implemented`
- **Score:** `0%`

### Evidence

- No PSP integration code, no payment endpoints, no tokenized payment method handling.

### What it takes to close

1. PSP integration abstraction (`IPaymentGateway`).
2. Payment intents + webhook processing.
3. Trust vs operating payout routing.
4. Refund/chargeback reconciliation rules.
5. Secure payment links + portal payment UX.

---

## 12) Client portal and communications

### Expected capability

- Client login, message threads, matter visibility, secure docs, invoices/payments.

### Current status

- **Status:** `Partial foundation only`
- **Score:** `20%`

### Evidence

- Client role exists in demo auth:
  - `backend/Infrastructure/Auth/DemoUserStore.cs`
- Placeholder client portal policy endpoint:
  - `backend/Features/Authorization/AuthorizationDemoEndpoints.cs`
- Client-scoped portal API (2026-10-12, F1): `backend/Features/ClientPortal/ClientPortalEndpoints.cs` (`/api/portal/matters`, own documents, document submission) using Client grants. No portal frontend pages yet.

### Missing for parity

- Dedicated client portal UI.
- Client-scoped matter/documents/invoices access rules.
- Two-way messaging/audit trail.
- Portal notifications and document requests.

---

## 13) Reporting, dashboards, and analytics

### Expected capability

- Productivity, profitability, matter pipeline, collections, user workload.

### Current status

- **Status:** `Partial baseline dashboard`
- **Score:** `30%`

### Evidence

- Operational dashboard cards + worklists:
  - `frontend/src/pages/workspace/WorkspaceHomePage.tsx`
  - `frontend/src/pages/workspace/WorkspaceWorklistsPage.tsx`
- No dedicated reporting backend or KPI data warehouse layer.

### Missing for parity

- Saved/custom report builder.
- Profitability and realization metrics.
- Billing/collections analytics.
- Scheduled report distribution.

---

## 14) Automations and AI assistance

### Expected capability

- Trigger workflows, document/task automation, AI-supported drafting/summaries.

### Current status

- **Status:** `Mostly Not Implemented`
- **Score:** `10%`

### Evidence

- Manual workflows dominate all modules.
- No automation engine or AI services in backend/frontend modules.

### Missing for parity

- Rule engine and scheduler.
- Prompt-safe AI pipelines for summaries/drafts.
- AI-assisted bill drafting or event extraction.

---

## 15) Integrations ecosystem

### Expected capability

- Accounting sync, email/calendar sync, cloud storage, e-sign integrations.

### Current status

- **Status:** `Not Implemented`
- **Score:** `0%`

### Evidence

- No integration connectors present in current backend/frontend modules.

### Missing for parity

- Integration framework:
  - OAuth connection manager
  - job workers
  - sync status/error monitoring
- Initial connectors:
  - QuickBooks/Xero
  - Microsoft 365/Google calendar + contacts
  - Google Drive/OneDrive/Dropbox
  - DocuSign/Dropbox Sign

---

## 16) Security, compliance, and identity maturity

### Expected capability

- MFA/SSO, strong auditing, granular policies, data retention controls.

### Current status

- **Status:** `Partial`
- **Score:** `45%`

### Evidence

- JWT + permissions model + policy checks:
  - `backend/Infrastructure/Auth/*`
- Audit events captured on many write operations.
- Strategy doc acknowledges managed IdP direction:
  - `backend/AUTHORIZATION_STRATEGY.md`

### Missing for parity

- Enterprise SSO (OIDC/SAML) and enforced MFA.
- Session management and token revocation model.
- Field-level encryption and key management policy.
- Compliance playbooks (SOC2 controls, access reviews, retention schedules).

---

## Overall parity summary

- Strongest current areas:
  - intake-to-matter lifecycle foundations,
  - calendar/worklist operations,
  - conflicts workflow,
  - role/permission architecture.
- Largest gaps:
  - billing/accounting/trust,
  - payments,
  - client portal,
  - integrations,
  - advanced CRM automation,
  - document/e-sign depth.

Estimated current parity toward full Clio replacement:

- **Approximately 40-45%** for internal operations foundation.
- **Approximately 20-25%** for full business-critical parity once finance/payments/portal are weighted appropriately.

---

## Build roadmap to close gaps

## Phase 1 (0-8 weeks): Core reliability and workflow depth

1. Document binary storage + secure access URLs.
2. Public intake form templates + submission mapping.
3. Intake automation (assignment/reminder/stage triggers).
4. Calendar recurrence and dependency chains.
5. Hardening:
   - background jobs,
   - retry policies,
   - structured telemetry.

## Phase 2 (8-16 weeks): Finance foundation

1. Time entry system.
2. Billing/invoice core model.
3. Operating/trust ledger model + immutable postings.
4. Basic payment integration and invoice payment flow.
5. Financial reporting MVP (AR aging, trust balances, collections).

## Phase 3 (16-24 weeks): Client experience and integrations

1. Client portal (matters/docs/messages/bills/payments).
2. E-sign workflows and signature audit trail.
3. First integration wave:
   - calendar/email sync,
   - accounting sync,
   - cloud doc providers.
4. Reporting suite expansion.

## Phase 4 (24+ weeks): SaaS readiness

1. Multi-tenant data architecture and tenant isolation controls.
2. Tenant-level configuration, branding, and permissions.
3. Subscription/billing for SaaS customers.
4. Advanced compliance and security posture for external customers.

---

## Execution checklist (update continuously)

- [ ] Create `TimeEntry` domain and APIs.
- [ ] Create `Invoice` and `InvoiceLine` domain and APIs.
- [ ] Create trust/operating ledger domain.
- [ ] Implement payment provider abstraction + webhooks.
- [ ] Build client portal routes and backend scopes. (Backend scopes and matter/document API done 2026-10-12; frontend pages, publication, messaging remain.)
- [ ] Build integration framework and first connectors.
- [ ] Build intake template engine and workflow automations.
- [ ] Build document upload/versioning/e-sign.
- [ ] Add reporting backend and downloadable report endpoints.
- [ ] Add managed IdP + MFA + security hardening.

---

## Maintenance notes

When new features ship, update:

1. the relevant module score,
2. evidence file paths,
3. gaps removed/added,
4. roadmap phase completion status.

This file should stay truthful enough that a new developer can understand:

- where we are,
- what is missing,
- and what to build next to replace Clio-class subscriptions with this app.
