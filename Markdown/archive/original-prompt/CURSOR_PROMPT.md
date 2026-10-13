# Cursor prompt — audit first, then build the linked law-firm SaaS blueprint

Copy the text below into Cursor while the actual application repository is open. Place the supplied `Markdown/` folder at the repository root, or use its content as the starting documentation specification. This prompt is outside that folder so importing it does not recursively regenerate itself.

---

You are working on a real law-firm SaaS product. Take your time. Prioritize correct understanding, coherent architecture, client confidentiality, financial correctness, and recoverability. Do not rush, skip unfamiliar modules, fabricate implementation status, or replace working code merely to match a preferred stack.

## Immediate objective
Before implementing features or changing the database architecture, review the entire codebase and understand how it is structured at the root and through each first-party module. Then create or integrate a root-level directory named exactly `Markdown`. Build a clear hierarchy of linked Markdown documents reflecting the product's domains. The starting blueprint is supplied with this prompt; adapt it to actual repository evidence.

The mission is one application that moves a law firm's clients from booking a consultation through intake, engagement, active representation, billing, resolution, and closure. Clients sign in to see approved matter information, next steps, appointments, document requests, shared documents, financial information, and secure messages. Staff manage the same matters without duplicating work across disconnected websites. External licensed/payment/signature/court services may be integrated behind a unified experience.

The first real customer/firm is **Maldanado and Benitez Law**, using the founder's supplied spelling until confirmed. This is a real tenant, not a fictional seed company. Do not invent firm facts, import real case data into development, or grant investors matter access. Initial practice areas are construction law, litigation, and personal injury. A SaaS tenant is a law firm; represented clients and their matters belong inside that firm.

## Stage 1 — repository discovery before application edits
Read applicable AGENTS.md and repository instructions. Inventory root folders, first-party modules, entry points, dependency manifests/lockfiles, framework versions, database/ORM/migrations, authentication/session handling, roles/policies, current tenant resolution, API routes, frontend routing, storage, queues, mail/calendar/payment adapters, tests, CI, environment examples, deployment scripts, and existing docs. Exclude generated/vendor artifacts from deep source review, but inspect dependency manifests and actual configuration. Do not print secrets or use real case data.

Trace representative paths end to end: sign-in, tenant selection, matter read/write, document access, background job, notification, payment/webhook, and any existing client portal. Mark absent paths as absent. Record current isolation assumptions, reusable modules, schema constraints, unsafe boundaries, missing capabilities, and limitations.

Create `Markdown/architecture/current-codebase-audit.md` with concrete existing file paths and evidence. Distinguish directly inspected behavior, inferred behavior, untested assumptions, and verified tests. Do not say that a path is secure merely because a middleware file exists. Record audit coverage by module and any unread/blocked areas.

## Stage 2 — root-level linked documentation
Preserve existing documentation and link to it. If `Markdown` already exists, merge carefully without overwriting unrelated content. Use relative links and parent navigation. Keep `Markdown/README.md` as the main index. Adopt the supplied domain hierarchy: product, architecture, database, modules, billing, practice-areas, security, operations, integrations, delivery, and research.

Add a gap map at `Markdown/delivery/implementation-gap-map.md`: requirement, current implementation path or absent, needed change, dependency, risk, acceptance evidence, owner/status. Keep detailed billing, confidentiality, security, database, and practice requirements in their own linked documents.

Record actual stack choices in `Markdown/architecture/stack-and-runtime.md`. Preserve .NET/React if that is what this repository uses; preserve other working stacks if verified. Do not migrate languages/frameworks without a documented concrete reason.

## Stage 3 — tenant architecture decision
Read `Markdown/database/tenancy-research.md` and its primary sources. Verify any source-dependent implementation detail against the selected database engine/provider. Evaluate shared tables/schema, schema per firm, database per firm, and dedicated instance. Do not conflate a legal client with a firm tenant or a schema namespace with an isolated server.

The supplied recommendation is shared application + minimal shared control plane + separate database per law firm, with one versioned product schema. It is proposed, not already implemented or legally mandated. Assess current architecture and produce an updated ADR-001 with rationale, cost/connection implications, migrations, tenant-safe foreign keys, jobs/caches/search/storage boundaries, backup/restore semantics, and a migration strategy if required. Do not blindly replace an existing sound design.

Document shared-instance backup limitations: a separate firm database does not guarantee native database-level point-in-time recovery. Include tested isolated restore/extraction and coordinated object/ledger reconciliation. No destructive production migration, infrastructure rollout, or real data import is authorized by this documentation prompt.

## Stage 4 — critical product rules
Client-visible content requires explicit publishing/audience permissions and server-side serializers. Internal strategy, conflict results, sensitive medical material, and joint-client private records must not leak through lists, detail views, counts, search, previews, exports, notification bodies, or background work.

Support controlled intake/conflict decisions and engagement acceptance. Legal deadlines preserve source, trigger facts, rule version, jurisdiction, date/time semantics, and attorney review. A scheduled consultation or successful payment does not automatically establish representation.

Separate SaaS subscriptions, firm legal invoices, operating accounts, and trust/settlement money. Use exact money types, atomic balanced ledger posting, immutable posted entries, verified webhook deduplication, concurrency-safe withdrawals, destination verification, and version-bound approvals. Native trust accounting requires validated reconciliation and firm/accountant review before production use. Prefer a reviewed integration over an incomplete imitation.

Private file uploads require quarantine, validation/scanning, immutable versions, access checks, preservation holds, and secure downloads/previews. Third-party API availability and licensing must be verified; do not invent LexisNexis Protégé API endpoints or redistribution rights.

## Stage 5 — concrete plan and safe implementation boundary
Deliver the architecture audit, linked docs, gap map, updated proposed ADR, and dependency-ordered implementation plan first. Explain observed current behavior and the exact next slice. Do not stop to ask about routine reversible documentation choices. Resolve independent work while clearly recording production-sensitive unknowns.

This initial prompt authorizes repository inspection and documentation creation, not a wholesale application rewrite. If implementation is separately authorized, work in small end-to-end slices using existing conventions, update docs alongside changes, and run appropriate builds/static checks and meaningful invariant/failure tests. Do not claim a feature complete because a screen exists.

## Required acceptance evidence
Verify all relative documentation links and agreement between the index, ADR, gap map, and current code. For implementation, cover firm/matter isolation across all surfaces, publication permissions, concurrent bookings, legal deadline provenance, duplicate/out-of-order webhooks, exact financial balances, overspending prevention, stale approval rejection, evidence holds, and one-firm recovery with current revocations preserved.

Finish with concise findings: inspected architecture, files created/updated, recommended tenancy with reasoning, existing capabilities to reuse, highest-risk gaps, validation results, unresolved blockers, and the first implementation slice. Be candid about what was not inspected or tested. Careful work takes priority over speed.
