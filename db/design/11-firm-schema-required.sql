-- MatterForge firm database: IMMEDIATELY REQUIRED additions. DESIGN ARTIFACT, NOT A MIGRATION.
-- Applies on top of 10-firm-current-schema.generated.sql (the real EF migration output).
-- These structures are prerequisites for the first tenancy steps (T1-T4 in the plan).
-- When implemented they must be created by EF Core migrations, not by running this file.
--
-- No FirmId/tenant_id column on firm-owned rows: a database belongs to exactly one firm,
-- so a cross-firm reference cannot exist inside it. The firm_identity row is the in-database
-- assertion of ownership, checked by the application after opening every connection lease.

-- Exactly one row. Written by provisioning with the migration user; runtime user: SELECT only.
CREATE TABLE firm_identity (
    Singleton        tinyint      NOT NULL DEFAULT 1,
    FirmId           char(36)     COLLATE ascii_general_ci NOT NULL,
    FirmSlug         varchar(63)  COLLATE ascii_general_ci NOT NULL,
    PlacementRoutingVersion int   NOT NULL,
    ProvisionedAtUtc datetime(6)  NOT NULL,
    CONSTRAINT PK_firm_identity PRIMARY KEY (Singleton),
    CONSTRAINT CK_firm_identity_Singleton CHECK (Singleton = 1)
) CHARACTER SET utf8mb4;

-- Firm-local directory of people who can act in this firm. ActorId is the stable key used by
-- every existing *ActorId column (today demo ids such as u-attorney-001; later the
-- control-plane platform_identities.Id rendered as text). Display names are firm data.
CREATE TABLE firm_users (
    ActorId          varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    IdentityId       char(36)     COLLATE ascii_general_ci NULL,
    UserKind         varchar(16)  COLLATE ascii_general_ci NOT NULL,      -- Staff | Client | Representative
    DisplayName      varchar(200) NOT NULL,
    RoleKey          varchar(32)  COLLATE ascii_general_ci NOT NULL,      -- Attorney | Paralegal | Admin | Client ...
    Status           varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Active',
    ClientPartyId    char(36)     COLLATE ascii_general_ci NULL,          -- Client/Representative only
    CreatedAtUtc     datetime(6)  NOT NULL,
    DeactivatedAtUtc datetime(6)  NULL,
    RowVersion       int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_firm_users PRIMARY KEY (ActorId),
    CONSTRAINT UX_firm_users_IdentityId UNIQUE (IdentityId),
    CONSTRAINT FK_firm_users_parties FOREIGN KEY (ClientPartyId) REFERENCES parties (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_firm_users_Kind CHECK (UserKind IN ('Staff','Client','Representative')),
    CONSTRAINT CK_firm_users_Status CHECK (Status IN ('Active','Suspended','Deactivated')),
    CONSTRAINT CK_firm_users_ClientParty CHECK (UserKind = 'Staff' OR ClientPartyId IS NOT NULL)
) CHARACTER SET utf8mb4;

-- Replaces max+1 matter numbering. Allocation: SELECT ... FOR UPDATE, increment, commit.
CREATE TABLE matter_number_sequences (
    SequenceKey  varchar(32) COLLATE ascii_general_ci NOT NULL,   -- e.g. '2026'
    NextValue    int         NOT NULL,
    UpdatedAtUtc datetime(6) NOT NULL,
    CONSTRAINT PK_matter_number_sequences PRIMARY KEY (SequenceKey),
    CONSTRAINT CK_matter_number_sequences_Next CHECK (NextValue > 0)
) CHARACTER SET utf8mb4;

-- Staff assignment with explicit role. Expand step: coexists with matters.ResponsibleAttorneyActorId
-- and ParalegalActorId, which stay as denormalized display columns until a contract migration.
CREATE TABLE matter_assignments (
    Id              char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId  char(36)     COLLATE ascii_general_ci NOT NULL,
    ActorId         varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    AssignmentRole  varchar(24)  COLLATE ascii_general_ci NOT NULL,
    AssignedByActorId varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    AssignedAtUtc   datetime(6)  NOT NULL,
    EndedAtUtc      datetime(6)  NULL,
    ActiveMarker    tinyint AS (CASE WHEN EndedAtUtc IS NULL THEN 1 ELSE NULL END) STORED,
    CONSTRAINT PK_matter_assignments PRIMARY KEY (Id),
    CONSTRAINT FK_matter_assignments_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE CASCADE,
    CONSTRAINT FK_matter_assignments_firm_users FOREIGN KEY (ActorId) REFERENCES firm_users (ActorId) ON DELETE RESTRICT,
    CONSTRAINT CK_matter_assignments_Role CHECK (AssignmentRole IN
        ('ResponsibleAttorney','OriginatingAttorney','Attorney','Paralegal','Assistant')),
    CONSTRAINT UX_matter_assignments_Active UNIQUE (MatterRecordId, ActorId, AssignmentRole, ActiveMarker),
    -- Serves "matters I am assigned to": equality on ActorId, then active rows, then matter.
    INDEX IX_matter_assignments_Actor_Active (ActorId, ActiveMarker, MatterRecordId)
) CHARACTER SET utf8mb4;

-- Ethical wall: listed actors are screened from a matter even if a role or grant would allow it.
CREATE TABLE ethical_walls (
    Id               char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId   char(36)     COLLATE ascii_general_ci NOT NULL,
    ScreenedActorId  varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    Reason           varchar(500) NOT NULL,
    CreatedByActorId varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    CreatedAtUtc     datetime(6)  NOT NULL,
    LiftedAtUtc      datetime(6)  NULL,
    LiftedByActorId  varchar(128) CHARACTER SET utf8mb4 NULL,
    ActiveMarker     tinyint AS (CASE WHEN LiftedAtUtc IS NULL THEN 1 ELSE NULL END) STORED,
    CONSTRAINT PK_ethical_walls PRIMARY KEY (Id),
    CONSTRAINT FK_ethical_walls_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT UX_ethical_walls_Active UNIQUE (MatterRecordId, ScreenedActorId, ActiveMarker),
    INDEX IX_ethical_walls_Actor_Active (ScreenedActorId, ActiveMarker, MatterRecordId)
) CHARACTER SET utf8mb4;

-- Transactional outbox. Written in the same transaction as the domain change.
-- Payload carries identifiers only; the dispatcher reloads and re-authorizes at execution.
CREATE TABLE outbox_events (
    Id               bigint       NOT NULL AUTO_INCREMENT,
    EventId          char(36)     COLLATE ascii_general_ci NOT NULL,
    EventType        varchar(96)  COLLATE ascii_general_ci NOT NULL,
    AggregateType    varchar(64)  COLLATE ascii_general_ci NOT NULL,
    AggregateId      varchar(64)  COLLATE ascii_general_ci NOT NULL,
    PayloadJson      json         NOT NULL,
    Status           varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Pending',
    Attempt          int          NOT NULL DEFAULT 0,
    AvailableAtUtc   datetime(6)  NOT NULL,
    LeaseOwner       varchar(128) COLLATE ascii_general_ci NULL,
    LeaseExpiresAtUtc datetime(6) NULL,
    ProcessedAtUtc   datetime(6)  NULL,
    LastErrorCode    varchar(64)  COLLATE ascii_general_ci NULL,
    CreatedAtUtc     datetime(6)  NOT NULL,
    CONSTRAINT PK_outbox_events PRIMARY KEY (Id),
    CONSTRAINT UX_outbox_events_EventId UNIQUE (EventId),
    CONSTRAINT CK_outbox_events_Status CHECK (Status IN ('Pending','Leased','Done','Dead')),
    -- Dispatcher claim: WHERE Status='Pending' AND AvailableAtUtc <= now ORDER BY AvailableAtUtc, Id.
    INDEX IX_outbox_events_Status_Available (Status, AvailableAtUtc, Id)
) CHARACTER SET utf8mb4;

-- Inbound provider webhooks for this firm (payments, e-signature). Dedup scope is provider account.
CREATE TABLE webhook_receipts (
    Id                 char(36)     COLLATE ascii_general_ci NOT NULL,
    Provider           varchar(32)  COLLATE ascii_general_ci NOT NULL,
    ProviderAccountRef varchar(128) COLLATE ascii_general_ci NOT NULL,
    EventId            varchar(255) COLLATE ascii_general_ci NOT NULL,
    EventType          varchar(96)  COLLATE ascii_general_ci NOT NULL,
    PayloadSha256      binary(32)   NOT NULL,
    SignatureVerified  tinyint(1)   NOT NULL,
    ReceivedAtUtc      datetime(6)  NOT NULL,
    ProcessedAtUtc     datetime(6)  NULL,
    Outcome            varchar(16)  COLLATE ascii_general_ci NULL,
    CONSTRAINT PK_webhook_receipts PRIMARY KEY (Id),
    CONSTRAINT UX_webhook_receipts_Event UNIQUE (Provider, ProviderAccountRef, EventId),
    CONSTRAINT CK_webhook_receipts_Verified CHECK (SignatureVerified = 1),
    CONSTRAINT CK_webhook_receipts_Outcome CHECK (Outcome IS NULL OR Outcome IN ('Applied','Ignored','Rejected')),
    INDEX IX_webhook_receipts_Unprocessed (ProcessedAtUtc, ReceivedAtUtc)
) CHARACTER SET utf8mb4;

-- Index additions on existing tables, justified in query-and-index-spec.md.
CREATE INDEX IX_matters_Status_CreatedAtUtc ON matters (Status, CreatedAtUtc, Id);
CREATE INDEX IX_matters_ParalegalActorId ON matters (ParalegalActorId);
CREATE INDEX IX_matter_tasks_Matter_Status_Due ON matter_tasks (MatterRecordId, Status, DueAtUtc);
CREATE INDEX IX_matter_tasks_Assignee_Status_Due ON matter_tasks (AssigneeActorId, Status, DueAtUtc);
CREATE INDEX IX_matter_documents_Matter_Uploaded ON matter_documents (MatterRecordId, UploadedAtUtc, Id);
CREATE INDEX IX_consultation_requests_Status_Preferred ON consultation_requests (Status, PreferredAtUtc, Id);
CREATE INDEX IX_intake_records_Stage_NextActionDue ON intake_records (Stage, NextActionDueAtUtc, Id);
