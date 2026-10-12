-- MatterForge POOLED (shared tables) ALTERNATIVE: DESIGN ARTIFACT, NOT RECOMMENDED FOR NOW.
-- Shows the minimum shape required if firms ever share tables. Used by validation to prove
-- composite tenant-safe foreign keys reject cross-firm references on MySQL 8.0.36.
--
-- These constraints protect REFERENCES only. They do not authorize READS: a query that omits
-- FirmId, an EF query that uses IgnoreQueryFilters(), or raw SQL returns every firm's rows.
-- MySQL has no native row-level security; EF Core global query filters are application code.

CREATE TABLE pooled_firms (
    Id   char(36)     COLLATE ascii_general_ci NOT NULL,
    Slug varchar(63)  COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT PK_pooled_firms PRIMARY KEY (Id),
    CONSTRAINT UX_pooled_firms_Slug UNIQUE (Slug)
) CHARACTER SET utf8mb4;

-- Primary key leads with FirmId so each firm's rows cluster together and every tenant-prefixed
-- index is a left prefix of the clustered key. UNIQUE (FirmId, Id) is implied by the PK.
CREATE TABLE pooled_matters (
    FirmId       char(36)     COLLATE ascii_general_ci NOT NULL,
    Id           char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterNumber varchar(64)  NOT NULL,
    Title        varchar(200) NOT NULL,
    Status       varchar(64)  NOT NULL,
    ResponsibleAttorneyActorId varchar(128) NOT NULL,
    CreatedAtUtc datetime(6)  NOT NULL,
    CONSTRAINT PK_pooled_matters PRIMARY KEY (FirmId, Id),
    CONSTRAINT FK_pooled_matters_firms FOREIGN KEY (FirmId) REFERENCES pooled_firms (Id),
    -- Matter numbers are unique per firm, not globally.
    CONSTRAINT UX_pooled_matters_Number UNIQUE (FirmId, MatterNumber),
    INDEX IX_pooled_matters_Firm_Status_Created (FirmId, Status, CreatedAtUtc)
) CHARACTER SET utf8mb4;

CREATE TABLE pooled_matter_tasks (
    FirmId         char(36)     COLLATE ascii_general_ci NOT NULL,
    Id             char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId char(36)     COLLATE ascii_general_ci NOT NULL,
    Title          varchar(240) NOT NULL,
    Status         varchar(64)  NOT NULL,
    DueAtUtc       datetime(6)  NULL,
    CONSTRAINT PK_pooled_matter_tasks PRIMARY KEY (FirmId, Id),
    -- Tenant-safe: the task's FirmId must equal the referenced matter's FirmId.
    CONSTRAINT FK_pooled_matter_tasks_matters FOREIGN KEY (FirmId, MatterRecordId)
        REFERENCES pooled_matters (FirmId, Id) ON DELETE CASCADE,
    INDEX IX_pooled_matter_tasks_Firm_Matter_Status_Due (FirmId, MatterRecordId, Status, DueAtUtc)
) CHARACTER SET utf8mb4;

CREATE TABLE pooled_matter_access_grants (
    FirmId         char(36)     COLLATE ascii_general_ci NOT NULL,
    Id             char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId char(36)     COLLATE ascii_general_ci NOT NULL,
    ActorId        varchar(128) NOT NULL,
    RevokedAtUtc   datetime(6)  NULL,
    CONSTRAINT PK_pooled_matter_access_grants PRIMARY KEY (FirmId, Id),
    CONSTRAINT FK_pooled_grants_matters FOREIGN KEY (FirmId, MatterRecordId)
        REFERENCES pooled_matters (FirmId, Id) ON DELETE CASCADE,
    INDEX IX_pooled_grants_Firm_Actor (FirmId, ActorId, RevokedAtUtc, MatterRecordId)
) CHARACTER SET utf8mb4;

CREATE TABLE pooled_invoices (
    FirmId         char(36)      COLLATE ascii_general_ci NOT NULL,
    Id             char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId char(36)      COLLATE ascii_general_ci NOT NULL,
    InvoiceNumber  varchar(32)   NOT NULL,
    Total          decimal(19,2) NOT NULL,
    CONSTRAINT PK_pooled_invoices PRIMARY KEY (FirmId, Id),
    CONSTRAINT UX_pooled_invoices_Number UNIQUE (FirmId, InvoiceNumber),
    CONSTRAINT FK_pooled_invoices_matters FOREIGN KEY (FirmId, MatterRecordId)
        REFERENCES pooled_matters (FirmId, Id)
) CHARACTER SET utf8mb4;

CREATE TABLE pooled_invoice_lines (
    FirmId    char(36)      COLLATE ascii_general_ci NOT NULL,
    Id        char(36)      COLLATE ascii_general_ci NOT NULL,
    InvoiceId char(36)      COLLATE ascii_general_ci NOT NULL,
    Amount    decimal(19,2) NOT NULL,
    CONSTRAINT PK_pooled_invoice_lines PRIMARY KEY (FirmId, Id),
    CONSTRAINT FK_pooled_invoice_lines_invoices FOREIGN KEY (FirmId, InvoiceId)
        REFERENCES pooled_invoices (FirmId, Id)
) CHARACTER SET utf8mb4;
