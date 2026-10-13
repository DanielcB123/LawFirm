# Slice History

A **slice** is one bounded implementation step: a small piece of work carried end to end (schema, API, UI where relevant, tests, documentation) that leaves the application runnable. Entries are append-only. Each heading's date and slice name is its stable ID. Later outcomes are added as new entries; earlier failures are not rewritten. Commit references appear only when a commit exists. Current state lives in [ProgressPlan.md](ProgressPlan.md).

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
