## 2026-10-23 - Firm partner user-management and seeded login expansion

- **Recorded:** 2026-10-23 15:36 EDT. **Not committed.**
- **Objective and scope:** seed firm-branded partner credentials, add partner-managed firm user onboarding/settings flows, and keep tenant/user boundaries fail-closed.

### Changes

- Added partner-manage permission in `laravel-backend/config/roles.php`:
  - `users.manage` granted to `Partner`.
- Added tenant user-management API controller:
  - `laravel-backend/app/Http/Controllers/Api/TenantUserController.php`.
- Added tenant routes (all permission-gated and tenant-scoped):
  - `GET /api/tenant/users`,
  - `POST /api/tenant/users`,
  - `PUT /api/tenant/users/{user}/membership`.
- Added firm-partner role guardrails:
  - endpoints require actor membership role `Partner` in the currently resolved tenant context,
  - cross-firm membership updates return `404` without leaking other-firm membership state.
- Expanded seeding in `laravel-backend/database/seeders/DatabaseSeeder.php`:
  - added `lawfirmbrianandsandra@matterforge.com` (`Partner`) and attached to seeded Brian & Sandra firm.
- Added frontend team-management feature:
  - `vue-frontend/src/features/team/api.ts`,
  - `vue-frontend/src/pages/workspace/WorkspaceUsersPage.vue`,
  - routed `/workspace/settings` to real team-access page (`vue-frontend/src/router/index.ts`).
- Updated operations/auth docs:
  - `Markdown/operations/docker-and-laravel-command-runbook.md` seeded-login note after `db:seed`,
  - `docs/seeded-auth-users-and-permissions.md` updated user list, role matrix, and tenant route permissions.
- Added/updated tests in `laravel-backend/tests/Feature/FirmTenancyTest.php`:
  - partner can create/list/update users in own firm,
  - non-partner blocked even when global permission is present.

### Checks run (actual results)

- `vendor/bin/pint --dirty --format=agent` -> passed.
- `php artisan test --compact tests/Feature/FirmTenancyTest.php` -> first run failed while authoring new partner test; after membership-role fix, rerun passed (19 tests, 146 assertions).
- `npm run build` in `vue-frontend` -> passed.
- `ReadLints` on edited backend/frontend files -> no diagnostics.

### Outcome and next action

- **Outcome:** firm partners can now onboard/manage firm users from workspace settings with role-based permission assignment and tenant-scoped enforcement.
- **Next action:** run `docker compose --profile dev exec -T backend-dev php artisan db:seed --force`, sign in as `lawfirmbrianandsandra@matterforge.com`, and validate create/change role flows at `/workspace/settings`.

## 2026-10-23 - Platform owner vs ops permission split hardening

- **Recorded:** 2026-10-23 13:01 EDT. **Not committed.**
- **Objective and scope:** split broad platform-admin authority into explicit `platform_owner` and `platform_ops` roles with strict server-side permission boundaries and compatibility-safe auth payload updates.

### Changes

- Added platform role and permission map in `laravel-backend/config/platform.php`.
- Added control-plane migration `laravel-backend/database/migrations/2026_10_23_165500_add_platform_role_to_users_table.php`:
  - `users.platform_role` nullable indexed string.
- Updated `laravel-backend/app/Models/User.php`:
  - role/permission helpers (`platformRole`, `platformPermissions`, `hasPlatformAccess`, `hasPlatformPermission`, owner/ops helpers),
  - compatibility fallback preserves behavior for legacy rows that only have `is_platform_administrator = true`.
- Added `RequirePlatformPermission` middleware and alias (`platform-permission`) in bootstrap.
- Applied permission middleware per platform route in `laravel-backend/routes/api.php`.
- Updated `FirmPolicy` to action-level permission checks and aligned controller authorization calls in `PlatformFirmController`.
- Updated auth payloads in `AuthController` and `FirmOnboardingController` to include:
  - `platformRole`,
  - `platformPermissions`,
  - compatibility boolean `isPlatformAdministrator` from `hasPlatformAccess()`.
- Updated seeding:
  - `partner@matterforge.local` -> `platform_owner`,
  - `adminit@matterforge.local` -> `platform_ops`.
- Updated frontend auth typing/session helpers and platform-admin page action visibility:
  - added `platformRole` and `platformPermissions` fields,
  - added `hasPlatformPermission()` session helper,
  - hid owner-only actions on `PlatformFirmsPage.vue` while keeping backend enforcement authoritative.
- Added/updated feature tests in `laravel-backend/tests/Feature/FirmTenancyTest.php`:
  - ops cannot execute owner-only transitions,
  - ops can execute allowed platform operations.

### Checks run (actual results)

- `php artisan test --compact tests/Feature/FirmTenancyTest.php tests/Feature/TenantMysqlProvisioningTest.php` -> first runs failed during new test authoring; final rerun passed (18 tests, 146 assertions).
- `vendor/bin/pint --dirty --format=agent` -> passed.
- `npm run build` in `vue-frontend` -> passed.
- `ReadLints` on edited backend/frontend files -> no diagnostics.

### Outcome and next action

- **Outcome:** platform-role boundaries are now explicit and enforced at route middleware + policy layers; frontend now reflects role capabilities without relying on UI-only security.
- **Next action:** run `php artisan migrate && php artisan db:seed`, then manually verify owner vs ops control visibility and direct-API 403 behavior for owner-only endpoints.

## 2026-10-23 - Platform-host login cross-origin redirect removal

- **Recorded:** 2026-10-23 11:15 EDT. **Not committed.**
- **Objective and scope:** remove cross-host redirects in post-login/firm-selection flow to prevent bearer-token loss across origins in local dev.

### Changes

- Updated `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue`:
  - removed tenant-host redirect for single-membership users on platform host,
  - set seeded platform-admin email as default login value for clearer local testing,
  - added explicit SaaS admin credential hint text.
- Updated `vue-frontend/src/pages/workspace/FirmSelectorPage.vue`:
  - removed tenant-host URL builder and host redirect,
  - selection now persists firm context then routes to `/workspace` on current host.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> passed.
- `ReadLints` on edited files -> no diagnostics.

### Outcome and next action

- **Outcome:** platform-host auth state now stays on one origin, avoiding guest fallback on tenant hosts due to `localStorage` scope.
- **Next action:** manually verify platform-admin login lands on `/workspace/platform/firms` and non-admin login stays on platform-host workspace with selected firm context.

## 2026-10-23 - Platform-admin routing alignment on platform hosts

- **Recorded:** 2026-10-23 11:04 EDT. **Not committed.**
- **Objective and scope:** ensure platform admins signing in from platform hosts land in SaaS admin workflows, not tenant-host customer/public flows.

### Changes

- Updated `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue` to prioritize platform-admin redirect to `/workspace/platform/firms` on platform hosts.
- Updated `vue-frontend/src/router/index.ts` to:
  - exclude platform admins from firm-selector enforcement,
  - route platform-admin `/workspace` to `/workspace/platform/firms` on platform hosts,
  - route guestOnly post-auth redirects for platform admins to platform-firms page.
- Updated `vue-frontend/src/layout/WorkspaceShell.vue` with platform-admin surface mode:
  - platform brand label,
  - SaaS-management nav options,
  - hides firm practice nav on platform-admin surface.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> passed.
- `ReadLints` on edited files -> no diagnostics.

### Outcome and next action

- **Outcome:** platform-host admin sign-in now remains on SaaS-admin side instead of redirecting into tenant-host customer context.
- **Next action:** manually verify browser flow and confirm `/workspace/platform/firms` is the first authenticated landing page for platform admins on `localhost:5173`.

## 2026-10-23 - Tenant-host redirect port preservation fix

- **Recorded:** 2026-10-23 10:57 EDT. **Not committed.**
- **Objective and scope:** fix local tenant-host workspace redirects that dropped the frontend port after platform-host login/selection.

### Changes

- Updated `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue` redirect logic to append the current browser port when redirecting to a preferred tenant host.
- Updated `vue-frontend/src/pages/workspace/FirmSelectorPage.vue` redirect helper to preserve the current browser port when building tenant workspace URLs.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> passed.
- `ReadLints` for edited files -> no diagnostics.

### Outcome and next action

- **Outcome:** local redirect now targets `http://<tenant-host>:5173/workspace` instead of defaulting to port 80.
- **Next action:** manually re-run the login flow and confirm browser no longer lands on connection-refused tenant host URLs.

## 2026-10-20 - Strict v2 Option B host-surface and discovery implementation

- **Recorded:** 2026-10-20 11:49 EDT. **Not committed.**
- **Objective and scope:** execute the strict v2 prompt implementation slice to establish SaaS-first platform hosts, app-host discovery behavior, and canonical tenant fallback host continuity.

### Changes

- Backend tenancy/config:
  - expanded platform/reserved host defaults and env-driven parsing in `laravel-backend/config/tenancy.php`,
  - added tenant fallback suffix config and platform branding defaults (`MatterForge`),
  - added fallback/preferred host helpers on `Firm` model and membership payload host metadata via `User::membershipPayload()`,
  - added automatic platform-managed verified fallback domain creation at firm registration in `FirmRegistry`.
- Frontend host-surface behavior:
  - added host-surface helpers (`vue-frontend/src/features/host/surface.ts`),
  - added SaaS route `/for-law-firms`,
  - added authenticated `/workspace/select-firm` multi-membership selector page,
  - updated router/login guards to require explicit firm selection on platform hosts for multi-membership users,
  - updated API client to include local-only explicit firm selector header when enabled and on platform hosts,
  - updated public home/contact/nav messaging to render SaaS-first copy on platform hosts while preserving tenant-host firm experience.
- Test updates:
  - expanded `FirmTenancyTest` for platform fallback domain creation and reserved platform-host claim rejection.
- Environment/runbook updates:
  - added tenancy host env examples in backend/frontend env files,
  - added frontend host env wiring to `docker-compose.yml`,
  - documented local host simulation and host-role variables in operations runbook.

### Checks run (actual results)

- `docker compose config` -> passed.
- `php artisan test --compact tests/Feature/FirmTenancyTest.php tests/Feature/TenantMysqlProvisioningTest.php` -> first run failed 1 test (reserved host expectation); after tenancy config default update, rerun passed (16 tests, 127 assertions).
- `vendor/bin/pint --dirty --format agent` -> passed.
- `npm run build` in `vue-frontend` -> first run failed on TypeScript implicit `any`; after typing fixes in host/api helpers, rerun passed.
- `ReadLints` on edited backend/frontend files -> no diagnostics.

### Outcome and next action

- **Outcome:** SaaS-first outer host surfaces are now implemented in code with stricter local selector gating and multi-membership discovery flow support.
- **Next action:** perform manual browser verification with hosts-file mappings to validate app-host selector UX and tenant-host redirection paths end-to-end.

## 2026-10-20 - Option B strict prompt v2 packaging

- **Recorded:** 2026-10-20 11:39 EDT. **Not committed.**
- **Objective and scope:** create a stricter copy/paste-ready implementation prompt with explicit checklists and stop-if-fail gates for each phase.

### Changes

- Added `doc/saas-first-root-and-tenant-hosts-implementation-prompt-strict-v2.md` with:
  - hard preflight pass criteria,
  - phase-by-phase checklists,
  - stop-if-fail gates per phase,
  - mandatory test protocol and completion report template.
- Updated planning/handoff artifacts (`ProgressPlan.md`, `SLICE_HISTORY.md`, and decision log) for traceability.

### Checks run (actual results)

- Documentation-only slice: no runtime code/config changes.
- No build/test commands were required in this packaging pass.

### Outcome and next action

- **Outcome:** strict v2 prompt is ready for direct execution with stronger safety and verification controls than v1.
- **Next action:** run strict v2 prompt for implementation when the team wants hard gates and auditable phase completion criteria.

## 2026-10-20 - Option B implementation prompt handoff packaging

- **Recorded:** 2026-10-20 11:28 EDT. **Not committed.**
- **Objective and scope:** convert the approved SaaS-first outer-layer recommendation into a run-ready implementation prompt and replace the brainstorming artifact.

### Changes

- Added `doc/saas-first-root-and-tenant-hosts-implementation-prompt.md` containing:
  - branch/working-tree safety rules,
  - explicit host-role split (`www`, `app`, tenant hosts + fallback URL),
  - phased implementation slices,
  - required tests and completion report format.
- Removed `doc/outer-saas-layer-implementation-brainstorming-and-options.md` after replacing it with the execution prompt document.
- Updated planning/handoff artifacts (`ProgressPlan.md`, `SLICE_HISTORY.md`, and decision log) for traceability.

### Checks run (actual results)

- Documentation-only slice: no runtime code/config changes.
- No build/test commands were required in this packaging pass.

### Outcome and next action

- **Outcome:** the approved Option B direction is now packaged as a detailed Cursor prompt that can be executed directly for implementation.
- **Next action:** run the prompt and implement SaaS-root/app-shell/tenant-host routing behavior in small tested slices.

## 2026-10-19 - Outer SaaS layer brainstorming and options document

- **Recorded:** 2026-10-19 18:39 EDT. **Not committed.**
- **Objective and scope:** evaluate whether root host should remain tenant-firm-first or become SaaS-first, and produce an implementation-options document with recommendation.

### Changes

- Added `doc/outer-saas-layer-implementation-brainstorming-and-options.md` with:
  - option comparison (firm-first vs SaaS-first vs hybrid),
  - recommendation (SaaS-first root + tenant hosts),
  - phased implementation blueprint,
  - risk/mitigation list.
- This artifact was later superseded by `doc/saas-first-root-and-tenant-hosts-implementation-prompt.md` when planning moved into execution-ready prompt packaging.
- Updated planning/handoff artifacts for decision traceability.

### Checks run (actual results)

- Research performed via current web references on:
  - host-header and multi-tenant security controls,
  - custom-domain onboarding flows,
  - platform vs tenant host entrypoint patterns.
- No runtime code changed in this slice; no build/test commands were required.

### Outcome and next action

- **Outcome:** recommendation is to shift to SaaS-first outer layer and keep tenant domains/subdomains as firm entrypoints for public/tenant experiences.
- **Next action:** if approved, implement root-host SaaS shell and preserve host-verified tenant routing for firm data access.

## 2026-10-19 - Complete firm onboarding/domain verification and tenant workspace isolation slice

- **Recorded:** 2026-10-19 20:35 EDT. **Not committed.**
- **Objective and scope:** close major multi-tenant gaps by adding secure domain ownership verification, host-to-tenant routing hardening, tenant-scoped workspace tables/routes, runtime credential abstraction, and a minimal platform-admin onboarding UI.

### Changes

- Added control-plane migrations:
  - `2026_10_20_010000_harden_firm_domains_and_public_site_key.php`
  - `2026_10_20_010100_create_firm_database_credentials_table.php`
- Added tenant migrations:
  - `2026_10_20_020000_create_tenant_contacts_table.php`
  - `2026_10_20_020100_create_tenant_intake_records_table.php`
  - `2026_10_20_020200_create_tenant_consultation_requests_table.php`
  - `2026_10_20_020300_create_tenant_matter_records_table.php`
  - `2026_10_20_020400_create_tenant_matter_tasks_table.php`
  - `2026_10_20_020500_create_tenant_calendar_entries_table.php`
- Added domain verification services:
  - `app/Tenancy/HostnameNormalizer.php`
  - `app/Tenancy/DnsTxtLookup.php`
  - `app/Tenancy/DomainOwnershipProofService.php`
- Added host-only public tenant routing service/middleware:
  - `app/Tenancy/PublicTenantResolver.php`
  - `app/Http/Middleware/EstablishPublicTenantContext.php`
- Expanded platform-admin API/controller flow:
  - firm listing and filtering,
  - reactivate transition,
  - domain challenge reissue/verify/revoke,
  - richer payload with domains/memberships/public site key.
- Moved workspace routes to tenant-prefixed API group and switched workspace models to `TenantModel`.
- Added backfill command:
  - `php artisan tenants:backfill-workspace --firm=<uuid> [--map-all-to-firm] [--apply]`
- Added credential abstraction and provisioning integration:
  - `app/Models/FirmDatabaseCredential.php`
  - `app/Tenancy/TenantCredentialStore.php`
  - updated `TenantSchemaProvisioner` and `TenantConnectionManager`.
- Added platform-admin workspace UI:
  - `vue-frontend/src/pages/workspace/PlatformFirmsPage.vue`
  - route wiring and sidebar visibility for platform admins.
- Added public-site key and extension typing:
  - `publicSiteKey` in branding payload/types,
  - `src/firms/extensions.ts` built-in key registry,
  - shell variant class application.

### Checks run (actual results)

- `php artisan test --compact tests/Feature/FirmTenancyTest.php tests/Feature/TenantMysqlProvisioningTest.php` -> passed (14 tests, 121 assertions).
- `npm run build` in `vue-frontend` -> passed.
- `vendor/bin/pint app database routes tests bootstrap --format agent` -> passed.
- `ReadLints` on edited backend/frontend paths -> no diagnostics.

### Checks not run, and why

- Full backend test suite was not run in this slice to keep execution focused on tenancy/domain/provisioning paths.
- Browser automation was not run; platform-admin UI and host-based flows were validated by route/build/test coverage in this pass.

### Outcome and next action

- **Outcome:** onboarding and tenant-routing path now includes explicit DNS ownership proof and tenant-scoped workspace writes/reads by default.
- **Risk still open:** shared-runtime fallback for DB credentials can still be enabled in non-production environments; strict per-tenant runtime DB users are still required for production readiness.
- **Next action:** execute reviewed backfill mapping in disposable/local data, reconcile row counts, then plan controlled retirement of legacy shared workspace tables.

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

## 2026-10-16 - Laravel/Vue/Docker migration baseline and documentation workspace

- **Recorded:** 2026-10-16 23:59 EDT (UTC−04:00). **Not committed.**
- **Objective and scope:** start migration safely from the repository root by establishing a reproducible baseline, checking Docker and port constraints (including host port 3307), capturing preexisting test/build outcomes, and creating persistent migration documentation in `docs/migration/` before any mass rewrite.

### Changes

- Created isolated branch: `migration/laravel-vue-docker-phase1`.
- Added migration documentation workspace:
  - `docs/migration/README.md`
  - `docs/migration/INVENTORY.md`
  - `docs/migration/CURRENT_ARCHITECTURE.md`
  - `docs/migration/FEATURE_PARITY.md`
  - `docs/migration/TARGET_ARCHITECTURE.md`
  - `docs/migration/DATA_MIGRATION.md`
  - `docs/migration/SECURITY_REVIEW.md`
  - `docs/migration/UI_PARITY.md`
  - `docs/migration/PLAN.md`
  - `docs/migration/PROGRESS.md`
- Updated `ProgressPlan.md` with the migration kickoff checkpoint and command evidence.
- Continued deep-audit pass by reading and tracing endpoint-level behavior in:
  - `backend/Features/Auth/AuthEndpoints.cs`
  - `backend/Features/Parties/PartiesEndpoints.cs`
  - `backend/Features/Conflicts/ConflictEndpoints.cs`
  - `backend/Features/Intake/IntakeEndpoints.cs`
  - `backend/Features/Matters/MattersEndpoints.cs`
  - `backend/Features/Matters/MatterAccessGrantEndpoints.cs`
  - `backend/Features/Calendar/CalendarEndpoints.cs`
  - `backend/Features/Consultations/ConsultationsEndpoints.cs`
  - `backend/Features/ClientPortal/ClientPortalEndpoints.cs`
  and reflected those findings in `docs/migration/INVENTORY.md` and `docs/migration/FEATURE_PARITY.md`.

### Checks run (actual results)

- Repository boundaries and git state:
  - `git rev-parse --show-toplevel` -> `D:/Documents/Projects/LearnCSharpDotNet`
  - `git status --short --branch` (before changes) -> `main...origin/main` with clean tree
  - `git branch --all --verbose --no-abbrev`, `git remote -v` -> single remote `origin`, single tracked branch `main`
- Baseline builds/tests:
  - `dotnet build MatterForge.sln -c Release` -> success, 0 warnings, 0 errors
  - `dotnet test backend.UnitTests -c Release` -> 10 passed, 0 failed
  - `dotnet test backend.IntegrationTests -c EfDesign` -> 48 passed, 0 failed
  - `npm run build` in `frontend/` -> success
  - `npm test` in `frontend/` -> 4 passed
  - `npm run lint` in `frontend/` -> fails with 7 errors and 3 warnings (preexisting baseline)
- Runtime startup check:
  - `dotnet run` in `backend/` -> failed at startup with MySQL connection error ("Unable to connect to any of the specified MySQL hosts.")
- Docker and port checks:
  - `docker version` -> client/server available (Docker Desktop)
  - `docker compose version` -> available
  - host listener check on `3307` -> no listener
  - host listener check on `5073`, `5173`, `3307` -> free at check time
  - process check found `xampp-control`, `httpd`, `mysqld`; MySQL listener observed on host `3306` (`mysqld`), not `3307`

### Checks not run, and why

- No Laravel or Vue migration implementation started yet; this slice is baseline/audit/documentation only.
- Full file-by-file deep review is not complete yet; `docs/migration/INVENTORY.md` explicitly marks reviewed vs pending coverage.
- Visual screenshot baseline capture is not complete yet; `docs/migration/UI_PARITY.md` marks this as pending.

### Outcome and next action

- **Outcome:** Safe baseline established with explicit pre-migration command evidence, Docker/port status, and migration documentation scaffolding.
- **Next action:** continue Phase 2 deep audit to full file-level coverage and workflow tracing, then implement Docker-first Laravel/Vue vertical slice with MySQL mapped as `127.0.0.1:3307:3306` and no XAMPP dependency.

## 2026-10-17 - Continue audit coverage and add authenticated Laravel vertical slice

- **Recorded:** 2026-10-17 EDT. **Not committed.**
- **Objective and scope:** continue full file-level audit coverage, verify/fix Dockerized Laravel DB runtime correctness, and add an authenticated vertical slice (login + identity + protected contacts) with checks after each step.

### Changes

- Expanded migration audit coverage and documented reviewed files in `docs/migration/INVENTORY.md`:
  - backend contracts + auth/access/error infrastructure abstractions,
  - frontend feature API/type modules,
  - backend integration test suite + unit tests.
- Updated migration tracker docs:
  - `docs/migration/FEATURE_PARITY.md`
  - `docs/migration/UI_PARITY.md`
  - `docs/migration/PROGRESS.md`
- Fixed Laravel runtime DB source drift:
  - added tracked `laravel-backend/.env.docker`,
  - updated `docker-compose.yml` backend to use `.env.docker` and copy it to runtime `.env` on startup.
- Added auth vertical slice in new Laravel backend:
  - installed `laravel/sanctum`,
  - added `app/Http/Controllers/Api/AuthController.php`,
  - added routes `POST /api/auth/login` and `GET /api/identity/me`,
  - protected contacts routes with `auth:sanctum`,
  - updated `app/Models/User.php` with `HasApiTokens`,
  - updated `database/seeders/DatabaseSeeder.php` with deterministic demo user,
  - updated `bootstrap/app.php` to return API-safe unauthenticated responses.
- Updated Vue migration app sign-in flow in `vue-frontend/src/App.vue` to use bearer token for protected contacts routes.

### Checks run (actual results)

- Docker/runtime checks:
  - `docker compose up -d --build backend mysql frontend` -> success
  - `docker compose ps` -> backend healthy, mysql healthy, frontend running
  - `docker compose exec -T mysql mysql -umatterforge -pmatterforge -D matterforge -e "SHOW TABLES;"` -> expected tables present
  - `curl.exe -i http://127.0.0.1:8080/api/health` -> 200 with `"database":"up"`
- Auth and contacts API checks:
  - unauthenticated `GET /api/contacts` initially returned 500 (`Route [login] not defined`) -> fixed by middleware guest-redirect override.
  - recheck unauthenticated `GET /api/contacts` -> `401` JSON (`Unauthenticated.`)
  - `POST /api/auth/login` -> success (token issued for seeded demo user)
  - `GET /api/identity/me` with bearer token -> success
  - authenticated `POST /api/contacts` + `GET /api/contacts` -> success and persisted rows returned.
- Legacy baseline recheck:
  - `dotnet run --urls http://127.0.0.1:5073` in `backend/` -> fails before serve with MySQL auth error (`Access denied for user 'lawfirm_app'... using password: NO`).

### Checks not run, and why

- Full legacy workspace visual baseline capture is still blocked by the legacy backend DB credential failure.
- No new legacy .NET tests were rerun in this slice because changes were additive to new Laravel/Vue/Docker migration directories and docs.

### Outcome and next action

- **Outcome:** Docker-first Laravel/Vue stack now has a working authenticated API slice on the required MySQL topology (`127.0.0.1:3307:3306` host mapping; app uses `mysql:3306`), and audit coverage has been expanded materially.
- **Benefit and possible future downfall:** [D-20261017-1](Markdown/planning/decision-log.md). Benefit: early auth + protected route enforcement validates critical API security wiring before broader workflow migration. Downfall: Sanctum token model currently lacks legacy role/permission parity and could drift if not aligned soon.
- **Next action:** close remaining file-by-file inventory gaps, then implement the next migrated workflow (intake path) with equivalent validation and unauthorized-path tests.

## 2026-10-17 - Close page audit gap and deliver intake + permission slice

- **Recorded:** 2026-10-17 EDT. **Not committed.**
- **Objective and scope:** finish the largest remaining audit gap (`frontend/src/pages/**`), implement intake list/create in Laravel + Vue, and add role/permission route enforcement with explicit unauthorized-path checks.

### Changes

- Inventory/audit:
  - reviewed remaining public/workspace frontend pages and updated `docs/migration/INVENTORY.md`.
  - updated migration status docs: `docs/migration/FEATURE_PARITY.md`, `docs/migration/TARGET_ARCHITECTURE.md`, `docs/migration/PROGRESS.md`.
- Laravel authorization and intake implementation:
  - added `app/Http/Middleware/RequirePermission.php`.
  - registered middleware alias in `bootstrap/app.php`.
  - extended `users` schema with `role` and `permissions` (`2026_10_17_010001_add_role_and_permissions_to_users_table.php`).
  - added intake schema/model/controller:
    - `database/migrations/2026_10_17_010101_create_intake_records_table.php`
    - `app/Models/IntakeRecord.php`
    - `app/Http/Controllers/Api/IntakeController.php`
  - guarded routes in `routes/api.php`:
    - contacts require `contacts.read` / `contacts.manage`
    - intake requires `intake.read` / `intake.manage`.
  - updated auth output and seed data:
    - `app/Http/Controllers/Api/AuthController.php` now returns `role` and `permissions`.
    - `database/seeders/DatabaseSeeder.php` now seeds attorney + read-only users for authorization-path checks.
    - `database/factories/UserFactory.php` updated with default role/permissions fields.
- Vue migration slice:
  - updated `vue-frontend/src/App.vue` with intake list/create UI and permission-aware behavior.

### Checks run (actual results)

- `docker compose exec -T backend php artisan migrate --force` -> success (new users/intake migrations applied).
- `docker compose exec -T backend php artisan db:seed --force` -> success.
- `docker compose exec -T backend php artisan test` -> success (includes `Tests\\Feature\\IntakeApiTest`, all pass).
- Runtime API checks:
  - unauthenticated `GET /api/intake` -> `401`.
  - attorney login -> success, can create/list intake.
  - read-only login -> `403` on intake create.
- Frontend verification:
  - `docker compose exec -T frontend npm run build` -> success.
  - host-side `npm run build` in `vue-frontend` -> fails due missing local `vue-tsc` binary; containerized build remains the validated path.

### Checks not run, and why

- Legacy backend visual workspace capture remains blocked by prior MySQL credential startup failure in the .NET app.

### Outcome and next action

- **Outcome:** intake list/create and route-level permission enforcement are now working in the new Dockerized Laravel/Vue stack with validated unauthorized and forbidden paths.
- **Benefit and possible future downfall:** [D-20261017-2](Markdown/planning/decision-log.md). Benefit: authorization behavior is now explicit and test-backed for migrated workflows. Downfall: permission data currently lives in a simple user JSON field and still needs full legacy policy parity/matter-scope modeling.
- **Next action:** continue inventory normalization for remaining first-party root docs/SQL, then implement intake detail/stage transitions with permission checks and parity tests.

## 2026-10-17 - User-directed hard decommission of legacy runtime code

- **Recorded:** 2026-10-17 EDT. **Not committed.**
- **Objective and scope:** remove legacy runtime code/folders after migration upgrade, retaining Laravel/Vue/Docker stack only.

### Changes

- Deleted legacy code and runtime-design paths:
  - `frontend/` (legacy React app),
  - `backend.IntegrationTests/`,
  - `backend.UnitTests/`,
  - `db/design/`,
  - `MatterForge.sln`,
  - `global.json`,
  - `Directory.Packages.props`,
  - all files under `backend/` (legacy ASP.NET backend implementation).
- Kept migration stack (`laravel-backend/`, `vue-frontend/`, `docker-compose.yml`, `docker/`, `docs/migration/`) intact.

### Checks run (actual results)

- `docker compose config` -> success.
- `docker compose ps` -> backend/mysql/frontend running and healthy.
- `docker compose exec -T backend php artisan test` -> pass (6 tests).
- `docker compose exec -T frontend npm run build` -> success.
- Path existence checks confirm deleted paths are gone, except one caveat below.

### Checks not run, and why

- No legacy .NET/React tests were rerun because those runtimes were intentionally removed in this slice.

### Outcome and next action

- **Outcome:** repository runtime is now Laravel/Vue/Docker-first with legacy runtime code removed.
- **Caveat:** empty top-level `backend/` directory remains due external process lock; contents are fully removed.
- **Benefit and possible future downfall:** [D-20261017-3](Markdown/planning/decision-log.md). Benefit: reduces split-brain maintenance by removing old runtime code from active repository state. Downfall: historical executable parity reference is reduced in-place, increasing dependence on git history/branches for rollback context.
- **Next action:** remove locked empty `backend/` directory when the external lock is released, then normalize remaining docs that still reference deleted .NET/React paths.

## 2026-10-17 - Recover frontend brand styling in Vue migration UI

- **Recorded:** 2026-10-17 EDT. **Not committed.**
- **Objective and scope:** fix local Laravel bootstrap failure (`SanctumServiceProvider` class missing) and restore the migrated Vue frontend to match prior React-era visual style direction instead of the temporary utilitarian prototype look.

### Changes

- Bootstrap/runtime unblock:
  - verified `laravel-backend/vendor` was missing locally, causing Artisan boot failure when resolving Sanctum provider.
  - ran `composer install` in `laravel-backend`, then reran `php artisan key:generate` successfully.
- Vue visual parity refresh:
  - rewrote `vue-frontend/src/App.vue` from inline-styled utility markup into a branded layout shell:
    - sticky top header,
    - hero section with action buttons and stats cards,
    - two-column card grid for sign-in, API status, contacts, and intake workflows,
    - styled footer matching prior product direction.
  - replaced `vue-frontend/src/style.css` with a warm palette and component styling derived from prior React-era UI language (hero layering, card treatments, typography, spacing, button states, mobile collapse behavior).
  - preserved existing Laravel workflow behavior (auth, contacts CRUD, intake create/list and permission-dependent visibility) while changing presentation.

### Checks run (actual results)

- `composer install` in `laravel-backend` -> success; Sanctum package installed and package discovery completed.
- `php artisan key:generate` in `laravel-backend` -> success.
- `docker compose ps` -> backend/mysql healthy; frontend running.
- `curl.exe -i http://localhost:8080/up` -> HTTP 200.
- `docker compose exec -T frontend npm run build` -> success.
- Cursor lint diagnostics for edited files (`App.vue`, `style.css`) -> no errors.

### Checks not run, and why

- No backend feature test rerun in this slice because behavior-level API logic was not changed; work targeted dependency/bootstrap and frontend presentation.

### Outcome and next action

- **Outcome:** migration UI now renders with branded structure and styling closer to the previous React experience, and local Laravel CLI bootstrap/key generation is restored.
- **Benefit and possible future downfall:** [D-20261017-4](Markdown/planning/decision-log.md). Benefit: improves stakeholder confidence and usability by reducing visible design regression during migration demos. Downfall: this is still a single-page Vue shell, not full route-for-route public/workspace parity yet.
- **Next action:** continue extracting page-level parity (home/about/practice/attorneys/contact and workspace route structure) into dedicated Vue routes/components while preserving the validated Laravel API contracts.

## 2026-10-17 - Restore pre-migration visual shell parity in Vue and repair compose file

- **Recorded:** 2026-10-17 EDT. **Not committed.**
- **Objective and scope:** align the migrated Vue frontend with the pre-migration React styling baseline from commit `a7352dc1d4e3778b9309fc5a435f81c646b3d293`, while keeping current Laravel auth/contacts/intake behavior and fixing Docker Compose corruption that could break consistent local frontend runtime.

### Changes

- Legacy reference extraction from commit `a7352dc1d4e3778b9309fc5a435f81c646b3d293`:
  - `frontend/src/app/AppShell.css`
  - `frontend/src/pages/public/HomePage.tsx`
  - `frontend/src/components/layout/SiteNav.tsx`
  - `frontend/src/components/layout/SiteFooter.tsx`
- Vue UI parity pass:
  - rewrote `vue-frontend/src/App.vue` to mirror prior public-shell composition:
    - branded top navigation
    - hero with layered image/glow/grid and stat cards
    - value proposition and consultation-hours cards
    - practice-area highlight cards
    - partner spotlight cards with attorney imagery
    - process and testimonial sections
    - migration workflow panel for sign-in/API/contacts/intake below the public shell
  - replaced `vue-frontend/src/style.css` with React-era design language tokens and component treatments (palette, spacing, typography, card states, button states, hero overlays, responsive behavior).
  - updated `vue-frontend/index.html` title to `Brian & Sandra Law`.
- Docker configuration repair:
  - removed and recreated `docker-compose.yml` to eliminate a malformed accidental `working_dir` line in `frontend` service and restore valid compose structure.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `docker compose config` at repository root -> success; generated compose output shows valid `frontend` `working_dir: /workspace` and all services/networks/volumes resolved.

### Checks not run, and why

- No backend tests rerun because this slice changed frontend presentation and compose file structure only; API behavior was not modified.

### Outcome and next action

- **Outcome:** migrated Vue UI now follows the previous React-era aesthetic much more closely and Docker Compose file corruption is removed.
- **Benefit and possible future downfall:** [D-20261017-5](Markdown/planning/decision-log.md). Benefit: preserves design continuity during platform migration while keeping functional Laravel workflows available. Downfall: single-page shell parity can hide remaining route-level and behavior-level parity gaps if not followed by component/route extraction.
- **Next action:** split this restored shell into dedicated Vue routes/components to complete page-level parity (`home`, `about`, `practice`, `attorneys`, `contact`) and keep validating against Laravel endpoint behavior.

## 2026-10-17 - Compose YAML diagnostic stabilization for frontend service

- **Recorded:** 2026-10-17 EDT. **Not committed.**
- **Objective and scope:** explain and mitigate persistent editor YAML diagnostics (`DUPLICATE_KEY`/`MULTILINE_IMPLICIT_KEY`) reported around `frontend.working_dir` in `docker-compose.yml` despite a compose-valid configuration.

### Changes

- Verified `docker-compose.yml` character content around `services.frontend` contains normal spaces and no hidden/tab characters.
- Normalized `frontend.command` from folded scalar form to explicit list form:
  - `sh`
  - `-lc`
  - `npm install && npm run dev -- --host 0.0.0.0 --port 5173`
- Kept backend command in explicit list form for consistency and parser resilience.

### Checks run (actual results)

- `docker compose -f docker-compose.yml config` -> success; full compose model renders correctly with `frontend.working_dir: /workspace`.
- Cursor diagnostics (`ReadLints`) -> still reports `DUPLICATE_KEY` on `frontend` keys, indicating likely stale/false-positive editor YAML parsing rather than runtime compose invalidity.

### Checks not run, and why

- Did not run full container startup cycle in this slice because the change only re-expressed command YAML structure; runtime validation via compose model expansion was sufficient for scope.

### Outcome and next action

- **Outcome:** compose file remains runtime-valid and command blocks are now written in a linter-friendlier explicit structure.
- **Benefit and possible future downfall:** [D-20261017-6](Markdown/planning/decision-log.md). Benefit: clearer YAML structure and lower risk of scalar-parsing ambiguity in future edits. Downfall: IDE diagnostics can still show stale false positives until YAML language server cache is refreshed.
- **Next action:** if editor squiggles persist, reload the Cursor window / YAML language service and continue using `docker compose config` as the source of truth for compose validity.

## 2026-10-17 - Resolve duplicate-key lint false positive in compose frontend block

- **Recorded:** 2026-10-17 EDT. **Not committed.**
- **Objective and scope:** eliminate YAML parser ambiguity that surfaced as `DUPLICATE_KEY` on `frontend.working_dir`/`frontend.command` in `docker-compose.yml`.

### Changes

- Rewrote `backend.command` and `frontend.command` to inline YAML sequence form:
  - `command: ["sh", "-lc", "..."]`
- Normalized top-level volume declarations to explicit maps:
  - `mysql_data: {}`
  - `laravel_vendor: {}`
  - `vue_node_modules: {}`

### Checks run (actual results)

- `docker compose -f docker-compose.yml config` -> success.
- Cursor diagnostics (`ReadLints`) for `docker-compose.yml` -> no errors.

### Checks not run, and why

- No container restart/load test in this slice because command semantics were preserved and compose model expansion plus lint pass were sufficient for syntax-scope validation.

### Outcome and next action

- **Outcome:** duplicate-key lint error is resolved and `docker-compose.yml` parses cleanly in both Docker Compose and editor diagnostics.
- **Benefit and possible future downfall:** [D-20261017-7](Markdown/planning/decision-log.md). Benefit: stable YAML parsing and fewer editor false alarms during compose edits. Downfall: inline command arrays are denser and can be less readable for very long shell chains.
- **Next action:** keep this command style for compose edits unless commands become long enough to warrant external script files.

## 2026-10-17 - Route-level Vue parity, consultation queue APIs, and workspace auth shell

- **Recorded:** 2026-10-17 EDT. **Not committed.**
- **Objective and scope:** execute approved parity continuation phases by replacing the single-page Vue shell with route-based public pages, restoring consultation booking/queue flow, and introducing a real guarded workspace shell with seeded role personas.

### Changes

- Frontend route and shell architecture:
  - added Vue Router and route map in `vue-frontend/src/router/index.ts`:
    - public routes: `/`, `/about`, `/practice-areas`, `/attorneys`, `/attorneys/:slug`, `/contact`
    - workspace routes: `/workspace/login`, `/workspace/*` (guarded)
    - fallback: `/:pathMatch(.*)*`
  - replaced `vue-frontend/src/App.vue` with router-only host (`<RouterView />`)
  - introduced `PublicShell`, `WorkspaceShell`, `SiteNav`, `SiteFooter`
- Public parity pages and styling:
  - added public page components for home/about/practice/attorneys/attorney profile/contact
  - added `useScrollReveal` composable (IntersectionObserver + reduced-motion handling) for reveal-on-scroll parity
  - expanded `vue-frontend/src/style.css` with marketing-card, attorney-profile, modal, workspace shell, and reveal classes
  - added `teamMembers` data model with attorney + staff role backgrounds
- Consultation booking flow:
  - added `ConsultationModal.vue` and consultation API client/types in `vue-frontend/src/features/consultations/`
  - wired Contact-page CTA to modal and request submission
- Backend consultation queue support:
  - generated and implemented `ConsultationRequest` model/factory/migration
  - implemented `ConsultationController` with:
    - public `availableSlots` and `store`
    - protected queue list and status update
  - added routes:
    - `GET /api/consultations/available-slots`
    - `POST /api/consultations`
    - `GET /api/consultations` (`consultations.read`)
    - `POST /api/consultations/{consultation}/status` (`consultations.manage`)
- Auth and role/permission seed matrix:
  - added `useAuthSession` token/identity session module for guarded routes
  - added `config/roles.php` role-to-permission map
  - expanded `DatabaseSeeder` with seeded personas:
    - Partner, Attorney, Paralegal, Accountant, LegalAssistant, AdminIT, Client
  - seeded sample contacts/intake/consultation queue records
- Tooling/setup side-effects:
  - installed `vue-router` in `vue-frontend`
  - installed Laravel Boost (`laravel/boost`) and generated its local config/guideline files in `laravel-backend`

### Checks run (actual results)

- `npm install` in `vue-frontend` -> success.
- `npm run build` in `vue-frontend` -> success.
- `php artisan test --compact` in `laravel-backend` -> success (11 tests, 50 assertions).
- `vendor/bin/pint --dirty --format agent` in `laravel-backend` -> success (clean formatting pass).
- `docker compose -f docker-compose.yml config` -> success.
- Cursor lint diagnostics (`ReadLints`) for edited frontend/backend paths -> no errors.

### Checks not run, and why

- No browser/E2E interaction run in this slice; verification was API tests + TypeScript build + lint/compose validation due scope and time.

### Outcome and next action

- **Outcome:** migration stack now has route-based public frontend parity, working consultation booking + queue API pipeline, and a guarded workspace shell with seeded operational personas.
- **Benefit and possible future downfall:** [D-20261017-8](Markdown/planning/decision-log.md). Benefit: restores high-confidence demo flow from public intake through internal follow-up queue while re-establishing authenticated workspace structure. Downfall: several workspace modules (calendar/matters/etc.) are still route placeholders pending deeper behavior migration.
- **Next action:** implement Phase 4 depth on `calendar` and `matters` modules (UI + API + permission-aware behavior) and replace remaining placeholder workspace routes with functional parity slices.

## 2026-10-17 - Strict shared consultation-modal parity across public CTAs

- **Recorded:** 2026-10-17 EDT. **Not committed.**
- **Objective and scope:** enforce strict pre-migration consultation modal parity by wiring all consultation-like public CTAs to one shared modal component with legacy-style layout, calendar behavior, accessibility semantics, and Laravel API contract compatibility.

### Changes

- Shared modal architecture:
  - added `vue-frontend/src/features/consultations/modal.ts` as single source of open/close state with trigger-focus return.
  - added `vue-frontend/src/components/consultation/ConsultationModalHost.vue` and mounted it once in `PublicShell`.
  - removed page-local modal duplication from contact page.
- CTA parity wiring:
  - home CTA `Book consultation` now opens modal in-place.
  - added home CTA `Start your consultation` and wired to same modal.
  - converted consultation-style CTAs on other public pages to same modal:
    - about: `Contact our office`
    - practice areas: `Request consultation`
    - attorneys: `Schedule consultation`
    - attorney profile: `Contact the firm`
    - contact page: `Request appointment`
- Modal behavior/layout parity:
  - rewrote `ConsultationModal.vue` to mirror legacy structure:
    - backdrop close + close button + Escape close,
    - `role="dialog"` + `aria-modal="true"` + `aria-labelledby`,
    - large card sizing (`min(1420px, calc(100vw - 2rem))` equivalent),
    - gradient header section,
    - two-column details + scheduling layout with responsive collapse at parity breakpoints,
    - 42-day calendar grid with month nav, availability states, selected state, past/outside-month handling, time-slot chips, selection summary,
    - first-available auto-selection and selected-day/slot synchronization,
    - month-switch modal-scroll position restoration.
  - added body scroll-lock when modal is open (`body.modal-open`).
  - preserved reveal animation behavior outside modal and reduced-motion handling.
- API contract parity update:
  - frontend slot client now supports `startDate` + `days`.
  - backend `availableSlots` now validates/accepts `startDate=YYYY-MM-DD` while keeping existing behavior compatible.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `php artisan test --compact` in `laravel-backend` -> success.
- `ReadLints` for edited frontend/backend files -> no errors.
- Playwright screenshot capture (desktop + mobile) from multiple public CTA entry points -> generated under `artifacts/`.

### Checks not run, and why

- No GIF capture run in this slice; static screenshot evidence was captured instead.

### Outcome and next action

- **Outcome:** all relevant consultation-intake public CTAs now open one shared global modal with legacy-style interaction and near-identical structure/visual behavior.
- **Benefit and possible future downfall:** [D-20261017-9](Markdown/planning/decision-log.md). Benefit: prevents drift from duplicated modal implementations and restores expected intake UX continuity. Downfall: keeping strict parity in one component increases complexity and requires careful maintenance during future design-token or workspace-shell refactors.
- **Next action:** continue Phase 4 module parity (calendar/matters logic) and keep consultation modal regression checks in screenshot/test verification.

## 2026-10-18 - Deterministic Laravel container bootstrap with compose dev/prod profile split

- **Recorded:** 2026-10-18 EDT. **Not committed.**
- **Objective and scope:** remove runtime dependency/cache drift in Docker bootstrap by shifting composer dependency install into image build, introducing an explicit migration job, and splitting Docker Compose into profile-driven dev and prod backend flows.

### Changes

- Added `docker/laravel/entrypoint.sh` with explicit modes:
  - `web`: run application server only.
  - `dev-web`: compare `composer.lock` hash against `vendor/.composer.lock.hash`, run `composer install` only when needed, then start server.
  - `migrate`: run `php artisan migrate --force` for one-shot schema convergence.
- Reworked `docker/laravel/Dockerfile` into deterministic multi-stage build:
  - composer stage installs dependencies from lock file (`--no-scripts`),
  - runtime stage copies vendor and app source, normalizes entrypoint LF line endings, and sets `ENTRYPOINT`.
- Replaced inline bootstrap shell chain in `docker-compose.yml` with profile-separated services:
  - `backend-dev` (`dev` profile, bind mount + vendor volume, command `dev-web`),
  - `backend` (`prod` profile, immutable image, command `web`),
  - `migrate` (`dev` + `prod` profiles, one-shot, command `migrate`),
  - `frontend` moved to `dev` profile and now depends on `backend-dev`.

### Checks run (actual results)

- `docker compose config` -> success.
- `docker compose --profile dev config` -> success.
- `docker compose --profile prod config` -> success.
- `docker compose --profile dev down --remove-orphans` -> success.
- `docker compose --profile dev up -d --build` -> success after resolving two build/startup issues:
  - composer stage script failure fixed by `composer install --no-scripts`,
  - entrypoint startup failure fixed by CRLF normalization in Dockerfile.
- `docker compose --profile dev ps` -> `backend-dev` healthy, `mysql` healthy, `frontend` running, `migrate` exited `0`.
- `curl.exe -i http://localhost:8080/up` -> HTTP 200.

### Checks not run, and why

- Did not run full `prod` profile runtime startup (`docker compose --profile prod up`) in this slice to avoid port/service contention while validating the active developer stack first.
- Did not run Laravel feature tests because this slice changed container/bootstrap orchestration and not application endpoint behavior.

### Outcome and next action

- **Outcome:** compose startup is now deterministic, dependency drift is constrained, and backend bootstrap responsibilities are split into explicit runtime (`web`/`dev-web`) and migration (`migrate`) phases.
- **Benefit and possible future downfall:** [D-20261018-1](Markdown/planning/decision-log.md). Benefit: removes a broad class of unhealthy-container incidents caused by stale `vendor` + cache drift. Downfall: profile-driven startup now requires explicit `--profile dev` / `--profile prod` usage and adds one more operational command pattern to team onboarding.
- **Next action:** add a short runbook section for profile-based startup commands and consider a dedicated `seed` job (separate from `migrate`) for deterministic non-production sample-data workflows.

## 2026-10-18 - Fix consultation modal always-open host binding bug

- **Recorded:** 2026-10-18 EDT. **Not committed.**
- **Objective and scope:** resolve a public-site regression where the shared consultation modal opened by default and could not be dismissed via close button or backdrop click.

### Changes

- Updated `vue-frontend/src/components/consultation/ConsultationModalHost.vue` to bind top-level modal refs/functions directly in template:
  - replaced `modal.isOpen`/`modal.closeModal` object-member binding with destructured `isOpen`/`closeModal`.
- This avoids Vue nested-ref truthiness behavior (`v-if` seeing a ref object as always truthy) and restores correct conditional mount/unmount for the modal component.

### Checks run (actual results)

- `docker compose --profile dev exec -T frontend npm run build` -> success.

### Checks not run, and why

- No automated browser interaction test was run in this slice; build validation confirms compile/runtime wiring, while final dismissal behavior is a direct runtime UI interaction check.

### Outcome and next action

- **Outcome:** consultation modal no longer auto-renders at page load and close handlers can now fully unmount modal state.
- **Benefit and possible future downfall:** [D-20261018-2](Markdown/planning/decision-log.md). Benefit: restores expected modal lifecycle and prevents permanent overlay lock on public routes. Downfall: future composable-return object patterns can reintroduce this class of Vue ref-unwrapping bug if nested refs are used directly in template conditionals.
- **Next action:** keep shared composable template bindings either top-level or explicitly unwrapped for any new modal/dialog hosts.

## 2026-10-18 - Capture Docker profile verification and run commands in architecture README

- **Recorded:** 2026-10-18 EDT. **Not committed.**
- **Objective and scope:** ensure architecture documentation explicitly contains the validated Docker profile rollout checks and operational run/stop commands for developers.

### Changes

- Updated `Markdown/architecture/README.md` with a new `Docker profile rollout verification` section.
- Added exact validation checklist entries and outcomes:
  - `docker compose config`
  - `docker compose --profile dev config`
  - `docker compose --profile prod config`
  - `docker compose --profile dev up -d --build`
  - `docker compose --profile dev ps` health summary (`backend-dev`, `mysql`, `frontend`, `migrate`)
  - `curl http://localhost:8080/up` HTTP 200
  - lint diagnostics status
- Added explicit operator commands for:
  - bringing up dev profile,
  - bringing up prod-style backend profile,
  - tearing down each profile with `--remove-orphans`.

### Checks run (actual results)

- Documentation update only; no runtime commands rerun in this slice because all listed checks were already executed and recorded in prior deterministic-bootstrap verification.

### Checks not run, and why

- No additional container startup or test execution was run in this slice; scope was limited to recording already-validated commands and outcomes in canonical architecture docs.

### Outcome and next action

- **Outcome:** architecture docs now include the exact rollout verification and profile command runbook requested for operational clarity.
- **Benefit and possible future downfall:** [D-20261018-3](Markdown/planning/decision-log.md). Benefit: reduces startup ambiguity and anchors profile-based operations in a stable shared reference. Downfall: command/checklist sections can become stale if compose profiles evolve without synchronized doc maintenance.
- **Next action:** keep the section updated whenever compose profile behavior or runtime command patterns change.

## 2026-10-18 - Document seeded authenticated users and role permissions in docs

- **Recorded:** 2026-10-18 EDT. **Not committed.**
- **Objective and scope:** add an operator-friendly reference in `/docs` that lists seeded authenticated users, role types, and the currently enforced permission gates for authenticated API routes.

### Changes

- Added `docs/seeded-auth-users-and-permissions.md` with:
  - complete seeded user list (`name`, `email`, `role`) sourced from `laravel-backend/database/seeders/DatabaseSeeder.php`,
  - role-to-permission matrix sourced from `laravel-backend/config/roles.php`,
  - permission-to-route gate mapping sourced from `laravel-backend/routes/api.php`,
  - note of shared seeded password for local seed usage (`Passw0rd!`).
- Updated `ProgressPlan.md` with a new current-step entry and refreshed last-updated timestamp to reflect this docs slice.

### Checks run (actual results)

- No runtime/build/test command executed; this is a documentation-only slice with direct source inspection.

### Checks not run, and why

- Did not run backend/frontend tests because no application code paths were modified.

### Outcome and next action

- **Outcome:** `/docs` now has a current, source-backed authentication persona and permissions reference for local sign-in and role-based validation workflows.
- **Benefit and possible future downfall:** [D-20261018-4](Markdown/planning/decision-log.md). Benefit: faster QA and onboarding for auth-permission scenarios without searching seed files. Downfall: document drift risk if seed roles or permissions change without updating this reference.
- **Next action:** keep this docs file synchronized whenever `DatabaseSeeder.php`, `config/roles.php`, or permission-gated routes change.

## 2026-10-18 - Workspace calendar/matters/tasks parity implementation

- **Recorded:** 2026-10-18 EDT. **Not committed.**
- **Objective and scope:** replace remaining signed-in placeholders for calendar and matters depth with functional Laravel/Vue workflows, add a dedicated tasks page, and upgrade consultation queue triage interactions while preserving public consultation intake continuity.

### Changes

- Laravel backend schema and domain:
  - added `matter_records`, `matter_tasks`, and `calendar_entries` migrations with UUID keys, indexes, and matter-task foreign key.
  - implemented models/factories: `MatterRecord`, `MatterTask`, `CalendarEntry`.
  - expanded seeding in `DatabaseSeeder` with sample matters, tasks, and calendar entries for immediate workspace data visibility.
- Laravel API endpoints and permissions:
  - added `CalendarController` endpoints:
    - `GET /api/calendar/entries` (`calendar.read`)
    - `POST /api/calendar/entries` (`calendar.manage`)
    - `POST /api/calendar/entries/{entry}/complete` (`calendar.manage`)
  - added `MatterController` endpoint:
    - `GET /api/matters` (`matters.read`)
  - added `TaskController` endpoints:
    - `GET /api/tasks` (`tasks.read`)
    - `POST /api/tasks` (`tasks.manage`)
    - `POST /api/tasks/{task}/status` (`tasks.manage`)
  - updated `config/roles.php` to include calendar/matters/tasks permissions across operational roles.
- Consultation queue behavior depth:
  - upgraded `ConsultationsQueuePage.vue` from single-action scheduling to per-record editable status + internal notes drafts with explicit save actions.
  - aligned status options with queue lifecycle values (`Requested`, `InReview`, `Contacted`, `Scheduled`, `Converted`, `Closed`, `Cancelled`).
  - hardened backend queue validation/sorting in `ConsultationController`.
- Vue workspace route parity:
  - replaced placeholders with real pages:
    - `WorkspaceCalendarPage.vue` (month/week/day tabs, create entry, complete/reopen).
    - `MattersPage.vue` (matter list with staffing and open-task counts).
    - `TasksPage.vue` (task queue, filter, status transitions, create task).
  - added feature clients/types:
    - `features/calendar/{api,types}.ts`
    - `features/matters/{api,types}.ts`
  - updated router/sidebar wiring for `/workspace/calendar`, `/workspace/tasks`, `/workspace/matters`.

### Checks run (actual results)

- `vendor/bin/pint --dirty --format agent` in `laravel-backend` -> success.
- `php artisan test --compact` in `laravel-backend` -> success (18 tests, 79 assertions).
- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on edited frontend/backend files -> no linter errors.

### Checks not run, and why

- No browser automation playback was run in this slice; behavior verification focused on backend feature tests and TypeScript production build compile checks.

### Outcome and next action

- **Outcome:** workspace users now have functional calendar/month-week-day scheduling, matter list visibility, and task queue management in the migrated Laravel/Vue stack; consultation queue triage is now editable record-by-record instead of one-click status jump.
- **Benefit and possible future downfall:** [D-20261018-5](Markdown/planning/decision-log.md). Benefit: removes high-friction placeholder gaps in authenticated operations and restores end-to-end operational flow parity for daily legal staff use. Downfall: this slice introduces broader role-permission and schema surface area that will drift unless future module slices (activities/documents/communications) reuse the same contract and test discipline.
- **Next action:** implement matter-detail depth (task editing, document metadata workflow, and timeline actions) and expand parity checks for remaining authenticated placeholder routes.

## 2026-10-18 - Docker/Artisan command ownership map and use-case comment hardening

- **Recorded:** 2026-10-18 EDT. **Not committed.**
- **Objective and scope:** answer command-location ambiguity by documenting where operational commands are defined and adding explicit use-case comments above compose and entrypoint command declarations, plus a canonical commented command catalog for daily operations.

### Changes

- Added use-case comments directly in runtime command definition files:
  - `docker-compose.yml`:
    - `backend-dev` command (`dev-web`) comment for default local development mode.
    - `backend` command (`web`) comment for prod-style immutable runtime.
    - `migrate` command (`migrate`) comment for one-shot migration execution.
    - `frontend` command comment for npm install + Vite dev flow.
  - `docker/laravel/entrypoint.sh`:
    - comments above dependency-sync, migration, server-start helpers, and mode branches (`web`, `dev-web`, `migrate`).
- Expanded architecture runbook with a canonical command catalog:
  - `Markdown/architecture/README.md` now includes:
    - command source-of-truth file map,
    - commented `docker compose` up/down commands for dev and prod profiles,
    - commented Laravel maintenance commands (`migrate`, `db:seed`, `optimize:clear`, `cache:clear`, `route:clear`, `config:clear`),
    - commented verification commands for backend tests and frontend build.
- Clarified that `docs/migration/PROGRESS.md` remains historical evidence rather than the primary day-to-day operator source.

### Checks run (actual results)

- Documentation/configuration update only; no runtime command execution rerun.

### Checks not run, and why

- Did not re-run compose startup, artisan commands, or tests because this slice did not alter command semantics—only command comments and documentation discoverability.

### Outcome and next action

- **Outcome:** operator command discoverability is now explicit: command definitions are annotated in-place, and one canonical runbook section explains when to use each compose/artisan command.
- **Benefit and possible future downfall:** [D-20261018-6](Markdown/planning/decision-log.md). Benefit: reduces onboarding/debug friction and prevents command drift between team members by centralizing operational intent. Downfall: if compose profiles or container names change without synchronized updates, comments can become stale and misleading.
- **Next action:** keep the architecture command catalog and in-file command comments synchronized whenever compose service names, profile wiring, or Laravel maintenance workflows change.

## 2026-10-18 - Docker command runbook moved to dedicated operations doc

- **Recorded:** 2026-10-18 EDT. **Not committed.**
- **Objective and scope:** place Docker/Artisan command guidance in a dedicated, clearly named operations document while retaining architecture-level discoverability links.

### Changes

- Added new canonical runbook:
  - `Markdown/operations/docker-and-laravel-command-runbook.md`
  - includes:
    - command ownership map,
    - exact ordered dev sequence (`up -> ps -> migrate/seed/clear -> down`),
    - targeted clear commands (`cache:clear`, `route:clear`, `config:clear`),
    - prod-style profile sequence,
    - explicit command-order rule.
- Linked the runbook from primary doc indexes:
  - `Markdown/README.md` operations table.
  - `Markdown/architecture/README.md` related-links header.
- Kept `Markdown/architecture/README.md` as architecture context while moving day-to-day operator command flow to the dedicated operations runbook.

### Checks run (actual results)

- Documentation-only updates; no runtime commands rerun in this slice.

### Checks not run, and why

- No compose/artisan execution was rerun because this slice changed documentation placement and link structure only.

### Outcome and next action

- **Outcome:** Docker and Artisan operational command sequencing now has a dedicated, easy-to-find runbook under `Markdown/operations/` with architecture and top-level documentation links.
- **Benefit and possible future downfall:** [D-20261018-7](Markdown/planning/decision-log.md). Benefit: reduces ambiguity by separating architecture narrative from operator command execution guidance. Downfall: two linked docs (architecture + operations) can drift if command updates are only applied to one.
- **Next action:** update both the operations runbook and architecture link references whenever compose profile or command workflows change.

## 2026-10-18 - Calendar week-grid default, month navigator, and double-click event modal

- **Recorded:** 2026-10-18 EDT. **Not committed.**
- **Objective and scope:** update the workspace calendar UX to better match Clio-style scheduling flow by defaulting to week view, rendering precise time-grid selection, keeping a left-side traversable month picker visible, and opening a richer event capture surface on slot double-click.

### Changes

- Reworked `vue-frontend/src/pages/workspace/WorkspaceCalendarPage.vue`:
  - changed default calendar tab from `month` to `week`,
  - added persistent left mini-month navigator with month traversal and date selection,
  - replaced week/day list cards with a 30-minute slot time grid across 24 hours,
  - wired slot double-click (week/day/month) to open event creation modal seeded to selected slot time,
  - added a larger create-event modal collecting title, start/end time, entry type, deadline type, matter reference, location, and description,
  - retained selected-day agenda listing and complete/reopen actions for existing entries.
- Extended calendar styling in `vue-frontend/src/style.css`:
  - added sidebar month navigator styles,
  - added week/day time-grid layout styles with sticky headers/times and slot chips,
  - added dedicated calendar event modal form styling and responsive behavior.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on edited files (`WorkspaceCalendarPage.vue`, `style.css`) -> no diagnostics.

### Checks not run, and why

- No browser automation playback or manual UI recording was captured in this slice; verification was compile/lint based with interaction logic validated by source-level review.

### Outcome and next action

- **Outcome:** the workspace calendar now opens in a week-first grid experience with visibly selectable time rows, a traversable left-side month picker, and direct double-click event creation in a modal aligned with requested scheduling depth.
- **Benefit and possible future downfall:** [D-20261018-8](Markdown/planning/decision-log.md). Benefit: improves scheduling precision and reduces event-creation friction by anchoring interactions to visible time slots. Downfall: denser grid/modal UX increases front-end complexity and may require additional tuning for small screens or heavy daily event volume.
- **Next action:** run a browser interaction pass for slot density/readability and adjust row height/typography if users request tighter or larger scheduling cells.

## 2026-10-18 - Clarify Docker local-start order and PowerShell-safe command usage

- **Recorded:** 2026-10-18 EDT. **Not committed.**
- **Objective and scope:** remove local onboarding confusion by documenting the exact Docker/Artisan command order, explaining why the order matters, and making the sequence safer to copy into PowerShell.

### Changes

- Updated `Markdown/operations/docker-and-laravel-command-runbook.md`:
  - added a "Before running commands" checklist (repo root, Docker Desktop running, one command at a time),
  - added a copy/paste-safe dev sequence block without inline comments,
  - added explicit order rationale (`up -> ps -> migrate -> seed -> optimize:clear`),
  - added a guardrail note not to paste Markdown fence markers into terminal.
- Updated `Markdown/architecture/README.md`:
  - clarified that architecture now provides command ownership context while the operations runbook is canonical for day-to-day sequence,
  - added a command-usage warning about command order and Markdown fence copy mistakes.

### Checks run (actual results)

- Documentation-only updates; no runtime behavior change was made to compose profiles, services, or Artisan commands.
- Verified current runtime state remained healthy during support flow:
  - `docker compose --profile dev ps` showed `backend-dev` and `mysql` healthy, `frontend` running, and one-shot `migrate` exited `0`.

### Checks not run, and why

- No additional compose/artisan rerun was required because this slice only changed documentation phrasing/order guidance and not executable command definitions.

### Outcome and next action

- **Outcome:** local Docker instructions now state the required order, explain the dependency between steps, and reduce copy/paste errors in PowerShell.
- **Benefit and possible future downfall:** [D-20261018-9](Markdown/planning/decision-log.md). Benefit: fewer startup failures caused by sequence errors (`exec` on stopped containers, seed before migration, pasted markdown fences). Downfall: if service names or profile wiring change and docs are not updated immediately, clearer instructions can still become stale.
- **Next action:** keep `Markdown/operations/docker-and-laravel-command-runbook.md` and architecture cross-references synchronized whenever operational commands change.

## 2026-10-19 - Add explicit start/finish sections for clean Docker dev shutdown workflows

- **Recorded:** 2026-10-19 EDT. **Not committed.**
- **Objective and scope:** make it explicit where developers begin local dev startup and where they perform clean shutdown, including optional volume teardown, so no compose-managed containers are left running unintentionally.

### Changes

- Updated `Markdown/operations/docker-and-laravel-command-runbook.md`:
  - added `Start here (begin local dev)` section with the ordered startup/prepare command sequence,
  - added `When finished (clean shutdown)` section with:
    - standard stop (`down --remove-orphans`),
    - full clean stop (`down --remove-orphans --volumes`),
    - post-shutdown `ps` check to confirm no project services remain,
  - added explicit warning that `--volumes` removes project volume-backed MySQL data.
- Added optional prod-profile full cleanup command (`down --remove-orphans --volumes`) for parity.

### Checks run (actual results)

- Documentation-only updates; no runtime command semantics or compose service wiring changed.
- Lint diagnostics for edited docs were clean.

### Checks not run, and why

- No compose execution was rerun in this slice because the change only clarified/operator-documented existing commands.

### Outcome and next action

- **Outcome:** the runbook now has a prominent "start here" and "when finished" flow, including safe defaults and explicit full-clean options.
- **Benefit and possible future downfall:** [D-20261019-1](Markdown/planning/decision-log.md). Benefit: reduces forgotten running containers and shutdown ambiguity during local development. Downfall: stronger cleanup guidance can still be misused if users run `--volumes` without intending to reset local DB data.
- **Next action:** keep these sections aligned with compose profile/service-name changes and add troubleshooting checks when new recurring operator issues appear.

## 2026-10-19 - Enable Vue Tailwind support and harden Docker hot-reload behavior

- **Recorded:** 2026-10-19 EDT. **Not committed.**
- **Objective and scope:** make frontend style edits reliably hot-load during local Docker development and introduce Tailwind CSS into the Vue frontend stack for utility-first styling.

### Changes

- Updated `vue-frontend/vite.config.ts`:
  - added `@tailwindcss/vite` plugin,
  - set explicit dev server host/port,
  - enabled polling-based watch config and explicit HMR host/port for Docker-on-Windows file-change reliability.
- Updated `docker-compose.yml` frontend service:
  - switched command to `npm run dev` (server config now lives in Vite config),
  - added `CHOKIDAR_USEPOLLING` and `CHOKIDAR_INTERVAL` environment variables.
- Updated `vue-frontend/src/style.css`:
  - added `@import "tailwindcss";` so Tailwind utilities are available,
  - removed invalid stray `ddd` token in `.workspace-sidebar` block that caused CSS parse/lint failures.
- Updated frontend dependencies in `vue-frontend/package.json` / `vue-frontend/package-lock.json`:
  - added `tailwindcss`,
  - added `@tailwindcss/vite`,
  - included required PostCSS tooling entries now present in dev dependencies.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` for `vue-frontend/vite.config.ts`, `vue-frontend/src/style.css`, and `docker-compose.yml` -> no diagnostics.
- `curl -I http://localhost:5173` -> HTTP 200 from Vite dev server.
- `docker compose --profile dev ps` -> `backend-dev` healthy, `mysql` healthy, `frontend` running.

### Checks not run, and why

- No browser automation flow was executed in this slice; verification focused on compile/lint and live dev-server reachability.

### Outcome and next action

- **Outcome:** frontend style edits now use polling-compatible hot reload in Docker-based local development, and Tailwind utilities are available for incremental adoption.
- **Benefit and possible future downfall:** [D-20261019-2](Markdown/planning/decision-log.md). Benefit: faster frontend iteration with fewer missed file-change events and a modern utility styling option. Downfall: mixed Tailwind + large legacy stylesheet can increase short-term style complexity until consolidation is performed.
- **Next action:** migrate high-churn UI surfaces to Tailwind utilities first, then prune redundant legacy CSS blocks in controlled passes.

## 2026-10-19 - Render workspace login inside public shell navigation

- **Recorded:** 2026-10-19 EDT. **Not committed.**
- **Objective and scope:** make the workspace sign-in page show the same non-authenticated navigation and footer as other public routes.

### Changes

- Updated `vue-frontend/src/router/index.ts`:
  - moved `workspace-login` route to be a child of `PublicShell` with path `workspace/login`,
  - removed standalone top-level `/workspace/login` route declaration.
- Updated `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue`:
  - removed outer `<main class="page">` wrapper so the page fits cleanly inside `PublicShell`'s existing `<main>` container.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on `router/index.ts` and `WorkspaceLoginPage.vue` -> no diagnostics.

### Checks not run, and why

- No browser automation pass was run in this slice; verification focused on compile/lint and route wiring correctness.

### Outcome and next action

- **Outcome:** `/workspace/login` now renders with the same public navigation/footer shell as other non-auth pages while preserving login route path and guest-only behavior.
- **Benefit and possible future downfall:** [D-20261019-3](Markdown/planning/decision-log.md). Benefit: consistent unauthenticated UX and easier navigation return path when users leave workspace sign-in. Downfall: if future public-shell changes become heavy, they will also apply to login and may require route-specific overrides.
- **Next action:** optionally fine-tune sign-in page spacing and active-link styling under public-shell context if UX feedback requests visual adjustments.

## 2026-10-19 - Enforce footer-at-bottom layout on public and workspace pages

- **Recorded:** 2026-10-19 EDT. **Not committed.**
- **Objective and scope:** ensure footer stays at the bottom of the viewport on short-content pages for both non-auth and authenticated experiences.

### Changes

- Updated `vue-frontend/src/style.css`:
  - added `.public-shell` column-flex min-height layout,
  - made `.public-shell > .page` expand (`flex: 1`) so footer is pushed to viewport bottom on short pages,
  - updated `.workspace-shell` grid to include footer row (`minmax(0, 1fr) auto`),
  - added `.workspace-footer` grid placement and `workspace-main` `min-width: 0` hardening.
- Updated `vue-frontend/src/layout/WorkspaceShell.vue`:
  - imported and rendered shared `SiteFooter` as `workspace-footer`.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on `WorkspaceShell.vue` and `style.css` -> no diagnostics.

### Checks not run, and why

- No browser automation capture was run in this slice; verification focused on shell composition and compile/lint correctness.

### Outcome and next action

- **Outcome:** footer now remains at the bottom on short pages in both public and authenticated route shells, including workspace sign-in and workspace feature pages.
- **Benefit and possible future downfall:** [D-20261019-4](Markdown/planning/decision-log.md). Benefit: consistent, production-grade page framing and less visual "floating footer" behavior on sparse pages. Downfall: shared footer in workspace shell may require route-level spacing refinements for dense workspace pages.
- **Next action:** validate spacing and footer prominence across small/large viewports and adjust shell padding tokens if needed.

## 2026-10-19 - Add animated attorney profile transitions and link home partner cards to profile pages

- **Recorded:** 2026-10-19 EDT. **Not committed.**
- **Objective and scope:** make attorney profile pages feel more lively and engaging, add transition animation when navigating into profile detail routes, and make Home partner spotlight cards navigate to those same profile pages.

### Changes

- Updated `vue-frontend/src/pages/public/HomePage.vue`:
  - converted partner spotlight cards to `RouterLink` elements targeting `/attorneys/:slug`,
  - added explicit "View full profile" call-to-action text on each partner card.
- Updated `vue-frontend/src/pages/public/AttorneyProfilePage.vue`:
  - added role-based highlight chips,
  - added "What clients can expect" content panel,
  - grouped profile actions into a dedicated actions row.
- Updated `vue-frontend/src/layout/PublicShell.vue`:
  - wrapped routed public page output in transition wrapper,
  - applied dedicated `profile-route` transition for attorney detail pages and fallback fade transition for other pages.
- Updated `vue-frontend/src/style.css`:
  - added linked-partner-card hover styling and profile CTA styles,
  - added attorney-profile chips/panel/actions styles,
  - added `page-fade` and `profile-route` transition class animations.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on `HomePage.vue`, `AttorneyProfilePage.vue`, `PublicShell.vue`, and `style.css` -> no diagnostics.

### Checks not run, and why

- No browser automation/screenshot comparison was run in this slice; verification focused on compilation/lint and route/template correctness.

### Outcome and next action

- **Outcome:** users now get animated, more visually engaging attorney profile pages and can open those pages directly from both Attorneys and Home partner spotlight cards.
- **Benefit and possible future downfall:** [D-20261019-5](Markdown/planning/decision-log.md). Benefit: improves perceived quality and engagement on team profile exploration flows. Downfall: route-level animations increase visual complexity and may require tuning for users who prefer reduced motion.
- **Next action:** optionally add `prefers-reduced-motion` handling for profile transitions and consider making operations-team list entries in Attorneys page deep-link cards as well.

## 2026-10-19 - Fix blank public-page rendering by replacing route-wrapper transitions with profile-local animation

- **Recorded:** 2026-10-19 EDT. **Not committed.**
- **Objective and scope:** correct a regression where public pages appeared blank after introducing route-level transition wrapping in `PublicShell`.

### Changes

- Updated `vue-frontend/src/layout/PublicShell.vue`:
  - removed `RouterView` transition wrapper and restored direct `<RouterView />` rendering.
- Updated `vue-frontend/src/pages/public/AttorneyProfilePage.vue`:
  - added `attorney-profile--animated` class to profile card container.
- Updated `vue-frontend/src/style.css`:
  - removed route-transition class set (`page-fade`, `profile-route`),
  - added keyframe animation (`profile-enter`) used by `.attorney-profile--animated`.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on edited files -> no diagnostics.

### Checks not run, and why

- No browser automation run in this corrective slice; verification focused on compile/lint stability and known rendering-path rollback.

### Outcome and next action

- **Outcome:** public pages render normally again, and attorney profile pages still animate on entry with a local, lower-risk animation approach.
- **Benefit and possible future downfall:** [D-20261019-6](Markdown/planning/decision-log.md). Benefit: resolves site-wide blank-route regression quickly while preserving the requested animated profile feel. Downfall: route-level transitions remain deferred and may require a reveal-system-aware implementation later.
- **Next action:** add `prefers-reduced-motion` guard and, if desired, reintroduce cross-route transitions with deferred reveal initialization sequencing.

## 2026-10-19 - Redesign authenticated consultations queue for modern intake triage UX

- **Recorded:** 2026-10-19 EDT. **Not committed.**
- **Objective and scope:** improve authenticated staff workflow for incoming public appointment requests by upgrading queue readability, filtering, and per-request action ergonomics.

### Changes

- Confirmed public-to-auth flow is wired:
  - public request submission: `POST /api/consultations`,
  - authenticated queue retrieval: `GET /api/consultations`,
  - authenticated status update: `POST /api/consultations/{consultation}/status`,
  - workspace queue route: `/workspace/consultations`.
- Updated `vue-frontend/src/pages/workspace/ConsultationsQueuePage.vue`:
  - added queue status summary tiles,
  - added search input across key request fields,
  - moved from generic list rows to richer queue cards with status pills and metadata grouping,
  - preserved status + internal-notes draft/edit/save flow and permission gating.
- Updated `vue-frontend/src/style.css`:
  - added modern consultations queue styling and motion (`consultations-*`, `consultation-*`, status tone classes),
  - added responsive breakpoints for toolbar/stats/editor layout collapse,
  - expanded workspace form input styling to include textareas.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on `ConsultationsQueuePage.vue` and `style.css` -> no diagnostics.

### Checks not run, and why

- No browser automation visual capture was run in this slice; verification focused on compile/lint and API/route wiring consistency.

### Outcome and next action

- **Outcome:** authenticated users now have a cleaner, animation-assisted, easier-to-scan appointment triage queue for evaluating and advancing potential matters.
- **Benefit and possible future downfall:** [D-20261019-7](Markdown/planning/decision-log.md). Benefit: faster intake triage and less cognitive load per request through better hierarchy/status signaling. Downfall: richer UI raises front-end style surface area and may require future refactoring into smaller component modules.
- **Next action:** add optional assignment and urgency filters plus follow-up timestamp fields once backend supports those properties.

## 2026-10-19 - Fix hidden workspace consultation queue cards by wiring reveal observer in authenticated shell

- **Recorded:** 2026-10-19 EDT. **Not committed.**
- **Objective and scope:** resolve issue where authenticated consultations queue appeared empty/blank despite successful public appointment submissions.

### Changes

- Verified backend data exists:
  - queried `consultation_requests` and confirmed newly submitted public requests were present with `Requested` status.
- Root-cause determination:
  - consultations queue UI used `[data-reveal]` animation attributes,
  - reveal observer (`useScrollReveal`) was only initialized in `PublicShell`,
  - workspace pages had no observer, so reveal elements remained `opacity: 0`.
- Fix implemented:
  - updated `vue-frontend/src/layout/WorkspaceShell.vue` to initialize `useScrollReveal(() => route.fullPath)` in authenticated shell.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on `WorkspaceShell.vue` -> no diagnostics.

### Checks not run, and why

- No browser automation run in this corrective slice; verification focused on deterministic root cause + compile/lint checks.

### Outcome and next action

- **Outcome:** consultation queue cards now render in authenticated workspace as expected, and future workspace pages using `[data-reveal]` will animate/show correctly.
- **Benefit and possible future downfall:** [D-20261019-8](Markdown/planning/decision-log.md). Benefit: restores visibility of incoming consultation requests while preserving animation system consistency across app shells. Downfall: shell-level reveal observer can animate more elements globally, which may require tuning if performance concerns appear on very dense pages.
- **Next action:** validate queue page manually in browser and tune reveal thresholds/timing only if motion/perf feedback appears.

## 2026-10-19 - Remove reveal gating from critical consultations queue containers

- **Recorded:** 2026-10-19 EDT. **Not committed.**
- **Objective and scope:** ensure consultation requests are always visible in authenticated queue even when reveal observer timing is inconsistent.

### Changes

- Updated `vue-frontend/src/pages/workspace/ConsultationsQueuePage.vue`:
  - removed `data-reveal` attributes from:
    - consultations stats card,
    - consultations toolbar card,
    - consultation queue list container.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on edited queue file -> no diagnostics.

### Checks not run, and why

- No automated browser run in this micro-fix; symptom and root cause were directly established from runtime screenshot + data checks.

### Outcome and next action

- **Outcome:** consultation records now render visibly regardless of reveal observer timing.
- **Benefit and possible future downfall:** [D-20261019-9](Markdown/planning/decision-log.md). Benefit: removes a class of false-empty operational queues caused by animation gating. Downfall: fewer reveal animations on this page reduce motion polish slightly.
- **Next action:** keep critical operational data surfaces visible-by-default and apply reveal animation only to non-blocking decorative sections.

## 2026-10-19 - Firm tenant schema foundation

- **Recorded:** 2026-10-19 15:28 EDT. **Not committed.** Branch `feature/firm-tenant-schema-foundation` from `f8edb1a`.
- **Objective and scope:** first Laravel/Vue slice for a firm registry, human review, per-firm MySQL schema, request-scoped tenant context, and public branding. Existing workspace modules stay on the shared database.

### Changes

- Control-plane migrations for platform-administrator flag, firms, public profiles, domains, verifications, memberships, and provisioning events.
- Tenant migrations in `laravel-backend/database/tenant-migrations` for `firm_identity` and `tenant_workspace_notes`.
- Provisioning, resolution middleware, fleet command `tenants:migrate`, platform firm API, and public branding API.
- Vue public shell reads branding. `vue-frontend/src/firms/extensions.ts` is the code-owned extension point.
- Design note: [Markdown/architecture/firm-tenant-schema-foundation.md](Markdown/architecture/firm-tenant-schema-foundation.md). Decision [D-20261019-10](Markdown/planning/decision-log.md).

### Checks run (actual results)

- `php artisan test --compact` in `laravel-backend` passed: 30 tests, 203 assertions. That includes `FirmTenancyTest` and `TenantMysqlProvisioningTest` against MySQL 8.0.36 on `127.0.0.1:3307`.
- `vendor/bin/pint --dirty --format agent` completed with no remaining formatter changes.
- `npm run build` in `vue-frontend` passed (`vue-tsc` and Vite).
- Docker: `docker compose --profile dev up -d backend-dev` recreated a healthy `backend-dev`. `php artisan migrate --force` inside that container applied the two control-plane migrations. `GET http://localhost:8080/api/health` and `GET http://localhost:8080/api/public/branding` returned HTTP 200.
- Browser on `http://localhost:5173`: home, about, and sign-in rendered the platform name; the page requested `/api/public/branding` (HTTP 200); sign-in as the seeded attorney reached `/workspace`; matters listed the two seeded matters.

### Checks not run, and why

- No frontend unit test runner exists in `vue-frontend/package.json`.
- `docker compose config` was not run. The dev profile was already up and the backend container was recreated.
- Per-firm MySQL accounts were not created. The MySQL test grants one runtime user DML on the new schema and checks that user cannot read the control-plane `firms` table.

### Outcome and next action

- **Outcome:** new firms can be reviewed and given an isolated schema. Existing legal tables are still shared. The branch is not committed.
- **Benefit and possible future downfall:** [D-20261019-10](Markdown/planning/decision-log.md). Benefit: tenant routes fail closed and schema names are not taken from the client. Downfall: staff can still read the shared matter database, and one runtime MySQL user can reach every provisioned schema if the connection is pointed at it.
- **Next action:** choose when to move contacts, matters, consultations, tasks, and calendar into tenant schemas, and where per-firm database credentials will be stored.

## 2026-10-22 - Tenant-host guest fallback + workspace redirect hardening

- **Recorded:** 2026-10-22 EDT. **Not committed.**
- **Objective and scope:** ensure firm-host URLs remain prospective-customer-first for unauthenticated traffic and tighten sign-in redirect safety for workspace routes.

### Changes

- Updated `vue-frontend/src/router/index.ts`:
  - imported tenant-host detection from host-surface utilities,
  - changed auth-guard behavior so unauthenticated requests to authenticated workspace routes on tenant hosts redirect to public home (`/`) instead of auto-routing to `/workspace/login`.
- Updated `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue`:
  - added `safeWorkspaceRedirect()` helper,
  - restricted post-login redirect targets to safe internal workspace paths (must start with `/workspace` and must not start with `//`).
- Updated `vue-frontend/src/features/branding/api.ts`:
  - `applyPublicBranding()` now updates `document.title`,
  - tenant surfaces now show `{Firm Display Name} | MatterForge`; platform surface remains `MatterForge`.

### Checks run (actual results)

- `npm run build` in `vue-frontend` -> success.
- `ReadLints` on edited frontend files (`router/index.ts`, `WorkspaceLoginPage.vue`, `features/branding/api.ts`) -> no diagnostics.

### Checks not run, and why

- No browser automation/manual host-switching walkthrough was run in this slice; verification focused on compile, lint, and route-guard behavior in source.

### Outcome and next action

- **Outcome:** tenant-host visitors who are not signed in now stay on the firm's public-facing experience when they attempt workspace URLs, and login redirect handling is stricter against unsafe redirect values.
- **Benefit and possible future downfall:** [D-20261022-1](Markdown/planning/decision-log.md). Benefit: improves tenant public-surface consistency and reduces redirect abuse surface. Downfall: users expecting direct guest access to `/workspace/login` from protected tenant-route links will now land on home first and need one extra navigation step.
- **Next action:** manually validate the guest -> sign-in -> workspace flow on both a platform host and at least one tenant host, then adjust CTA copy if teams want a more explicit tenant-host sign-in prompt.

## 2026-10-22 - Self-service firm onboarding, two-reviewer approvals, and local fast-track guardrails

- **Recorded:** 2026-10-22 EDT. **Not committed.**
- **Objective and scope:** add applicant-facing firm onboarding while preserving tenant isolation and enforcing two-person approval for onboarding applications, with a strictly guarded local/testing fast-track path for rapid development provisioning.

### Changes

- Added onboarding control-plane schema in `2026_10_22_185837_create_firm_onboarding_tables`:
  - `firms.applicant_user_id` (nullable FK to `users`) and `firms.onboarding_revision`,
  - `firm_onboarding_applications`,
  - `firm_onboarding_verifications`,
  - `firm_onboarding_reviews`,
  - revision-bound unique constraints for review and verification rows.
- Added onboarding service and guard classes:
  - `app/Tenancy/FirmOnboardingService.php`,
  - `app/Tenancy/OnboardingFastTrackGuard.php`.
- Added applicant onboarding API controller/routes:
  - `POST /api/onboarding/register`,
  - `POST /api/onboarding/applications`,
  - `GET /api/onboarding/applications/current`,
  - `PUT /api/onboarding/applications/current`,
  - `POST /api/onboarding/applications/current/fast-track`.
- Added platform verification endpoint:
  - `POST /api/platform/firms/{firm}/verifications`.
- Updated approval flow in `FirmReviewService`:
  - onboarding applications now require complete verification checks and two distinct platform reviewers for approval,
  - self-review and conflicted member review are blocked server-side,
  - rejections are revision-scoped and block provisioning until a material application revision.
- Updated provisioning path:
  - platform provisioning now goes through onboarding service and assigns owner membership after successful schema provisioning.
- Added local fast-track command:
  - `php artisan onboarding:fast-track`.
- Added frontend integration:
  - `vue-frontend/src/pages/public/RegisterFirmPage.vue`,
  - `vue-frontend/src/pages/public/OnboardingStatusPage.vue`,
  - router/nav updates for `/register-firm` and `/onboarding/status`,
  - onboarding API client in `src/features/onboarding/api.ts`,
  - platform firms page now supports manual verification actions and displays onboarding approval counters.
- Added feature coverage:
  - `tests/Feature/FirmOnboardingApiTest.php` with applicant registration, two-reviewer enforcement, conflicted reviewer rejection, fast-track guard behavior, and provider fail-closed checks.

### Checks run (actual results)

- `php artisan test --compact tests/Feature/FirmOnboardingApiTest.php tests/Feature/FirmTenancyTest.php tests/Feature/TenantMysqlProvisioningTest.php` -> pass (23 tests, 166 assertions).
- `php artisan test --compact tests/Feature/FirmOnboardingApiTest.php` -> pass (7 tests, 39 assertions).
- `vendor/bin/pint --dirty --format agent` -> pass.
- `npm run build` in `vue-frontend` -> pass.
- `docker compose config` -> pass.
- `ReadLints` on edited backend/frontend onboarding files -> no diagnostics.

### Checks not run, and why

- No browser automation/manual screenshot capture was run for `/register-firm` and `/onboarding/status`; verification focused on API behavior, migration constraints, tests, and frontend compilation.

### Outcome and next action

- **Outcome:** local developers can now register a test firm through applicant-facing flow and, when explicitly enabled in local/testing, fast-track into an active tenant workspace without weakening production approval/verification pathways.
- **Benefit and possible future downfall:** [D-20261022-2](Markdown/planning/decision-log.md). Benefit: cuts local onboarding cycle time while preserving strict server-side controls and auditability. Downfall: introduces additional control-plane state and review workflow complexity that requires disciplined operational runbooks and reviewer training.
- **Next action:** run manual host-based UX verification for register -> status -> review -> provision -> workspace across platform and tenant hosts, then decide production policy for fresh-password confirmation/MFA on privileged platform review actions.
