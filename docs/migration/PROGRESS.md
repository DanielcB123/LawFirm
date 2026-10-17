# Migration Progress

## Current phase

- Phase 1: Safe baseline - in progress (substantially complete for this session).
- Phase 2: Full system audit - in progress.

## Completed in this session

- Confirmed repository root and boundaries.
- Read required session handoff files and repository rules:
  - `AGENTS.md`
  - `ProgressPlan.md`
  - `SLICE_HISTORY.md`
- Captured git baseline:
  - branch `main` synchronized with `origin/main` at start of session.
  - created isolated branch: `migration/laravel-vue-docker-phase1`.
- Verified there are no existing Docker Compose or Dockerfile artifacts in the repository.
- Established baseline command results (pre-migration evidence):
  - `dotnet build MatterForge.sln -c Release` -> success.
  - `dotnet test backend.UnitTests -c Release` -> 10 passed.
  - `dotnet test backend.IntegrationTests -c EfDesign` -> 48 passed.
  - `npm run build` in `frontend/` -> success.
  - `npm test` in `frontend/` -> 4 passed.
  - `npm run lint` in `frontend/` -> fails (7 errors, 3 warnings; preexisting baseline).
- Checked runtime startup baseline:
  - `dotnet run` in `backend/` fails because API cannot connect to configured MySQL host.
- Checked Docker and ports:
  - Docker Engine available (`docker version` successful).
  - Docker Compose available (`docker compose version` successful).
  - host port `3307` currently free (no listener at check time).
  - host ports `5073`, `5173`, and `3307` were free at check time.
- Checked XAMPP-related process state:
  - active `xampp-control`, `httpd`, and `mysqld` processes detected.
  - detected MySQL listener on host port `3306` from running `mysqld`.
  - no listener detected on host port `3307` at check time.
- Expanded deep audit coverage:
  - reviewed backend endpoint implementations for auth, parties, conflicts, intake, matters, matter access grants, calendar, consultations, and client portal.
  - updated migration inventory and feature trace docs with concrete workflow evidence.
- Visual baseline capture progress:
  - started frontend dev server on `127.0.0.1:5173` for capture.
  - installed Playwright Chromium runtime.
  - captured desktop and mobile-sized screenshots for public routes into `docs/migration/screenshots/`.
  - initial mobile run using iPhone device profile failed due missing WebKit browser runtime; switched to Chromium with explicit mobile viewport and completed capture.
- Continued continuation-slice audit:
  - reviewed backend contracts, auth/authorization helpers, error middleware, tenancy/outbox/payment/signature/storage interfaces, and integration/unit tests.
  - reviewed frontend feature API/type modules and layout footer component.
- Stabilized Docker-first Laravel/Vue vertical slice:
  - added tracked backend runtime env file `laravel-backend/.env.docker`.
  - updated `docker-compose.yml` backend service to use `env_file: .env.docker` and copy it into `.env` at container start to prevent SQLite fallback.
  - verified Laravel contacts vertical slice against MySQL-backed runtime.
- Attempted legacy backend startup for workspace visual capture:
  - fails with MySQL auth error (`Access denied for user 'lawfirm_app'... using password: NO`), so authenticated workspace screenshots remain blocked.
- Added auth vertical slice in Laravel + Vue migration app:
  - installed Sanctum and created personal access tokens migration.
  - added `POST /api/auth/login` and `GET /api/identity/me`.
  - protected contacts endpoints with `auth:sanctum`.
  - updated Vue migration slice UI with sign-in form and bearer-token API usage.
  - fixed unauthenticated API behavior to return JSON 401 (no missing `login` route redirect failure).
- Added permission-aware intake workflow slice:
  - implemented `intake_records` schema + model + API controller (`GET /api/intake`, `POST /api/intake`).
  - added permission middleware alias (`permission:*`) and route-level guards for contacts + intake.
  - extended auth response payload with role and permissions.
  - updated demo seed users for authorized (`attorney@matterforge.local`) and read-only (`readonly@matterforge.local`) scenarios.
  - updated Vue migration slice UI to display role/permissions and exercise intake create/list by permission.
- Legacy runtime decommission (user-directed hard delete):
  - removed legacy .NET runtime/test/runtime-design paths:
    - `backend.IntegrationTests/`
    - `backend.UnitTests/`
    - `db/design/`
    - `MatterForge.sln`
    - `global.json`
    - `Directory.Packages.props`
  - removed legacy React workspace:
    - `frontend/` removed.
  - removed legacy backend code contents:
    - all files under `backend/` removed.
  - note: top-level empty `backend/` directory currently remains locked by another running process and cannot yet be removed by automation.

## Open issues and blockers

- The current backend runtime does not start without reachable MySQL.
- Existing local MySQL/XAMPP state may contain data not represented in repository artifacts.
- No existing Dockerized local development runtime in repository yet.
- Full file-by-file audit is still in progress; coverage is tracked in `INVENTORY.md`.

## Verification commands run

- `git rev-parse --show-toplevel`
- `git status --short --branch`
- `git branch --all --verbose --no-abbrev`
- `git remote -v`
- `docker version`
- `docker compose version`
- `dotnet build MatterForge.sln -c Release`
- `dotnet test backend.UnitTests -c Release`
- `dotnet test backend.IntegrationTests -c EfDesign`
- `npm run build` (frontend)
- `npm test` (frontend)
- `npm run lint` (frontend)
- `dotnet run` (backend)
- Windows listener checks for ports `3307`, `5073`, `5173`.
- Process checks for `xampp`, `mysql`, and `httpd`.
- Vertical-slice runtime and DB checks:
  - `docker compose up -d --build backend mysql`
  - `docker compose exec -T backend php artisan optimize:clear`
  - `docker compose exec -T backend php artisan migrate --force`
  - `docker compose exec -T mysql mysql -umatterforge -pmatterforge -D matterforge -e "SHOW TABLES;"`
  - `docker compose ps`
  - `curl.exe -i http://127.0.0.1:8080/api/health`
  - `curl.exe -i http://127.0.0.1:8080/api/contacts`
  - PowerShell `Invoke-RestMethod` POST+GET on `/api/contacts` (row create/read verified)
- Legacy runtime verification:
  - `dotnet run --urls http://127.0.0.1:5073` in `backend/` (fails with MySQL auth error before serve).
- Auth-slice verification:
  - `docker compose exec -T backend composer require laravel/sanctum --no-interaction`
  - `docker compose exec -T backend php artisan vendor:publish --provider="Laravel\Sanctum\SanctumServiceProvider" --tag="sanctum-migrations"`
  - `docker compose exec -T backend php artisan migrate --force`
  - `curl.exe -i http://127.0.0.1:8080/api/contacts` -> `401 Unauthenticated` after middleware adjustment
  - PowerShell `Invoke-RestMethod` for:
    - `POST /api/auth/login` (success),
    - `GET /api/identity/me` (success),
    - authenticated `POST /api/contacts` + `GET /api/contacts` (success).
- Intake + authorization verification:
  - `docker compose exec -T backend php artisan migrate --force` -> includes new users/intake migrations.
  - `docker compose exec -T backend php artisan db:seed --force` -> demo users seeded.
  - `docker compose exec -T backend php artisan test` -> all tests pass including `Tests\\Feature\\IntakeApiTest`.
  - `curl.exe ... /api/intake` without auth -> `401`.
  - authenticated attorney `POST /api/intake` + `GET /api/intake` -> success.
  - authenticated read-only user `POST /api/intake` -> `403`.
  - `docker compose exec -T frontend npm run build` -> success.
  - host `npm run build` in `vue-frontend` remains unavailable without local `vue-tsc` binary (expected; containerized command is canonical).
- Post-decommission verification:
  - `docker compose config` -> success.
  - `docker compose ps` -> backend/mysql/frontend services healthy/running.
  - `docker compose exec -T backend php artisan test` -> pass (6 tests).
  - `docker compose exec -T frontend npm run build` -> success.

## Next concrete actions

1. Continue deep file-level audit and update `INVENTORY.md` to full reviewed coverage.
2. Finish end-to-end feature workflow traces in `FEATURE_PARITY.md`.
3. Capture visual baseline screenshots (or document tooling limitations if capture cannot be completed safely).
4. Finalize target migration architecture selection (Laravel API + Vue SPA vs Inertia) with evidence.
5. Build initial Docker Compose vertical slice:
   - Laravel runtime
   - Vue 3 runtime
   - MySQL with `127.0.0.1:3307:3306`
   - health checks and persistent volumes
6. Validate containerized app startup and DB connectivity without requiring XAMPP services.
7. Remove the remaining empty locked legacy `backend/` directory after the external process releases its lock.
8. Normalize docs that still reference removed .NET/React paths so the repository reflects Laravel/Vue/Docker-only runtime.
9. Continue migrated workflow parity (intake detail/stage transitions, then broader authorization parity).
