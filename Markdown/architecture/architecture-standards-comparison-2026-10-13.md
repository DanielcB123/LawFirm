# Architecture standards comparison (2026-10-13)

[Home](../README.md) · [Architecture](README.md) · [Proposed architecture](README.md) · [Solution structure](solution-structure.md) · [Module sweep](module-interaction-and-vulnerability-sweep-2026-10-13.md)

## Purpose
This is a neutral comparison of:
- **Target standards** documented in `Markdown/architecture/` (especially `README.md` and `solution-structure.md`), and
- **Current implementation** in backend/frontend source.

Ratings:
- **Meets:** implemented and used in runtime paths.
- **Partially meets:** implemented in part, scoped, or missing enforcement/coverage.
- **Does not meet:** absent or only boilerplate/design.

## Standards matrix

| Standard | Target from architecture docs | Current implementation status | Rating |
|---|---|---|---|
| Feature-based modular monolith | Backend modules as vertical slices with `IEndpointModule` | Implemented and active across core modules | Meets |
| Shared contracts and entity organization | Contracts under `Features/<Module>/Contracts`, entities under persistence entities | Implemented in repository structure | Meets |
| Backend composition root only | `Program.cs` wires modules/services, not business logic | Implemented | Meets |
| Centralized matter authorization | Shared enforcement helper to avoid per-endpoint drift | `IMatterAccessService` used by matters/portal/calendar paths | Meets (single-firm scope) |
| Centralized restricted-party masking | Shared visibility policy | `PartyVisibility` used by parties/conflicts | Meets |
| Frontend feature boundaries | `app -> pages -> features -> lib/components/types`; no deep feature imports | Implemented with boundary lint rule | Meets |
| Real-engine integration tests | Integration tests on MySQL, not in-memory | Present (`backend.IntegrationTests`) and passing in prior slices | Meets |
| Unit + frontend tests | Separate unit tests and frontend tests | Present but still thin coverage | Partially meets |
| CI quality gates | Build/lint/test gates on PR | Not present | Does not meet |
| Request boundary with trusted firm resolution | Auth -> trusted firm context -> membership -> authorization | Firm context/membership resolution not implemented yet | Does not meet |
| Tenant-isolated data plane | Per-firm data access and routing | Single database, single firm runtime | Does not meet |
| Durable outbox + background workers | External effects via outbox/workers | Interfaces/placeholder dispatcher only, not wired | Does not meet |
| Private document pipeline | Secure file handling and controlled access | Metadata-only document flow; no binary pipeline | Does not meet |
| Managed identity hardening | Managed IdP, MFA, robust session model | Demo/JWT local model only | Does not meet |
| Operational readiness | Provisioning, recovery, and observability flow in code | Design docs exist; runtime capability not implemented | Does not meet |

## Unbiased verdict
**The repository meets the structural coding standards for a modular monolith, but it does not yet meet the platform architecture standards required for multi-firm SaaS operation.**

### What is objectively strong now
- Clear module layout and endpoint composition.
- Shared access-control primitives are in place and reused.
- Migration-first schema handling exists and legacy unmanaged schema is blocked.
- Frontend has an enforceable feature boundary pattern.

### What is objectively below target
- No trusted firm resolution or per-firm routing in runtime.
- No persistent identity/membership model for a multi-firm platform.
- No outbox/worker execution model for reliable external effects.
- No production-grade document pipeline or payments/trust implementation.
- No CI gate enforcement and no production operations substrate.

## Overall grade (architecture adherence)
- **Code structure adherence:** High.
- **Security/tenancy architecture adherence:** Low to medium.
- **SaaS readiness adherence (new-firm onboarding capable in production):** Low.

This means the current codebase is a solid foundation, but not yet at the standards described in `Markdown/architecture/README.md` for operating as a multi-firm SaaS in production.
