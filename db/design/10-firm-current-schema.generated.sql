CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `audit_events` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Action` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ObjectType` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ObjectId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ActorDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `CorrelationId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `MetadataJson` longtext CHARACTER SET utf8mb4 NOT NULL,
    `OccurredAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_audit_events` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `calendar_entries` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `EntryType` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `DeadlineType` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `OwnerActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `OwnerDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `BackupActorId` varchar(128) CHARACTER SET utf8mb4 NULL,
    `BackupDisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `ScheduledAtUtc` datetime(6) NOT NULL,
    `IsAllDay` tinyint(1) NOT NULL,
    `TimeZone` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `MatterReference` varchar(128) CHARACTER SET utf8mb4 NULL,
    `SourceReference` varchar(200) CHARACTER SET utf8mb4 NULL,
    `VerifiedAtUtc` datetime(6) NULL,
    `VerifiedByActorId` longtext CHARACTER SET utf8mb4 NULL,
    `VerifiedByDisplayName` longtext CHARACTER SET utf8mb4 NULL,
    `OverrideReason` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `IsAcknowledged` tinyint(1) NOT NULL,
    `AcknowledgedAtUtc` datetime(6) NULL,
    `IsCompleted` tinyint(1) NOT NULL,
    `CompletedAtUtc` datetime(6) NULL,
    `RemindersJson` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_calendar_entries` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `conflict_checks` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Query` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `RequestedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `RequestedByActorDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `MatchSnapshotJson` longtext CHARACTER SET utf8mb4 NOT NULL,
    `RequestedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_conflict_checks` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `consultation_requests` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `FullName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Email` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `Phone` varchar(64) CHARACTER SET utf8mb4 NULL,
    `PracticeArea` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Message` varchar(3000) CHARACTER SET utf8mb4 NULL,
    `PreferredAtUtc` datetime(6) NOT NULL,
    `TimeZone` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `AssignedToActorId` varchar(128) CHARACTER SET utf8mb4 NULL,
    `AssignedToDisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `InternalNotes` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    `UpdatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_consultation_requests` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `intake_records` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `PracticeArea` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `IntakeType` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Stage` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `ProspectiveClientName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ProspectiveClientEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
    `ProspectiveClientPhone` varchar(64) CHARACTER SET utf8mb4 NULL,
    `OwnerActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `OwnerDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `NextAction` varchar(200) CHARACTER SET utf8mb4 NULL,
    `NextActionDueAtUtc` datetime(6) NULL,
    `ReferralSource` varchar(128) CHARACTER SET utf8mb4 NULL,
    `ConflictCheckId` char(36) COLLATE ascii_general_ci NULL,
    `ApprovedConflictDecisionId` char(36) COLLATE ascii_general_ci NULL,
    `DeclineReason` varchar(500) CHARACTER SET utf8mb4 NULL,
    `NotesSummary` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `CreatedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_intake_records` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `matters` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `MatterNumber` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `PracticeArea` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Stage` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `ResponsibleAttorneyActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ResponsibleAttorneyDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ParalegalActorId` varchar(128) CHARACTER SET utf8mb4 NULL,
    `ParalegalDisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `LinkedIntakeRecordId` char(36) COLLATE ascii_general_ci NULL,
    `CreatedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_matters` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `parties` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `PartyType` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `DisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `PrimaryEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
    `PrimaryPhone` varchar(64) CHARACTER SET utf8mb4 NULL,
    `IsRestricted` tinyint(1) NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_parties` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `conflict_decisions` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ConflictCheckId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Decision` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Rationale` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
    `DecidedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `DecidedByActorDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `MatchSnapshotJson` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DecidedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_conflict_decisions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_conflict_decisions_conflict_checks_ConflictCheckId` FOREIGN KEY (`ConflictCheckId`) REFERENCES `conflict_checks` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `intake_notes` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `IntakeRecordId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Note` varchar(3000) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedByDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_intake_notes` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_intake_notes_intake_records_IntakeRecordId` FOREIGN KEY (`IntakeRecordId`) REFERENCES `intake_records` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `intake_stage_history` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `IntakeRecordId` char(36) COLLATE ascii_general_ci NOT NULL,
    `FromStage` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `ToStage` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `ChangedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ChangedByDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ChangeReason` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `ChangedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_intake_stage_history` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_intake_stage_history_intake_records_IntakeRecordId` FOREIGN KEY (`IntakeRecordId`) REFERENCES `intake_records` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `matter_documents` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `MatterRecordId` char(36) COLLATE ascii_general_ci NOT NULL,
    `FileName` varchar(240) CHARACTER SET utf8mb4 NOT NULL,
    `DocumentType` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
    `StorageKey` varchar(512) CHARACTER SET utf8mb4 NULL,
    `UploadedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `UploadedByDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `UploadedAtUtc` datetime(6) NOT NULL,
    `ReviewStatus` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `ReviewNotes` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `ReviewedByActorId` varchar(128) CHARACTER SET utf8mb4 NULL,
    `ReviewedByDisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `ReviewedAtUtc` datetime(6) NULL,
    CONSTRAINT `PK_matter_documents` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_matter_documents_matters_MatterRecordId` FOREIGN KEY (`MatterRecordId`) REFERENCES `matters` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `matter_stage_history` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `MatterRecordId` char(36) COLLATE ascii_general_ci NOT NULL,
    `FromStage` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `ToStage` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `ChangedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ChangedByDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ChangeReason` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `ChangedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_matter_stage_history` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_matter_stage_history_matters_MatterRecordId` FOREIGN KEY (`MatterRecordId`) REFERENCES `matters` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `matter_tasks` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `MatterRecordId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Title` varchar(240) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(4000) CHARACTER SET utf8mb4 NULL,
    `Status` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Priority` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `AssigneeActorId` varchar(128) CHARACTER SET utf8mb4 NULL,
    `AssigneeDisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `DueAtUtc` datetime(6) NULL,
    `IsDeadlineVerified` tinyint(1) NOT NULL,
    `DeadlineVerifiedAtUtc` datetime(6) NULL,
    `DeadlineVerifiedByActorId` varchar(128) CHARACTER SET utf8mb4 NULL,
    `DeadlineVerifiedByDisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `CompletedAtUtc` datetime(6) NULL,
    `CreatedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedByDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_matter_tasks` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_matter_tasks_matters_MatterRecordId` FOREIGN KEY (`MatterRecordId`) REFERENCES `matters` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `matter_timeline_actions` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `MatterRecordId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ActionType` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
    `Summary` varchar(400) CHARACTER SET utf8mb4 NOT NULL,
    `Details` varchar(4000) CHARACTER SET utf8mb4 NULL,
    `OccurredAtUtc` datetime(6) NOT NULL,
    `CreatedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedByDisplayName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_matter_timeline_actions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_matter_timeline_actions_matters_MatterRecordId` FOREIGN KEY (`MatterRecordId`) REFERENCES `matters` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `party_aliases` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `PartyId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Alias` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `AliasType` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_party_aliases` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_party_aliases_parties_PartyId` FOREIGN KEY (`PartyId`) REFERENCES `parties` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `party_relationships` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `FromPartyId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ToPartyId` char(36) COLLATE ascii_general_ci NOT NULL,
    `RelationshipType` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_party_relationships` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_party_relationships_parties_FromPartyId` FOREIGN KEY (`FromPartyId`) REFERENCES `parties` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_party_relationships_parties_ToPartyId` FOREIGN KEY (`ToPartyId`) REFERENCES `parties` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_audit_events_OccurredAtUtc` ON `audit_events` (`OccurredAtUtc`);

CREATE INDEX `IX_calendar_entries_OwnerActorId` ON `calendar_entries` (`OwnerActorId`);

CREATE INDEX `IX_calendar_entries_ScheduledAtUtc` ON `calendar_entries` (`ScheduledAtUtc`);

CREATE INDEX `IX_conflict_decisions_ConflictCheckId` ON `conflict_decisions` (`ConflictCheckId`);

CREATE INDEX `IX_consultation_requests_CreatedAtUtc` ON `consultation_requests` (`CreatedAtUtc`);

CREATE INDEX `IX_consultation_requests_PreferredAtUtc` ON `consultation_requests` (`PreferredAtUtc`);

CREATE INDEX `IX_consultation_requests_Status` ON `consultation_requests` (`Status`);

CREATE INDEX `IX_intake_notes_IntakeRecordId` ON `intake_notes` (`IntakeRecordId`);

CREATE INDEX `IX_intake_records_OwnerActorId` ON `intake_records` (`OwnerActorId`);

CREATE INDEX `IX_intake_records_PracticeArea` ON `intake_records` (`PracticeArea`);

CREATE INDEX `IX_intake_records_Stage` ON `intake_records` (`Stage`);

CREATE INDEX `IX_intake_stage_history_IntakeRecordId` ON `intake_stage_history` (`IntakeRecordId`);

CREATE INDEX `IX_matter_documents_DocumentType` ON `matter_documents` (`DocumentType`);

CREATE INDEX `IX_matter_documents_MatterRecordId` ON `matter_documents` (`MatterRecordId`);

CREATE INDEX `IX_matter_stage_history_MatterRecordId` ON `matter_stage_history` (`MatterRecordId`);

CREATE INDEX `IX_matter_tasks_DueAtUtc` ON `matter_tasks` (`DueAtUtc`);

CREATE INDEX `IX_matter_tasks_MatterRecordId` ON `matter_tasks` (`MatterRecordId`);

CREATE INDEX `IX_matter_timeline_actions_MatterRecordId` ON `matter_timeline_actions` (`MatterRecordId`);

CREATE INDEX `IX_matter_timeline_actions_OccurredAtUtc` ON `matter_timeline_actions` (`OccurredAtUtc`);

CREATE UNIQUE INDEX `IX_matters_MatterNumber` ON `matters` (`MatterNumber`);

CREATE INDEX `IX_matters_ResponsibleAttorneyActorId` ON `matters` (`ResponsibleAttorneyActorId`);

CREATE INDEX `IX_matters_Stage` ON `matters` (`Stage`);

CREATE INDEX `IX_parties_DisplayName` ON `parties` (`DisplayName`);

CREATE UNIQUE INDEX `IX_party_aliases_PartyId_Alias` ON `party_aliases` (`PartyId`, `Alias`);

CREATE UNIQUE INDEX `IX_party_relationships_FromPartyId_ToPartyId_RelationshipType` ON `party_relationships` (`FromPartyId`, `ToPartyId`, `RelationshipType`);

CREATE INDEX `IX_party_relationships_ToPartyId` ON `party_relationships` (`ToPartyId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261012050714_Baseline', '8.0.13');

COMMIT;

START TRANSACTION;

CREATE TABLE `matter_access_grants` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `MatterRecordId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `GrantType` varchar(16) CHARACTER SET utf8mb4 NOT NULL,
    `GrantedByActorId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `GrantedAtUtc` datetime(6) NOT NULL,
    `RevokedByActorId` varchar(128) CHARACTER SET utf8mb4 NULL,
    `RevokedAtUtc` datetime(6) NULL,
    `ActiveMarker` int AS ((CASE WHEN `RevokedAtUtc` IS NULL THEN 1 ELSE NULL END)) STORED NULL,
    CONSTRAINT `PK_matter_access_grants` PRIMARY KEY (`Id`),
    CONSTRAINT `CK_matter_access_grants_GrantType` CHECK (GrantType IN ('Client', 'Staff')),
    CONSTRAINT `FK_matter_access_grants_matters_MatterRecordId` FOREIGN KEY (`MatterRecordId`) REFERENCES `matters` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_matter_access_grants_ActorId_RevokedAtUtc` ON `matter_access_grants` (`ActorId`, `RevokedAtUtc`);

CREATE UNIQUE INDEX `IX_matter_access_grants_MatterRecordId_ActorId_ActiveMarker` ON `matter_access_grants` (`MatterRecordId`, `ActorId`, `ActiveMarker`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261012050741_MatterAccessGrants', '8.0.13');

COMMIT;

