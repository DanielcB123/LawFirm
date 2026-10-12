# Tenant architecture research

[Home](../README.md) · [Database](README.md) · [Decision](ADR-001-tenancy.md) · [Sources](../research/sources.md)

Research checked October 12, 2026. The company examples below describe published architecture, sometimes from older engineering articles; they are not claims about every current product or internal deployment.

## What major platforms actually document

| Platform/source | Published approach | Useful lesson for this product |
|---|---|---|
| Salesforce Architects [S1] | A platform instance uses a shared multitenant database/schema, OrgID-scoped records, metadata, and a tenant-aware runtime | Shared storage can work at scale; enforcement and operations make it safe, not the presence of a tenant column alone |
| Shopify Engineering [S4–S5] | Pods group shops with database shards and associated datastores; shard movement requires coordinated routing and migration | Grouped tenant placement limits blast radius and requires disciplined movement tooling |
| Slack Engineering [S6] | Describes adoption of Vitess after limitations of workspace sharding; supports more flexible sharding | Tenant-per-shard is not a universal permanent answer; access patterns and large customers change the design |
| Apple CloudKit research [S7] | FoundationDB Record Layer exposes independent logical databases at very large scale | A logical database is not necessarily a separately provisioned SQL server; portability of lessons is limited |
| Microsoft Azure SQL [S2] | Documents database-per-tenant, pooled, sharded, and hybrid options | Choose against isolation, restore, cost, workload, and operational requirements |
| AWS [S3, S8] | Documents silo, pool, bridge, and tenant-aware recovery | Shared and isolated layers can coexist; managed snapshots often cover an instance/cluster |
| Citus [S9] | Supports schema-based sharding as well as row-based distribution | Schema per firm is technically viable but requires engine/tooling-specific analysis |

Do not recreate Salesforce's flexible-column metadata storage for core legal or financial data. Normalized, typed records and explicit financial invariants are more appropriate here. Configurable forms can use versioned metadata without turning all matter data into arbitrary strings.

## Comparing the alternatives

| Criterion | Shared tables/schema | Schema per firm | Database per firm | Instance per firm |
|---|---|---|---|---|
| Baseline resource cost | Generally lowest | Shared compute | More catalog/connection overhead; shared compute possible | Generally highest |
| Data access boundary | Tenant predicates plus optional RLS | Namespace plus grants | Database grants and routed connections | Stronger infrastructure boundary |
| Accidental wrong query | Can expose another firm if defenses fail | Wrong search path/grants can expose another firm | Wrong routing/credentials can still expose another firm | Wrong routing/credentials still possible |
| Product schema rollout | One rollout per shard | Fan-out across namespaces | Fan-out across databases | Fan-out plus infrastructure operations |
| One-firm export/restore | Requires carefully scoped extraction | Namespace extraction is possible | Logical extraction is cleaner; platform PITR semantics vary | Usually most independent |
| Noisy neighbors | Shared | Shared | Still shared if on same server/pool | Reduced with reserved independent resources |
| Large tenant relocation | Scoped relational graph and files | Move schema plus files | Move database plus files/routing | Move infrastructure and dependencies |
| Operational burden | Low initially; selective recovery complex | Medium to high | Medium to high | Highest |
| Best fit here | Future economical pool if proven | Conditional engine-specific option | Recommended initial firm data boundary | Optional large-firm isolation tier |

These are qualitative engineering assessments, not benchmark results or vendor pricing. A schema namespace is not automatically a security boundary. PostgreSQL warns about trusting schemas on search_path [S10]; RLS has bypass and ownership considerations [S11]. Shared models require policies for reads and writes, tenant-safe foreign keys, and database-role testing.

## Recommendation for the actual starting conditions
The starting customer count is small, the unit of tenancy is a firm, records are confidential, financial correctness matters, and single-firm recovery/offboarding is valuable. Prefer database per firm behind a shared app and control plane. This is our recommendation, not a claim that legal rules mandate separate databases.

Do not introduce distributed shards, Vitess, Citus, or Salesforce-style metadata engines on day one. Keep routing and tenant context explicit so placement can change later. Additional firms should reuse the same schema and application release, not accumulate bespoke schema forks.

## Verification against the actual engine (MySQL 8.0)
Checked 2026-10-12 against primary documentation for the engine used by this repository:

- `CREATE SCHEMA` is a synonym for `CREATE DATABASE` [S19]. The "schema per firm" column above collapses into "database per firm" on MySQL. The PostgreSQL `search_path` and RLS cautions [S10–S11] do not apply directly.
- MySQL 8.0 lacks a native row-security feature equivalent to PostgreSQL RLS. A pooled shared-table design would rely on application-level filters only. This is an engineering assessment to re-check if a pooled tier is proposed.
- MySqlConnector (used by Pomelo) keeps a connection pool per distinct connection string, with a default `Maximum Pool Size` of 100 [S20]. Per-firm credentials therefore multiply pools; size them explicitly.
- Managed MySQL point-in-time restore creates a new server from backups [S21], and binary-log PITR is server-wide [S22]. Per-firm recovery means restore to an isolated server, then extract one database.

The resulting decision is recorded in [ADR-001](ADR-001-tenancy.md).

## Required validation before adopting
1. Inventory current engine and tenancy behavior; identify migration risk.
2. Compare operating the first firm, ten firms, and one hundred firms using actual provider quotas and current pricing. These are planning scenarios, not sales forecasts.
3. Model active connections, pool count, job throughput, file storage, backup overhead, and busiest-firm workload.
4. Benchmark list/search/document and billing workflows with synthetic realistic volumes; set targets first.
5. Rehearse one-firm restore from actual provider backups, including objects and ledger reconciliation.
6. Demonstrate that wrong tenant IDs, domains, jobs, caches, exports, and reused connections do not cross boundaries.
7. Record cost and operational ownership in ADR-001. Revisit when measured constraints justify it.

## Diagram tools
Maintain a logical ERD, physical tenancy layout, trust-ledger relationships, request authorization flow, and restore sequence. Mermaid stored in Git is the initial source. Lucidchart is optional for collaborative workshops; link a versioned export and decision date back into the repository. Never place real client names, case narratives, secrets, or production identifiers into diagram SaaS without the firm's approved data policy. No Lucidchart account access or diagram creation is assumed here.
