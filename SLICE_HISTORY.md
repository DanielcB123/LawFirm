# Slice History

A **slice** is one bounded implementation step: a small piece of work carried end to end (schema, API, UI where relevant, tests, documentation) that leaves the application runnable. Entries are append-only. Each heading's date and slice name is its stable ID. Later outcomes are added as new entries; earlier failures are not rewritten. Commit references appear only when a commit exists. Current state lives in [ProgressPlan.md](ProgressPlan.md). Why a choice was made, what it helps, and how it could fail later live in [Markdown/planning/decision-log.md](Markdown/planning/decision-log.md). Entries before 2026-10-13 22:40 EDT were not rewritten into that shape.

## 2026-10-04 - Slice S1 + S2 foundation

Implemented initial vertical slice for:

- identity and actor profile APIs
- authorization boundaries for contact/conflict workflows
- immutable audit trail records for write actions
- consistent error contract with correlation IDs
- contact/organization graph with aliases and relationships
- conflict search and attorney/admin decision workflow with retained snapshots
- frontend workspace flows for sign-in, actor context, parties, and conflict review

### Backend highlights

- Added persistence layer with SQLite EF Core context:
  - `backend/Infrastructure/Persistence/LawFirmDbContext.cs`
  - `backend/Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`
- Added error contract middleware and correlation IDs:
  - `backend/Infrastructure/Errors/*`
- Added immutable audit event writer:
  - `backend/Infrastructure/Audit/AuditTrailService.cs`
- Added endpoint modules:
  - `backend/Features/Identity/IdentityEndpoints.cs`
  - `backend/Features/Parties/PartiesEndpoints.cs`
  - `backend/Features/Conflicts/ConflictEndpoints.cs`
- Updated auth permissions/policies and demo users for contacts/conflicts workflows.

### Frontend highlights

- Added auth provider and route protection:
  - `frontend/src/features/auth/AuthContext.tsx`
  - `frontend/src/features/auth/RequireAuth.tsx`
- Added workspace pages:
  - `frontend/src/pages/WorkspaceLoginPage.tsx`
  - `frontend/src/pages/WorkspaceHomePage.tsx`
  - `frontend/src/pages/PartiesPage.tsx`
  - `frontend/src/pages/ConflictChecksPage.tsx`
- Added API services and types for auth/parties/conflicts:
  - `frontend/src/services/authApi.ts`
  - `frontend/src/services/partiesApi.ts`
  - `frontend/src/services/conflictsApi.ts`
  - `frontend/src/types/auth.ts`
  - `frontend/src/types/parties.ts`
- Updated shared API client to parse backend error envelopes and correlation IDs.

### Validation run

- Backend: `dotnet build -o ./.build-out` (success)
- Frontend: `npm run build` (success)

### Known operational notes

- A separately running backend process can lock default `bin/Debug` outputs.
- Build validation was run with alternate output path for backend.

## 2026-10-04 - Staff Tasks & Calendar slice

Implemented authorized staff-only calendar/task tracking for non-client users.

### Backend highlights

- Added calendar permissions/policies:
  - `calendar.read`
  - `calendar.manage`
- Added calendar persistence model:
  - `CalendarEntry` in `backend/Infrastructure/Persistence/LawFirmDbContext.cs`
- Added calendar API endpoints in `backend/Features/Calendar/CalendarEndpoints.cs`:
  - `GET /api/calendar/entries`
  - `POST /api/calendar/entries`
  - `POST /api/calendar/entries/{id}/acknowledge`
  - `POST /api/calendar/entries/{id}/complete`
- Added audit events for create/acknowledge/complete transitions.

### Frontend highlights

- Added `Tasks & Calendar` workspace route/page:
  - `frontend/src/pages/TasksCalendarPage.tsx`
- Added calendar service/types:
  - `frontend/src/services/calendarApi.ts`
  - `frontend/src/types/calendar.ts`
- Added authenticated nav link to calendar workspace:
  - `frontend/src/components/SiteNav.tsx`

### Validation run

- Backend: `dotnet build -o ./.build-out` (success)
- Frontend: `npm run build` (success)

## 2026-10-04 - Slice N1 Intake Pipeline + Matter Shell

Implemented the first intake-to-matter production workflow slice with seeded data for manual testing.

### Backend highlights

- Added intake + matter shell persistence models in `backend/Infrastructure/Persistence/LawFirmDbContext.cs`:
  - `IntakeRecord`, `IntakeStageHistory`, `IntakeNote`
  - `MatterRecord`, `MatterStageHistory`
  - `IntakeStages` constants
- Added MySQL-safe table creation for new entities in `backend/Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`.
- Added intake permissions/policies:
  - `intake.read`
  - `intake.manage`
  - policy wiring in `backend/Infrastructure/Auth/AuthServiceCollectionExtensions.cs`
- Added intake API in `backend/Features/Intake/IntakeEndpoints.cs`:
  - `GET /api/intake`
  - `GET /api/intake/{intakeId}`
  - `POST /api/intake`
  - `POST /api/intake/{intakeId}/stage`
  - `POST /api/intake/{intakeId}/notes`
  - `POST /api/intake/{intakeId}/convert-to-matter`
- Implemented stage transition rules and acceptance gate requiring an approved conflict decision for `Accepted`.
- Added matter shell API in `backend/Features/Matters/MattersEndpoints.cs`:
  - `GET /api/matters-v2`
  - `GET /api/matters-v2/{matterId}`
- Added development seeding in `backend/Infrastructure/Persistence/DevelopmentDataSeeder.cs` and wired startup seed call in `backend/Program.cs`.

### Frontend highlights

- Added intake board/detail flows:
  - `frontend/src/pages/IntakeBoardPage.tsx`
  - `frontend/src/pages/IntakeDetailPage.tsx`
  - `frontend/src/services/intakeApi.ts`
  - `frontend/src/types/intake.ts`
- Added matter list + workspace shell:
  - `frontend/src/pages/MattersPage.tsx`
  - `frontend/src/pages/MatterWorkspacePage.tsx`
  - `frontend/src/pages/MatterWorkspaceTabs.tsx`
  - `frontend/src/services/mattersApi.ts`
  - `frontend/src/types/matters.ts`
- Added workspace routing for intake/matters and nested matter tabs in `frontend/src/app/AppShell.tsx`.
- Updated permission-aware nav links for intake/matters in `frontend/src/components/SiteNav.tsx`.
- Added kanban/workspace tab styling in `frontend/src/app/AppShell.css`.

### Validation run

- Backend: `dotnet build -o ./.build-out` (success)
- Frontend: `npm run build` (success)
- Lints: `ReadLints` on edited backend/frontend files (no errors)

### Current limits

- Matter tab sections for tasks/documents are placeholders for Slice N2 deeper functionality.
- Intake board is stage-grouped kanban but does not yet support drag/drop stage transitions.

## 2026-10-04 - Slice N2 Matter Workspace Tasks + Documents + Timeline Actions

Implemented the second workspace slice to support matter execution workflows (task/deadline operations, document index metadata, and timeline actions) with seed data for manual validation.

### Backend highlights

- Extended persistence model in `backend/Infrastructure/Persistence/LawFirmDbContext.cs`:
  - `MatterTask`
  - `MatterDocument`
  - `MatterTimelineAction`
- Added MySQL-safe table creation in `backend/Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`:
  - `matter_tasks`
  - `matter_documents`
  - `matter_timeline_actions`
- Expanded `backend/Features/Matters/MattersEndpoints.cs` with new endpoints:
  - tasks:
    - `GET /api/matters-v2/{matterId}/tasks`
    - `POST /api/matters-v2/{matterId}/tasks`
    - `POST /api/matters-v2/{matterId}/tasks/{taskId}`
    - `DELETE /api/matters-v2/{matterId}/tasks/{taskId}`
  - documents:
    - `GET /api/matters-v2/{matterId}/documents`
    - `POST /api/matters-v2/{matterId}/documents`
    - `POST /api/matters-v2/{matterId}/documents/{documentId}/review`
    - `DELETE /api/matters-v2/{matterId}/documents/{documentId}`
  - timeline actions:
    - `GET /api/matters-v2/{matterId}/timeline`
    - `POST /api/matters-v2/{matterId}/timeline-actions`
- Added audit events for all new write actions (task/document/timeline).

### Frontend highlights

- Expanded matter types in `frontend/src/types/matters.ts` for:
  - tasks + create/update payloads
  - document metadata + review payloads
  - timeline actions + create payload
- Expanded API service in `frontend/src/services/mattersApi.ts` for all N2 endpoints.
- Reworked `frontend/src/pages/MatterWorkspaceTabs.tsx`:
  - **Tasks tab**: add/list/update status/verify deadline/delete tasks
  - **Documents tab**: add/list/review/delete document metadata entries
  - **Timeline tab**: add/list custom timeline actions and display stage history context

### Seed data updates

- Extended `backend/Infrastructure/Persistence/DevelopmentDataSeeder.cs` with:
  - sample matter tasks
  - sample matter documents
  - sample matter timeline actions

### Validation run

- Backend: `dotnet build -o ./.build-out` (success)
- Frontend: `npm run build` (success)
- Lints: `ReadLints` on edited backend/frontend files (no errors)

### Current limits

- Document handling is metadata index only in this slice (no binary upload/storage pipeline yet).
- Task updates currently use a full-field post model from UI quick actions rather than granular PATCH semantics.

## 2026-10-12 - Slice A0 Repository audit and linked documentation

- **Recorded:** retroactively on 2026-10-12 01:22 EDT (UTC−04:00). The work was committed by the repository owner as `91a9ded` ("Markdown and the PLAN. Follow the PLAN") at 2026-10-12 00:28:32 EDT.
- **Objective and scope:** audit the codebase against the law-firm SaaS blueprint in `Markldown/CURSOR_PROMPT.md` and produce linked documentation. Documentation only.
- **Changes:** created `Markdown/` (27 blueprint documents) plus `Markdown/architecture/current-codebase-audit.md`, `Markdown/architecture/stack-and-runtime.md`, `Markdown/delivery/implementation-gap-map.md`; rewrote `Markdown/database/ADR-001-tenancy.md` (MySQL, database per firm); added the dependency-ordered plan F1–F13 to `Markdown/delivery/roadmap-and-acceptance.md`. 30 files changed, all Markdown.
- **Decisions:** keep .NET 8 / EF Core + Pomelo / MySQL 8 / React; tenant is the law firm; database per firm proposed in ADR-001 (not implemented).
- **Checks run:** relative links in the new documents resolved (184 links, all valid); `npm run lint` recorded as failing with 7 errors and 3 warnings; source read for every audit claim.
- **Checks not run:** application code was unchanged, so no build or test was needed for the change itself. No database or API behavior was exercised; audit findings about runtime behavior came from reading source.
- **Remaining defects found:** matter routes not assignment-scoped; clients could create document metadata on any matter; restricted parties unmasked in `/api/parties`; demo users registered in all environments; secrets in tracked config; `EnsureCreated` schema; no tests. See the gap map.
- **Outcome:** documentation complete. **Next action:** F1 (test harness + matter authorization).

## 2026-10-12 - Slice F1 Matter authorization, real-MySQL test harness, migrations baseline, secrets and demo-auth gating

- **Recorded:** 2026-10-12 01:22 EDT (UTC−04:00). **Not committed** (working tree on top of `91a9ded`).
- **Objective and scope:** enforce matter-level access on every matter-owned path for one firm, fix restricted-party disclosure, prove it on real MySQL, and remove the schema and secrets prerequisites that blocked testing. Multitenancy explicitly out of scope.

### Changes

- Access control (new): `backend/Infrastructure/Access/` — `MatterAccessService.cs` (`IMatterAccessService`: readable, writable, and client matter queries; grant-manager check), `MatterAccessResults.cs` (uniform 404/403), `PartyVisibility.cs`, `ActorClaims.cs`, `AccessServiceCollectionExtensions.cs`.
- Policies and permissions: `AuthPolicies.cs` (`MatterStaffRead`, `MatterStaffWrite`, `ClientMatterPortal`), `AuthPermissions.cs` (`contacts.read.restricted`), `AuthServiceCollectionExtensions.cs` (policies; `AddLawFirmDemoAuthentication`), `DemoUserStore.cs` (Admin: `matters.read.all/update.all` replaced by `read.assigned/update.assigned`; Attorney and Admin gain `contacts.read.restricted`), `AuthConstants.cs`.
- Endpoints: `Features/Matters/MattersEndpoints.cs` (all routes scoped; writes checked), `Features/Matters/MatterAccessGrantEndpoints.cs` (new: list/create/revoke grants, audited), `Features/ClientPortal/ClientPortalEndpoints.cs` (new: `/api/portal/matters`), `Features/Calendar/CalendarEndpoints.cs` (entries filtered and write-checked through the linked matter number; create/update must name a writable matter), `Features/Parties/PartiesEndpoints.cs`, `Features/Conflicts/ConflictEndpoints.cs` (masked matches get a fresh id), `Features/Auth/AuthEndpoints.cs` (503 when demo sign-in is not configured).
- Schema: `MatterAccessGrant` entity in `Infrastructure/Persistence/LawFirmDbContext.cs`; migrations `Infrastructure/Persistence/Migrations/20261012050714_Baseline` and `20261012050741_MatterAccessGrants`; `LawFirmDesignTimeDbContextFactory.cs`; `PersistenceServiceCollectionExtensions.cs` rewritten (no `EnsureCreated`, no raw DDL, no fallback connection string; refuses unmanaged databases; applies migrations only when `Database:ApplyMigrationsOnStartup` is true); `DevelopmentDataSeeder.cs` seeds one Client grant for `u-client-001` on `MAT-2026-0001`.
- Configuration: `backend/appsettings.json` and `appsettings.Development.json` now have empty connection string and signing key (values were not printed); `Authentication:DemoUsers:Enabled` false by default; `UserSecretsId` and `Microsoft.EntityFrameworkCore.Design` added to the API project; `dotnet-tools.json` (dotnet-ef 8.0.13); `Program.cs` (`public partial class Program`, registrations).
- Tests (new): `backend.IntegrationTests/` — `AuthenticationTests`, `MatterAccessTests`, `ClientPortalTests`, `CalendarMatterScopeTests`, `PartyVisibilityTests`, `SchemaSafetyTests`, fixtures in `Infrastructure/`.
- Repository: `.gitignore` (test project outputs, `.test-results/`); root `AGENTS.md`; this file; `ProgressPlan.md`; `README.md`.
- Documentation: new `Markdown/security/matter-access-policy.md` and `Markdown/database/migration-baseline.md`; updated gap map, roadmap, codebase audit, stack and runtime, security and database indexes, client-portal and matter-management modules, `backend/AUTHORIZATION_STRATEGY.md`, `backend/DEV_AUTH_CREDENTIALS.md`.
- Removed: an empty root-level `DemoUserStore.cs` accidentally created by a failed shell edit during this session (never committed).

### Decisions

- Admins get no implicit matter access; attorneys keep firm-wide access pending firm confirmation (no ethical walls yet).
- Clients use separate portal routes and DTOs; staff matter routes return 403 to clients.
- Denial is 404 for anything the caller cannot read, matching nonexistent ids; 403 only when the caller can already read the matter.
- Grants are soft-revoked rows; MySQL enforces one unrevoked grant per matter and actor through a stored generated column in a unique index.
- Existing `EnsureCreated` databases are refused at startup, not migrated automatically.

### Checks run (actual results)

- **Fail-before:** with harness, schema, and permission changes in place but endpoints not yet enforcing, `dotnet test backend.IntegrationTests -c EfDesign` against `mysql:8.0.36`: **26 failed, 17 passed, 43 total.** The 17 that passed covered behavior that was already correct or not yet changed: assigned/firm-wide reads, policy-level 403 for staff without update permission, child lookup filtering by matter id, database constraints, schema refusal, login 503, 401s on existing routes.
- **Pass-after:** same command after enforcement: **43 passed.** After adding the seeder test: **44 passed, 0 failed.**
- Backend build `dotnet build -o ./.build-out` (in `backend/`): success, 0 warnings, 0 errors. Test project build `-c EfDesign`: success.
- `dotnet-ef migrations has-pending-model-changes`: no pending changes.
- Collation probe on a throwaway `mysql:8.0.36` container: a `ascii_general_ci` foreign key to a utf8mb4 `char(36)` primary key fails with error 3780.
- Tracked config inspected by script printing only lengths: connection strings and signing keys are empty. No `password=`/`pwd=` strings in backend `.cs`/`.json` files.
- Frontend `npm run build`: success. `npm run lint`: **7 errors, 3 warnings**, the same counts as the audit baseline; no frontend files changed.
- Markdown relative links: PowerShell check over all 41 `.md` files (excluding build output): 246 relative links, 0 broken, anchors included.

### Checks not run, and why

- Starting the API in Development with user-secrets and demo login against a new database: not exercised (would need local secrets; the user's API process was running).
- Baselining any existing database: deliberately not attempted; no existing database was touched.
- Frontend behavior against the changed API: not exercised; no frontend changes in scope.
- CI: does not exist.
- Credential rotation: not performed (owner action). Git history: not rewritten.

### Remaining defects

- The calendar UI sends free-text matter references, which now return 400 unless they name a writable matter.
- Admin users see no matters until granted.
- Grant actor ids are unvalidated text. No ethical walls. Intake not scoped. Calendar verifier still taken from the request body. Staff document route still accepts `StorageKey`. Audit metadata contains restricted party names. Audit is not in the same transaction as the change.

### Outcome and next action

- **Outcome:** Verified within stated scope (single firm, API level, MySQL 8.0.36, synthetic data).
- **Next action:** owner reviews and commits, rotates the listed credentials, then the multitenant database architecture and security design prompt.

## 2026-10-12 - Slice M0 MatterForge working name and multi-firm database architecture (design)

- **Recorded:** 2026-10-12 02:15 EDT (UTC−04:00). **Not committed** (same working tree as F1).
- **Objective and scope:** adopt the working product name MatterForge without breaking identifiers, and design the multi-firm (tenant = law firm) MySQL architecture, controls, and operations. Validate the design SQL on a disposable database. Implementing tenancy, deploying, importing case data, and touching existing databases were all out of scope, and none was done.

### Changes

- Branding (user-visible only): `README.md` title and intro, `Markdown/README.md` title, `frontend/index.html` `<title>`, the Swagger title in `backend/Program.cs`. Every other identifier is unchanged and classified in `Markdown/product/naming-and-rename.md`: namespaces, JWT issuer and audience, `ConnectionStrings:LawFirm`, `UserSecretsId`, the localStorage key, the Git remote, database names, and the demo firm branding.
- **Folder rename: not performed.** The API (`backend\bin\Debug`) and the Vite dev server were running from the folder, and the Cursor workspace was open. No duplicate repository was created. The manual steps are recorded.
- New design SQL in `db/design/`: control plane (01); generated current firm schema (10, from `dotnet-ef migrations script`); required additions (11); future structures (12); ledger triggers (13); pooled alternative (20); synthetic data (40); representative queries (50); engine checks (60); money checks (70); `run-validation.ps1`; `validation/mysql-8.0.36-validation.log`; `README.md`.
- New docs:
  - `Markdown/database/ADR-002-multitenant-architecture.md`, `data-dictionary.md`, `query-and-index-spec.md`;
  - `Markdown/security/multitenant-threat-model.md`;
  - `Markdown/operations/fleet-provisioning-and-migration-runbook.md`, `recovery-and-offboarding-runbook.md`;
  - `Markdown/delivery/multitenant-acceptance-matrix.md`, `multitenant-implementation-plan.md`;
  - `Markdown/product/naming-and-rename.md`.
- Updated: ADR-001 (pointer to ADR-002), open decisions, gap map (design notes only; no status upgraded), roadmap, `Markdown/README.md` index, database and security READMEs, `ProgressPlan.md`, and `.gitignore`. The `.gitignore` change is an exception so the validation log is tracked despite `*.log`.

### Decisions (proposed; owner review pending)

- **Database per firm (option B)**, refined with:
  - placement cells with connection budgets;
  - small per-firm pools;
  - per-account `MAX_USER_CONNECTIONS`;
  - generated database names `mf_f_<32hex>`;
  - a `firm_identity` check on every connection lease;
  - **table-level runtime grants from a manifest**;
  - a separate migration account;
  - a privileged DEFINER step for triggers.
- The strongest alternative is the hybrid (option C). Reconsideration triggers are listed in ADR-002.
- Money is DECIMAL(19,2) with a currency code. Legal dates are DATE plus zone or jurisdiction. Append-only tables use bigint ids plus a PublicId. Approvals are bound to a content hash.

### Checks run (actual results)

- `run-validation.ps1` on a uniquely named `mysql:8.0.36` container. The random root password was never printed, and the container was removed at the end. Final run 06:01:44–06:03:29 UTC: every check matched its expectation. That covers E1–E12b, G1–G16b, P1–P4b, L1–L18, R1–R4, and plans Q0–Q8c. The log has 51 expected ERROR lines; a grep found no credentials.
- `dotnet test backend.IntegrationTests -c EfDesign` after the `Program.cs` title change: **44 passed, 0 failed** (this also rebuilt the API project).
- Every acceptance id referenced in the threat model exists in the acceptance matrix (script check).

### Failures during the session, and fixes

- **Trigger creation as the restricted migration user failed with ERROR 1419** (binary logging on, `log_bin_trust_function_creators = 0`). This is a real finding. The triggers moved to file 13, and the validation now shows both the failure and the privileged `CREATE DEFINER` path.
- Early query plans showed table scans. The cause was GUIDs bound as utf8mb4 user variables against `ascii_general_ci` columns, also a real finding (kept as Q0). The checks now bind ascii-collated values.
- G11 at first reported 0 rows updated because the value was unchanged. The check now writes a new value.
- The R4 expectation was corrected from 1203 to the observed **1226**.
- Script issues under Windows PowerShell 5.1: native stderr raised errors under `ErrorActionPreference Stop`, and console output was redirected into a folder that didn't exist yet. Both were fixed in the script.
- A table-level REVOKE can't carve rights out of a database-level grant (1147, and the write still succeeded). This changed the grant design to table-level only.

### Checks not run, and why

- No application multi-firm code exists, so every application-level acceptance case is **Designed** only.
- No managed host was inspected. The connection limits, backup granularity, and costs in the docs are placeholders or measurements from the local container. RPO and RTO are unverified.
- No load test was run. Synthetic firm A is about 506 MB, so no scale or latency claim is made.
- The existing development database was not baselined (forbidden in this stage). Credentials were not rotated (owner action).
- The frontend build and lint were not re-run. The only frontend change is the `<title>` text.
- F2's same-transaction audit (G16) is still open.

### Outcome and next action

- **Outcome:** Verified within stated scope (design artifacts plus disposable MySQL 8.0.36 engine validation with synthetic data). Not production ready. Branding not cleared. Folder not renamed.
- **Next action:**
  1. The owner optionally renames the folder by hand: stop terminals and processes, close the workspace, rename it in File Explorer, and reopen `D:\Documents\Projects\MatterForge`.
  2. The owner reviews and commits F1 and M0.
  3. Rotate the credentials.
  4. Start T1 with the prompt below. If the folder was renamed, update the path first.

#### Prompt for T1

```text
Repository: D:\Documents\Projects\LearnCSharpDotNet (MatterForge, working name). Follow AGENTS.md: read ProgressPlan.md, the latest SLICE_HISTORY.md entries, ADR-002, the multi-firm threat model, the acceptance matrix, and the implementation plan before changing anything; inspect the code rather than trusting the docs.

Implement plan step T1 only: a minimal control-plane catalog, trusted firm resolution, and per-firm DbContext routing.
1. A ControlPlaneDbContext with EF migrations for firms, firm_domains, database_servers, and firm_placements only (shape per db/design/01-control-plane.sql), on its own database and connection-string key. Do not change ConnectionStrings:LawFirm, the JWT issuer or audience, the namespaces, or the user-secrets id.
2. Firm resolution from the request host, through a verified domain, an active firm, and an active placement. No firm selector is accepted from headers, body, query, or route. Unknown or unverified hosts and suspended firms fail closed before any firm query. Store the resolved firm in an immutable per-request firm context.
3. A per-firm LawFirmDbContext factory that uses that placement's runtime connection string. Secrets come from configuration or user secrets in tests; never log them. On each connection lease, read firm_identity and compare it with the resolved firm. A mismatch returns 503 and logs a code without data.
4. Add the firm_identity table to the firm schema through a migration.
5. Tests in backend.IntegrationTests on real MySQL 8.0.36 (no in-memory provider): one container, a control database, and two synthetic firm databases with deliberately overlapping matter ids. Each firm has a restricted runtime account with table-level grants (no database-level grants, no DDL; firm_identity SELECT-only). Cover AT-RES-01, AT-RES-02, AT-RES-03, AT-RES-05, AT-RES-06, AT-AUTHZ-01, AT-CTX-01 (concurrent interleaved requests), AT-CTX-02, AT-DB-01, and AT-REG-01 (all 44 existing tests still pass, routed to a firm). Show fail-before and pass-after where applicable.
Out of scope: memberships and identity (T2), the provisioner, the fleet migrator, storage, billing, deployment, real data, and any existing non-test database. Do not rewrite Git history or print secrets.
Finish by updating ProgressPlan.md, appending a truthful SLICE_HISTORY.md entry (including failures), updating the acceptance-matrix statuses and the gap map rows G01 and G02, and reporting the exact commands and results.
```

## 2026-10-12 - Slice R1 Solution structure, boilerplate, and documentation folder cleanup

- **Recorded:** 2026-10-12 21:45 EDT (UTC−04:00). **Not committed** (working tree on top of `0fc6831`, which contains F1 and M0).
- **Objective and scope:** research current structure guidance for a .NET minimal API with a React SPA, apply it to the repository, add not-implemented boilerplate for missing modules, and replace the confusing `Markdown/` plus `Markldown/` pair with one documentation folder. Out of scope: any endpoint behavior, schema, or authorization change; the folder rename; the namespace rename; fixing the existing lint errors.

### Changes

- Documentation folder: `git mv Markldown Markdown/archive/original-prompt` (prompt and zip unchanged). Live links updated in `Markdown/README.md`, `Markdown/delivery/roadmap-and-acceptance.md`, `Markdown/architecture/current-codebase-audit.md`, and `AGENTS.md`, whose rule ("do not move or delete it") was replaced at the owner's request. Historical mentions in earlier entries above are left as written.
- Repository root (new): `MatterForge.sln` (API plus a `tests` folder), `Directory.Build.props` (`Nullable`, `ImplicitUsings`), `Directory.Packages.props` (Central Package Management with the exact versions previously in the project files), `global.json` (SDK 10.0.300, `latestFeature`), `.editorconfig` (suggestion-level style only). Both existing project files now reference packages without versions. `.gitignore` adds `backend.UnitTests/bin/` and `obj/`.
- Backend moves, no behavior change:
  - The 20 entity and constant classes left `LawFirmDbContext.cs` for `Infrastructure/Persistence/Entities/{Party,Conflict,Audit,Calendar,Intake,Matter,Consultation}Entities.cs`, keeping the namespace.
  - Request and response records left the nine endpoint files for `Features/<Module>/Contracts/*Contracts.cs`, keeping each module's namespace.
  - Swagger setup moved from `Program.cs` to `Infrastructure/OpenApi/OpenApiServiceCollectionExtensions.cs` (same middleware order).
  - `Features/WeatherForecast` (starter template) was deleted, which removes `/api/weather/forecast` and `/api/weatherforecast`. The `.http` file now calls `/api/health`.
- Backend boilerplate (**not implemented**; nothing registers or calls it except `TimeProvider`):
  - `Common/Paging/` (`PageRequest`, `PagedResult<T>`, `ToPagedResultAsync`).
  - Interfaces: `Infrastructure/Tenancy/IFirmContext.cs`, `Infrastructure/Outbox/` (`IOutbox`, `OutboxEvent`, unregistered `OutboxDispatcher`), `Infrastructure/Storage/IDocumentStorage.cs`, `Infrastructure/Messaging/IEmailSender.cs`, `Infrastructure/Payments/IPaymentGateway.cs`, `Infrastructure/Signatures/ISignatureProvider.cs`.
  - Empty authenticated route groups: `Features/Documents` (`/documents-v2`), `Engagements`, `Billing` (`/billing-v2`), `Payments`, `Trust`, `Communications`.
  - `TimeProvider.System` registered in `Program.cs`.
- Unit tests (new): `backend.UnitTests/` with `Common/PageRequestTests.cs` (10 tests). The API project has `InternalsVisibleTo` for it.
- Frontend moves (all `git mv`, no UI change intended):
  - `services/<x>Api.ts` and `types/<x>.ts` became `features/<x>/api.ts` and `types.ts`, each feature with an `index.ts`. Conflict types were split out of the party types into `features/conflicts/types.ts`.
  - `services/apiClient.ts` became `lib/apiClient.ts`, now prefixed with an optional `VITE_API_BASE_URL` (`config/env.ts`, `vite-env.d.ts`, `.env.example`; empty by default, so request paths are unchanged).
  - `HealthStatusCard` moved into `features/health`, and `SiteNav` and `SiteFooter` into `components/layout/`.
  - Pages moved into `pages/public/` and `pages/workspace/`.
  - Routes moved to `app/AppRoutes.tsx` and providers to `app/AppProviders.tsx`. `AppShell.tsx` keeps the layout and the scroll-reveal effect.
  - `App.tsx`, `App.css`, and three template assets were deleted.
- Frontend additions: `features/portal` (API functions and types for the existing `/api/portal/matters` routes; no pages), `features/billing` (draft types), `types/money.ts`, the ESLint `no-restricted-imports` feature-boundary rule, Vitest 5.0.4 (`npm test`, `test:watch`, a `test` block in `vite.config.ts`), and `lib/apiClient.test.ts` (4 tests).
- Documentation:
  - New: `Markdown/architecture/solution-structure.md`.
  - Updated: the architecture README, stack and runtime, the codebase audit (path-update note), `Markdown/README.md`, the gap map (R1 notes on G07, G24, G32, G43, G50–G52, with no status change), sources (S23–S31), the naming record (`MatterForge.sln`), `frontend/src/ARCHITECTURE.md`, `frontend/README.md` (replaced the Vite template text), root `README.md`, `AGENTS.md`, and `ProgressPlan.md`. The ProgressPlan parity tracker had 32 page paths updated.

### Decisions

- Keep one API project with feature folders, the lighter variant the sources describe. Per-module projects, per-module DbContexts, and a `src/`+`tests/` move are deferred, with triggers recorded in the solution-structure decisions table. A per-module DbContext would split migrations across the per-firm fleet runner, and moving top-level folders would break the running processes and every doc link.
- Entities keep their namespace, because the EF snapshot records CLR names.
- New placeholder prefixes `/documents-v2` and `/billing-v2` avoid the existing demo stubs.
- Vitest 3 was tried first. It pulled in a second Vite (7.3.7) next to the project's Vite 8, so it was replaced with Vitest 5.0.4, which uses the project's Vite 8.2.2 (checked with `npm ls vite`).

### Checks run (actual results)

- `dotnet build -o ./.build-out` in `backend/` after the split and before the boilerplate: success, 0 warnings.
- `dotnet build MatterForge.sln -c Release` on the final tree: success, 0 warnings, 0 errors. The first attempt with `-c EfDesign` failed with MSB4126 because that configuration exists only at project level, not in the solution.
- `dotnet test backend.UnitTests -c Release --no-build`: 10 passed.
- `dotnet test backend.IntegrationTests -c EfDesign` on the final backend tree: **44 passed, 0 failed** (disposable `mysql:8.0.36`).
- `dotnet-ef migrations has-pending-model-changes --project backend --configuration EfDesign`: **failed** at design time with "Connection string 'LawFirm' is not configured". The same command on a clean temporary worktree of `0fc6831` failed identically, so this predates R1 (the F1 run likely had a local connection string). Re-run with a dummy `ConnectionStrings__LawFirm` environment variable that is never opened: "No changes have been made to the model since the last migration."
- `npm run build`: success. `npm test`: 4 passed.
- `npm run lint`: 7 errors and 3 warnings. These are the same rules as the baseline (`set-state-in-effect` ×6, `only-export-components` ×1 in `AuthContext.tsx`, `exhaustive-deps` ×3), now reported under `pages/workspace/`. Piping a deep-import probe through `eslint --stdin` confirmed the boundary rule reports it.
- The running Vite dev server logged a transient import error while files were mid-move, then reloaded and served `src/app/AppRoutes.tsx` with HTTP 200.
- `npm audit --omit=dev`: 2 moderate advisories in `react-router` 6.x. That dependency was not changed in this slice; not fixed.
- Markdown relative links (including `#anchors`) across 52 `.md` files: 407 links, 0 broken.

### Checks not run, and why

- The running API (`dotnet run`, started before R1) was not restarted, so the new tree has not been exercised through it. Behavior is covered by the 44 integration tests.
- Pages were not clicked through in a browser. The type-check and build cover import wiring, not rendering.
- `db/design/run-validation.ps1`: not re-run (no `db/design` changes).
- CI: still does not exist.

### Remaining defects and follow-ups

- The design-time `dotnet-ef` commands need a dummy `ConnectionStrings__LawFirm` variable. The design-time factory exists but EF resolves the app's services first. Consider making `AddLawFirmPersistence` defer the missing-value check to first use.
- Lint errors are unchanged (F3). The `react-router` advisories are open.
- The temporary worktree used for the `0fc6831` comparison was removed (`git worktree list` shows only the main tree; the folder was deleted after `dotnet build-server shutdown`).

### Outcome and next action

- **Outcome:** Verified within stated scope (structure and boilerplate; existing behavior covered by integration tests; no runtime behavior change intended). Not production ready.
- **Next action:** the owner reviews and commits R1. Restart the API and the dev server and click through the public site and workspace once. Then continue with T1 (prompt in the M0 entry) or F3 (lint plus CI).

## 2026-10-13 - Slice A1 Architecture sweep, standards comparison, and SaaS onboarding runbook

- **Recorded:** 2026-10-13 14:20 EDT (UTC−04:00). **Not committed** (working tree on top of `0fc6831` plus uncommitted R1 documentation changes).
- **Objective and scope:** perform a repository-wide source sweep to map module communication, identify current vulnerabilities/risk points, compare implementation against documented architecture standards without bias, and create a formal runbook for onboarding a new law-firm SaaS tenant. Out of scope: code behavior changes, schema changes, new tests, or deployment changes.

### Changes

- New architecture evidence documents:
  - `Markdown/architecture/module-interaction-and-vulnerability-sweep-2026-10-13.md`
  - `Markdown/architecture/architecture-standards-comparison-2026-10-13.md`
- New operations runbook:
  - `Markdown/operations/saas-firm-onboarding-runbook.md`
- Cross-document synchronization:
  - `Markdown/README.md` links for the new architecture and operations docs.
  - `Markdown/architecture/README.md` link bar updated with the sweep and standards comparison.
  - `Markdown/operations/reliability-and-recovery.md` link bar updated with the SaaS onboarding runbook.
  - `Markdown/delivery/implementation-gap-map.md` intro note updated to reference the 2026-10-13 sweep and revalidated risk rows.
  - `Markdown/delivery/roadmap-and-acceptance.md` updated with a note linking the new architecture and onboarding artifacts.
  - `ProgressPlan.md` updated: last-updated timestamp, completed-work row `A1`, new current-step section, revalidated risk bullets, and next-action updates.

### Findings captured

- Module wiring is coherent and largely follows the modular-monolith conventions in `Markdown/architecture/solution-structure.md`.
- Highest-priority revalidated risk areas remain:
  - consultation slot race (G20),
  - public endpoint abuse controls absent (G22),
  - intake acceptance not tightly bound to conflict review context (G23),
  - caller-controlled calendar verification fields (G30),
  - non-atomic audit persistence and unbounded correlation id handling (G16).
- Architecture-standards verdict captured explicitly: structural standards are mostly met; multi-firm SaaS runtime standards are not yet met.

### Checks run (actual results)

- No build/test/lint commands were re-run in this slice.
- Verification method: static source and documentation inspection only.

### Checks not run, and why

- Runtime/API execution: out of scope for this documentation and assessment slice.
- Integration and unit tests: unchanged code paths; no implementation changes were made.
- Markdown link checker: not run in this slice; links were updated carefully by direct path.

### Outcome and next action

- **Outcome:** Verified within stated scope (documentation and assessment artifacts only). No claim of runtime behavior change or production readiness.
- **Next action:** commit R1 + A1 documentation changes; then resume implementation on T1 (firm resolution and per-firm routing) or F3 (lint + CI), with G16/G20/G22/G23/G30 as priority risk closures.

## 2026-10-13 - Slice A2 Vulnerability hardening implementation

- **Recorded:** 2026-10-13 15:45 EDT (UTC−04:00). **Not committed** (working tree on top of `0fc6831` plus uncommitted R1/A1 docs work).
- **Objective and scope:** close or reduce the highest-priority vulnerabilities identified in A1: consultation double-booking race (G20), anonymous abuse controls (G22), intake acceptance conflict-binding (G23), calendar verifier spoofing (G30), and unbounded correlation-id input (part of G16). Out of scope: multitenancy, billing/trust, audit atomicity redesign, and immutable audit-table permissions.

### Changes

- Backend hardening:
  - `backend/Program.cs`
    - Added ASP.NET Core rate limiter registration and middleware.
    - Added policy `PublicConsultationCreate` (fixed window, per remote IP).
  - `backend/Features/Consultations/ConsultationsEndpoints.cs`
    - Added `.RequireRateLimiting("PublicConsultationCreate")` to public consultation create route.
    - Added per-slot MySQL named lock (`GET_LOCK`/`RELEASE_LOCK`) around validation and insert to serialize booking for the same slot.
    - Returns validation error when the slot lock cannot be acquired quickly.
  - `backend/Features/Intake/IntakeEndpoints.cs`
    - Strengthened acceptance checks: intake must already reference a conflict check and approved decision must belong to that same check.
  - `backend/Features/Calendar/CalendarEndpoints.cs`
    - Verification metadata (`VerifiedAtUtc`, `VerifiedByActorId`, `VerifiedByDisplayName`) is now server-controlled.
    - Only attorney/admin callers can set verification intent.
  - `backend/Infrastructure/Errors/CorrelationIdMiddleware.cs`
    - Added sanitization/allowlist and bounded correlation-id generation.
    - Oversized values now map to stable hash-based correlation IDs.
- Integration test updates:
  - `backend.IntegrationTests/Infrastructure/MySqlApiFixture.cs`
    - Fixed runtime connection-string key override to `ConnectionStrings:LawFirm`.
  - `backend.IntegrationTests/Infrastructure/TestData.cs`
    - Added `IntakeManager()` test actor helper.
  - `backend.IntegrationTests/VulnerabilityHardeningTests.cs` (new)
    - Added tests for:
      - serialized consultation booking for same slot,
      - intake rejection when approved decision is from another conflict check,
      - calendar verification metadata hardening,
      - correlation-id sanitization/bounds.
- Documentation and tracking sync:
  - `Markdown/delivery/implementation-gap-map.md` rows G16, G20, G22, G23, G30 updated with new status/evidence.
  - `Markdown/architecture/module-interaction-and-vulnerability-sweep-2026-10-13.md` updated with A2 implementation note and residual-risk framing.
  - `ProgressPlan.md` updated for A2 status and next actions.

### Checks run (actual results)

- `dotnet test backend.IntegrationTests -c EfDesign` (initial run): failed 2 tests (test-shape issues in the new test file only), then fixed.
- `dotnet test backend.IntegrationTests -c EfDesign` (after fixes): **Passed, 48/48**.
- `dotnet build MatterForge.sln -c Release`: **Success, 0 warnings, 0 errors**.
- Cursor diagnostics (`ReadLints`) on edited backend and test files: no lints reported.

### Checks not run, and why

- `dotnet test backend.UnitTests -c Release`: unchanged unit-test target; no paging logic touched.
- Frontend build/lint/tests: backend-only implementation slice.
- `db/design/run-validation.ps1`: no `db/design` changes.

### Outcome and next action

- **Outcome:** Verified within stated scope. High-priority vulnerabilities are reduced with runtime controls and integration evidence. Not production ready and not "100% secure."
- **Residuals still open:** audit atomicity/immutability (G16), CAPTCHA/bot controls for public intake (G22), deeper party-set conflict-binding semantics (G23), and full legal deadline provenance model (G30).
- **Next action:** implement F2 remainder for same-transaction append-only audit (G16), then F3 lint+CI, then T1 multi-firm routing.

## 2026-10-13 - Slice A3 Tailwind CSS onboarding and frontend migration pass

- **Recorded:** 2026-10-13 18:20 EDT (UTC−04:00). **Not committed** (working tree on top of `0fc6831` and prior uncommitted slices).
- **Objective and scope:** integrate Tailwind CSS into the React/Vite frontend, perform a broad styling sweep for where utility-first classes should be used first, migrate shared and high-traffic public page styling, then verify with a fresh frontend install and dev-server run. Out of scope: resolving pre-existing lint debt and rewriting all feature/page styles in one slice.

### Changes

- Frontend dependency and build integration:
  - `frontend/package.json`: added `tailwindcss`, `postcss`, and `autoprefixer` (dev dependencies).
  - `frontend/postcss.config.js` (new): Tailwind + Autoprefixer plugin chain.
  - `frontend/tailwind.config.js` (new): content scanning for `index.html` and `src/**/*.{ts,tsx}`.
  - `frontend/src/index.css`: added Tailwind directives (`@tailwind base`, `@tailwind components`, `@tailwind utilities`).
  - `frontend/package-lock.json`: refreshed by install.
- Styling migration sweep (hybrid approach):
  - `frontend/src/app/AppShell.css`: migrated high-reuse structural primitives to Tailwind-backed `@apply` patterns (shell/layout/grid/card/form/modal/dashboard/list utility groups), while preserving custom variables and visual gradients.
  - `frontend/src/components/layout/SiteNav.tsx`: added utility classes for key layout/interaction wrappers while preserving existing class contracts.
  - `frontend/src/components/layout/SiteFooter.tsx`: added utility classes for spacing/container treatment.
  - `frontend/src/pages/public/HomePage.tsx`: added utility classes to container and card/lift patterns for immediate Tailwind usage.
  - `frontend/src/pages/public/ContactPage.tsx`: utility-class migration for key page containers, cards, modal, and form controls.
- Documentation sync:
  - `frontend/README.md`: now states Tailwind CSS is enabled via PostCSS in hybrid mode.
  - `Markdown/architecture/stack-and-runtime.md`: added Styling row documenting Tailwind + existing CSS hybrid strategy.
  - `ProgressPlan.md`: updated with this A3 status and verification notes.

### Checks run (actual results)

- `npm install` in `frontend/`: success (dependencies installed and lockfile updated).
- `npm run dev` in `frontend/`: Vite started successfully; port `5173` was occupied so Vite served on `http://localhost:5174/`.
- `npm run build` in `frontend/`: success (`tsc -b` + Vite production build).
- `npm run lint` in `frontend/`: fails with the existing baseline (7 errors, 3 warnings) in workspace pages and `AuthContext`; no new lint categories introduced by A3.

### Checks not run, and why

- Frontend Vitest (`npm test`): unchanged API/logic pathways in this slice; scope focused on styling/build integration.
- Backend build/tests: frontend-only slice.

### Outcome and next action

- **Outcome:** Verified within stated scope. Tailwind is now fully wired into the frontend toolchain and actively used in shared/public UI without breaking the existing custom visual system.
- **Known residuals:** style system is intentionally hybrid until deeper migration; frontend lint debt remains as previously documented; npm audit reports vulnerabilities not addressed in this slice.
- **Next action:** continue phased migration of workspace pages/components to Tailwind utilities and component abstractions, then tackle F3 lint and CI gating.

## 2026-10-13 - Public nav Menu button hidden on desktop

- **Recorded:** 2026-10-13 22:20 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** explain and fix the public header "Menu" button staying visible beside the desktop links on `/practice-areas`.

### Cause

- `.site-menu-toggle` in `frontend/src/app/AppShell.css` is `display: none` until `@media (max-width: 860px)`, where it becomes `inline-flex` and `.site-nav` becomes `display: none`.
- The Tailwind pass added `inline-flex` on that button in `SiteNav.tsx`. `frontend/src/main.tsx` imports `AppShell` (and therefore `AppShell.css`) before `index.css`, so the built stylesheet emits `.inline-flex { display: inline-flex }` after `.site-menu-toggle { display: none }`. Same specificity, later rule wins, so the toggle stayed visible at desktop widths while the link row stayed visible too.

### Changes

- `frontend/src/components/layout/SiteNav.tsx`: removed `inline-flex items-center justify-center` from the public menu toggle. Mobile show/hide stays in `AppShell.css`.

### Checks run (actual results)

- Chrome on the already running Vite server at `http://localhost:5173/practice-areas`: at 1400px, `.site-menu-toggle` computed `display: none` and `.site-nav` computed `display: flex`. At a narrow viewport (reported `innerWidth` 465), the toggle computed `display: flex` and `.site-nav` computed `display: none`.
- Cursor browser could not open the page (`ERR_CONNECTION_REFUSED`); the check above used local Chrome against `[::1]:5173`.

### Outcome and next action

- **Outcome:** Verified within stated scope for the public header toggle only. Not committed.
- **Next action:** unchanged from A3: continue the Tailwind migration, then F3 lint and CI gating.

## 2026-10-13 - Signed-in sidebar and Contacts directory

- **Recorded:** 2026-10-13 22:40 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** rename the signed-in Parties item to Contacts and show the firm's people and organizations there. Take the Clio sidebar's order and footer as a guide for modules that already exist. Out of scope: new billing, accounts, documents, communications, reports, integrations, or settings screens.

### Changes

- `frontend/src/components/layout/SiteNav.tsx`: signed-in order is Dashboard, Calendar, Tasks, Matters, Contacts, Intake, Consultations, Conflicts. Footer shows the signed-in name and role, sign out, and collapse.
- `frontend/src/pages/workspace/ContactsPage.tsx`: contact directory with name, person or organization, email, phone, and aliases. Search and add contact stay on the existing parties API.
- `frontend/src/app/AppRoutes.tsx`: `/workspace/contacts` is the page. `/workspace/parties` redirects there.
- `frontend/src/pages/workspace/PartiesPage.tsx`: removed.
- `frontend/src/pages/workspace/WorkspaceHomePage.tsx`: dashboard quick action opens Contacts.
- `frontend/src/app/AppShell.css`: contact rows and sidebar identity styles.

### Checks run (actual results)

- Browser, signed in as Avery Attorney on `http://localhost:5173`:
  - Sidebar showed the new order, Avery Attorney at the bottom, and collapse/expand.
  - Contacts listed Alicia Warren and Northgate Builders LLC with email and phone.
  - Search for "Northgate" returned that one organization.
  - `/workspace/parties` redirected to `/workspace/contacts`.
  - Tasks opened the worklists page.
  - After sign-out, the public menu was Home, About, Practice Areas, Attorneys, Contact, and Sign in.
- Editor diagnostics on the edited frontend files: no lints reported.

### Checks not run, and why

- Frontend production build and Vitest: UI-only change, exercised in the running dev server.
- Backend build and tests: no API or schema change.

### Outcome and next action

- **Outcome:** Verified within stated scope in the browser against the running API and seeded contacts.
- **Benefit and possible future downfall:** [decision log D-20261013-1](Markdown/planning/decision-log.md). Benefit: staff open Contacts and see email and phone. Downfall: the screen says Contact while the stored record stays a Party, so a later change could create a second contacts store.
- **Next action:** keep later sidebar items off the nav until those modules exist. Continue F3 lint and CI, or the next product slice the owner chooses.

## 2026-10-13 - Full signed-in sidebar

- **Recorded:** 2026-10-13 22:55 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** add every Clio sidebar destination, plus Intake, Consultations, and Conflicts, and give each destination a page. Show stored records where they exist. Do not invent invoices, trust balances, or connected integrations.

### Changes

- `frontend/src/components/layout/SiteNav.tsx`: menu order matches Clio, with a Practice group for Intake, Consultations, and Conflicts. Resource centre is in the footer. The menu scrolls when it is taller than the screen.
- New pages: Activities, Billing, Accounts, Documents, Communications, Reports, Integrations, Settings, Resource centre.
- Activities, Documents, Communications, and Reports read the existing matter, consultation, intake, calendar, and contact APIs.
- Billing, Accounts, and Integrations state that those records are not stored.

### Checks run (actual results)

- Browser, signed in as Avery Attorney on `http://localhost:5173`:
  - All menu items and Resource centre were present.
  - Activities listed the filing and client-call timeline rows.
  - Documents listed Damage-Photo-Set.zip and Retainer-Agreement-Signed.pdf.
  - Communications listed Taylor Bennett and Morgan Hale.
  - Reports showed open tasks 0, active matters 1, intake 2, contacts 2. The active-matters card opened Matters.
  - Billing, Accounts, and Integrations opened with empty, non-financial states.
  - Settings showed Avery Attorney and the attorney permissions.
  - Resource centre Documents topic opened the documents page.

### Checks not run, and why

- Frontend production build and Vitest: UI-only change, exercised in the running dev server.
- Backend build and tests: no API or schema change.

### Outcome and next action

- **Outcome:** Verified within stated scope in the browser. Billing, trust, and integrations are navigation destinations only; they do not record money or connect outside services.
- **Benefit and possible future downfall:** [decision log D-20261013-2](Markdown/planning/decision-log.md). Benefit: activities, documents, communications, and reports show stored records, and the money pages do not print a fake balance. Downfall: those empty pages can later be mistaken for a ledger, a file download, or a connected integration.
- **Next action:** build the billing and trust ledgers before those pages can show real accounts or invoices.

## 2026-10-13 - Decision log for why, benefit, and future downfall

- **Recorded:** 2026-10-13 23:05 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** give the sidebar work a look-back record. No application code in this entry.

### Changes

- `Markdown/delivery/decision-log.md` (new): D-20261013-1 Contacts rename, D-20261013-2 full sidebar, D-20261013-3 starting this log.
- `AGENTS.md`: later sessions append the decision log as well as the slice history.
- `Markdown/README.md`, `ProgressPlan.md`, and this file now point at the decision log.
- `Markdown/delivery/implementation-gap-map.md`: G32, G41, G42, G50, and G52 note that the new screens do not implement those gaps.
- `ProgressPlan.md` frontend module list includes the new workspace pages and marks the empty ones as screens only.

### Checks run (actual results)

- Documentation edit only. Links use paths that exist: `Markdown/delivery/decision-log.md` from the repo root, and `decision-log.md` from the gap map in the same folder.

### Checks not run, and why

- No build, test, or browser check. Application code was not changed in this entry.

### Outcome and next action

- **Outcome:** The sidebar decisions now record why, the observed benefit, and the future downfall. Older slice entries were not rewritten.
- **Next action:** append the decision log on the next code change. Build billing and trust before those pages show accounts or invoices.

## 2026-10-13 - Planning folder and database class diagram

- **Recorded:** 2026-10-13 23:15 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** put timestamped decisions in one planning folder and put the database history next to a UML class diagram. No application code.

### Changes

- `Markdown/planning/README.md`, `decision-log.md`, and `open-decisions.md`. The log index is oldest-first by full timestamp, with who decided. Method follows MADR (date, decision-makers, good and bad consequences) and stays one file so order stays visible.
- `Markdown/delivery/decision-log.md` and `open-decisions.md` are pointers only.
- `Markdown/database/schema-evolution.md`: MySQL 8.0.36, EF migrations in UTC order from the migration ids (`Baseline` at 2026-10-12 05:07:14 UTC, `MatterAccessGrants` at 05:07:41 UTC), and columns that are not foreign keys.
- `Markdown/database/uml-class-diagrams.md`: class to table, attribute to column, association to foreign key, for the applied schema. The data-dictionary ERD stays the designed model.
- Links updated from the README, AGENTS.md, ProgressPlan, this file, the gap map, and the documents that pointed at the old open-decision path.

### Checks run (actual results)

- Documentation only. Class and foreign-key names were taken from `LawFirmDbContext` and the entity files. `ConflictCheck` has no `Status` column; the diagram uses `Query`. `MatterAccessGrant` uses `ActorId`.

### Checks not run, and why

- No build or browser check. The schema was not migrated.

### Outcome and next action

- **Outcome:** Decision order is the timestamp index in `Markdown/planning/decision-log.md` (D-20261013-4). Database evolution and the class diagram refer to each other.
- **Benefit and possible future downfall:** [D-20261013-4](Markdown/planning/decision-log.md). Benefit: a later decision cannot be filed above an earlier one. Downfall: editing the old `delivery/` pointers as if they were the record.
- **Next action:** on the next schema change, append schema evolution and update the class diagram in the same session.

## 2026-10-14 - Appointment request dialog layout

- **Recorded:** 2026-10-14 02:35 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** restyle the public Request appointment dialog so it is wider and the request fits without scrolling the dialog on a desktop window.

### Changes

- `frontend/src/pages/public/ContactPage.tsx`: appointment dialog only. Contact fields are a two-by-two grid, the message is shorter, and the calendar sits beside the time list.
- `frontend/src/app/AppShell.css`: `.modal-card--appointment` is up to 1040px wide, with a tinted header. The shared `.modal-card` width is unchanged, so the workspace calendar dialog stays narrow.
- Below about 860px the columns stack.

### Checks run (actual results)

- Opened `http://127.0.0.1:5173/contact` with the API already running. On a 1440×900 viewport the dialog was 1040×554 and its scroll height matched its client height.
- Selected Thursday, October 15, then 10:30 AM. The summary updated. Typed a name. Next month changed the calendar to November 2026.
- At about 456px wide the form stacked, the dialog scrolled vertically, and the page did not scroll sideways.

### Checks not run, and why

- Did not submit a consultation request, so no new intake row was created.
- Did not open the workspace calendar dialog in the browser. Its markup still uses `.modal-card` without the appointment class.

### Outcome and next action

- **Outcome:** The public appointment dialog shows the form and the schedule together on a desktop window.
- **Benefit and possible future downfall:** [D-20261014-1](Markdown/planning/decision-log.md). Benefit: the request is visible without scrolling the dialog at 1440×900. Downfall: windows under about 860px stack and scroll again.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Appointment dialog scale and overlap guard

- **Recorded:** 2026-10-14 02:59 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** keep the new appointment modal design, scale it up, and prevent overlap between the input section and the appointment-time section.

### Changes

- `frontend/src/app/AppShell.css`: increased `.modal-card--appointment` max width from 1040px to 1220px and scaled up modal typography/spacing for a larger visual presentation.
- `frontend/src/app/AppShell.css`: increased spacing and control sizes in the appointment header, form, calendar, and time chips.
- `frontend/src/app/AppShell.css`: added a responsive guardrail so the form stacks to one column below 1200px and keeps base typography below 980px, avoiding input/picker overlap in medium-width windows.

### Checks run (actual results)

- Opened `http://127.0.0.1:5173/contact` with the API already running and opened Request appointment.
- On a 1400×900 viewport, measured modal size 1220×611 with scroll gap 0 (full request visible without modal scrolling).
- Confirmed no overlap between the basic input block and appointment-time block in the two-column layout at that viewport.
- At smaller widths, confirmed the new single-column fallback engages and prevents horizontal collision.

### Checks not run, and why

- Did not submit a consultation request, so no new intake row was created.
- Did not run backend or frontend test suites because this slice only changed CSS.

### Outcome and next action

- **Outcome:** The modal keeps the same structure, appears larger, and avoids overlap between core input and scheduling sections.
- **Benefit and possible future downfall:** [D-20261014-2](Markdown/planning/decision-log.md). Benefit: larger, clearer modal without input overlap. Downfall: the larger footprint and earlier stacking reduce visible page context and side-by-side layout at medium widths.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Global background image switcher

- **Recorded:** 2026-10-14 03:25 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** allow law-themed background images across the whole app and provide an in-app way to switch among them.

### Changes

- Added `frontend/src/app/backgroundThemes.ts` with four switchable background choices.
- Updated `frontend/src/app/AppShell.tsx` to:
  - load/save the selected background in `localStorage` (`matterforge.app.background-theme`),
  - set the global CSS background variable on startup and when changed,
  - render a fixed Background picker with clickable options.
- Updated `frontend/src/app/AppShell.css` with background picker styles.
- Updated `frontend/src/index.css` so `body` uses the selected global background image variable with fixed/cover positioning.

### Checks run (actual results)

- `npm run build` in `frontend/` succeeded.
- Browser check on `http://127.0.0.1:5173/`:
  - opened the Background picker and verified four options appeared,
  - selected “Law Library,”
  - confirmed `localStorage` key `matterforge.app.background-theme` changed to `law-books`.
- Navigated to `http://127.0.0.1:5173/contact` and confirmed the selected background remained active.

### Checks not run, and why

- Did not verify in authenticated workspace routes because this was validated on shared shell behavior and public route navigation.
- No backend tests were run because the slice only changed frontend UI and styling.

### Outcome and next action

- **Outcome:** The app now supports switchable, persistent full-page background imagery.
- **Benefit and possible future downfall:** [D-20261014-3](Markdown/planning/decision-log.md). Benefit: easy visual variety across all pages. Downfall: external image dependencies and potential readability drift if overlays are not tuned.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Background visibility fix for global theme switcher

- **Recorded:** 2026-10-14 03:51 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** investigate why background selection looked inactive and make the chosen background visibly apply across the entire app.

### Changes

- `frontend/src/app/AppShell.tsx`: changed the CSS variable assignment to store only the selected image URL.
- `frontend/src/app/AppShell.css`: added a dedicated app-wide render layer using `.app-shell::before` that combines:
  - a lighter readability overlay, and
  - the selected background image variable.
- `frontend/src/index.css`: removed body-level image painting so background rendering is owned by the shared app shell layer.

### Checks run (actual results)

- `npm run build` in `frontend/` succeeded.
- Browser deep check on `http://127.0.0.1:5173/`:
  - opened Background picker and selected “Courthouse Columns,”
  - confirmed `localStorage` changed to `columns`,
  - confirmed `--app-shell-background-image` and `.app-shell::before` computed styles matched the selected image.
- Navigated to `http://127.0.0.1:5173/contact` and confirmed the same selected background stayed active there.

### Checks not run, and why

- Did not run backend tests because this slice changed frontend rendering only.
- Did not test all authenticated routes individually; this fix applies at shared shell scope.

### Outcome and next action

- **Outcome:** Background selections now visibly change the full app background across pages instead of appearing unchanged.
- **Benefit and possible future downfall:** [D-20261014-4](Markdown/planning/decision-log.md). Benefit: clearer, visible theme switching with persistence. Downfall: lighter overlays can require future contrast tuning.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Background catalog expansion

- **Recorded:** 2026-10-14 03:58 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** expand the background selector with many choices across corporate office, downtown, legal, and artistic legal-adjacent aesthetics.

### Changes

- `frontend/src/app/backgroundThemes.ts`: expanded from 4 to 23 selectable backgrounds.
- `frontend/src/app/AppShell.css`: made the Background options panel scrollable (`max-height` + `overflow-y`) so the larger list is usable on smaller screens.

### Checks run (actual results)

- `npm run build` in `frontend/` succeeded.
- Browser check on `http://127.0.0.1:5173/`:
  - opened Background picker and confirmed all options render,
  - measured 23 `.background-picker__option` entries,
  - verified panel scroll behavior (`scrollHeight` 1463 vs `clientHeight` 436).

### Checks not run, and why

- Did not validate every new external image URL manually; this slice focused on option availability and selector behavior.
- No backend tests were run because the slice is frontend-only.

### Outcome and next action

- **Outcome:** The app now offers a broad background catalog with many styles and a usable scrolling picker.
- **Benefit and possible future downfall:** [D-20261014-5](Markdown/planning/decision-log.md). Benefit: much richer visual choice without code edits. Downfall: larger reliance on external image hosts.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Skyline pinned, picker hidden

- **Recorded:** 2026-10-14 04:06 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** keep City Skyline at Night as the app-wide background and temporarily hide the dropdown background selector.

### Changes

- `frontend/src/app/AppShell.tsx`:
  - pinned active background to `city-skyline-night`,
  - commented out the Background picker section in JSX (kept in file for quick restore).
- Left the background catalog and picker CSS in place so it can be re-enabled without rebuilding options.

### Checks run (actual results)

- `npm run build` in `frontend/` succeeded.
- Browser check on `http://127.0.0.1:5173/`:
  - confirmed no `.background-picker` element rendered,
  - confirmed `localStorage` key `matterforge.app.background-theme` equals `city-skyline-night`,
  - confirmed `--app-shell-background-image` is set to the skyline image URL.

### Checks not run, and why

- Did not test each workspace route individually because the background is applied at app shell scope.
- No backend tests were run because this slice is frontend-only.

### Outcome and next action

- **Outcome:** City Skyline at Night is now the fixed global background and the dropdown is hidden.
- **Benefit and possible future downfall:** [D-20261014-6](Markdown/planning/decision-log.md). Benefit: consistent visual baseline for feedback sessions. Downfall: users cannot compare alternatives live until the picker returns.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - About and Practice hero color warming

- **Recorded:** 2026-10-14 04:10 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** make only the About and Practice Areas hero headers less gray/white and less dull.

### Changes

- `frontend/src/app/AppShell.css`:
  - `marketing-hero--about`: replaced flatter pale overlay with warmer layered gradients (accent radial + warm linear).
  - `marketing-hero--practice`: same treatment with a distinct warm tint, limited to that page class.
- No other hero variants (`attorneys`, `contact`, `not-found`) were changed.

### Checks run (actual results)

- `npm run build` in `frontend/` succeeded.
- Browser computed-style checks confirmed both selectors now use the updated warmer gradient stacks.
- Practice Areas page visual check confirms the header now appears warmer and less washed out.

### Checks not run, and why

- About page screenshot output in this browser session did not render page body correctly despite DOM content being present; used computed-style verification for that hero in this pass.
- No backend tests were run because this slice is frontend CSS only.

### Outcome and next action

- **Outcome:** About and Practice Areas headers now have richer, warmer tone while preserving page-specific scope.
- **Benefit and possible future downfall:** [D-20261014-7](Markdown/planning/decision-log.md). Benefit: less dull hero appearance on the two requested pages only. Downfall: future image swaps may require minor contrast retuning.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Hero vibrancy pass two

- **Recorded:** 2026-10-14 04:16 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** further reduce dull gray tone in Practice and About hero headers and add a more vibrant aura on Home hero.

### Changes

- `frontend/src/app/AppShell.css`:
  - strengthened warm overlays for `marketing-hero--about` and `marketing-hero--practice`,
  - increased Home hero aura via richer accent gradients, higher image presence, and stronger glow treatment.
- Scope remained limited to Home, About, and Practice hero styling.

### Checks run (actual results)

- `npm run build` in `frontend/` succeeded.
- Browser computed-style checks confirmed updated gradient stacks for:
  - `.home-hero`
  - `.marketing-hero--about`
  - `.marketing-hero--practice`

### Checks not run, and why

- Browser screenshot output in this session intermittently produced blank viewport captures despite populated DOM snapshots; verification relied on computed styles and successful build.
- No backend tests were run because this slice is frontend CSS only.

### Outcome and next action

- **Outcome:** The targeted headers now have a stronger, more vibrant look with less gray wash.
- **Benefit and possible future downfall:** [D-20261014-8](Markdown/planning/decision-log.md). Benefit: more visual life on requested headers. Downfall: stronger overlays may need future tuning for contrast balance.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Hero vibrancy midpoint rebalance

- **Recorded:** 2026-10-14 04:18 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** tone down the most recent hero vibrancy pass and settle on a balanced midpoint for Home, About, and Practice.

### Changes

- `frontend/src/app/AppShell.css`:
  - reduced Home hero aura intensity (`.home-hero`, `.home-hero__image`, `.home-hero__glow`),
  - reduced overlay strength for About and Practice hero classes (`.marketing-hero--about`, `.marketing-hero--practice`).
- Kept scope limited to those three page hero treatments.

### Checks run (actual results)

- `npm run build` in `frontend/` succeeded.
- `ReadLints` reported no linter errors in `frontend/src/app/AppShell.css`.

### Checks not run, and why

- No browser visual capture in this step due intermittent screenshot rendering issues in this session; this pass used controlled value reduction from the prior state.
- No backend tests were run because this slice is frontend CSS only.

### Outcome and next action

- **Outcome:** Hero styling is less intense than the prior pass while retaining richer tone than the earlier dull state.
- **Benefit and possible future downfall:** [D-20261014-9](Markdown/planning/decision-log.md). Benefit: visual balance between flat and over-vibrant. Downfall: subjective tuning may prompt additional iterations.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - About and Practice hero balance parity

- **Recorded:** 2026-10-14 04:34 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** align the visual balance of the `Practice Areas` and `About` hero headers with the calmer target shown on `Contact` and `Attorneys` (not too vibrant, not too gray).

### Changes

- `frontend/src/app/AppShell.css`:
  - `marketing-hero--about`: removed accent radial overlays and switched to a neutral two-stop top-to-bottom overlay matching the Contact/Attorneys balance profile.
  - `marketing-hero--practice`: same adjustment to remove standout warmth and match the same balanced overlay profile.
- Scope was intentionally limited to those two page hero variants.

### Checks run (actual results)

- `npm run build` in `frontend/`: success.
- `ReadLints` on `frontend/src/app/AppShell.css`: no linter errors.

### Checks not run, and why

- No browser screenshot capture in this pass; the change was a direct normalization to the already approved Contact/Attorneys overlay profile.
- No backend tests were run because this slice is frontend CSS only.

### Outcome and next action

- **Outcome:** About and Practice hero headers now share the same calm contrast balance as Contact and Attorneys, reducing visual over-emphasis while keeping readability.
- **Benefit and possible future downfall:** [D-20261014-10](Markdown/planning/decision-log.md). Benefit: consistent hero tone across marketing pages. Downfall: strict visual parity can reduce per-page mood differentiation.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Centralized signed-in page header styling

- **Recorded:** 2026-10-14 04:43 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** centralize the signed-in workspace page header styling so title/intro updates can be made in one place instead of per-page utility class edits.

### Changes

- `frontend/src/app/AppShell.css`:
  - Added authenticated-shell scoped selectors for workspace page headers:
    - `.app-shell--authenticated .page > h1`
    - `.app-shell--authenticated .page > h1 + .lead`
  - These selectors now control signed-in page title weight/size/color and intro lead width/type scale.
- `frontend/src/pages/workspace/TasksCalendarPage.tsx`:
  - Removed page-specific utility overrides from the title and intro paragraph so the page uses centralized workspace header styling.

### Checks run (actual results)

- `npm run build` in `frontend/`: success.
- `ReadLints` on edited files: no linter errors.

### Checks not run, and why

- No browser click-through of every workspace page in this step; selectors were scoped to existing `.page` structure under the authenticated shell and validated by successful build and diagnostics.
- No backend tests were run because this slice changes frontend styling only.

### Outcome and next action

- **Outcome:** Signed-in header typography can now be tuned centrally in one CSS location and applied across workspace pages automatically.
- **Benefit and possible future downfall:** [D-20261014-11](Markdown/planning/decision-log.md). Benefit: one-place updates for consistent workspace heading tone. Downfall: pages with intentionally unique heading treatment may need explicit opt-out classes.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Home hero left-gradient reduction

- **Recorded:** 2026-10-14 04:52 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** remove or greatly reduce the burgundy-left visual weight on the Home page hero header while keeping the existing overall structure.

### Changes

- `frontend/src/app/AppShell.css`:
  - `.home-hero` first radial accent was reduced from `var(--accent) 21%` to `var(--accent) 6%` and expanded slightly outward (`transparent 56%`) to soften concentration.
- Scope remained limited to Home hero background tinting.

### Checks run (actual results)

- `npm run build` in `frontend/`: success.
- `ReadLints` on `frontend/src/app/AppShell.css`: no linter errors.

### Checks not run, and why

- No browser screenshot was captured in this pass; this was a targeted magnitude reduction on one existing gradient stop.
- No backend tests were run because this slice changes frontend CSS only.

### Outcome and next action

- **Outcome:** The Home hero now has a much softer burgundy-left influence while retaining the rest of the hero composition.
- **Benefit and possible future downfall:** [D-20261014-12](Markdown/planning/decision-log.md). Benefit: cleaner header balance with less heavy color cast on initial page load. Downfall: reducing accent too far can make the hero feel flatter against bright backgrounds.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Home hero left accent hue shift (blue)

- **Recorded:** 2026-10-14 04:55 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** replace the Home hero left-side burgundy tint with a blueish hue while keeping the same overall hero composition.

### Changes

- `frontend/src/app/AppShell.css`:
  - `.home-hero` left radial accent changed from `var(--accent)` mix to `#4b82c7` mix at similar subtle magnitude.
- Scope remained limited to the Home hero left accent color.

### Checks run (actual results)

- `npm run build` in `frontend/`: success.
- `ReadLints` on `frontend/src/app/AppShell.css`: no linter errors.

### Checks not run, and why

- No browser screenshot captured in this pass; this was a single-hue substitution in an existing gradient layer.
- No backend tests were run because this slice changes frontend CSS only.

### Outcome and next action

- **Outcome:** Home header left-side accent now reads as a subtle blue tone instead of burgundy.
- **Benefit and possible future downfall:** [D-20261014-13](Markdown/planning/decision-log.md). Benefit: cooler accent reduces warm color dominance on first impression. Downfall: blue tint could feel less aligned with warm brand accents used elsewhere.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Home hero red-cast removal (blue rebalance)

- **Recorded:** 2026-10-14 04:58 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** remove the remaining red-gray cast from the Home hero and rebalance the entire header tint to blue.

### Changes

- `frontend/src/app/AppShell.css`:
  - `.home-hero` left radial accent increased slightly in blue presence for clearer cool tone.
  - `.home-hero` right radial accent switched from warm peach (`#c9907a`) to cool blue (`#7ea6d8`).
  - `.home-hero` base linear gradient mix replaced warm maroon/beige blend with deep-blue/light-blue neutral blend.
- Scope remained limited to Home hero background tint layers.

### Checks run (actual results)

- `npm run build` in `frontend/`: success.
- `ReadLints` on `frontend/src/app/AppShell.css`: no linter errors.

### Checks not run, and why

- No browser screenshot capture in this pass; verification focused on direct CSS hue substitution and successful frontend diagnostics.
- No backend tests were run because this slice changes frontend CSS only.

### Outcome and next action

- **Outcome:** Home hero now reads with a blue-neutral tint stack and no intentional warm red overlay components.
- **Benefit and possible future downfall:** [D-20261014-14](Markdown/planning/decision-log.md). Benefit: the Home header now aligns with the requested cool-toned direction. Downfall: cooler tint may require minor contrast re-checks against warm CTA button colors.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Home hero plain-blue isolation test

- **Recorded:** 2026-10-14 05:00 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** set the Home hero to plain blue for direct visual testing and remove any remaining warm influence from layered overlays.

### Changes

- `frontend/src/app/AppShell.css`:
  - `.home-hero` background replaced with solid `#6f98cf`.
  - `.home-hero__image` set to `display: none`.
  - `.home-hero__glow` set to `display: none`.
- Scope remained limited to Home hero visuals.

### Checks run (actual results)

- `npm run build` in `frontend/`: success.
- `ReadLints` on `frontend/src/app/AppShell.css`: no linter errors.

### Checks not run, and why

- No browser screenshot captured in this pass; this change was an explicit flat-color test configuration.
- No backend tests were run because this slice changes frontend CSS only.

### Outcome and next action

- **Outcome:** Home hero is now plain blue with no image/glow overlays, allowing direct confirmation of non-red coloration.
- **Benefit and possible future downfall:** [D-20261014-15](Markdown/planning/decision-log.md). Benefit: clean isolation test to validate hue without layered effects. Downfall: this removes the richer branded hero depth until styling is reintroduced.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Home hero restored with blue-only layered styling

- **Recorded:** 2026-10-14 05:03 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** restore the full Home hero appearance (Lady Liberty image + depth layers) while keeping all tinting blue instead of red.

### Changes

- `frontend/src/app/AppShell.css`:
  - `.home-hero` switched from flat test blue back to layered gradients using only cool blue tones.
  - `.home-hero__image` re-enabled with a cool blue overlay on top of the Lady Liberty image.
  - `.home-hero__glow` re-enabled with a soft blue glow.
- Scope remained limited to Home hero visual layers.

### Checks run (actual results)

- `npm run build` in `frontend/`: success.
- `ReadLints` on `frontend/src/app/AppShell.css`: no linter errors.

### Checks not run, and why

- No screenshot captured in this pass; verification focused on deterministic CSS restoration with blue-only palette values and successful build/lint.
- No backend tests were run because this slice changes frontend CSS only.

### Outcome and next action

- **Outcome:** Home hero keeps the original rich composition while replacing warm red cast with cool blue treatment.
- **Benefit and possible future downfall:** [D-20261014-16](Markdown/planning/decision-log.md). Benefit: preserves visual richness while matching the requested cool hue direction. Downfall: layered blue overlays may still need fine opacity tuning for perfect contrast on all displays.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.

## 2026-10-14 - Home hero manual-tuning comments

- **Recorded:** 2026-10-14 05:06 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** add inline CSS comments to the Home hero block so manual visual tuning is faster and clearer.

### Changes

- `frontend/src/app/AppShell.css`:
  - Added comments above Home hero gradient layers describing how alpha and color changes affect left tint, right tint, and base wash.
  - Added comments in `.home-hero__image` explaining how `opacity` and overlay alpha values affect Lady Liberty image visibility and wash strength.
  - Added comments in `.home-hero__glow` explaining how `opacity`, `blur`, color alpha, and transparent stop affect glow punch and spread.
- Scope was documentation comments only; no value changes in this pass.

### Checks run (actual results)

- `npm run build` in `frontend/`: success.
- `ReadLints` on `frontend/src/app/AppShell.css`: no linter errors.

### Checks not run, and why

- No browser visual pass was needed because this slice only added explanatory comments without CSS value changes.
- No backend tests were run because this slice is frontend stylesheet comments only.

### Outcome and next action

- **Outcome:** Home hero color/opacity controls are now self-documented inline for quick manual experimentation.
- **Benefit and possible future downfall:** [D-20261014-17](Markdown/planning/decision-log.md). Benefit: faster tuning with less trial-and-error context loss. Downfall: stale comments can mislead if styles evolve but comments are not updated.
- **Next action:** billing and trust ledgers before those pages show real accounts or invoices.
