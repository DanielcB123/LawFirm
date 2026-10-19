# ProgressPlan

**Last updated:** 2026-10-19 05:22 EDT (UTC−04:00). **Primary handoff document.** Read this first, then the latest [SLICE_HISTORY.md](SLICE_HISTORY.md) entries, then [Markdown/README.md](Markdown/README.md). Agent rules: [AGENTS.md](AGENTS.md).

Status vocabulary: **Planned**, **In progress**, **Blocked**, **Implemented—verification pending**, **Verified within stated scope**. "Verified" always names what was exercised. Code existing, code building, behavior verified by integration tests, and production readiness are separate claims. Nothing in this repository is production ready. Owner is **Unassigned** for everything unless stated.

## Current step: Public-page rendering recovery after route-transition integration

**Status: Verified within stated scope.** Not committed. Scope: resolve blank-content regression across public routes after transition-wrapper integration while preserving animated feel on attorney detail pages.

| Criterion | Result |
|---|---|
| Rendering reliability | Removed `RouterView` transition wrapper from `PublicShell` to restore stable rendering across all public routes |
| Animation preservation | Added page-level entrance animation class on attorney profile card (`attorney-profile--animated`) to retain requested "fun/exciting" effect |
| CSS transition cleanup | Removed now-unused route-transition classes and replaced with keyframe-based profile entry animation |
| Verification | `npm run build` in `vue-frontend` passed; `ReadLints` on edited shell/profile/style files returned no diagnostics |

Next action: if needed, add reduced-motion media query guard for profile animation and then selectively reintroduce route-level transitions after coordinating with reveal observer timing.

## Current step: Attorney detail-page experience uplift and partner-card deep links

**Status: Verified within stated scope.** Not committed. Scope: enrich attorney profile detail pages with more dynamic visual structure and add route transition animation for profile navigation, while making Home partner spotlight cards link directly to profile detail pages.

| Criterion | Result |
|---|---|
| Home partner deep links | Converted partner spotlight cards on Home into `RouterLink` cards to `/attorneys/:slug` |
| Profile page richness | Added highlight chips, expectation panel, and action grouping in attorney detail page |
| Route transition animation | Added targeted profile route transition (`profile-route`) via `PublicShell` `RouterView` transition wrapper; preserved subtle fallback fade for other public routes |
| Visual polish | Added hover/cta styling for partner cards and new profile layout utility styles |
| Verification | `npm run build` in `vue-frontend` passed; `ReadLints` on edited files returned no diagnostics |

Next action: if desired, extend profile-card links to operations-team entries on Attorneys page so non-attorney staff can also open the same rich profile detail page pattern.

## Current step: Sticky footer baseline across public and authenticated shells

**Status: Verified within stated scope.** Not committed. Scope: keep footer anchored to the bottom of the viewport on short pages for both public routes and authenticated workspace routes.

| Criterion | Result |
|---|---|
| Public-shell footer behavior | Added shell-level flex layout so public-route `<main>` expands and footer stays at viewport bottom |
| Auth-shell footer behavior | Added shared `SiteFooter` to `WorkspaceShell` and grid row allocation so footer is always rendered after workspace content |
| Layout safety | Added `workspace-main` `min-width: 0` and footer grid placement to avoid overflow/column collapse regressions |
| Verification | `npm run build` in `vue-frontend` passed; `ReadLints` on edited shell/style files returned no diagnostics |

Next action: if desired, tune footer spacing specifically for workspace pages to visually differentiate authenticated and public contexts while keeping sticky behavior.

## Current step: Workspace login page routed through public shell navigation

**Status: Verified within stated scope.** Not committed. Scope: make `/workspace/login` display the same non-authenticated top navigation/footer as public pages by routing it through the public shell instead of rendering as a standalone top-level route.

| Criterion | Result |
|---|---|
| Navigation parity | `workspace-login` route moved under `PublicShell` children, so `SiteNav` and `SiteFooter` render on sign-in page |
| URL continuity | Route path remains `/workspace/login` (no external link changes required) |
| Template semantics | Removed nested `<main>` wrapper from `WorkspaceLoginPage.vue` to fit inside `PublicShell`'s existing `<main class="page">` container |
| Verification | `npm run build` in `vue-frontend` passed; `ReadLints` on edited route/login files returned no diagnostics |

Next action: if requested, update nav active-state styling for `/workspace/login` so it can be visually distinguished from other public navigation states.

## Current step: Vue frontend hot-reload hardening and Tailwind CSS enablement

**Status: Verified within stated scope.** Not committed. Scope: ensure CSS/frontend edits apply immediately during local Docker dev sessions and add Tailwind CSS support to the migrated Vue frontend without disrupting existing stylesheet behavior.

| Criterion | Result |
|---|---|
| Save-to-refresh reliability | Enabled Vite polling watch + explicit HMR host/port in `vue-frontend/vite.config.ts` for Docker-on-Windows bind-mount reliability |
| Docker watch compatibility | Added `CHOKIDAR_USEPOLLING=true` and polling interval in `docker-compose.yml` frontend service environment |
| Tailwind integration | Installed `tailwindcss` + `@tailwindcss/vite` (with PostCSS dependencies present) and enabled plugin in Vite config |
| CSS entry wiring | Added `@import "tailwindcss";` at the top of `vue-frontend/src/style.css` so utility classes are available immediately |
| Quality cleanup | Removed an invalid stray token in `.workspace-sidebar` CSS block that caused stylesheet parse/lint errors |
| Verification | `npm run build` in `vue-frontend` passed; `ReadLints` for edited files reported no diagnostics; `http://localhost:5173` returned HTTP 200 |

Next action: use Tailwind classes incrementally in Vue templates while preserving existing custom CSS blocks, then retire duplicated legacy style rules where practical.

## Current step: Docker runbook start/finish clarity and volume-clean shutdown guidance

**Status: Verified within stated scope.** Not committed. Scope: make local environment startup/finish behavior explicit in operations docs so developers have one clear "start here" path and one clear "when finished" shutdown path, including optional full cleanup (`--volumes`).

| Criterion | Result |
|---|---|
| Start section clarity | Added `Start here (begin local dev)` command block with ordered `up -> ps -> migrate -> seed -> optimize:clear` sequence |
| Finish section clarity | Added `When finished (clean shutdown)` with both standard stop and full clean stop (`down --remove-orphans --volumes`) |
| Process hygiene | Added explicit expectation check (`docker compose --profile dev ps`) to confirm no compose-managed services remain after shutdown |
| Data safety warning | Added clear note that `--volumes` deletes Docker-volume-backed MySQL data for this compose project |
| Verification | Documentation-only slice; no compose profile or runtime command behavior changed |

Next action: keep this runbook section synchronized with compose service/profile naming so shutdown/startup instructions stay accurate and safe.

## Current step: Docker local-start command order clarification and PowerShell-safe runbook edits

**Status: Verified within stated scope.** Not committed. Scope: reduce local startup confusion by making command order explicit in docs, adding a copy/paste-safe command sequence for PowerShell, and documenting why each command must be run in order.

| Criterion | Result |
|---|---|
| PowerShell-safe sequence | Added a no-comment command block in `Markdown/operations/docker-and-laravel-command-runbook.md` for `up -> ps -> migrate -> seed -> optimize:clear` |
| Order rationale | Added a "why this order matters" explanation describing container lifecycle, health checks, schema-before-seed, and cache consistency |
| Operator guardrails | Added explicit notes to run one command at a time and to avoid pasting Markdown fence markers into terminal |
| Architecture alignment | Updated `Markdown/architecture/README.md` command section to point to the operations runbook and include terminal-paste safety notes |
| Verification | Documentation-only slice; commands had already been validated in the active local runtime (`docker compose --profile dev ps` healthy) |

Next action: keep the runbook and architecture references synchronized whenever compose profile names, service names, or Artisan maintenance workflows change.

## Current step: Workspace calendar Clio-style week-grid UX and double-click event modal

**Status: Verified within stated scope.** Not committed. Scope: reshape the signed-in calendar page into a week-first scheduling experience with accurate time-grid selection, persistent left-side mini month navigation, and a richer event-capture modal opened by double-clicking a calendar slot.

| Criterion | Result |
|---|---|
| Default tab behavior | Calendar now defaults to `week` view instead of `month` |
| Time-grid precision | Week/day views now render a 30-minute slot grid across 24 hours for more precise slot targeting |
| Left-side month traversal | Added persistent mini month navigator with month prev/next controls and day selection |
| Slot interaction | Double-clicking a week/day/month slot opens a calendar event modal seeded to that selected date/time |
| Event-capture modal | Added larger event modal fields for title, start/end, type, deadline type, matter reference, location, and description before save |
| Existing-item continuity | Selected-day agenda remains visible with complete/reopen actions for existing items |
| Verification | `npm run build` in `vue-frontend` passed; `ReadLints` on edited calendar files returned no diagnostics |

Next action: validate end-to-end user interaction in browser (week-grid double-click create flow, mobile responsiveness, and slot readability density) and tune spacing if users request denser or larger slot rows.

## Current step: Dedicated Docker/Laravel command runbook placement under operations docs

**Status: Verified within stated scope.** Not committed. Scope: move command-order guidance into a dedicated operations document and link it from the top-level documentation index and architecture page so operators can find command sequencing quickly.

| Criterion | Result |
|---|---|
| Dedicated runbook file | Added `Markdown/operations/docker-and-laravel-command-runbook.md` as canonical command-order and use-case guide |
| Sequence coverage | Included exact dev and prod-style ordered sequences (`up -> ps -> exec -> down`) with migration/seed/cache clear commands |
| Discoverability | Linked runbook in `Markdown/README.md` operations index and `Markdown/architecture/README.md` header links |
| Ownership clarity | Runbook documents command-definition ownership: `docker-compose.yml`, `docker/laravel/entrypoint.sh`, and historical logs in `docs/migration/PROGRESS.md` |
| Verification | Documentation-only slice; no runtime command behavior changed |

Next action: keep this runbook synchronized whenever compose profile names, container names, or artisan maintenance workflows change.

## Current step: Docker/Artisan command source-of-truth comments and runbook hardening

**Status: Verified within stated scope.** Not committed. Scope: answer operator command-location question by documenting command ownership files and adding use-case comments directly above compose/runtime command definitions plus canonical runbook commands.

| Criterion | Result |
|---|---|
| Command ownership map | Documented where commands live: `docker-compose.yml` (command mode wiring), `docker/laravel/entrypoint.sh` (mode behavior), and `Markdown/architecture/README.md` (canonical operator catalog) |
| Compose comments | Added use-case comments above `backend-dev`, `backend`, `migrate`, and `frontend` `command` entries |
| Entrypoint comments | Added use-case comments above dependency-sync, migration, and mode case branches (`web`, `dev-web`, `migrate`) |
| Runbook command comments | Added explicit commented command blocks for profile up/down, migrate, seed, `optimize:clear`, cache/route/config clear, tests, and frontend build in architecture docs |
| Verification | Source-inspection slice; no runtime behavior change to commands themselves |

Next action: keep this command catalog synchronized whenever compose profiles, entrypoint modes, or Laravel maintenance workflows change.

## Current step: Calendar + matters + tasks parity slice on Laravel/Vue workspace

**Status: Verified within stated scope.** Not committed. Scope: replace placeholder signed-in routes with working calendar/matters/tasks flows, align consultation queue triage behavior with legacy flow depth, and preserve public-to-authenticated consultation queue continuity.
| Criterion | Result |
|---|---|
| Calendar route parity | Replaced workspace calendar placeholder with a permission-gated page supporting month/week/day tabs, date-range navigation, list rendering, create entry, and complete/reopen actions |
| Backend calendar APIs | Added Laravel `CalendarController` + `calendar_entries` schema with authenticated `GET /api/calendar/entries`, `POST /api/calendar/entries`, and `POST /api/calendar/entries/{entry}/complete` guarded by `calendar.read` / `calendar.manage` |
| Matters route parity | Replaced workspace matters placeholder with a matter list page backed by `GET /api/matters` and open-task counts per matter |
| Tasks route parity | Added new workspace tasks page, route, and sidebar entry backed by `GET /api/tasks`, `POST /api/tasks`, and `POST /api/tasks/{task}/status` with role permissions `tasks.read` / `tasks.manage` |
| Consultation queue depth | Upgraded queue UI from one-click "Mark scheduled" to per-record status + internal notes drafts with explicit save, while preserving public non-auth consultation submission pipeline |
| Seed and permission expansion | Expanded role matrix (`calendar`, `matters`, `tasks` permissions) and seeded sample matter/task/calendar records to make logged-in pages immediately usable |
| Verification | `vendor/bin/pint --dirty --format agent` passed; `php artisan test --compact` passed (18 tests, 79 assertions); `npm run build` passed in `vue-frontend`; edited-file lint diagnostics clean |

Next action: migrate the remaining authenticated placeholder destinations (activities/documents/communications/reports) and deepen matter detail workflows (task editing, document metadata, and timeline actions) using the same permission-first Laravel route pattern.

## Current step: Seeded auth personas documentation in docs

**Status: Verified within stated scope.** Not committed. Scope: create a source-backed docs reference for seeded authenticated users, role types, and permission gates in the current Laravel migration stack.

| Criterion | Result |
|---|---|
| Seeded user visibility | Added a docs matrix listing all seeded auth users with name, email, and role |
| Permission visibility | Added role-to-permission mapping sourced from `config/roles.php` |
| Route authorization mapping | Added permission-to-endpoint mapping for currently protected authenticated API routes |
| Verification | Source-only documentation slice; values were taken directly from `DatabaseSeeder.php`, `roles.php`, and `routes/api.php` |

Next action: continue the in-progress parity migration work (calendar and matters behavior depth) while keeping seeded role docs synchronized with any future auth matrix changes.

## Current step: Route-level Vue parity + consultation queue + workspace auth restoration

**Status: Verified within stated scope.** Not committed. Scope: execute the approved migration continuation phases by restoring route-level public pages, consultation booking flow, workspace authentication shell, and backend consultation queue support with seeded role personas.

| Criterion | Result |
|---|---|
| Route parity foundation | Added Vue Router with dedicated public routes (`/`, `/about`, `/practice-areas`, `/attorneys`, `/attorneys/:slug`, `/contact`) and workspace routes under `/workspace/*` with 404 fallback |
| Public shell + animations | Introduced `PublicShell`, reusable `SiteNav`/`SiteFooter`, and IntersectionObserver scroll-reveal behavior (`data-reveal`) for parity-style section animations |
| Attorney/staff information | Ported and expanded `teamMembers` data with attorney and operational roles (partner, attorney, paralegal, accountant, legal assistant, admin IT) and wired attorney list/profile pages |
| Book consultation UX | Implemented Contact-page consultation modal with available-slot loading and request submission via `GET /api/consultations/available-slots` + `POST /api/consultations` |
| Strict shared modal parity | Replaced per-page modal usage with one global `ConsultationModalHost` and shared `useConsultationModal()` state so consultation CTAs open the same component across public pages (Home/About/Practice/Attorneys/Attorney Profile/Contact) |
| CTA contract parity | Home `Book consultation` and `Start your consultation` now open the modal in-place; Contact button uses legacy-copy `Request appointment`; additional consultation CTAs (`Contact our office`, `Request consultation`, `Schedule consultation`, `Contact the firm`) route to the same modal |
| Modal interaction parity | Modal now follows legacy structure/behavior: full-screen dark overlay, large card scale, gradient header, two-column details+scheduler layout, 42-day calendar grid, day/time state styling, month navigation with scroll-position preservation, and selection-summary rendering |
| API query parity | Slot API now supports legacy-style `startDate` + `days` contract while remaining backward-compatible |
| Backend queue flow | Added `ConsultationRequest` model/migration/factory, API controller, and protected queue/status routes (`consultations.read` / `consultations.manage`) |
| Auth/workspace structure | Added session-based auth store, `/workspace/login`, guarded workspace shell/sidebar, and restored workspace navigation route set (functional `contacts`, `intake`, `consultations`; placeholders for remaining modules) |
| Role/permission seed matrix | Added `config/roles.php` mapping and seeded personas for Partner, Attorney, Paralegal, Accountant, LegalAssistant, AdminIT, and Client, plus sample contacts/intake/consultation data |
| Verification | `npm run build` (vue-frontend) passed; `php artisan test --compact` passed (11/11); `vendor/bin/pint --dirty --format agent` clean; `docker compose config` passed; lint diagnostics clean; Playwright captures generated for desktop/mobile modal entry points |

Next action: continue Phase 4 parity depth by migrating calendar and matters behavior-level logic from pre-migration app into the new workspace routes and Laravel APIs while preserving permission boundaries.

## Current step: Vue visual parity recovery against pre-migration React snapshot

**Status: Verified within stated scope.** Not committed. Scope: match the migrated `vue-frontend` shell to the prior React-era public styling direction from commit `a7352dc1d4e3778b9309fc5a435f81c646b3d293` while preserving current Laravel API workflows.

| Criterion | Result |
|---|---|
| Legacy reference source | Pulled style/structure references from `frontend/src/app/AppShell.css`, `frontend/src/pages/public/HomePage.tsx`, `SiteNav.tsx`, and `SiteFooter.tsx` at commit `a7352dc1d4e3778b9309fc5a435f81c646b3d293` |
| Vue page structure | `vue-frontend/src/App.vue` now renders a branded top nav, hero, value/capability panels, practice highlights, partner spotlight cards, process cards, and testimonial cards before migration tools |
| Workflow preservation | Existing sign-in, health check, contacts create/list, and intake create/list behavior remains in the same Vue page under a dedicated "Migration workspace" section |
| Theme parity | `vue-frontend/src/style.css` replaced with React-era visual language: warm palette tokens, hero image layering, card treatments, typography, spacing, hover states, and responsive collapse behavior |
| Runtime fix | Recreated `docker-compose.yml` to remove a malformed accidental `working_dir` line and restore valid Docker Compose configuration |
| Compose lint hardening | Converted both `backend` and `frontend` container commands to inline YAML sequence form (JSON-style array) and normalized top-level volume declarations to explicit empty maps (`{}`) |
| Verification | `npm run build` in `vue-frontend` passed; `docker compose config` passed and confirms clean service configuration |
| Diagnostics note | Cursor YAML diagnostics for `docker-compose.yml` now report no errors after command/volume normalization |

Next action: split this branded shell into route-level Vue components/pages (`home`, `about`, `practice`, `attorneys`, `contact`, and workspace route shells) while keeping Laravel endpoint parity checks active.

## Current step: Laravel + Vue + Docker migration baseline and audit kickoff

**Status: In progress.** Not committed. A dedicated branch `migration/laravel-vue-docker-phase1` now contains the migration planning workspace in `docs/migration/`.

| Criterion | Result |
|---|---|
| Repository baseline checks | `dotnet build MatterForge.sln -c Release` passed; `dotnet test backend.UnitTests -c Release` 10 passed; `dotnet test backend.IntegrationTests -c EfDesign` 48 passed; `frontend` build and tests passed; frontend lint remains preexisting baseline failure (7 errors, 3 warnings) |
| Runtime check | `dotnet run` in `backend/` failed at startup because MySQL host was unreachable |
| Docker availability | Docker Engine and `docker compose` are installed and responding |
| Port checks | Host ports 5073, 5173, and 3307 were free when checked |
| XAMPP/host services | `xampp-control`, `httpd`, and `mysqld` processes were present; MySQL listener observed on host port 3306; no listener on 3307 at check time |
| Existing Docker config | No `docker-compose*.yml` or `Dockerfile*` currently exists in the repository |
| Migration documentation | Added `docs/migration/README.md`, `INVENTORY.md`, `CURRENT_ARCHITECTURE.md`, `FEATURE_PARITY.md`, `TARGET_ARCHITECTURE.md`, `DATA_MIGRATION.md`, `SECURITY_REVIEW.md`, `UI_PARITY.md`, `PLAN.md`, `PROGRESS.md` |

Next action: continue full file-by-file audit coverage in `docs/migration/INVENTORY.md`, complete workflow traces and visual baseline, then begin Docker-first Laravel/Vue vertical-slice implementation while preserving legacy behavior for comparison.

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
| Billing, trust, payments, documents binary pipeline, MFA, multitenancy | Absent | — | Workspace screens exist for Billing, Accounts, Documents, Communications, Reports, Integrations, and Settings. They do not implement these capabilities. [Decision log](Markdown/planning/decision-log.md) D-20261013-2 |

## Completed work

| ID | Work | Status |
|---|---|---|
| A0 | Repository audit and linked `Markdown/` documentation (commit `91a9ded`) | Verified within stated scope (documentation; links checked; app code unchanged; DB/API not exercised) |
| F1 | Real-MySQL test harness + matter-level authorization + restricted-party masking (committed `0fc6831`) | Verified within stated scope |
| F2 (part) | Migrations baseline, startup refusal for unmanaged DBs, secrets removed from tracked config, demo identity gated | Implemented—verification pending for existing-DB baselining and Development opt-in; remaining F2 item (same-transaction append-only audit, G16) **Planned** |
| R1 | Solution structure: root build files, backend entity/contract split, not-implemented module and provider boilerplate, unit test project, feature-based frontend, Vitest, `Markldown/` archived under `Markdown/` (uncommitted) | **Verified within stated scope**: solution build, 44 integration + 10 unit + 4 frontend tests, no EF model change, frontend build; lint at baseline. No runtime behavior change intended or observed |
| M0 | MatterForge working-name branding plus multi-firm architecture: ADR-002, data dictionary, query and index spec, threat model, runbooks, acceptance matrix, implementation plan, `db/design` SQL and validation (committed `0fc6831`) | **Verified within stated scope**: design SQL applied and checked on disposable MySQL 8.0.36 with synthetic data (log in `db/design/validation/`); docs link-checked. No application multi-firm code; nothing deployed; no existing database touched |
| A1 | Repository-wide module communication sweep, vulnerability mapping, architecture-standards comparison, and SaaS firm onboarding runbook docs | **Verified within stated scope**: static source and docs inspection, architecture comparison artifacts, onboarding runbook authored. No runtime behavior or schema change; no builds/tests re-run in this docs-only slice |
| A2 | Vulnerability hardening for consultation booking, public abuse controls, intake conflict binding, calendar verification spoofing, and correlation-id sanitation | **Verified within stated scope**: solution build and integration tests (48/48). No multitenancy, no billing/trust, and no audit-atomicity redesign in this slice |
| A3 | Tailwind CSS onboarding and frontend styling migration pass | **Verified within stated scope**: Tailwind/PostCSS integration, hybrid utility migration in shared layout and public pages, frontend install/build/dev verification. Existing frontend lint baseline unchanged |
| A4 | Public hero balance calibration for About and Practice headers | **Verified within stated scope**: matched About and Practice overlay profile to the calmer Contact/Attorneys balance; frontend build and stylesheet diagnostics clean |
| A5 | Centralized signed-in page header styling | **Verified within stated scope**: moved workspace header typography to shared authenticated-shell CSS selectors and removed one page-level override in Tasks & Calendar |
| A6 | Home hero burgundy-left gradient reduction | **Verified within stated scope**: reduced left accent radial intensity on Home header to soften burgundy dominance while preserving existing hero layout and readability |
| A7 | Home hero left accent shifted to blue hue | **Verified within stated scope**: replaced burgundy-toned left radial with a soft blue tint while preserving hero structure and readability |
| A8 | Home hero full hue neutralization (red to blue) | **Verified within stated scope**: replaced remaining warm red/beige Home hero gradient components with cooler blue-toned equivalents to remove reddish cast |
| A9 | Home hero plain-blue isolation test | **Verified within stated scope**: set Home hero to a flat blue background and disabled hero image/glow layers to remove all warm tint sources |
| A10 | Home hero restored with blue-only layered treatment | **Verified within stated scope**: restored Lady Liberty image/glow hero composition using only cool blue overlays and gradients (no red-toned tint layers) |
| A11 | Home hero tuning comments for manual editing | **Verified within stated scope**: added inline CSS guidance comments explaining which values control blue strength, gradient depth, image visibility, and glow intensity |

## Current step: global background image options

**Status: Verified within stated scope.** Not committed. The app background is currently pinned to City Skyline at Night, and the background selector UI is commented out temporarily for visual feedback review.

| Criterion | Result |
|---|---|
| Options | 23 choices remain in code for later review, but selection is temporarily locked |
| Scope | Background applies globally through `body` styling, so it appears on public and workspace pages |
| Control | Background picker UI is commented out in `AppShell` for now |
| Persistence | Local storage key `matterforge.app.background-theme` is set to `city-skyline-night` by the pinned theme effect |
| Verification | Confirmed no background picker is rendered and confirmed the Skyline image URL is active in `--app-shell-background-image` |
| Rendering fix | Moved background drawing to `.app-shell::before` with a lighter overlay so option changes are visibly distinct across the full app |
| Usability | Scrollable picker styles remain in place for when the selector is re-enabled |
| Header tone tune | Rebalanced hero styling to split the difference: toned down the latest vibrancy while keeping more life than the original dull gray/white look |
| Header parity tune | About and Practice now use the same balanced top-to-bottom neutral overlay profile as Contact/Attorneys to reduce over-emphasis |
| Workspace header centralization | Signed-in page title and intro lead styling now lives in authenticated `.page` selectors, reducing per-page header class duplication |
| Home aura refinement | Reduced the left-side burgundy gradient magnitude on Home hero to keep a cleaner, less color-heavy first impression |
| Home hue shift | Converted the Home hero left accent from burgundy to a muted blue hue for a cooler, less warm visual anchor |
| Home hue cleanup | Converted the Home hero's remaining warm secondary radial and base linear tint to cool blue neutrals to eliminate red-gray cast |
| Home plain-blue test | Home hero now uses a solid blue background with decorative image/glow layers disabled to validate color behavior without overlays |
| Home blue restore | Re-enabled full hero visuals (image and glow) with blue-only tint stack to keep the original beauty while avoiding red cast |
| Home tuning assist | Added inline comments in Home hero CSS to speed manual color/opacity experimentation without hunting through layers |

## Previous step: public appointment request dialog

**Status: Verified within stated scope.** Not committed. The public Contact page “Request appointment” dialog keeps the same layout but scales up in size, and it now switches to a stacked layout sooner to prevent overlap between the input block and the appointment picker.

## Previous step: full signed-in sidebar

**Status: Verified within stated scope.** Committed in `0ff69b3`. The signed-in menu now includes every Clio sidebar destination plus Intake, Consultations, and Conflicts. Pages that already have records show those records. Billing, accounts, and integrations do not invent balances, invoices, or connected services.

| Criterion | Result |
|---|---|
| Menu | Dashboard, Calendar, Tasks, Matters, Contacts, Activities, Billing, Accounts, Documents, Communications, Reports, Integrations, Settings, then Practice: Intake, Consultations, Conflicts. Resource centre sits above the signed-in person |
| Activities | Two matter timeline rows, including the draft complaint filing |
| Documents | Damage-Photo-Set.zip and Retainer-Agreement-Signed.pdf, each linked to the matter |
| Communications | Two consultation messages, Taylor Bennett and Morgan Hale |
| Reports | Open tasks 0, active matters 1, intake 2, contacts 2. The active-matters card opened Matters |
| Billing, accounts, integrations | Pages open and state that no invoices, balances, or connections exist |
| Settings | Shows Avery Attorney, email, role, and permissions |
| Resource centre | Topic list. Documents topic opened the documents page |
| Look-back | Why, who, and timestamp order are in [decision log](Markdown/planning/decision-log.md). Applied tables and foreign keys are in [schema evolution](Markdown/database/schema-evolution.md) and the [UML class diagrams](Markdown/database/uml-class-diagrams.md) |

## Previous step: signed-in navigation and Contacts directory

**Status: Verified within stated scope.** Not committed. Parties was renamed to Contacts and the directory shows people and organizations.

## Previous step: A3 Tailwind CSS onboarding and frontend migration pass

**Status: Verified within stated scope.** Not committed (working tree on top of `0fc6831` plus prior uncommitted work). Scope: install Tailwind CSS into the Vite frontend pipeline, migrate shared layout primitives to Tailwind utilities (`@apply` and inline utility classes), and harden public page styling migration on Home and Contact without visual regressions.

| Criterion | Result |
|---|---|
| Tailwind install | Added `tailwindcss`, `postcss`, `autoprefixer` and created `frontend/tailwind.config.js` + `frontend/postcss.config.js` |
| Tailwind activation | Added Tailwind directives in `frontend/src/index.css` (`@tailwind base/components/utilities`) |
| Styling migration sweep | Converted shared layout/form/modal/grid primitives in `frontend/src/app/AppShell.css` to Tailwind-backed `@apply` utilities while preserving custom theme variables |
| Component migration | Added utility-class usage in `SiteNav`, `SiteFooter`, `HomePage`, and `ContactPage` while keeping existing semantic classes for complex visuals |
| Verification | `npm install` succeeded, `npm run build` succeeded, `npm run dev` started (Vite on port 5174 because 5173 was occupied). Frontend lint remains baseline failures (7 errors, 3 warnings) |
| Public nav toggle | Desktop Menu button was visible because Tailwind `inline-flex` overrode `.site-menu-toggle { display: none }`. Utility classes removed from that button. Checked on the running dev server: at 1400px the toggle is `display: none` and the links are `display: flex`; at a narrow viewport the toggle is shown and the links are `display: none` |

## Previous step: A2 vulnerability hardening implementation

## Earlier step: A1 architecture evidence refresh and SaaS onboarding runbook

## Earlier step: R1 solution structure, boilerplate, and documentation folder cleanup

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
| 2 | Decide multitenant database architecture and controls | F1 | **Design done (M0)**; owner review of ADR-002 and of the [open decisions](Markdown/planning/open-decisions.md) pending |
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
- **Risk (updated 2026-10-13):** consultation booking race is mitigated in the application path with named locks; a schema-level slot reservation primitive is still optional future hardening.
- **Risk (updated 2026-10-13):** consultation create has rate limiting, but CAPTCHA/bot scoring are still absent.
- **Risk (updated 2026-10-13):** intake acceptance now binds to the intake conflict check, but party-set invalidation remains open.
- **Risk (updated 2026-10-13):** calendar verification spoofing is blocked, but full legal deadline provenance fields are still incomplete.
- **Risk (updated 2026-10-13):** correlation id input is now bounded/sanitized; audit writes are still non-atomic and audit table immutability is still open.
- **Frontend migration risk (A3):** styling is now hybrid (Tailwind plus legacy custom CSS). This is intentional for phased migration, but increases short-term style-surface complexity until deeper consolidation.
- **Frontend dependency risk (A3):** `npm install` reports 11 vulnerabilities (4 moderate, 7 high) in npm audit output; not remediated in this slice.
- **Risk:** the user's running API process holds `backend/bin/Debug`; builds use `-o ./.build-out`, `-c EfDesign` (project level only), or `-c Release` (solution level).
- **Note (R1):** the running `dotnet run` process was started before R1 and still serves the old build until restarted. After restart it builds the new tree; behavior should be identical except that `/api/weather/forecast` and `/api/weatherforecast` (template routes) no longer exist.
- **Note (R1):** boilerplate interfaces and empty route groups are placeholders. Nothing registers or calls them; don't count them as features.

## Exact next action and resume instructions

1. Optionally rename the folder by hand: stop project terminals and app processes, close the Cursor workspace, rename `LearnCSharpDotNet` to `MatterForge` in File Explorer, and reopen `D:\Documents\Projects\MatterForge` in Cursor. Update the paths in any prompt afterward.
2. Review and commit the R1 + A1 + A2 working tree (SLICE_HISTORY entries `2026-10-12 - Slice R1`, `2026-10-13 - Slice A1`, and `2026-10-13 - Slice A2` once appended). F1 and M0 are already in `0fc6831`.
3. Rotate the credentials above, then set local secrets as in [README.md](README.md#running-locally).
4. Start T1 with the prompt in the M0 history entry. Inputs: [ADR-002](Markdown/database/ADR-002-multitenant-architecture.md), [threat model](Markdown/security/multitenant-threat-model.md), [acceptance matrix](Markdown/delivery/multitenant-acceptance-matrix.md), [plan](Markdown/delivery/multitenant-implementation-plan.md).
5. To re-verify code after the next implementation slice: start Docker, then from the repository root run `dotnet build MatterForge.sln -c Release`, `dotnet test backend.UnitTests -c Release`, `dotnet test backend.IntegrationTests -c EfDesign`, and `powershell -ExecutionPolicy Bypass -File db/design/run-validation.ps1`; in `frontend/` run `npm run build` and `npm test`.

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
  - `frontend/src/pages/workspace/ContactsPage.tsx`
  - `frontend/src/pages/workspace/ConflictChecksPage.tsx`
  - `frontend/src/pages/workspace/IntakeBoardPage.tsx`
  - `frontend/src/pages/workspace/IntakeDetailPage.tsx`
  - `frontend/src/pages/workspace/MattersPage.tsx`
  - `frontend/src/pages/workspace/MatterWorkspacePage.tsx`
  - `frontend/src/pages/workspace/MatterWorkspaceTabs.tsx`
  - `frontend/src/pages/workspace/WorkspaceConsultationsPage.tsx`
  - `frontend/src/pages/workspace/ActivitiesPage.tsx`
  - `frontend/src/pages/workspace/DocumentsPage.tsx`
  - `frontend/src/pages/workspace/CommunicationsPage.tsx`
  - `frontend/src/pages/workspace/ReportsPage.tsx`
  - `frontend/src/pages/workspace/BillingPage.tsx` (screen only; no invoices)
  - `frontend/src/pages/workspace/AccountsPage.tsx` (screen only; no ledger)
  - `frontend/src/pages/workspace/IntegrationsPage.tsx` (none connected)
  - `frontend/src/pages/workspace/SettingsPage.tsx` (current session only)
  - `frontend/src/pages/workspace/ResourceCentrePage.tsx`

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
  - `frontend/src/pages/workspace/ContactsPage.tsx`

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

---

## 2026-10-17 migration checkpoint (continuation)

- Continued audit coverage through backend contracts/infrastructure auth and error handling, frontend feature modules, and backend integration/unit test files; updated `docs/migration/INVENTORY.md`.
- Legacy workspace visual capture remains blocked by reproducible backend startup failure: MySQL authentication error for `lawfirm_app` before API serve.
- Docker-first Laravel/Vue stack is live with required DB networking:
  - MySQL publish: `127.0.0.1:3307:3306`
  - App-to-DB: `DB_HOST=mysql`, `DB_PORT=3306`
- Stabilized runtime config by introducing tracked `laravel-backend/.env.docker` and copying it to runtime `.env` at backend start.
- Added auth vertical slice:
  - Sanctum installed and migrated.
  - `POST /api/auth/login` and `GET /api/identity/me` implemented.
  - contacts endpoints protected by `auth:sanctum`.
  - unauthenticated contacts now return JSON 401 (fixed previous guest redirect route failure).
  - Vue migration app updated with sign-in flow and bearer-token calls.
- Verified commands and outcomes are logged in `docs/migration/PROGRESS.md`.

## 2026-10-17 migration checkpoint (intake + authz continuation)

- Closed the major remaining frontend page audit gap by reviewing the rest of `frontend/src/pages/**` and recording coverage in `docs/migration/INVENTORY.md`.
- Implemented next Docker-first migrated workflow slice:
  - Laravel intake list/create endpoints (`/api/intake`) and backing `intake_records` table/model.
  - Vue migration app intake create/list UI wired to authenticated API calls.
- Added behavior-level authorization mapping in migration stack:
  - `permission:*` middleware alias and `RequirePermission` middleware.
  - Route guards for contacts (`contacts.read` / `contacts.manage`) and intake (`intake.read` / `intake.manage`).
  - Auth payload now exposes `role` and `permissions`.
- Added and passed unauthorized/forbidden-path verification:
  - Laravel feature tests in `laravel-backend/tests/Feature/IntakeApiTest.php`.
  - runtime checks confirm `401` for unauthenticated intake and `403` for user lacking `intake.manage`.

## 2026-10-17 migration checkpoint (legacy runtime decommission)

- Per direct user instruction, removed legacy runtime code and old test/runtime-design directories that are no longer part of the Laravel/Vue/Docker stack.
- Removed paths:
  - `frontend/` (React app),
  - `backend.IntegrationTests/`,
  - `backend.UnitTests/`,
  - `db/design/`,
  - `MatterForge.sln`,
  - `global.json`,
  - `Directory.Packages.props`,
  - all files previously inside `backend/`.
- Remaining caveat:
  - empty top-level `backend/` directory still exists due an external file lock; contents are already removed.
- Post-removal verification:
  - Docker compose stack remains healthy.
  - Laravel tests pass.
  - Vue build passes in container runtime.

## 2026-10-17 migration checkpoint (frontend visual parity reset + local bootstrap fix)

- Resolved local Laravel bootstrap blocker seen during `php artisan key:generate`:
  - root cause: missing local `laravel-backend/vendor` packages (Sanctum class load failure during Artisan boot),
  - action: ran `composer install` in `laravel-backend`, then reran key generation successfully.
- Confirmed Docker runtime behavior and host/container port mapping:
  - backend serves on host `http://localhost:8080` (container `8000`),
  - frontend serves on host `http://localhost:5174` (container `5173`),
  - direct host `http://localhost:8000` is expected to refuse connection.
- Delivered first-pass visual parity restoration for migrated Vue UI:
  - replaced the temporary plain utilitarian styling in `vue-frontend/src/App.vue` with a structured branded layout (header, hero, cards, forms, footer),
  - rebuilt `vue-frontend/src/style.css` using prior React-era design language (warm palette, hero treatment, card styling, typography, interactions) while keeping existing Laravel workflow functionality.
- Validation:
  - `docker compose ps` -> backend/mysql healthy; frontend running,
  - `curl.exe -i http://localhost:8080/up` -> HTTP 200,
  - `docker compose exec -T frontend npm run build` -> success,
  - edited-file lint diagnostics -> no errors.

## 2026-10-18 migration checkpoint (deterministic Docker bootstrap + dev/prod profile split)

- Implemented deterministic container bootstrap to prevent dependency/cache drift from breaking healthchecks:
  - added `docker/laravel/entrypoint.sh` with explicit modes (`web`, `dev-web`, `migrate`),
  - `dev-web` now checks `composer.lock` hash against `vendor/.composer.lock.hash` and runs `composer install` only when drift is detected.
- Reworked `docker/laravel/Dockerfile` into multi-stage build with dependency install in image build-time:
  - composer install runs in vendor stage with `--no-scripts`,
  - runtime stage copies vendor + source and sets normalized executable entrypoint.
- Replaced monolithic backend startup shell chain in `docker-compose.yml` with profile-based services:
  - `backend-dev` (profile `dev`, bind mount + vendor volume),
  - `backend` (profile `prod`, immutable runtime),
  - `migrate` one-shot service (profiles `dev` + `prod`),
  - `frontend` now scoped to `dev` profile and depends on `backend-dev`.
- Validation:
  - `docker compose config` -> success,
  - `docker compose --profile dev config` -> success,
  - `docker compose --profile prod config` -> success,
  - `docker compose --profile dev up -d --build` -> success,
  - `docker compose --profile dev ps` -> `backend-dev` healthy, `mysql` healthy, `frontend` running, `migrate` exited `0`,
  - `curl.exe -i http://localhost:8080/up` -> HTTP 200.
- Operational note:
  - Standard local startup now requires explicit profile invocation (`docker compose --profile dev up -d --build`).

## 2026-10-18 migration checkpoint (consultation modal host unwrapping fix)

- Fixed a regression in the shared consultation modal host where the modal appeared open on initial page load and close interactions could not dismiss it.
- Root cause:
  - `ConsultationModalHost.vue` used `v-if="modal.isOpen"` where `isOpen` was a nested ref property on a plain object, making the expression effectively always truthy in template conditional rendering.
- Change:
  - destructured top-level values from `useConsultationModal()` and bound `v-if="isOpen"` plus `@close="closeModal"`.
- Validation:
  - `docker compose --profile dev exec -T frontend npm run build` -> success.

## 2026-10-18 migration checkpoint (architecture README operational verification capture)

- Added explicit Docker profile validation evidence and operator command runbook to `Markdown/architecture/README.md`.
- Captured exact verification outcomes:
  - `docker compose config`,
  - `docker compose --profile dev config`,
  - `docker compose --profile prod config`,
  - `docker compose --profile dev up -d --build`,
  - `docker compose --profile dev ps` health status summary,
  - `curl http://localhost:8080/up` HTTP 200,
  - lint diagnostics clean.
- Added explicit run/stop commands for dev and prod-style profile usage.
