# MatterForge

**MatterForge is a working product name.** No trademark, domain, or public-branding clearance has been done, so don't use it publicly until that's done ([rename record](Markdown/product/naming-and-rename.md)). The project was previously described as the "Law Firm Unified Platform". Code namespaces, the Git remote, and authentication identifiers still use their original names on purpose.

MatterForge is law-firm software that carries a client from the first consultation through matter closure, with the firm's own operations in the same application. Each tenant is a **law firm**. The first firm is Maldanado and Benitez Law (spelling pending confirmation), with construction law, litigation, and personal injury as its practice areas.

This repository is building a **single system for law-firm operations** so the firm does not need:

- one product for the public website,
- a second product for intake/CRM,
- and a third product for practice management and operations.

The product direction is to consolidate those workflows into one app that can eventually be offered as multi-tenant SaaS for other firms.

## Current state (2026-10-12)

- **Single firm only.** No multitenancy and no cross-firm isolation is implemented. The multi-firm architecture is **designed but not implemented**: [ADR-002](Markdown/database/ADR-002-multitenant-architecture.md) refines [ADR-001](Markdown/database/ADR-001-tenancy.md). Its reviewable SQL lives in [`db/design/`](db/design/README.md) and was checked on a disposable MySQL 8.0.36 container with synthetic data only.
- Matter-level authorization, a client-portal API, and restricted-party masking are enforced on the server. They are verified by integration tests against real MySQL 8.0.36 for one firm ([matter access policy](Markdown/security/matter-access-policy.md)).
- The schema is managed by EF Core migrations. Databases created by the old `EnsureCreated` startup are **refused** until baselined ([migration baseline](Markdown/database/migration-baseline.md)).
- No secrets are in the tracked config files. Earlier commits contain credentials that need rotation (listed in [ProgressPlan.md](ProgressPlan.md)).
- Not production ready. Billing, trust, payments, the document binary pipeline, MFA, and the portal frontend are absent.

## Read first

1. [ProgressPlan.md](ProgressPlan.md): current status, checks, next action.
2. [SLICE_HISTORY.md](SLICE_HISTORY.md): what each implementation step did and what was verified.
3. [Markdown/README.md](Markdown/README.md): design, domain, and security documentation.

Agents and contributors also follow [AGENTS.md](AGENTS.md).

## Why this exists

Many firms run a fragmented stack:

- marketing website + contact forms,
- a legal CRM/intake product,
- a legal practice management system,
- separate accounting and payment tools,
- disconnected document and e-sign systems.

This project is intentionally taking the opposite path:

- modern public site for lead generation,
- internal legal operations workspace,
- intake-to-matter lifecycle in one system,
- role-based access across legal staff and clients,
- extensible architecture for future billing, trust accounting, portal, and integrations.

## Product strategy

The baseline benchmark is Clio's combined capability footprint across:

- Clio Manage (practice management),
- Clio Grow (legal CRM/intake),
- Clio Payments (legal payment + trust-aware flows),
- and ecosystem integrations.

We are **not embedding Clio**. We are replicating and adapting core module patterns so this firm can operate inside its own branded platform.

## Current codebase snapshot

### Frontend

- React + TypeScript + Vite
- Route-based app with:
  - public pages (marketing + contact + appointment request),
  - authenticated workspace pages (calendar, parties, conflicts, intake, matters, consultations).
- Role/permission-aware navigation and protected routes.

### Backend

- ASP.NET Core minimal APIs
- EF Core persistence (`LawFirmDbContext`) with modules for:
  - parties and aliases,
  - conflict checks and decisions,
  - calendar entries,
  - intake records/stage history/notes,
  - matters/tasks/documents/timeline,
  - consultation requests,
  - audit events.
- JWT auth with policy/permission model.

## Current implemented modules (high-level)

- Public website + content + animated UX
- Authentication and role-based authorization
- Contacts/parties management
- Conflict check + legal review decision recording
- Staff calendar/task scheduling
- Intake pipeline with stage transitions and notes
- Intake conversion to matter shell
- Matter workspace tabs (overview, tasks, docs metadata, timeline)
- Consultation request flow from contact page + internal queue
- Audit logging hooks on major write operations

## What is intentionally not complete yet

These major categories are still early or missing compared with Clio-scale systems:

- legal billing and invoicing engine,
- trust accounting and reconciliation workflows,
- payments processing,
- client self-service portal,
- document generation/e-sign,
- workflow automation/rules engine,
- reporting/analytics suite,
- integration marketplace and sync connectors,
- multi-office/multi-entity accounting controls,
- enterprise security controls (SSO/MFA/step-up auth),
- tenant architecture for SaaS distribution.

## Source-of-truth planning artifact

Use `ProgressPlan.md` in the repository root as the handoff document and ongoing parity tracker.

`ProgressPlan.md` includes:

- Clio capability map,
- current implementation status in this repository,
- evidence pointers to code,
- gap analysis,
- technical roadmap to close gaps,
- and priority sequencing.

Update it as each module is implemented so progress remains measurable over time.

## Running locally

Each command below is marked **executed** (run on 2026-10-12 with the result shown) or **inspected** (checked against the repository files but not run).

Prerequisites: .NET 8 SDK or newer (the machine used had SDK 10.0.300, targeting `net8.0`), Node.js (v22 used), MySQL 8.0, and Docker for the tests.

### Backend

Tracked config contains no connection string or signing key; startup fails until you supply them. From `backend/`, use user-secrets (**inspected**):

```powershell
dotnet user-secrets set "ConnectionStrings:LawFirm" "Server=localhost;Port=3306;Database=<new_empty_db>;User=<user>;Password=<password>"
dotnet user-secrets set "Jwt:SigningKey" "<random string of at least 32 characters>"
dotnet user-secrets set "Authentication:DemoUsers:Enabled" "true"   # optional, Development only
dotnet run                                                          # serves http://localhost:5073
```

- In Development, pending migrations are applied and fictional demo data is seeded. Use a **new empty database**. An existing database from before 2026-10-12 is refused; see [migration baseline](Markdown/database/migration-baseline.md).
- Demo sign-in only works when `Authentication:DemoUsers:Enabled` is true *and* the environment is Development. Enabling it elsewhere stops startup. Without it, `/api/auth/login` returns 503.
- Build (**executed**, success): `dotnet build -o ./.build-out`. The alternate output avoids `bin/Debug` being locked by a running API.

### Tests

From the repository root, with Docker running (**executed**, 44 passed): `dotnet test backend.IntegrationTests -c EfDesign`. The tests start a disposable `mysql:8.0.36` container, create the schema with migrations, and use synthetic data only. They never use your configured database.

### Frontend

From `frontend/`: `npm install` (**inspected**), `npm run dev` (**inspected**; proxies `/api` to `http://localhost:5073` per `vite.config.ts`), `npm run build` (**executed**, success), `npm run lint` (**executed**, fails with 7 errors and 3 warnings that predate this work).

## Development credentials (local only)

Demo users and passwords are listed in `backend/DEV_AUTH_CREDENTIALS.md`.

These are for development/testing only and must never be used in production. They are disabled unless explicitly enabled in Development.

## References used for capability benchmarking

- [Clio Manage](https://www.clio.com/manage/)
- [Clio Features](https://www.clio.com/features/)
- [Clio Pricing / plan capability matrix](https://www.clio.com/pricing/)
- [Clio case management features](https://www.clio.com/features/case-management/)
- [Clio online intake forms](https://www.clio.com/features/online-intake-forms/)
- [Clio legal CRM](https://www.clio.com/features/legal-crm-software/)
- [Clio Payments](https://www.clio.com/features/payments/)
- [Clio integrations overview](https://www.clio.com/features/integrations/)
