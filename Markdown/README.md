# MatterForge — architecture and product blueprint

**MatterForge is a working product name**, not cleared public branding. The repository folder, Git remote, namespaces, and authentication identifiers keep their original names; see the [naming and rename record](product/naming-and-rename.md).

**Planning baseline:** October 12, 2026. **First real firm:** Maldanado and Benitez Law (spelling supplied by founder; confirm legal entity and branding before onboarding). **Focus:** construction law, civil litigation, and personal injury.

## Mission
One application carries a prospective client from consultation booking through intake, engagement, active representation, billing, resolution, and file delivery. Staff manage the matter in the same system. Clients sign in to see approved information, upcoming appointments, requested actions, documents, and financial information without repeatedly calling for routine updates.

A tenant means a **law firm**, not an individual legal client. A firm can have thousands of clients and matters. A client can participate in multiple matters and potentially work with multiple firms; those permissions remain separate.

“One stop” means a unified experience and system of record. Payment networks, licensed legal research, calendar providers, signature providers, and courts can remain external services behind integrations. Do not promise that software eliminates attorney communication or human judgment.

## Current state (audited 2026-10-12)
The repository is an ASP.NET Core 8 + EF Core (Pomelo/MySQL) API with a React 19 + Vite SPA. It runs as **one firm on one database with no tenant concept**. It uses demo identities, gated to Development. It has no CI, file storage, background jobs, portal frontend, billing, or payments. Since slice F1 it has EF Core migrations, a real-MySQL integration test suite, and assignment- and grant-based matter authorization (single firm). Multi-firm operation is **designed, not implemented**: [ADR-002](database/ADR-002-multitenant-architecture.md), [design SQL and validation](../db/design/README.md). Start with the [codebase audit](architecture/current-codebase-audit.md), then the [gap map](delivery/implementation-gap-map.md).

## Read in this order

| Area | Entry point | Purpose |
|---|---|---|
| Product | [Mission and scope](product/vision-and-scope.md) | Outcomes, users, boundaries |
| Product | [Complete client journey](product/client-journey.md) | Consultation through closure |
| First firm | [Maldanado and Benitez rollout](product/first-firm.md) | Real customer discovery and onboarding |
| Product | [Naming and rename record](product/naming-and-rename.md) | Working name versus cleared branding; folder rename status; identifier inventory |
| Architecture | [Current codebase audit](architecture/current-codebase-audit.md) | What exists today, with file evidence |
| Architecture | [Stack and runtime](architecture/stack-and-runtime.md) | Actual frameworks and versions kept |
| Architecture | [Application architecture](architecture/README.md) | Modules, boundaries, integration strategy |
| Database | [Database overview](database/README.md) | Vocabulary and storage responsibilities |
| Database | [Tenancy research](database/tenancy-research.md) | Primary-source analysis and tradeoffs |
| Database | [ADR-001 tenant isolation](database/ADR-001-tenancy.md) | Proposed decision and conditions |
| Database | [ADR-002 multi-firm architecture](database/ADR-002-multitenant-architecture.md) | Options, capacity scenarios, refined decision, reconsideration triggers |
| Database | [Data dictionary](database/data-dictionary.md) | Control plane and firm tables, identifiers, money, dates, ERDs |
| Database | [Query and index spec](database/query-and-index-spec.md) | Real queries, observed plans, partitioning findings |
| Database | [Design SQL and validation](../db/design/README.md) | Reviewable DDL, synthetic data, disposable-MySQL checks |
| Database | [Data model](database/data-model.md) | Entities, ownership, invariants |
| Database | [Provisioning and migration](database/provisioning-and-migrations.md) | Operational tenant lifecycle |
| Database | [Migration baseline](database/migration-baseline.md) | Moving existing `EnsureCreated` databases onto EF Core migrations |
| Portal | [Client portal](modules/client-portal.md) | What clients see and do |
| Intake | [Intake and consultations](modules/intake-and-consultations.md) | Lead qualification and conflicts |
| Matters | [Matter management](modules/matter-management.md) | Work, deadlines, and accountability |
| Documents | [Documents and evidence](modules/documents-and-evidence.md) | Versions, signatures, preservation |
| Communication | [Messaging and updates](modules/communications.md) | Published updates and notifications |
| Billing | [Billing overview](billing/README.md) | Legal billing versus SaaS subscriptions |
| Billing | [Payments and ledger](billing/payments-and-ledger.md) | Financial correctness and security |
| Billing | [Trust and settlement](billing/trust-and-settlement.md) | Reconciliation and distribution controls |
| Practice | [Construction law](practice-areas/construction-law.md) | Projects, parties, evidence, claims |
| Practice | [Personal injury](practice-areas/personal-injury.md) | Treatment, coverage, liens, settlement |
| Security | [Security requirements](security/README.md) | Threats, controls, verification |
| Security | [Confidentiality and access](security/confidentiality-and-access.md) | Ethical walls and audience boundaries |
| Security | [Matter access policy](security/matter-access-policy.md) | Implemented single-firm matter authorization, role/action matrix, evidence |
| Security | [Multi-firm threat model](security/multitenant-threat-model.md) | Threats T1–T32, controls, residual risks |
| Operations | [Recovery and reliability](operations/reliability-and-recovery.md) | Restore, incidents, monitoring |
| Operations | [Fleet provisioning and migration](operations/fleet-provisioning-and-migration-runbook.md) | Idempotent provisioning, fleet migrations, first-firm move |
| Operations | [Recovery and offboarding](operations/recovery-and-offboarding-runbook.md) | Pools, noisy neighbor, one-firm restore, movement, deletion |
| Integrations | [Provider integration contracts](integrations/README.md) | Failure handling and licensed services |
| Delivery | [Implementation gap map](delivery/implementation-gap-map.md) | Requirement → current code → change → evidence |
| Delivery | [Roadmap and acceptance](delivery/roadmap-and-acceptance.md) | Safe implementation order and first slice |
| Delivery | [Multi-firm implementation plan](delivery/multitenant-implementation-plan.md) | Dependency-ordered steps T0–T12 and blocking decisions |
| Delivery | [Multi-firm acceptance matrix](delivery/multitenant-acceptance-matrix.md) | Acceptance-test ids, status, evidence |
| Delivery | [Open decisions](delivery/open-decisions.md) | Known facts versus unresolved choices |
| Research | [Source register](research/sources.md) | References, dates, limitations |
| Cursor | [Copyable Cursor prompt](../Markldown/CURSOR_PROMPT.md) | Repository audit before implementation |

## Existing repository documents
These predate this directory and are preserved as-is: [README](../README.md), [ProgressPlan (Clio parity tracker)](../ProgressPlan.md), [Slice history](../SLICE_HISTORY.md), [Authorization strategy](../backend/AUTHORIZATION_STRATEGY.md), [Dev credentials](../backend/DEV_AUTH_CREDENTIALS.md), [Frontend structure](../frontend/src/ARCHITECTURE.md). Where they disagree with the audit (for example "immutable" audit events, or SQLite in early slice notes), the audit reflects the code.

## Documentation conventions
Each file has a parent link and related links. This directory mirrors product domains, rather than pretending to be the existing source tree. Documentation is versioned with the repository. Record actual code paths only after auditing that repository. A proposal is not implemented functionality. Keep evidence, assumptions, decisions, and acceptance criteria distinct.

**Proposed starting shape:** shared application, modular monolith, shared control plane, separate MySQL database per law firm, private object storage, durable background work. The audit supports this recommendation (see [ADR-001](database/ADR-001-tenancy.md)). [ADR-002](database/ADR-002-multitenant-architecture.md) refines it with placement cells, connection budgets, table-level grants, and engine evidence from disposable MySQL. It remains conditional on hosting validation, and none of it is implemented yet.
