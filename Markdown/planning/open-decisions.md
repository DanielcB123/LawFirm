# Open decisions and assumptions

[Planning](README.md) · [Decision log](decision-log.md) · [Home](../README.md) · [First firm](../product/first-firm.md) · [ADR-001](../database/ADR-001-tenancy.md) · [ADR-002](../database/ADR-002-multitenant-architecture.md)

Moved here on 2026-10-13 23:15 EDT (D-20261013-4). The 2026-10-12 text below is kept as it was written. Later work changed some of those facts (EF migrations exist; tests exist). Those later facts are in the [decision log](decision-log.md) and the [schema evolution](../database/schema-evolution.md), not edited into the 2026-10-12 paragraphs.

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
See [ADR-002](../database/ADR-002-multitenant-architecture.md) and the [implementation plan](../delivery/multitenant-implementation-plan.md).

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

## Still open after the 2026-10-19 Laravel tenancy slice

The sentence above that says no tenancy exists is the 2026-10-12 audit. The Laravel slice now has a control plane and a schema per new firm. These choices are still open:

| Decision | Why it matters | Owner |
|---|---|---|
| When to retire legacy shared workspace rows and drop control-plane workspace tables | New writes now use tenant schemas, but legacy shared rows still require reviewed reconciliation and cutover cleanup | Founder and technical owner |
| Per-firm MySQL accounts and where their passwords live | The shared runtime user can use every schema it has been granted | Technical owner |
| How a human reviewer proves a firm is legitimate, and how long evidence references are kept | The software only stores the review. It does not check a bar association | Firm and legal advisers |
| Whether a platform administrator may ever open a firm schema for support | This slice denies tenant data without a membership | Founder and legal advisers |
| Custom-domain DNS proof beyond an administrator typing the hostname | Public branding trusts that attested hostname | Technical owner |
| Exact legal name and public branding of the first firm | Still unconfirmed | Founder and first firm |
| Which external provider (if any) will be used for automated business-existence and applicant-authority verification | Onboarding now has a provider slot that fails closed when unconfigured; manual review remains the only implemented mode | Founder and legal advisers |
| Whether privileged platform-review actions require fresh password confirmation in addition to platform-admin status | Current implementation relies on authenticated platform-admin identity and existing environment controls; production MFA policy is still external | Founder and technical owner |
