# Research source register

[Home](../README.md) · [Tenancy research](../database/tenancy-research.md)

Checked October 12, 2026. Primary-source descriptions establish documented mechanisms, not proof that this application is safe or that every vendor uses one architecture everywhere. Older engineering posts are historical evidence. No vendor private architecture, pricing quote, firm identity verification, or production benchmark was obtained.

| ID | Source | Relevance |
|---|---|---|
| S1 | [Salesforce Platform Multitenant Architecture](https://architect.salesforce.com/docs/architect/fundamentals/guide/platform-multitenant-architecture.html) | Shared schema, OrgID, metadata/runtime enforcement |
| S2 | [Microsoft multitenant SaaS database patterns](https://learn.microsoft.com/en-us/azure/azure-sql/database/saas-tenancy-app-design-patterns?view=azuresql) | Database-per-tenant, pooled, sharded, hybrid tradeoffs |
| S3 | [AWS bridge model](https://docs.aws.amazon.com/whitepapers/latest/saas-tenant-isolation-strategies/the-bridge-model.html) | Combining shared and isolated layers |
| S4 | [Shopify pods architecture](https://shopify.engineering/a-pods-architecture-to-allow-shopify-to-scale) | Tenant grouping and datastores |
| S5 | [Shopify shard balancing](https://shopify.engineering/mysql-database-shard-balancing-terabyte-scale) | Coordinated tenant movement |
| S6 | [Slack scaling datastores with Vitess](https://slack.engineering/scaling-datastores-at-slack-with-vitess/) | Sharding evolution and workload limits |
| S7 | [FoundationDB Record Layer paper](https://arxiv.org/abs/1901.04452) | CloudKit independent logical databases; different storage model |
| S8 | [AWS managed tenant backup/recovery](https://aws.amazon.com/blogs/database/managed-database-backup-and-recovery-in-a-multi-tenant-saas-application/) | Instance/cluster snapshots and selective extraction |
| S9 | [Citus schema-based sharding](https://www.citusdata.com/blog/2023/07/18/citus-12-schema-based-sharding-for-postgres/) | Schema versus row distribution |
| S10 | [PostgreSQL schema security](https://www.postgresql.org/docs/current/ddl-schemas.html) | Namespace/search-path risks |
| S11 | [PostgreSQL row security](https://www.postgresql.org/docs/current/ddl-rowsecurity.html) | Policies, ownership/bypass considerations |
| S12 | [State Bar current rules PDF](https://www.texasbar.com/tdrpc/) | Effective March 7, 2025; confidentiality, communication, safekeeping, prospects |
| S13 | [State Bar trust accounts](https://www.texasbar.com/AM/Template.cfm?ContentID=64488&Section=Trust_Accounts&Template=%2FCM%2FHTMLDisplay.cfm) | Reconciliation guidance; older numbering must be cross-checked |
| S14 | [HHS business associates](https://www.hhs.gov/hipaa/for-professionals/privacy/guidance/business-associates/index.html) | HIPAA relationship-dependent scope |
| S15 | [OWASP ASVS index](https://cheatsheetseries.owasp.org/IndexASVS) | Security requirements mapping |
| S16 | [OWASP authorization guidance](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html) | Server-side, default-deny access |
| S17 | [OWASP file-upload guidance](https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html) | File validation, scanning, private storage |
| S18 | [Clio client portal](https://www.clio.com/features/legal-client-portal-software/) | Market reference for a unified client experience; marketing is not security evidence |
| S19 | [MySQL 8.0 CREATE DATABASE](https://dev.mysql.com/doc/refman/8.0/en/create-database.html) | `CREATE SCHEMA` is a synonym for `CREATE DATABASE`; checked 2026-10-12 |
| S20 | [MySqlConnector connection options](https://mysqlconnector.net/connection-options/) | Per-connection-string pooling; `Maximum Pool Size` default 100; checked 2026-10-12 |
| S21 | [Azure Database for MySQL Flexible Server backup and restore](https://learn.microsoft.com/en-us/azure/mysql/flexible-server/concepts-backup-restore) | PITR creates a new server; example of server-level restore semantics; hosting not yet selected; checked 2026-10-12 |
| S22 | [MySQL 8.0 point-in-time recovery](https://dev.mysql.com/doc/refman/8.0/en/point-in-time-recovery.html) | Binary-log PITR follows a full server backup; checked 2026-10-12 |

## Evidence boundaries
Database-per-firm is this blueprint's engineering recommendation; Salesforce's model does not mandate it, nor do the legal references. Provider documentation must be checked for the actual chosen engine/version/service. Texas is a planning context to confirm with the firm; no fixed jurisdictional deadline or universal legal retention period is asserted here.
