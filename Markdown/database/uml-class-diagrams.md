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
