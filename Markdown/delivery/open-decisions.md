# Open decisions and assumptions

[Home](../README.md) · [First firm](../product/first-firm.md) · [ADR-001](../database/ADR-001-tenancy.md) · [ADR-002](../database/ADR-002-multitenant-architecture.md)

## Confirmed from the founder
One-stop law-firm SaaS; client consultation-to-closure portal; first real firm named Maldanado and Benitez Law; focus on construction law/litigation and personal injury; root-level Markdown directory with linked domain documents; Cursor must inspect the entire codebase first and work carefully.

## Resolved by the repository audit (2026-10-12)
Stack is ASP.NET Core 8 + EF Core/Pomelo + MySQL 8.0 + React 19/Vite and is kept ([stack](../architecture/stack-and-runtime.md)). Current tenancy is none: single firm, single database ([audit](../architecture/current-codebase-audit.md)). No deployment, CI, tests, or migrations exist. Identity is a demo in-memory store.

## Proposed, not confirmed
Modular monolith; shared control plane; database per law firm on MySQL ([ADR-001](../database/ADR-001-tenancy.md), refined by [ADR-002](../database/ADR-002-multitenant-architecture.md) with placement cells, connection budgets, and table-level grants); provider-backed payments/signatures; explicit client publication; attorney verification of legal dates; staged adoption. No hosting account was inspected.

## New production-sensitive unknowns from the audit
- Hosting provider and its MySQL backup/restore and `max_connections` limits (drives ADR-001 pool sizing and restore procedure).
- Whether the JWT signing key and DB passwords in tracked `appsettings*.json` were ever used outside local development (determines rotation scope).
- Identity provider choice for staff MFA and client sign-in.
- Firm time zone and business hours for consultation slots (currently UTC).
- Object storage and malware-scanning provider.

## New decisions from the multi-firm architecture work (2026-10-12)
See [ADR-002](../database/ADR-002-multitenant-architecture.md) and the [implementation plan](multitenant-implementation-plan.md).

| Decision | Why it matters | Owner/source |
|---|---|---|
| Hosting provider: measured `max_connections`, point-in-time restore granularity (server-level only?), TLS, parameter control | Cell connection budget; one-firm restore path; trigger approach | Technical owner |
| Trigger privilege: privileged `DEFINER` provisioning step (validated) versus setting `log_bin_trust_function_creators` on the server | Migration accounts can't create triggers when binary logging is on (ERROR 1419) | Technical owner and host capabilities |
| Rounding rule for time amounts (designed: half away from zero, per line, to cents) | Invoice correctness | Firm attorney or accountant |
| Retention schedule and offboarding retention period | Purge, holds, deletion evidence | Firm and legal advisers |
| Key management (KMS) for secrets and field encryption | Secret references, backups, exports | Technical owner |
| Cell sizing: firms per cell, per-firm pool size, per-account connection cap | Noisy neighbor, cost; needs load measurement | Technical owner, after measurement |
| Clearance of the "MatterForge" brand (trademark, domains) | Working name only today ([naming record](../product/naming-and-rename.md)) | Founder and legal advisers |
| Exact spelling and legal entity of "Maldanado and Benitez Law" | Firm record, contracts, branding | Founder and first firm |

## Resolve through audit/discovery

| Decision | Why it matters | Owner/source |
|---|---|---|
| Existing frameworks, engine, auth, deployment | Determines implementation/migration strategy | Resolved except deployment (none exists) — see audit |
| Exact firm name/legal entity and jurisdiction | Contracts, configuration, legal rules | Founder and first firm |
| Fee agreements and trust practices | Ledger and distribution design | Firm attorney/accountant |
| Hosting region, residency, recovery targets | Cost, contracts, backup design | Technical owner and firm |
| Required current tools/provider exports | Migration and one-stop workflow | First-firm walkthrough |
| Client/representative/joint-client permissions | Confidentiality and portal behavior | Firm policy |
| Current schema/tenancy | Whether proposed isolation change is justified | Resolved: no tenancy exists; ADR-001 revised |
| Investment/customer commercial relationship | Does not imply legal-data access | Founder and legal advisers |
| Branding, languages, accessibility needs | Client experience | First firm |
| Research API licensing | Permitted integrated research | Provider procurement |
| Provider PCI/HIPAA/BAA obligations if applicable | Scope and contractual safeguards | Provider/firm review |

Proceed with documentation, synthetic tests, and reversible foundation work where these questions do not block correctness. Record blockers precisely rather than silently selecting production-sensitive facts.
