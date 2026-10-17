# Current Architecture (Evidence-Based)

## Runtime stack in repository

- Backend: ASP.NET Core minimal API (`net8.0`), EF Core + Pomelo MySQL provider.
- Frontend: React 19 + TypeScript + Vite.
- Tests:
  - `backend.IntegrationTests` using Testcontainers and `mysql:8.0.36`.
  - `backend.UnitTests` for isolated logic.
  - frontend Vitest tests.

## Process model

- Backend starts via `backend/Program.cs`.
- `AddEndpointModules()` discovers `IEndpointModule` classes and maps all endpoints under `/api`.
- Persistence is initialized at startup through `InitializeLawFirmPersistenceAsync()` before serving requests.
- In Development, synthetic seed data is loaded if tables are empty.

## Database model and lifecycle

- Primary application context: `LawFirmDbContext`.
- Single-firm data model (no implemented tenant dimension in active schema).
- Startup behavior:
  - fails if connection string is missing,
  - refuses legacy unmanaged schema state,
  - applies pending EF migrations only when explicitly enabled by config.

## Authentication and authorization

- JWT bearer auth with policy-based authorization.
- Role and permission model in `Infrastructure/Auth`.
- Demo users are hardcoded and only allowed if explicitly enabled in Development.
- Matter-level access control implemented through `IMatterAccessService`.

## Frontend architecture

- React SPA with route map in `frontend/src/app/AppRoutes.tsx`.
- Shared shell in `AppShell.tsx` with public/workspace navigation and global background behavior.
- Frontend API communication via feature-level API modules and Vite proxy in dev mode.

## Current local runtime dependencies

- Backend needs reachable MySQL at startup.
- Frontend dev server proxies `/api` to backend at `http://localhost:5073`.
- Integration tests require Docker Engine.
- No repository-native Docker Compose runtime exists yet for app development.

## Docker and host service observations

- Docker Engine and Docker Compose are available on this workstation.
- Host port `3307` was free at baseline check.
- XAMPP-related processes were running (`xampp-control`, `httpd`, `mysqld`).
- Detected host MySQL listener on `3306` (not `3307`) at baseline.

## Architectural gaps relative to migration goal

- No Laravel backend currently exists.
- No Vue 3 frontend currently exists.
- No Compose-defined app runtime currently exists.
- Current stack still expects direct host/runtime setup rather than fully containerized workflow.
