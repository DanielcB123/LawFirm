# UML class diagrams

[Database](README.md) · [Schema evolution](schema-evolution.md) · [ERD and data dictionary](data-dictionary.md) · [EF model](../../backend/Infrastructure/Persistence/LawFirmDbContext.cs)

Developers use a UML class diagram to design a relational schema: a class becomes a table, an attribute becomes a column, and an association becomes a foreign key. This file is that map for the **applied** MySQL schema. The entity-relationship diagrams in the [data dictionary](data-dictionary.md) are the **designed** multi-firm model, including tables that are not created yet. When the two disagree, the class diagram plus [schema evolution](schema-evolution.md) describe the database the application uses.

## Mapping rules used here

| UML | Relational | In this codebase |
|---|---|---|
| Class | Table | EF `ToTable` on the class in `LawFirmDbContext` |
| Attribute | Column | The property name is the column name unless EF is told otherwise |
| Association, one to many | Foreign key on the many side | `HasForeignKey` in `LawFirmDbContext` |
| Association, many to many | A class of its own | `PartyRelationship` is a table with two foreign keys, not a hidden join |
| Composition, cascade delete | `ON DELETE CASCADE` | Marked on the association |
| Aggregation that must survive | `ON DELETE RESTRICT` | Party relationships |
| Attribute with no association | A column that is not a foreign key | Listed under "Not associations" in the schema evolution file |

Identifiers are `char(36)` GUIDs. Instants are `datetime(6)` UTC. The diagrams show the attributes that identify the row and the association. The full column list is the EF configuration, not a second copy kept here.

## People and conflicts

```mermaid
classDiagram
  class Party {
    <<table parties>>
    Guid Id
    string PartyType
    string DisplayName
    string PrimaryEmail
    string PrimaryPhone
    bool IsRestricted
  }
  class PartyAlias {
    <<table party_aliases>>
    Guid Id
    Guid PartyId
    string Alias
    string AliasType
  }
  class PartyRelationship {
    <<table party_relationships>>
    Guid Id
    Guid FromPartyId
    Guid ToPartyId
    string RelationshipType
  }
  class ConflictCheck {
    <<table conflict_checks>>
    Guid Id
    string Query
    string RequestedByActorId
  }
  class ConflictDecision {
    <<table conflict_decisions>>
    Guid Id
    Guid ConflictCheckId
    string Decision
  }
  Party "1" --> "*" PartyAlias : PartyId CASCADE
  Party "1" --> "*" PartyRelationship : FromPartyId RESTRICT
  Party "1" --> "*" PartyRelationship : ToPartyId RESTRICT
  ConflictCheck "1" --> "*" ConflictDecision : ConflictCheckId CASCADE
```

`Party` is the stored contact. The signed-in screen calls it a Contact (decision D-20261013-1). There is no second contacts table.

## Intake, matters, calendar, audit

```mermaid
classDiagram
  class IntakeRecord {
    <<table intake_records>>
    Guid Id
    string Stage
    Guid ConflictCheckId
    Guid ApprovedConflictDecisionId
  }
  class IntakeStageHistory {
    <<table intake_stage_history>>
    Guid Id
    Guid IntakeRecordId
  }
  class IntakeNote {
    <<table intake_notes>>
    Guid Id
    Guid IntakeRecordId
  }
  class MatterRecord {
    <<table matters>>
    Guid Id
    string MatterNumber
    Guid LinkedIntakeRecordId
  }
  class MatterStageHistory {
    <<table matter_stage_history>>
    Guid Id
    Guid MatterRecordId
  }
  class MatterTask {
    <<table matter_tasks>>
    Guid Id
    Guid MatterRecordId
  }
  class MatterDocument {
    <<table matter_documents>>
    Guid Id
    Guid MatterRecordId
    string FileName
    string StorageKey
  }
  class MatterTimelineAction {
    <<table matter_timeline_actions>>
    Guid Id
    Guid MatterRecordId
  }
  class MatterAccessGrant {
    <<table matter_access_grants>>
    Guid Id
    Guid MatterRecordId
    string ActorId
    string GrantType
  }
  class ConsultationRequest {
    <<table consultation_requests>>
    Guid Id
    string Status
  }
  class CalendarEntry {
    <<table calendar_entries>>
    Guid Id
    string MatterReference
  }
  class AuditEvent {
    <<table audit_events>>
    Guid Id
    string ObjectType
    string ObjectId
  }
  IntakeRecord "1" --> "*" IntakeStageHistory : IntakeRecordId CASCADE
  IntakeRecord "1" --> "*" IntakeNote : IntakeRecordId CASCADE
  MatterRecord "1" --> "*" MatterStageHistory : MatterRecordId CASCADE
  MatterRecord "1" --> "*" MatterTask : MatterRecordId CASCADE
  MatterRecord "1" --> "*" MatterDocument : MatterRecordId CASCADE
  MatterRecord "1" --> "*" MatterTimelineAction : MatterRecordId CASCADE
  MatterRecord "1" --> "*" MatterAccessGrant : MatterRecordId CASCADE
```

`ConsultationRequest`, `CalendarEntry`, and `AuditEvent` have no foreign key to another class. `IntakeRecord.ConflictCheckId`, `IntakeRecord.ApprovedConflictDecisionId`, and `MatterRecord.LinkedIntakeRecordId` are attributes only. `CalendarEntry.MatterReference` is text. Drawing those as associations would claim a constraint MySQL does not have. See [schema evolution](schema-evolution.md).

`MatterDocument` is metadata. The class has no version collection and no binary. That matches gap G32.

## When this file changes

Update it in the same change as the EF migration, then add a row to [schema evolution](schema-evolution.md). Do not paste the future billing or trust classes here until a migration creates their tables. Those designed tables stay on the ERD.

## Laravel migration stack snapshot (2026-10-18)

The active migration stack in this repository is Laravel. The following class diagram captures the newly added workspace-parity entities implemented in this session:

```mermaid
classDiagram
  class MatterRecord {
    <<table matter_records>>
    uuid Id
    string MatterNumber
    string Title
    string PracticeArea
    string Stage
    string Status
  }

  class MatterTask {
    <<table matter_tasks>>
    uuid Id
    uuid MatterRecordId
    string Title
    string Status
    string Priority
    datetime DueAtUtc
  }

  class CalendarEntry {
    <<table calendar_entries>>
    uuid Id
    string Title
    string EntryType
    string DeadlineType
    datetime ScheduledAtUtc
    bool IsCompleted
    string MatterReference
  }

  MatterRecord "1" --> "*" MatterTask : MatterRecordId CASCADE
```

## Laravel control plane and tenant schema (2026-10-19)

Applied by the migrations in [schema evolution](schema-evolution.md) rows L4–L7. `firm_identity` and `tenant_workspace_notes` are created in each firm schema, not in the control-plane database.

```mermaid
classDiagram
  class User {
    <<table users>>
    bigint id
    bool is_platform_administrator
    string platform_role
  }
  class Firm {
    <<table firms>>
    uuid id
    string lifecycle_status
    string provisioning_status
    string schema_name
  }
  class FirmPublicProfile {
    <<table firm_public_profiles>>
    uuid firm_id
  }
  class FirmDomain {
    <<table firm_domains>>
    uuid id
    uuid firm_id
    string hostname
  }
  class FirmVerification {
    <<table firm_verifications>>
    uuid id
    uuid firm_id
    bigint reviewer_user_id
  }
  class FirmMembership {
    <<table firm_memberships>>
    uuid id
    uuid firm_id
    bigint user_id
    string role
  }
  class FirmProvisioningEvent {
    <<table firm_provisioning_events>>
    uuid id
    uuid firm_id
    bigint actor_user_id
  }
  class FirmIdentity {
    <<table firm_identity tenant schema>>
    bigint id
    uuid firm_id
  }
  class TenantWorkspaceNote {
    <<table tenant_workspace_notes tenant schema>>
    uuid id
  }
  Firm "1" --> "1" FirmPublicProfile : firm_id CASCADE
  Firm "1" --> "*" FirmDomain : firm_id CASCADE
  Firm "1" --> "*" FirmVerification : firm_id CASCADE
  Firm "1" --> "*" FirmMembership : firm_id CASCADE
  Firm "1" --> "*" FirmProvisioningEvent : firm_id CASCADE
  User "1" --> "*" FirmMembership : user_id CASCADE
  User "1" --> "*" FirmVerification : reviewer_user_id SET NULL
  User "1" --> "*" FirmProvisioningEvent : actor_user_id SET NULL
```

`FirmIdentity.firm_id` matches `Firm.id` in application code after the connection opens. It is not a database foreign key across schemas.

## Laravel control-plane and tenant additions (2026-10-20)

Applied by schema-evolution rows L8-L15.

```mermaid
classDiagram
  class FirmDomain {
    <<table firm_domains>>
    uuid id
    string hostname
    datetime claimed_at
    datetime verified_at
    datetime revoked_at
    string verification_token_hash
  }
  class FirmDatabaseCredential {
    <<table firm_database_credentials>>
    uuid id
    uuid firm_id
    string username
    text password_encrypted
    datetime rotated_at
    datetime revoked_at
  }
  class FirmPublicProfile {
    <<table firm_public_profiles>>
    uuid firm_id
    string public_site_key
  }
  class TenantContact {
    <<table contacts tenant schema>>
    bigint id
  }
  class TenantIntakeRecord {
    <<table intake_records tenant schema>>
    uuid id
  }
  class TenantConsultationRequest {
    <<table consultation_requests tenant schema>>
    uuid id
  }
  class TenantMatterRecord {
    <<table matter_records tenant schema>>
    uuid id
    string matter_number
  }
  class TenantMatterTask {
    <<table matter_tasks tenant schema>>
    uuid id
    uuid matter_record_id
  }
  class TenantCalendarEntry {
    <<table calendar_entries tenant schema>>
    uuid id
  }

  Firm "1" --> "*" FirmDatabaseCredential : firm_id CASCADE
  TenantMatterRecord "1" --> "*" TenantMatterTask : matter_record_id CASCADE
```

## Laravel onboarding control-plane additions (2026-10-22 and 2026-10-23)

Applied by schema-evolution row L16.

```mermaid
classDiagram
  class User {
    <<table users>>
    bigint id
  }
  class Firm {
    <<table firms>>
    uuid id
    bigint applicant_user_id
    int onboarding_revision
  }
  class FirmOnboardingApplication {
    <<table firm_onboarding_applications>>
    uuid id
    uuid firm_id
    bigint applicant_user_id
    int revision
    bool dev_fast_tracked
  }
  class FirmOnboardingVerification {
    <<table firm_onboarding_verifications>>
    uuid id
    uuid firm_id
    int revision
    string verification_type
    string status
    string method
    string notes
    json evidence_payload
    bigint reviewer_user_id
  }
  class FirmOnboardingReview {
    <<table firm_onboarding_reviews>>
    uuid id
    uuid firm_id
    int revision
    bigint reviewer_user_id
    string decision
  }

  User "1" --> "*" Firm : applicant_user_id SET NULL
  Firm "1" --> "1" FirmOnboardingApplication : firm_id CASCADE
  User "1" --> "*" FirmOnboardingApplication : applicant_user_id CASCADE
  Firm "1" --> "*" FirmOnboardingVerification : firm_id CASCADE
  User "1" --> "*" FirmOnboardingVerification : reviewer_user_id SET NULL
  Firm "1" --> "*" FirmOnboardingReview : firm_id CASCADE
  User "1" --> "*" FirmOnboardingReview : reviewer_user_id CASCADE
```
