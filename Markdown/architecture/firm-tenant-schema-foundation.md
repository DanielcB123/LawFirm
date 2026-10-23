# Firm tenant schema foundation

[Architecture](README.md) · [ADR-002](../database/ADR-002-multitenant-architecture.md) · [Schema evolution](../database/schema-evolution.md) · [Docker runbook](../operations/docker-and-laravel-command-runbook.md)

**Status:** implemented on branch `feature/firm-tenant-schema-foundation`, starting from `f8edb1a`. Not production ready. This is the Laravel/Vue application slice. It now includes tenant-scoped workspace tables and APIs, DNS TXT domain ownership verification, a platform-admin onboarding UI route (`/workspace/platform/firms`), and SaaS-first host surfaces (`www`/root platform shell, app-host discovery shell, tenant hosts for firm public/workspace entry).

## What the repository was

On 2026-10-19 the running application is Laravel 13 (`laravel/framework` ^13.17), PHP 8.5 in Docker, Sanctum bearer tokens, Vue 3.5, Vite, and MySQL 8.0.36. Docker Compose profile `dev` publishes the API on port 8080, the Vue dev server on port 5173, and MySQL on `127.0.0.1:3307`. The default Laravel connection is one database, `matterforge`. Users, contacts, intake, consultations, matters, tasks, and calendar rows all live there. There was no tenant registry, no per-firm schema, and no request-scoped firm connection.

ADR-001 and ADR-002 already choose a database per law firm plus a control plane. That decision was design-only and described an ASP.NET Core API. This slice follows the database-per-firm decision on the Laravel stack that is actually running. It does not add placement cells, per-firm database accounts, subscriptions, or a secrets manager.

## Tenant model

A law firm is the tenant. Confidential firm work belongs in that firm's MySQL schema. The shared control-plane database holds only:

| Table | Role |
|---|---|
| `users` | Login identity. `is_platform_administrator` is a platform flag, not a firm role. |
| `firms` | Stable firm id, lifecycle, provisioning status, and the server-generated schema name. |
| `firm_public_profiles` | Public website fields only. |
| `firm_domains` | Claimed hostnames, DNS TXT challenge metadata, verification timestamps, and revocation state. |
| `firm_verifications` | Human review decision, reason, evidence references, reviewer, and time. |
| `firm_memberships` | Which users may enter which firm, and the firm role. |
| `firm_database_credentials` | Runtime DB account material per firm (encrypted at rest). |
| `firm_provisioning_events` | Sanitized audit lines. |

Each firm schema, named `mf_f_` plus 32 hex characters, holds `firm_identity`, `tenant_workspace_notes`, `contacts`, `intake_records`, `consultation_requests`, `matter_records`, `matter_tasks`, and `calendar_entries`. The schema name is generated on the server, stored on the firm row, and hidden from API responses. Clients cannot submit a schema name.

Each firm now also receives a canonical platform-managed fallback hostname (`<firm-slug>.<TENANCY_TENANT_FALLBACK_DOMAIN_SUFFIX>`). It is stored in `firm_domains` as verified with source `platform_managed`. Custom domains are additive; they do not replace this fallback identity.

`tenant_workspace_notes` is the first tenant-scoped record used to prove isolation. It is not a client, matter, or document store.

## Lifecycle

Human review and schema provisioning are separate.

```text
pending_review -> approved -> active
pending_review -> rejected
active -> suspended
```

- Platform control-plane actions require platform access (`users.platform_role` or compatibility `is_platform_administrator=true`) plus action-level permission checks:
  - `platform_owner`: full platform-firm lifecycle powers (review finalize, provision, suspend/reactivate, domain verify/revoke).
  - `platform_ops`: day-to-day onboarding operations (firm create/read, memberships, domain claim/reissue, branding, verification records) without owner-only finalization controls.
- Approval records a `firm_verifications` row. It does not open the firm.
- `POST /api/platform/firms/{firm}/provision` creates the schema, runs `database/tenant-migrations`, writes one `firm_identity` row, and attempts to create a per-firm runtime DB account. If local MySQL permissions block user creation, a documented shared-runtime fallback is used and the environment remains non-production-ready.
- Failure leaves the firm `approved` (or whatever lifecycle it already had), sets provisioning to `failed`, and stores a fixed reason such as "Tenant schema migration did not complete." Exception text is not stored or logged.
- Retry uses the same schema name and the Laravel migrations table in that schema. MySQL DDL is not rolled back by a transaction. A migration file must be safe to run again until Laravel records it as finished.
- There is no tenant deletion command in this slice.

Provisioning statuses: `not_requested`, `requested`, `provisioning`, `ready`, `failed`.

## Request lifecycle

Login and `/api/identity/me` use the control plane only. They return membership ids, display names, lifecycle, and firm role. They do not open a tenant schema and do not return a schema name.

Tenant routes (`/api/tenant/...`) run `EstablishTenantContext` after `auth:sanctum`, on every request:

1. Clear any previous tenant connection.
2. Load the user's memberships from the control plane.
3. If the request host matches a verified domain, that firm is the candidate. `localhost` and `127.0.0.1` are platform hosts and never select a firm.
4. `X-Firm-Id` is accepted only when `TENANCY_ALLOW_HEADER_FIRM_SELECTION=true` and it is a UUID of a firm the user belongs to. Query parameters and route parameters do not select a schema.
5. If the host and the header name different firms, the request is rejected.
6. One usable membership is selected automatically. Several usable memberships require explicit firm selection and return `409` with `firm_selection_required` until selection is provided.
7. Pending, rejected, suspended, approved-but-not-provisioned, and failed provisioning states return `403` with "Tenant is not available."
8. The tenant connection is pointed at the stored schema name. `firm_identity.firm_id` must match. Then the controller runs.
9. The middleware `terminate` method clears the connection and context.

Workspace APIs (`/api/tenant/contacts`, `/api/tenant/intake`, `/api/tenant/consultations`, `/api/tenant/matters`, `/api/tenant/tasks`, `/api/tenant/calendar/entries`) now run under tenant middleware and use tenant-scoped models.

Public consultation intake (`/api/consultations`, `/api/consultations/available-slots`) runs under host-only tenant resolution middleware. Unknown or unverified hosts fail with a safe 404 and do not open a tenant connection.

## Host surfaces (Option B)

- Root/platform hosts (`localhost`, `127.0.0.1`, `www.localhost`, and configured platform hosts) serve SaaS-platform messaging and onboarding IA.
- App hosts (for example `app.localhost`) are discovery/sign-in shells. After authentication:
  - single-firm users can be redirected to their preferred verified firm host,
  - multi-firm users are routed to firm selection before tenant workspace access.
- Tenant hosts (platform fallback subdomain or verified custom domain) are the only production source for tenant public/workspace context.
- Local development may use `X-Firm-Id` only when explicitly enabled (`TENANCY_ALLOW_HEADER_FIRM_SELECTION=true`).

Queue jobs must carry the firm id, call `TenantConnectionManager::establish()`, and clear in a `finally` block. `RecordTenantWorkspaceNote` is the pattern. Queue `before`, `after`, `failing`, and `looping` callbacks also clear the connection so a worker cannot keep one firm's PDO into the next job. `php artisan tenants:migrate` does the same per firm. Do not resolve a tenant only inside login.

## Public branding

`GET /api/public/branding` reads the control plane, not a tenant schema. A verified non-revoked domain for an **active** firm returns display name, plain-text copy, phone, email, address, hex colors, an allow-listed typeface (`system-sans` or `system-serif`), `publicSiteKey` (`default_classic` or `modern_feature`), allow-listed modules (`public_site`, `consultations`), and logo/favicon paths under `/storage/branding/`. Unknown, pending, rejected, suspended, and unverified hosts get the same platform payload shape, so a pending firm name is not disclosed. The payload has no firm id and no schema name. Branding is not authorization.

Colors must be `#RRGGBB`. Text fields cannot contain `<` or `>`. Remote CSS, script, and HTML are rejected on write and stripped on read. The Vue shell applies colors only as custom properties after the same checks. `vue-frontend/src/firms/extensions.ts` defines code-owned site variants. Branding JSON can only choose a validated site key and cannot load arbitrary scripts or components.

On the local split (Vite `:5173`, API `:8080`) the API host is `localhost`, so visitors see the platform branding payload (`MatterForge`) rather than tenant branding unless they navigate through a verified tenant host.

## Migration commands

`php artisan migrate` runs `database/migrations` against the control plane only. Tenant files live in `database/tenant-migrations` and are not on that path.

`php artisan tenants:migrate` runs the tenant path for firms whose provisioning status is `ready`. It prints firm id, `ok` or `failed`, and a sanitized message. It keeps going after a failure. It does not create schemas and does not change a firm to `active`. A non-zero exit means at least one firm failed. Run it again to resume completed migration files. Optional `--firm=` takes a firm UUID, not a schema name.

## Security boundaries that are still open

- Legacy shared rows still exist in control-plane tables for workspace modules from earlier slices. New reads/writes are tenant-scoped; migration cleanup and table retirement are deferred until a separate reconciliation release.
- Provisioning now has a per-firm credential abstraction, but local environments can fall back to one shared runtime account when MySQL user-management privileges are unavailable. Production must disable fallback and enforce distinct runtime accounts per firm.
- Login has no throttle. The SPA stores the Sanctum bearer token in `localStorage`. CSRF cookie authentication and MFA are not implemented.
- Production must configure trusted proxies before `Host` is treated as the public site's hostname. Membership is still required for tenant routes.
- Review reasons and evidence references are stored for the reviewer. They are not written to the application log. Evidence contents are not accepted; only reference strings are.
- This slice does not claim a bar-association check, SOC 2, or any regulatory certification. No external legal-research or AI provider is called.

## How later modules should use this

Authenticate on the control plane, require a membership, then read and write through a `TenantModel` inside middleware or a job that establishes and clears `TenantContext`. Do not add a `tenant_id` column to the shared tables as a substitute. Do not create a schema from a login request.
