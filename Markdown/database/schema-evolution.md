# Schema evolution

[Database](README.md) · [UML class diagrams](uml-class-diagrams.md) · [Data dictionary and ERD](data-dictionary.md) · [Applied SQL migrations](../../backend/Infrastructure/Persistence/Migrations) · [Design SQL](../../db/design/README.md) · [Planning log](../planning/decision-log.md)

This is the ordered history of the relational database. The product decision log does not replace it. [Evolutionary database design](https://www.martinfowler.com/articles/evodb.html) treats each schema change as a versioned SQL migration, applied in order, with the database itself recording which migrations have run (`__EFMigrationsHistory`).

## Engine

| Term | What this repository uses |
|---|---|
| DBMS / RDBMS | MySQL 8.0. The server version used in tests and design validation is 8.0.36. In MySQL, `SCHEMA` and `DATABASE` are the same thing. |
| Access from the app | EF Core 8 with Pomelo. The model is `LawFirmDbContext`. |
| SQL that has been applied to the application | EF Core migrations in `backend/Infrastructure/Persistence/Migrations/`. |
| SQL that is design only | `db/design/`. It was checked on a disposable MySQL 8.0.36. It is not the application schema. |
| Current picture of classes and foreign keys | [UML class diagrams](uml-class-diagrams.md) |
| Designed future picture | Entity-relationship diagrams in the [data dictionary](data-dictionary.md) |

One firm, one database, no tenant column. That is the implemented shape. Database-per-firm is designed in ADR-002 and is not implemented.

## Order

The `When` value is the timestamp inside the EF migration id. Those ids are UTC. A local offset was not stored on the migration, so these rows are not converted to EDT. They are still earlier than every 2026-10-13 planning entry.

| Order | When (UTC) | Migration id | Decided by | What changed |
|---|---|---|---|---|
| 1 | 2026-10-12 05:07:14 | `20261012050714_Baseline` | Unassigned. Recorded with the migration baseline. | First EF migration. Creates the tables that existed after the old `EnsureCreated` path: parties, aliases, relationships, conflict checks and decisions, audit events, calendar entries, intake records, stage history, notes, matters, matter stage history, tasks, documents, timeline actions, consultation requests. |
| 2 | 2026-10-12 05:07:41 | `20261012050741_MatterAccessGrants` | Unassigned. Recorded with matter authorization (F1). | Adds `matter_access_grants`, with a foreign key to `matters` and the active-grant uniqueness pattern. |

Nothing in this table was applied after 2026-10-12 05:07:41 UTC. The 2026-10-13 workspace screens did not change the schema (decision D-20261013-2).

## How to add the next change

1. Change the EF model and add a migration. Do not edit an applied migration.
2. Append a row to the table above in timestamp order. Use the migration id's UTC time. Name who decided. Say which classes and foreign keys moved.
3. Update [uml-class-diagrams.md](uml-class-diagrams.md) so the class diagram matches the migration. If the change is design-only SQL in `db/design/`, update the ERD in the data dictionary and say in this file that it is not applied.
4. If the change is also a product choice, add a planning-log entry and put its id on this row.

## Columns that look like foreign keys and are not

These are in the applied schema. They are called out so a later diagram does not draw an association the database does not enforce.

| Column | Looks like | Actual constraint |
|---|---|---|
| `intake_records.ConflictCheckId` | conflict check | Guid column, no foreign key |
| `intake_records.ApprovedConflictDecisionId` | conflict decision | Guid column, no foreign key |
| `matters.LinkedIntakeRecordId` | intake record | Guid column, no foreign key |
| `calendar_entries.MatterReference` | matter | Free text, not a foreign key |
