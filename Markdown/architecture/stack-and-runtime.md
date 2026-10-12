# Stack and runtime

[Home](../README.md) · [Architecture](README.md) · [Codebase audit](current-codebase-audit.md) · [ADR-001](../database/ADR-001-tenancy.md)

Recorded from repository evidence at commit `61e5134`. See the [audit](current-codebase-audit.md) for paths.

## Decision: keep the existing stack
The repository is a working ASP.NET Core + React + MySQL application with consistent conventions. No evidence justifies changing languages, frameworks, or database engine. All blueprint designs are adapted to this stack.

| Layer | Current choice | Version evidence | Keep? |
|---|---|---|---|
| API runtime | ASP.NET Core minimal APIs, `IEndpointModule` per feature | `net8.0` in `backend/EnterpriseKnowledgeAssistant.Api.csproj`; builds with SDK 10.0.300 | Yes. .NET 8 is LTS; plan a deliberate move to the next LTS rather than ad hoc |
| ORM | EF Core via Pomelo.EntityFrameworkCore.MySql | 8.0.3 | Yes; adopt EF Core migrations (currently `EnsureCreated`) |
| Database | MySQL (InnoDB assumed) | `MySqlServerVersion(8, 0, 36)` in code; dev port 3307 | Yes; ADR-001 targets MySQL semantics |
| Auth | Local JWT bearer (HS256), permission claims, policy handler | Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11 | Keep the policy model; replace token issuance with persistent identity + managed IdP/MFA, as `backend/AUTHORIZATION_STRATEGY.md` already proposes |
| API docs | Swashbuckle (Development only) | 6.8.1 | Yes |
| Frontend | React + TypeScript SPA, react-router-dom | React 19.2, router 6.30, TS ~6.0 | Yes |
| Build tooling | Vite, ESLint flat config | Vite 8.2, ESLint 10 | Yes; fix failing lint before adding CI gates |
| Dev integration | Vite proxy `/api` → `http://localhost:5073` | `frontend/vite.config.ts` | Yes for dev; production hosting undefined |

## Absent runtime capabilities (to be chosen, not assumed)
Object storage, malware scanning, background job runner/outbox dispatcher, email/SMS provider, payment processor, e-signature provider, secrets manager, hosting platform, CI, observability backend. Each choice needs a short decision record before implementation. Prefer .NET-native options (`IHostedService`/`BackgroundService` for a first outbox dispatcher; ASP.NET Core rate limiting middleware; Data Protection) before adding infrastructure.

## Naming
The C# namespace and project are `EnterpriseKnowledgeAssistant.Api`, left over from the starter. Renaming is cosmetic and touches every file, so defer it unless it blocks packaging.

## Runtime configuration rules going forward
Move the signing key and connection strings out of tracked `appsettings*.json` into user-secrets (dev) and a secrets manager (deployed). Rotate any values that may have been reused outside local development. Add an `appsettings.Example.json` that contains no secrets. Gate `DemoUserStore` and `SeedDevelopmentDataAsync` to Development, and fail startup elsewhere if demo identity is configured.
