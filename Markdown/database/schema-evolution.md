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

The EF Core history above is one firm and one database. As of 2026-10-19 the Laravel application also has a control plane and a separate schema per provisioned firm ([firm tenant foundation](../architecture/firm-tenant-schema-foundation.md)). Existing matter, contact, consultation, task, and calendar tables are still in the control-plane database.

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

## Laravel migration stack additions (2026-10-18)

The current runtime stack in this repository is Laravel + Vue. The following schema changes were added in the Laravel migration timeline during this session:

| Order | When (UTC) | Migration id | Decided by | What changed |
|---|---|---|---|---|
| L1 | 2026-10-18 08:31:59 | `2026_10_18_083159_create_matter_records_table` | Founder | Added `matter_records` with UUID key, matter number uniqueness, stage/status fields, staffing actor fields, and indexes on `practice_area`, `stage`, and `status`. |
| L2 | 2026-10-18 08:31:59 | `2026_10_18_083159_create_matter_tasks_table` | Founder | Added `matter_tasks` with UUID key, foreign key `matter_record_id -> matter_records.id` (`cascadeOnDelete`), assignee/priority/status fields, due/completed timestamps, and indexes on `status` and `due_at_utc`. |
| L3 | 2026-10-18 08:31:59 | `2026_10_18_083159_create_calendar_entries_table` | Founder | Added `calendar_entries` with UUID key, entry/deadline classification, owner fields, schedule and completion metadata, reminder JSON, and indexes on `entry_type`, `scheduled_at_utc`, and `is_completed`. |
| L4 | 2026-10-19 19:00:00 | `2026_10_19_190000_add_platform_administrator_to_users_table` | Founder | Added `users.is_platform_administrator` boolean, default false. Not a foreign key. |
| L5 | 2026-10-19 19:01:00 | `2026_10_19_190100_create_firm_control_plane_tables` | Founder | Control plane only. Added `firms`, `firm_public_profiles` (`firm_id` PK and FK to `firms`), `firm_domains`, `firm_verifications` (`reviewer_user_id` nullable FK to `users`), `firm_memberships` (`firm_id` + `user_id` unique, both FKs), and `firm_provisioning_events` (`actor_user_id` nullable FK to `users`). Decision [D-20261019-10](../planning/decision-log.md). |
| L6 | 2026-10-19 19:02:00 | `database/tenant-migrations/2026_10_19_190200_create_firm_identity_table` | Founder | Tenant schema only, not the control plane. `firm_identity` with unique `firm_id` and unique `singleton`. |
| L7 | 2026-10-19 19:03:00 | `database/tenant-migrations/2026_10_19_190300_create_tenant_workspace_notes_table` | Founder | Tenant schema only. `tenant_workspace_notes` has no foreign key to the control plane. |
| L8 | 2026-10-20 01:00:00 | `2026_10_20_010000_harden_firm_domains_and_public_site_key` | Founder | Control plane only. Added domain claim/verification/revocation fields on `firm_domains` and added `firm_public_profiles.public_site_key`. |
| L9 | 2026-10-20 01:01:00 | `2026_10_20_010100_create_firm_database_credentials_table` | Founder | Control plane only. Added `firm_database_credentials` with FK `firm_id -> firms.id` for encrypted per-firm runtime DB credentials and rotation/revocation timestamps. |
| L10 | 2026-10-20 02:00:00 | `database/tenant-migrations/2026_10_20_020000_create_tenant_contacts_table` | Founder | Tenant schema only. Added tenant-scoped `contacts`. |
| L11 | 2026-10-20 02:01:00 | `database/tenant-migrations/2026_10_20_020100_create_tenant_intake_records_table` | Founder | Tenant schema only. Added tenant-scoped `intake_records`. |
| L12 | 2026-10-20 02:02:00 | `database/tenant-migrations/2026_10_20_020200_create_tenant_consultation_requests_table` | Founder | Tenant schema only. Added tenant-scoped `consultation_requests`. |
| L13 | 2026-10-20 02:03:00 | `database/tenant-migrations/2026_10_20_020300_create_tenant_matter_records_table` | Founder | Tenant schema only. Added tenant-scoped `matter_records`. |
| L14 | 2026-10-20 02:04:00 | `database/tenant-migrations/2026_10_20_020400_create_tenant_matter_tasks_table` | Founder | Tenant schema only. Added tenant-scoped `matter_tasks` with FK to tenant `matter_records`. |
| L15 | 2026-10-20 02:05:00 | `database/tenant-migrations/2026_10_20_020500_create_tenant_calendar_entries_table` | Founder | Tenant schema only. Added tenant-scoped `calendar_entries`. |
| L16 | 2026-10-22 18:58:37 | `2026_10_22_185837_create_firm_onboarding_tables` | Founder | Control plane only. Added `firms.applicant_user_id` (nullable FK to `users`) and `firms.onboarding_revision`; added `firm_onboarding_applications` (FKs to `firms` and `users`), `firm_onboarding_verifications` (FKs to `firms` and `users`), and `firm_onboarding_reviews` (FKs to `firms` and `users`) with revision-bound uniqueness to support two-person approvals. |
| L17 | 2026-10-23 16:55:00 | `2026_10_23_165500_add_platform_role_to_users_table` | Founder | Control plane only. Added `users.platform_role` (nullable, indexed) to split platform authorization into explicit roles (`platform_owner`, `platform_ops`) while preserving compatibility with legacy `is_platform_administrator` during transition. |
