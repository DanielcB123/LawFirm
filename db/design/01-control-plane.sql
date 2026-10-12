-- MatterForge (working name) control-plane database: DESIGN ARTIFACT, NOT A MIGRATION.
-- Reviewable reference DDL for MySQL 8.0 (validated on 8.0.36, see validation/).
-- The control plane holds routing, identity references, memberships, lifecycle, and
-- operations state only. It must never hold legal narratives, client contact details,
-- documents, medical records, messages, or money movements of firm clients.
--
-- Conventions (same as the firm schema produced by EF Core/Pomelo today):
--   * Ids: char(36) COLLATE ascii_general_ci (EF Guid mapping). Ids never confer access.
--   * Instants: datetime(6) holding UTC. Column names end in AtUtc.
--   * Optimistic concurrency: RowVersion int incremented by the application on update.
--   * Secrets: never stored. Only references into an external secret store.
--   * Enumerations: varchar plus CHECK so values are readable in dumps and diffable.

CREATE TABLE firms (
    Id                 char(36)     COLLATE ascii_general_ci NOT NULL,
    Slug               varchar(63)  COLLATE ascii_general_ci NOT NULL,  -- DNS-label safe, immutable
    LegalName          varchar(200) NOT NULL,
    DisplayName        varchar(200) NOT NULL,
    Status             varchar(24)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Provisioning',
    StatusReason       varchar(500) NULL,
    StatusChangedAtUtc datetime(6)  NOT NULL,
    CurrentPlacementId char(36)     COLLATE ascii_general_ci NULL,
    CreatedAtUtc       datetime(6)  NOT NULL,
    RowVersion         int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_firms PRIMARY KEY (Id),
    CONSTRAINT UX_firms_Slug UNIQUE (Slug),
    CONSTRAINT CK_firms_Slug CHECK (Slug REGEXP '^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$'),
    CONSTRAINT CK_firms_Status CHECK (Status IN
        ('Provisioning','Active','ReadOnly','Suspended','Offboarding','Retired','Failed'))
) CHARACTER SET utf8mb4;

-- Host names that resolve a firm. Unverified domains never route traffic.
CREATE TABLE firm_domains (
    Id                    char(36)     COLLATE ascii_general_ci NOT NULL,
    FirmId                char(36)     COLLATE ascii_general_ci NOT NULL,
    Host                  varchar(253) COLLATE ascii_general_ci NOT NULL,  -- lower-case, punycode
    Purpose               varchar(16)  COLLATE ascii_general_ci NOT NULL,  -- Workspace | Portal | Public
    VerificationTokenHash binary(32)   NULL,
    VerifiedAtUtc         datetime(6)  NULL,
    Status                varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Pending',
    CreatedAtUtc          datetime(6)  NOT NULL,
    RowVersion            int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_firm_domains PRIMARY KEY (Id),
    CONSTRAINT UX_firm_domains_Host UNIQUE (Host),
    CONSTRAINT FK_firm_domains_firms FOREIGN KEY (FirmId) REFERENCES firms (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_firm_domains_Purpose CHECK (Purpose IN ('Workspace','Portal','Public')),
    CONSTRAINT CK_firm_domains_Status CHECK (Status IN ('Pending','Verified','Revoked')),
    CONSTRAINT CK_firm_domains_Verified CHECK (Status <> 'Verified' OR VerifiedAtUtc IS NOT NULL),
    INDEX IX_firm_domains_FirmId (FirmId)
) CHARACTER SET utf8mb4;

-- Minimal platform identity: a reference to the identity provider subject, nothing else.
CREATE TABLE platform_identities (
    Id            char(36)     COLLATE ascii_general_ci NOT NULL,
    Issuer        varchar(255) COLLATE ascii_general_ci NOT NULL,
    Subject       varchar(255) COLLATE ascii_general_ci NOT NULL,
    Status        varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Active',
    SessionEpoch  int          NOT NULL DEFAULT 1,     -- bump to revoke all sessions of the identity
    MfaEnrolledAtUtc datetime(6) NULL,
    CreatedAtUtc  datetime(6)  NOT NULL,
    DisabledAtUtc datetime(6)  NULL,
    RowVersion    int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_platform_identities PRIMARY KEY (Id),
    CONSTRAINT UX_platform_identities_Issuer_Subject UNIQUE (Issuer, Subject),
    CONSTRAINT CK_platform_identities_Status CHECK (Status IN ('Active','Disabled'))
) CHARACTER SET utf8mb4;

-- Membership of an identity in a firm. MembershipVersion is embedded in tokens and
-- re-checked per request (short-TTL cache). Bumping it revokes outstanding tokens.
CREATE TABLE firm_memberships (
    Id                char(36)    COLLATE ascii_general_ci NOT NULL,
    FirmId            char(36)    COLLATE ascii_general_ci NOT NULL,
    IdentityId        char(36)    COLLATE ascii_general_ci NOT NULL,
    MemberKind        varchar(16) COLLATE ascii_general_ci NOT NULL,   -- Staff | Client | Representative
    Status            varchar(16) COLLATE ascii_general_ci NOT NULL DEFAULT 'Invited',
    MembershipVersion int         NOT NULL DEFAULT 1,
    RequiresMfa       tinyint(1)  NOT NULL DEFAULT 1,
    ActivatedAtUtc    datetime(6) NULL,
    RevokedAtUtc      datetime(6) NULL,
    RevokedByIdentityId char(36)  COLLATE ascii_general_ci NULL,
    CreatedAtUtc      datetime(6) NOT NULL,
    RowVersion        int         NOT NULL DEFAULT 1,
    CONSTRAINT PK_firm_memberships PRIMARY KEY (Id),
    CONSTRAINT UX_firm_memberships_Firm_Identity UNIQUE (FirmId, IdentityId),
    CONSTRAINT FK_firm_memberships_firms FOREIGN KEY (FirmId) REFERENCES firms (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_firm_memberships_identities FOREIGN KEY (IdentityId) REFERENCES platform_identities (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_firm_memberships_Kind CHECK (MemberKind IN ('Staff','Client','Representative')),
    CONSTRAINT CK_firm_memberships_Status CHECK (Status IN ('Invited','Active','Suspended','Revoked')),
    INDEX IX_firm_memberships_IdentityId_Status (IdentityId, Status)
) CHARACTER SET utf8mb4;

-- Invitation email is held only until acceptance or expiry, then nulled.
CREATE TABLE firm_invitations (
    Id               char(36)     COLLATE ascii_general_ci NOT NULL,
    FirmId           char(36)     COLLATE ascii_general_ci NOT NULL,
    MembershipId     char(36)     COLLATE ascii_general_ci NOT NULL,
    EmailNormalized  varchar(320) NULL,                 -- sensitive; erased after acceptance/expiry
    TokenHash        binary(32)   NOT NULL,             -- SHA-256 of a single-use random token
    ExpiresAtUtc     datetime(6)  NOT NULL,
    AcceptedAtUtc    datetime(6)  NULL,
    CreatedAtUtc     datetime(6)  NOT NULL,
    CONSTRAINT PK_firm_invitations PRIMARY KEY (Id),
    CONSTRAINT UX_firm_invitations_TokenHash UNIQUE (TokenHash),
    CONSTRAINT FK_firm_invitations_firms FOREIGN KEY (FirmId) REFERENCES firms (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_firm_invitations_memberships FOREIGN KEY (MembershipId) REFERENCES firm_memberships (Id) ON DELETE CASCADE,
    INDEX IX_firm_invitations_FirmId_ExpiresAtUtc (FirmId, ExpiresAtUtc)
) CHARACTER SET utf8mb4;

-- Pointers into an external secret store (for example a key vault). No secret values.
CREATE TABLE secret_references (
    Id             char(36)     COLLATE ascii_general_ci NOT NULL,
    Purpose        varchar(32)  COLLATE ascii_general_ci NOT NULL,
    Provider       varchar(32)  COLLATE ascii_general_ci NOT NULL,
    ExternalRef    varchar(512) COLLATE ascii_general_ci NOT NULL,
    ExternalVersion varchar(128) COLLATE ascii_general_ci NULL,
    RotatedAtUtc   datetime(6)  NULL,
    ExpiresAtUtc   datetime(6)  NULL,
    Status         varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Active',
    CreatedAtUtc   datetime(6)  NOT NULL,
    CONSTRAINT PK_secret_references PRIMARY KEY (Id),
    CONSTRAINT UX_secret_references_ExternalRef UNIQUE (Provider, ExternalRef),
    CONSTRAINT CK_secret_references_Purpose CHECK (Purpose IN
        ('DbRuntime','DbMigration','DbServerAdmin','StorageScope','PaymentAccount','WebhookSigning','SigningKey')),
    CONSTRAINT CK_secret_references_Status CHECK (Status IN ('Active','Rotating','Retired'))
) CHARACTER SET utf8mb4;

-- A MySQL server (or managed instance) that hosts firm databases. "Cell" = one server.
CREATE TABLE database_servers (
    Id                char(36)     COLLATE ascii_general_ci NOT NULL,
    Name              varchar(64)  COLLATE ascii_general_ci NOT NULL,
    EndpointRef       varchar(255) COLLATE ascii_general_ci NOT NULL,   -- host:port or private DNS
    Region            varchar(64)  COLLATE ascii_general_ci NOT NULL,
    EngineVersion     varchar(32)  COLLATE ascii_general_ci NOT NULL,
    Tier              varchar(16)  COLLATE ascii_general_ci NOT NULL,   -- Shared | Dedicated
    AdminSecretRefId  char(36)     COLLATE ascii_general_ci NOT NULL,
    MaxConnections    int          NOT NULL,                           -- measured server setting
    ConnectionBudget  int          NOT NULL,                           -- reserved for app pools
    MaxFirmDatabases  int          NOT NULL,                           -- placement policy, not engine limit
    Status            varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Active',
    CreatedAtUtc      datetime(6)  NOT NULL,
    RowVersion        int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_database_servers PRIMARY KEY (Id),
    CONSTRAINT UX_database_servers_Name UNIQUE (Name),
    CONSTRAINT FK_database_servers_admin_secret FOREIGN KEY (AdminSecretRefId) REFERENCES secret_references (Id),
    CONSTRAINT CK_database_servers_Tier CHECK (Tier IN ('Shared','Dedicated')),
    CONSTRAINT CK_database_servers_Status CHECK (Status IN ('Active','Draining','Closed')),
    CONSTRAINT CK_database_servers_Budget CHECK (ConnectionBudget > 0 AND ConnectionBudget < MaxConnections)
) CHARACTER SET utf8mb4;

-- Placement of a firm's database. Rows are versioned, never edited in place for a move:
-- a move inserts a new RoutingVersion and flips firms.CurrentPlacementId under a write freeze.
CREATE TABLE firm_placements (
    Id                  char(36)    COLLATE ascii_general_ci NOT NULL,
    FirmId              char(36)    COLLATE ascii_general_ci NOT NULL,
    RoutingVersion      int         NOT NULL,
    ServerId            char(36)    COLLATE ascii_general_ci NOT NULL,
    DatabaseName        varchar(64) COLLATE ascii_general_ci NOT NULL,
    RuntimeSecretRefId  char(36)    COLLATE ascii_general_ci NOT NULL,
    MigrationSecretRefId char(36)   COLLATE ascii_general_ci NOT NULL,
    StoragePrefix       varchar(128) COLLATE ascii_general_ci NOT NULL,
    SchemaVersion       varchar(150) COLLATE ascii_general_ci NULL,     -- last applied EF MigrationId
    Status              varchar(16) COLLATE ascii_general_ci NOT NULL DEFAULT 'Provisioning',
    PoolMaxSize         int         NOT NULL DEFAULT 5,
    CreatedAtUtc        datetime(6) NOT NULL,
    RetiredAtUtc        datetime(6) NULL,
    RowVersion          int         NOT NULL DEFAULT 1,
    CONSTRAINT PK_firm_placements PRIMARY KEY (Id),
    CONSTRAINT UX_firm_placements_Firm_RoutingVersion UNIQUE (FirmId, RoutingVersion),
    CONSTRAINT UX_firm_placements_Server_Database UNIQUE (ServerId, DatabaseName),
    CONSTRAINT FK_firm_placements_firms FOREIGN KEY (FirmId) REFERENCES firms (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_firm_placements_servers FOREIGN KEY (ServerId) REFERENCES database_servers (Id),
    CONSTRAINT FK_firm_placements_runtime_secret FOREIGN KEY (RuntimeSecretRefId) REFERENCES secret_references (Id),
    CONSTRAINT FK_firm_placements_migration_secret FOREIGN KEY (MigrationSecretRefId) REFERENCES secret_references (Id),
    -- Database names are generated by the platform, never accepted from a request.
    CONSTRAINT CK_firm_placements_DatabaseName CHECK (DatabaseName REGEXP '^mf_f_[0-9a-f]{32}$'),
    CONSTRAINT CK_firm_placements_Status CHECK (Status IN
        ('Provisioning','Active','Migrating','ReadOnly','Moving','Retired','Failed')),
    CONSTRAINT CK_firm_placements_Pool CHECK (PoolMaxSize BETWEEN 1 AND 50),
    INDEX IX_firm_placements_ServerId_Status (ServerId, Status)
) CHARACTER SET utf8mb4;

ALTER TABLE firms
    ADD CONSTRAINT FK_firms_current_placement FOREIGN KEY (CurrentPlacementId) REFERENCES firm_placements (Id);

-- Idempotent provisioning/offboarding/move operations. One row per requested operation.
CREATE TABLE provisioning_operations (
    Id              char(36)     COLLATE ascii_general_ci NOT NULL,
    FirmId          char(36)     COLLATE ascii_general_ci NOT NULL,
    OperationType   varchar(24)  COLLATE ascii_general_ci NOT NULL,
    IdempotencyKey  varchar(128) COLLATE ascii_general_ci NOT NULL,
    State           varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Pending',
    CurrentStep     varchar(48)  COLLATE ascii_general_ci NOT NULL,
    StepsCompletedJson json      NOT NULL,
    Attempt         int          NOT NULL DEFAULT 0,
    LeaseOwner      varchar(128) COLLATE ascii_general_ci NULL,
    LeaseExpiresAtUtc datetime(6) NULL,
    NextAttemptAtUtc  datetime(6) NULL,
    LastErrorCode   varchar(64)  COLLATE ascii_general_ci NULL,          -- codes only, no payloads
    RequestedByIdentityId char(36) COLLATE ascii_general_ci NOT NULL,
    CreatedAtUtc    datetime(6)  NOT NULL,
    UpdatedAtUtc    datetime(6)  NOT NULL,
    RowVersion      int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_provisioning_operations PRIMARY KEY (Id),
    CONSTRAINT UX_provisioning_operations_IdempotencyKey UNIQUE (OperationType, IdempotencyKey),
    CONSTRAINT FK_provisioning_operations_firms FOREIGN KEY (FirmId) REFERENCES firms (Id),
    CONSTRAINT CK_provisioning_operations_Type CHECK (OperationType IN
        ('Provision','Move','Offboard','Restore','RotateCredentials')),
    CONSTRAINT CK_provisioning_operations_State CHECK (State IN
        ('Pending','Running','WaitingRetry','Succeeded','Failed','Cancelled')),
    INDEX IX_provisioning_operations_State_Next (State, NextAttemptAtUtc),
    INDEX IX_provisioning_operations_FirmId (FirmId, CreatedAtUtc)
) CHARACTER SET utf8mb4;

-- A fleet schema rollout and its per-placement targets.
CREATE TABLE schema_migration_runs (
    Id                char(36)     COLLATE ascii_general_ci NOT NULL,
    TargetMigrationId varchar(150) COLLATE ascii_general_ci NOT NULL,
    AppReleaseMin     varchar(32)  COLLATE ascii_general_ci NOT NULL,   -- oldest release compatible after run
    Phase             varchar(16)  COLLATE ascii_general_ci NOT NULL,   -- Expand | Backfill | Contract
    MaxConcurrency    int          NOT NULL DEFAULT 2,
    FailureBudget     int          NOT NULL DEFAULT 0,                  -- failures before automatic halt
    Status            varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Planned',
    CreatedAtUtc      datetime(6)  NOT NULL,
    StartedAtUtc      datetime(6)  NULL,
    FinishedAtUtc     datetime(6)  NULL,
    RowVersion        int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_schema_migration_runs PRIMARY KEY (Id),
    CONSTRAINT CK_schema_migration_runs_Phase CHECK (Phase IN ('Expand','Backfill','Contract')),
    CONSTRAINT CK_schema_migration_runs_Status CHECK (Status IN
        ('Planned','Canary','Rolling','Halted','Succeeded','Abandoned')),
    CONSTRAINT CK_schema_migration_runs_Concurrency CHECK (MaxConcurrency BETWEEN 1 AND 64)
) CHARACTER SET utf8mb4;

CREATE TABLE schema_migration_targets (
    RunId          char(36)    COLLATE ascii_general_ci NOT NULL,
    PlacementId    char(36)    COLLATE ascii_general_ci NOT NULL,
    Wave           int         NOT NULL,                                -- 0 = canary
    Status         varchar(16) COLLATE ascii_general_ci NOT NULL DEFAULT 'Pending',
    Attempt        int         NOT NULL DEFAULT 0,
    FromMigrationId varchar(150) COLLATE ascii_general_ci NULL,
    DriftDetected  tinyint(1)  NOT NULL DEFAULT 0,
    LastErrorCode  varchar(64) COLLATE ascii_general_ci NULL,
    StartedAtUtc   datetime(6) NULL,
    FinishedAtUtc  datetime(6) NULL,
    CONSTRAINT PK_schema_migration_targets PRIMARY KEY (RunId, PlacementId),
    CONSTRAINT FK_schema_migration_targets_runs FOREIGN KEY (RunId) REFERENCES schema_migration_runs (Id),
    CONSTRAINT FK_schema_migration_targets_placements FOREIGN KEY (PlacementId) REFERENCES firm_placements (Id),
    CONSTRAINT CK_schema_migration_targets_Status CHECK (Status IN
        ('Pending','Running','Succeeded','Failed','Skipped','Quarantined')),
    INDEX IX_schema_migration_targets_Run_Status_Wave (RunId, Status, Wave)
) CHARACTER SET utf8mb4;

-- MatterForge's own SaaS subscription with the firm (not the firm's client billing).
CREATE TABLE subscriptions (
    Id                       char(36)     COLLATE ascii_general_ci NOT NULL,
    FirmId                   char(36)     COLLATE ascii_general_ci NOT NULL,
    PlanKey                  varchar(64)  COLLATE ascii_general_ci NOT NULL,
    Status                   varchar(16)  COLLATE ascii_general_ci NOT NULL,
    BillingProvider          varchar(32)  COLLATE ascii_general_ci NULL,
    ProviderCustomerRef      varchar(128) COLLATE ascii_general_ci NULL,
    ProviderSubscriptionRef  varchar(128) COLLATE ascii_general_ci NULL,
    CurrentPeriodEndAtUtc    datetime(6)  NULL,
    CreatedAtUtc             datetime(6)  NOT NULL,
    RowVersion               int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_subscriptions PRIMARY KEY (Id),
    CONSTRAINT UX_subscriptions_FirmId UNIQUE (FirmId),
    CONSTRAINT UX_subscriptions_ProviderSub UNIQUE (BillingProvider, ProviderSubscriptionRef),
    CONSTRAINT FK_subscriptions_firms FOREIGN KEY (FirmId) REFERENCES firms (Id),
    CONSTRAINT CK_subscriptions_Status CHECK (Status IN ('Trial','Active','PastDue','Cancelled'))
) CHARACTER SET utf8mb4;

CREATE TABLE platform_webhook_receipts (
    Id             char(36)     COLLATE ascii_general_ci NOT NULL,
    Provider       varchar(32)  COLLATE ascii_general_ci NOT NULL,
    ProviderAccountRef varchar(128) COLLATE ascii_general_ci NOT NULL,
    EventId        varchar(255) COLLATE ascii_general_ci NOT NULL,
    PayloadSha256  binary(32)   NOT NULL,
    ReceivedAtUtc  datetime(6)  NOT NULL,
    ProcessedAtUtc datetime(6)  NULL,
    CONSTRAINT PK_platform_webhook_receipts PRIMARY KEY (Id),
    CONSTRAINT UX_platform_webhook_receipts_Event UNIQUE (Provider, ProviderAccountRef, EventId)
) CHARACTER SET utf8mb4;

-- Platform support may enter a firm only through a firm-approved, scoped, expiring request.
CREATE TABLE support_access_requests (
    Id                     char(36)     COLLATE ascii_general_ci NOT NULL,
    FirmId                 char(36)     COLLATE ascii_general_ci NOT NULL,
    RequestedByIdentityId  char(36)     COLLATE ascii_general_ci NOT NULL,
    TicketRef              varchar(64)  COLLATE ascii_general_ci NOT NULL,
    Reason                 varchar(1000) NOT NULL,          -- must not contain client facts
    Scope                  varchar(24)  COLLATE ascii_general_ci NOT NULL,  -- MetadataOnly | ReadOnlyMatter | Configuration
    ScopeMatterNumber      varchar(64)  NULL,
    Status                 varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Requested',
    ApprovedByIdentityId   char(36)     COLLATE ascii_general_ci NULL,       -- a firm admin, never the requester
    ApprovedAtUtc          datetime(6)  NULL,
    ExpiresAtUtc           datetime(6)  NULL,
    RevokedAtUtc           datetime(6)  NULL,
    CreatedAtUtc           datetime(6)  NOT NULL,
    RowVersion             int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_support_access_requests PRIMARY KEY (Id),
    CONSTRAINT FK_support_access_requests_firms FOREIGN KEY (FirmId) REFERENCES firms (Id),
    CONSTRAINT CK_support_access_requests_Scope CHECK (Scope IN ('MetadataOnly','ReadOnlyMatter','Configuration')),
    CONSTRAINT CK_support_access_requests_Status CHECK (Status IN ('Requested','Approved','Denied','Expired','Revoked')),
    CONSTRAINT CK_support_access_requests_Approver CHECK (ApprovedByIdentityId IS NULL OR ApprovedByIdentityId <> RequestedByIdentityId),
    CONSTRAINT CK_support_access_requests_Expiry CHECK (Status <> 'Approved' OR (ApprovedAtUtc IS NOT NULL AND ExpiresAtUtc IS NOT NULL)),
    INDEX IX_support_access_requests_Firm_Status (FirmId, Status, ExpiresAtUtc)
) CHARACTER SET utf8mb4;

-- Append-only. Runtime user receives INSERT and SELECT only on this table.
CREATE TABLE control_audit_events (
    Id             bigint       NOT NULL AUTO_INCREMENT,
    FirmId         char(36)     COLLATE ascii_general_ci NULL,
    ActorIdentityId char(36)    COLLATE ascii_general_ci NULL,
    Action         varchar(96)  COLLATE ascii_general_ci NOT NULL,
    ObjectType     varchar(64)  COLLATE ascii_general_ci NOT NULL,
    ObjectId       varchar(128) COLLATE ascii_general_ci NOT NULL,
    CorrelationId  varchar(64)  COLLATE ascii_general_ci NOT NULL,
    MetadataJson   json         NULL,                   -- identifiers and codes only
    OccurredAtUtc  datetime(6)  NOT NULL,
    CONSTRAINT PK_control_audit_events PRIMARY KEY (Id),
    INDEX IX_control_audit_events_Firm_Time (FirmId, OccurredAtUtc)
) CHARACTER SET utf8mb4;
