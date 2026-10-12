# Database architecture

[Home](../README.md) · [Research](tenancy-research.md) · [ADR-001](ADR-001-tenancy.md) · [Data model](data-model.md) · [Provisioning](provisioning-and-migrations.md)

## Vocabulary
**Tenant = firm. Client = person or organization represented by that firm. Matter = legal work container.** Never create a database per represented client.

A shared schema means firms share tables, separated by tenant keys and authorization. Schema per firm means separate namespaces/tables within a database on engines such as PostgreSQL. Database per firm means distinct databases with the same product schema version. Dedicated instance means a distinct database server/cluster or resource boundary. A separate database on a shared server does not guarantee isolated CPU or independent infrastructure snapshots.

MySQL uses schema/database terminology differently from PostgreSQL and SQL Server. **This repository uses MySQL 8.0** (Pomelo EF Core), where `SCHEMA` is a synonym for `DATABASE` [S19]. "Schema per firm" and "database per firm" therefore mean the same thing here.

## Current state
One MySQL database, 16 tables defined in `backend/Infrastructure/Persistence/LawFirmDbContext.cs`. No tenant columns, no migrations (`EnsureCreated` + raw DDL), GUID `char(36)` keys, `datetime(6)` instants, no money columns yet. Details are in the [codebase audit](../architecture/current-codebase-audit.md).

## Proposed responsibility split

| Storage | Contents |
|---|---|
| Control database | Firm registry, domain mapping, identity references, membership routing, SaaS plans, provisioning jobs |
| Firm database | Clients, matters, access grants, documents metadata, workflow, legal billing, trust ledger, audit events |
| Private object store | Original evidence, versions, derived previews, signed documents |
| Derived search | Scoped searchable projections; rebuildable and permission filtered |
| Secrets manager | Database and provider secrets, encryption references; no plaintext credentials in catalog |

Financial amounts use exact decimal or integer minor units with currency. Use UTC timestamps for instants plus original zone/rule context for appointments and legal deadlines. Date-only legal deadlines must not become midnight UTC accidentally.

Retain stable external and internal identifiers, enforce referential integrity, and record schema versions. Backups must include relationships between SQL records, object versions, and audit evidence. See the recovery document before equating database separation with turnkey per-firm restore.
