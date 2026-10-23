# Application architecture

[Home](../README.md) · [Codebase audit](current-codebase-audit.md) · [Stack and runtime](stack-and-runtime.md) · [Solution structure](solution-structure.md) · [Module sweep (2026-10-13)](module-interaction-and-vulnerability-sweep-2026-10-13.md) · [Standards comparison (2026-10-13)](architecture-standards-comparison-2026-10-13.md) · [Tenancy ADR](../database/ADR-001-tenancy.md) · [Firm tenant foundation](firm-tenant-schema-foundation.md) · [Integrations](../integrations/README.md) · [Docker/Laravel command runbook](../operations/docker-and-laravel-command-runbook.md)

## Running stack (2026-10-19)

The application that Docker runs is Laravel 13, Vue 3, and MySQL 8.0.36. The current multi-firm slice is [firm tenant schema foundation](firm-tenant-schema-foundation.md), now including DNS TXT domain verification, tenant-scoped workspace routes, and platform onboarding controls. The sections below this one describe the 2026-10-12 ASP.NET and React audit. They are historical and are not the running tree.

## Audited starting point
The repository was audited on 2026-10-12 ([findings](current-codebase-audit.md)). It is a .NET 8 minimal-API backend with EF Core on MySQL and a React/Vite SPA, and that stack is kept ([rationale](stack-and-runtime.md)). Feature code is already organized as modules (`backend/Features/<Module>/<Module>Endpoints.cs` implementing `IEndpointModule`), all sharing one `LawFirmDbContext`. That is a reasonable modular-monolith seed. Endpoints currently query the context directly with inline DTO projection. Keep that convention, but move authorization filtering (matter grants, audience) into shared query helpers so it cannot be forgotten per endpoint. Matter and party filtering moved into `Infrastructure/Access` in slice F1.

## Folder conventions
The backend and frontend folder layout, the research behind it, and the steps for adding a module are in [solution and folder structure](solution-structure.md) (slice R1). In short: one API project organized by feature folders with `Contracts/` per module and entities under `Infrastructure/Persistence/Entities/`; root build files (`MatterForge.sln`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`); separate unit and integration test projects; and a feature-based React `src/` with an `index.ts` public API per feature. The planned modules (documents, engagements, billing, payments, trust, communications) and provider interfaces (tenancy, outbox, storage, email, payments, e-signature) exist as **not implemented** boilerplate.

## Proposed shape
A modular monolith with boundaries for identity/access, intake, matters, portal publishing, documents/evidence, calendaring, communications, billing/trust, practice extensions, and integration adapters. Modules own their writes. Services may use a single transaction within a tenant database. Durable outbox events coordinate external work; do not require a successful provider call inside a database transaction.

```mermaid
flowchart TD
  UI["Firm workspace and client portal"] --> API["Authenticated application"]
  API --> CTX["Membership and tenant resolution"]
  CTX --> MOD["Domain modules"]
  CTX --> CP["Control-plane catalog"]
  MOD --> DB["Selected firm database"]
  MOD --> OBJ["Private document storage"]
  DB --> OUT["Transactional outbox"]
  OUT --> WORK["Scoped workers and provider adapters"]
```

Control plane stores tenant routing, domains, platform identity references, memberships, subscriptions, and provisioning status. It should not store confidential legal narratives. Data plane stores each firm's legal and financial records. Object storage is private and scoped independently of SQL isolation.

## Request boundary
Authenticate → resolve trusted firm context → verify active membership → enforce role/matter/audience permissions → access the selected tenant repository → serialize an allowed DTO → audit sensitive action. A hostname selects a candidate firm, not permission. Tenant identifiers in payloads are never authoritative.

Background jobs, cache keys, full-text search, exports, scheduled tasks, telemetry, and provider credentials must carry firm scope. Clear scoped context between jobs and on pooled connections. Platform administrators must not become implicit legal superusers.

## API and consistency
Version external contracts; validate server-side; use concurrency tokens for contested changes. Paginate matter lists and search results. Use deterministic idempotency for payments, signatures, imports, and webhook processing. Request IDs are for tracing, not authorization. Distinguish pending work from failure. Explicitly document which changes become visible immediately and which are eventual.

## Docker profile rollout verification

### Validation performed

- `docker compose config` ✅
- `docker compose --profile dev config` ✅
- `docker compose --profile prod config` ✅
- `docker compose --profile dev up -d --build` ✅
- `docker compose --profile dev ps` shows:
  - `backend-dev` healthy
  - `mysql` healthy
  - `frontend` running
  - `migrate` exited `0`
- `curl http://localhost:8080/up` returns HTTP `200` ✅
- Lint diagnostics on changed files: no errors ✅

### New run commands

- **Dev stack:** `docker compose --profile dev up -d --build`
- **Prod-style backend stack:** `docker compose --profile prod up -d --build`
- **Stop stacks:**
  - `docker compose --profile dev down --remove-orphans`
  - `docker compose --profile prod down --remove-orphans`

## Docker and Artisan command source of truth

Use this section as a command ownership map. The canonical day-to-day operator sequence is in `Markdown/operations/docker-and-laravel-command-runbook.md`. Historical one-off command evidence remains in `docs/migration/PROGRESS.md`.

### Where command definitions live

- `docker-compose.yml`
  - Defines runtime service command modes: `dev-web`, `web`, `migrate`, frontend `npm install && npm run dev`.
- `docker/laravel/entrypoint.sh`
  - Implements what each backend mode does (`dev-web`, `web`, `migrate`).
- `Markdown/architecture/README.md` (this file)
  - Canonical day-to-day command catalog with use cases.
- `docs/migration/PROGRESS.md`
  - Historical migration logs and verification transcripts (not the primary runbook).

### Canonical command catalog with use-case comments

Important usage note:

- Run one command at a time, in order: `up -> ps -> exec -> down`.
- Paste only command lines into PowerShell. Do not paste Markdown fence markers like ```` ``` ```` or ```` ```bash ````.

```bash
# Use case: start the full local dev stack (Laravel + MySQL + Vue dev server).
docker compose --profile dev up -d --build

# Use case: stop and clean the local dev stack and orphaned containers.
docker compose --profile dev down --remove-orphans

# Use case: run only the prod-style backend profile locally for parity checks.
docker compose --profile prod up -d --build

# Use case: stop the prod-style backend profile stack.
docker compose --profile prod down --remove-orphans
```

```bash
# Use case: apply pending Laravel migrations in the running dev backend container.
docker compose --profile dev exec -T backend-dev php artisan migrate --force

# Use case: reseed local database demo data after schema resets or role/fixture updates.
docker compose --profile dev exec -T backend-dev php artisan db:seed --force

# Use case: clear cached config/routes/views/compiled files after route/config changes.
docker compose --profile dev exec -T backend-dev php artisan optimize:clear

# Use case: clear only the application cache store when debugging stale cache behavior.
docker compose --profile dev exec -T backend-dev php artisan cache:clear

# Use case: clear only route cache when route registration appears stale.
docker compose --profile dev exec -T backend-dev php artisan route:clear

# Use case: clear only config cache when .env/config edits are not reflected.
docker compose --profile dev exec -T backend-dev php artisan config:clear
```

```bash
# Use case: run backend tests after API/model/middleware changes.
docker compose --profile dev exec -T backend-dev php artisan test --compact

# Use case: run frontend production build verification after Vue/TS changes.
docker compose --profile dev exec -T frontend npm run build
```
