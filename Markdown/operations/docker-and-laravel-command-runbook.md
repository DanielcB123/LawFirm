# Docker and Laravel command runbook

[Home](../README.md) · [Architecture](../architecture/README.md) · [Operations](reliability-and-recovery.md)

This file is the canonical runbook for Docker Compose and Laravel Artisan operations in the current Laravel/Vue runtime.

## What this runbook is for

- Starting and stopping local stacks in the correct profile.
- Running Laravel maintenance commands (`migrate`, `seed`, `optimize:clear`, cache clears) in the right container.
- Avoiding command-order mistakes (for example, `exec` before containers are up).

## Before running commands

- Open PowerShell in the repository root: `D:\Documents\Projects\LearnCSharpDotNet`.
- Ensure Docker Desktop is running before any `docker compose` command.
- Run one command at a time and wait for it to finish before the next command.
- In terminal, paste only command lines. Do not paste Markdown fences like ```` ``` ```` or ```` ```bash ````.

## Start here (begin local dev)

Run from `D:\Documents\Projects\LearnCSharpDotNet`:

```bash
docker compose --profile dev up -d --build
docker compose --profile dev ps
docker compose --profile dev exec -T backend-dev php artisan migrate --force
docker compose --profile dev exec -T backend-dev php artisan db:seed --force
docker compose --profile dev exec -T backend-dev php artisan optimize:clear
```

Expected result before developing:

- `backend-dev` is `running (healthy)`.
- `mysql` is `running (healthy)`.
- `frontend` is `running`.
- `migrate` may show `exited (0)` (one-shot job, expected).

After `db:seed`, these key local logins should exist:

- SaaS platform owner: `partner@matterforge.local` / `Passw0rd!`
- Firm partner (Brian and Sandra demo firm): `lawfirmbrianandsandra@matterforge.com` / `Passw0rd!`
- Firm staff examples: `attorney@matterforge.local`, `paralegal@matterforge.local`, `assistant@matterforge.local` / `Passw0rd!`

## When finished (clean shutdown)

Standard stop (most common, keeps local named volumes/data):

```bash
docker compose --profile dev down --remove-orphans
docker compose --profile dev ps
```

Full clean stop (removes local named volumes too):

```bash
docker compose --profile dev down --remove-orphans --volumes
docker compose --profile dev ps
```

Important:

- Use `--volumes` only when you intentionally want a clean data reset.
- Volume removal deletes MySQL data in Docker volumes for this compose project.
- If `docker compose --profile dev ps` shows no services, there are no compose-managed containers from this project still running.

## Where command definitions live

- Runtime command wiring: `docker-compose.yml`
- Backend mode behavior (`dev-web`, `web`, `migrate`): `docker/laravel/entrypoint.sh`
- Historical migration command logs (not canonical runbook): `docs/migration/PROGRESS.md`

## Daily sequence (dev profile)

Run commands from repository root (`D:\Documents\Projects\LearnCSharpDotNet`).

Copy/paste-safe sequence (PowerShell):

```bash
docker compose --profile dev up -d --build
docker compose --profile dev ps
docker compose --profile dev exec -T backend-dev php artisan migrate --force
docker compose --profile dev exec -T backend-dev php artisan db:seed --force
docker compose --profile dev exec -T backend-dev php artisan optimize:clear
```

Why this order matters:

1. `up` creates network/containers and starts services.
2. `ps` confirms services are actually running/healthy before `exec`.
3. `migrate` applies schema changes before seed or app use.
4. `db:seed` depends on the current schema and inserts demo fixtures.
5. `optimize:clear` flushes stale caches after config/route/view changes.

Detailed sequence with use-case notes:

```bash
# 1) Start local dev stack (Laravel + MySQL + Vue).
docker compose --profile dev up -d --build

# 2) Confirm containers are healthy/running before exec commands.
docker compose --profile dev ps

# 3) Apply pending schema changes.
docker compose --profile dev exec -T backend-dev php artisan migrate --force

# 4) Reseed demo data when schema or fixtures changed.
docker compose --profile dev exec -T backend-dev php artisan db:seed --force

# 5) Clear framework optimization/cache artifacts after route/config/view changes.
docker compose --profile dev exec -T backend-dev php artisan optimize:clear
```

Optional targeted clears:

```bash
# Use when app cache values are stale.
docker compose --profile dev exec -T backend-dev php artisan cache:clear

# Use when route registration appears stale.
docker compose --profile dev exec -T backend-dev php artisan route:clear

# Use when config/.env values are stale.
docker compose --profile dev exec -T backend-dev php artisan config:clear
```

Verification:

```bash
# Backend test verification.
docker compose --profile dev exec -T backend-dev php artisan test --compact

# Frontend build verification.
docker compose --profile dev exec -T frontend npm run build
```

Stop stack:

```bash
docker compose --profile dev down --remove-orphans
```

## Prod-style local sequence (profile parity check)

Use this sequence when validating prod profile behavior locally.

```bash
# 1) Start prod-style profile.
docker compose --profile prod up -d --build

# 2) Confirm runtime status.
docker compose --profile prod ps

# 3) Run maintenance command in prod-style backend container.
docker compose --profile prod exec -T backend php artisan config:clear
```

Stop:

```bash
docker compose --profile prod down --remove-orphans
```

Optional full cleanup for prod profile:

```bash
docker compose --profile prod down --remove-orphans --volumes
```

## Tenant schemas

`php artisan migrate` updates the control-plane database only. Firm schemas are created by a platform administrator calling `POST /api/platform/firms/{firm}/provision` after approval. They are not created at login.

After a deploy that adds files under `laravel-backend/database/tenant-migrations`, run:

```text
docker compose --profile dev exec -T backend-dev php artisan tenants:migrate
```

The command migrates firms that are already provisioned (`provisioning_status = ready`). It reports each firm id as `ok` or `failed` and can be run again. It does not drop schemas. Details: [firm tenant schema foundation](../architecture/firm-tenant-schema-foundation.md).

Legacy shared-workspace backfill (dry run first):

```text
docker compose --profile dev exec -T backend-dev php artisan tenants:backfill-workspace --firm=<firm-uuid> --map-all-to-firm
docker compose --profile dev exec -T backend-dev php artisan tenants:backfill-workspace --firm=<firm-uuid> --map-all-to-firm --apply
```

The command is idempotent and does not drop legacy shared tables. Keep it in dry-run mode until ownership mapping is reviewed.

## Local host simulation for SaaS/app/tenant split

The frontend now distinguishes platform hosts, app hosts, and tenant hosts.

- Platform hosts default: `localhost`, `127.0.0.1`, `www.localhost`, `app.localhost`
- Tenant fallback suffix default: `firms.localhost`

Example local hosts-file entries:

```text
127.0.0.1 app.localhost
127.0.0.1 www.localhost
127.0.0.1 alpha.firms.localhost
127.0.0.1 beta.firms.localhost
```

Environment toggles:

- Backend: `TENANCY_PLATFORM_HOSTS`, `TENANCY_RESERVED_DOMAINS`, `TENANCY_TENANT_FALLBACK_DOMAIN_SUFFIX`
- Frontend: `VITE_PLATFORM_HOSTS`, `VITE_APP_HOSTS`
- Local-only explicit firm selector header: `TENANCY_ALLOW_HEADER_FIRM_SELECTION=true` and `VITE_ALLOW_HEADER_FIRM_SELECTION=true`

Do not enable header-based firm selection in production.

## Command-order rule

Always run in this order:

1. `up`
2. `ps` (optional but recommended)
3. `exec ... php artisan ...`
4. `down` only when you want to stop/clean up

Do not run `exec` before `up`; container context will not exist.
