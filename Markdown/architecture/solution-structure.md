# Solution and folder structure

[Home](../README.md) · [Architecture](README.md) · [Stack and runtime](stack-and-runtime.md) · [Codebase audit](current-codebase-audit.md) · [Sources](../research/sources.md)

**Status:** adopted 2026-10-12 (slice R1). The layout below is what is in the repository now. Boilerplate pieces are labelled **Not implemented**: they compile and are documented, but nothing calls them yet.

## Research summary

Sources are listed in the [source register](../research/sources.md#solution-structure-2026-10-12). Current guidance for a .NET API with a React SPA, maintained by one small team, agrees on these points:

- **Backend: modular monolith organized by feature (vertical slices).** Each feature keeps its endpoints, request and response contracts, and logic together instead of spreading them across `Controllers/`, `Services/`, and `Models/` folders. Modules talk to each other through a small public surface. Per-module projects and per-module DbContexts are the strict form; a single project with feature folders is the documented lighter variant, and it is the usual starting point.
- **Repository root build files:** a solution file, `Directory.Build.props` (shared MSBuild settings), `Directory.Packages.props` (Central Package Management, so one place holds every NuGet version), `global.json` (SDK pin with roll-forward), and one root `.editorconfig`.
- **Tests:** separate unit and integration test projects. Integration tests run on the real database engine.
- **Frontend: feature-based `src/`.** `app/` holds composition (providers, routes, shell), `features/<name>/` holds a domain's API calls, types, hooks, and components behind an `index.ts` public API, `components/` holds generic UI, `lib/` holds configured infrastructure such as the HTTP client, and `pages/` holds route-level screens that compose features. Dependencies flow one way: `app` → `pages` → `features` → `lib`/`components`/`types`. A lint rule blocks deep imports into a feature.

## Decisions and what was deferred

| Decision | Adopted | Deferred, with the trigger to revisit |
|---|---|---|
| Backend shape | One API project, feature folders (`Features/<Module>`), shared `Infrastructure/` and `Common/` | Per-module projects (`<Module>` + `<Module>.Contracts`) and architecture tests: revisit when a second team works in parallel, or when modules start reaching into each other's tables |
| DbContext | One `LawFirmDbContext` (per firm database, per ADR-002) | Per-module DbContexts: they would split migrations across the per-firm fleet runner, so revisit after T1–T3 of the [multi-firm plan](../delivery/multitenant-implementation-plan.md) |
| Top-level folders | `backend/`, `backend.IntegrationTests/`, `backend.UnitTests/`, `frontend/` kept | `src/` + `tests/` layout: the API and Vite dev server run from these folders, and every doc links them. Move together with the folder rename in the [naming record](../product/naming-and-rename.md) if wanted |
| Namespace | `EnterpriseKnowledgeAssistant.Api` kept | Rename deferred per [stack and runtime](stack-and-runtime.md#naming) |
| Handlers | Endpoints call `LawFirmDbContext` and shared services directly | MediatR or a handler abstraction: not needed at this size |
| Validation | Existing per-endpoint validation dictionaries | FluentValidation or .NET 10 built-in minimal-API validation, when the runtime moves to the next LTS |
| Frontend data fetching | Existing `useEffect` + typed API functions | TanStack Query (also fixes the `set-state-in-effect` lint errors); decide in the F3 lint slice |
| Path aliases | Relative imports | `@/` alias: optional; adopt everywhere at once or not at all |

## Repository root

| Path | Purpose |
|---|---|
| `MatterForge.sln` | Solution with the API, and a `tests` folder holding both test projects. Configurations: Debug, Release. The `-c EfDesign` name used in earlier commands is a project-level configuration and does not exist at solution level |
| `Directory.Build.props` | `Nullable` and `ImplicitUsings` for every .NET project |
| `Directory.Packages.props` | Every NuGet version. Project files list packages without `Version` |
| `global.json` | SDK `10.0.300`, `rollForward: latestFeature`. The SDK builds the `net8.0` target |
| `.editorconfig` | Indentation and C# style preferences (suggestion severity only; builds are unaffected) |
| `dotnet-tools.json` | Local tool manifest (`dotnet-ef` 8.0.13) |
| `db/design/` | Reviewable multi-firm SQL and its validation ([README](../../db/design/README.md)) |
| `Markdown/` | All design and domain documentation. `Markdown/archive/original-prompt/` holds the original prompt and blueprint zip, read-only |

## Backend (`backend/`)

```text
backend/
  Program.cs                 composition root only: register modules and middleware
  Common/                    framework-light primitives shared by features
    Paging/                  PageRequest, PagedResult<T>, ToPagedResultAsync
  Features/<Module>/         one folder per business module (vertical slice)
    <Module>Endpoints.cs     IEndpointModule: routes, handlers, validation
    Contracts/               request and response records (same namespace as the module)
  Infrastructure/            technical capabilities shared across modules
    Access/                  matter access service, party visibility, actor claims
    Audit/                   audit trail writer
    Auth/                    JWT, permissions, policies, demo users
    Endpoints/               IEndpointModule discovery and /api mapping
    Errors/                  error envelope, correlation id, exception middleware
    OpenApi/                 Swagger setup (Development only)
    Persistence/             LawFirmDbContext, Entities/, Migrations/, seeding, design-time factory
    Tenancy/                 IFirmContext (Not implemented; plan step T1)
    Outbox/                  IOutbox, OutboxDispatcher (Not implemented or registered; gap G43)
    Storage/                 IDocumentStorage (Not implemented; gap G32)
    Messaging/               IEmailSender (Not implemented; gap G43)
    Payments/                IPaymentGateway (Not implemented; gap G51)
    Signatures/              ISignatureProvider (Not implemented; gap G24)
```

### Modules

| Module folder | Route prefix (under `/api`) | Status |
|---|---|---|
| `Auth`, `Identity` | `/auth`, `/identity` | Present (demo identity) |
| `Authorization` | demo policy routes | Present; placeholders removed as real modules ship |
| `Health` | `/health` | Present |
| `Parties`, `Conflicts`, `Intake`, `Consultations`, `Calendar` | as before | Present |
| `Matters` | `/matters-v2` and grants | Present |
| `ClientPortal` | `/portal/matters` | Present (API only) |
| `Documents` | `/documents-v2` | **Not implemented** (empty authenticated group) |
| `Engagements` | `/engagements` | **Not implemented** |
| `Billing` | `/billing-v2` | **Not implemented** |
| `Payments` | `/payments` | **Not implemented** |
| `Trust` | `/trust` | **Not implemented** |
| `Communications` | `/communications` | **Not implemented** |

An empty route group adds no endpoints and nothing appears in Swagger. The new prefixes `/documents-v2` and `/billing-v2` avoid the existing demo stubs at `/documents/upload` and `/billing/*`.

### Rules

- **Entities** live in `Infrastructure/Persistence/Entities/<Area>Entities.cs` and keep the `...Infrastructure.Persistence` namespace. EF Core's migration snapshot records entity CLR names, so moving an entity to another namespace needs a check with `has-pending-model-changes` first. The split on 2026-10-12 was verified to produce no model change.
- **Contracts** are records in `Features/<Module>/Contracts/`. They never expose entities. Client-facing contracts are separate from staff contracts (see [matter access policy](../security/matter-access-policy.md)).
- **Authorization filtering** goes through `Infrastructure/Access` services, not per-endpoint queries.
- **Lists** page with `PageRequest` and `ToPagedResultAsync` after access filtering, so counts never include unreadable rows.
- **Time** comes from the registered `TimeProvider`, not `DateTimeOffset.UtcNow`, in new code. Existing code is unchanged.
- **External providers** are called from the outbox dispatcher through the interfaces above, never inside a request's database transaction.
- **A module uses another module** through `Infrastructure` services or its contracts, not its entities' write paths.

### Adding a backend module

1. Create `Features/<Module>/<Module>Endpoints.cs` implementing `IEndpointModule`. It's discovered automatically.
2. Add records under `Features/<Module>/Contracts/`.
3. Add entities under `Infrastructure/Persistence/Entities/`, a `DbSet` and configuration in `LawFirmDbContext`, then a migration: `dotnet tool run dotnet-ef migrations add <Name> --project backend`.
4. Add integration tests in `backend.IntegrationTests/` (real MySQL) for authorization and data rules, and unit tests in `backend.UnitTests/` for pure logic.
5. Update the gap map row and the module document.

## Tests

| Project | What it covers | Needs |
|---|---|---|
| `backend.IntegrationTests/` | HTTP-level behavior through `WebApplicationFactory` on a disposable `mysql:8.0.36` container | Docker |
| `backend.UnitTests/` | Pure logic with no database or HTTP (currently `Common/Paging`) | Nothing |
| `frontend/src/**/*.test.ts(x)` | Vitest, colocated with the code (currently `lib/apiClient.test.ts`) | Nothing |

## Frontend (`frontend/src/`)

```text
src/
  main.tsx               mounts <AppProviders><AppShell/></AppProviders>
  app/                   composition only
    AppProviders.tsx     BrowserRouter + AuthProvider
    AppRoutes.tsx        every route
    AppShell.tsx         layout (nav, main, footer) and scroll-reveal effect
  config/env.ts          typed VITE_ variables (VITE_API_BASE_URL)
  lib/apiClient.ts       fetch wrapper, error envelope mapping, base URL
  components/layout/     SiteNav, SiteFooter
  features/<name>/       api.ts, types.ts, components, hooks; index.ts is the public API
    auth calendar conflicts consultations health intake matters parties
    portal               client portal API and types (backend routes exist; no pages yet)
    billing              draft types only (Not implemented; gap G50)
  pages/
    public/              marketing site and contact
    workspace/           staff workspace (Clio-shaped nav; decision log D-20261013-2)
    NotFoundPage.tsx
  types/money.ts         shared Money type (decimal string + currency)
  data/                  static marketing content
```

### Rules

- Code outside a feature imports `../features/<name>`, never `../features/<name>/api`. ESLint enforces this (`no-restricted-imports`).
- Features don't import pages or `app/`. If two features need the same thing, promote it to `lib/`, `components/`, or `types/`.
- API functions take the token explicitly and call `lib/apiClient`; components don't call `fetch`.
- Money is never a JavaScript `number` on the wire; use `Money` from `types/money.ts`.
- Only `VITE_`-prefixed variables reach the browser, so they must never hold secrets. Copy `frontend/.env.example` to `.env.local` (git-ignored) to override.

### Adding a frontend feature

1. Create `features/<name>/api.ts` and `types.ts`, and an `index.ts` that re-exports the public parts.
2. Add route-level screens under `pages/<area>/` and register them in `app/AppRoutes.tsx`.
3. Add navigation in `components/layout/SiteNav.tsx`, gated by the actor's permissions (the server still enforces access).
4. Add a colocated `*.test.ts` for logic worth protecting.

## Verification of this layout (2026-10-12)

The checks run for this change, with results, are recorded in [SLICE_HISTORY.md](../../SLICE_HISTORY.md) (entry `2026-10-12 - Slice R1`) and [ProgressPlan.md](../../ProgressPlan.md).
