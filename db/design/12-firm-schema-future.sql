-- MatterForge firm database: FUTURE domain structures. DESIGN ARTIFACT, NOT A MIGRATION.
-- Applies on top of 10 (current EF schema) and 11 (required additions). Each block is
-- introduced by the delivery slice that needs it (see dependency-ordered plan); nothing
-- here is implemented in application code.
--
-- Identifier choice:
--   * Entity rows keep char(36) GUIDs, matching existing EF mappings and allowing ids to be
--     generated before insert and carried in imports and exports.
--   * High-volume append-only rows (messages, custody events, journal lines) use bigint
--     AUTO_INCREMENT clustered keys so inserts append to the B-tree and keyset pagination is
--     cheap. Values are firm-database local and overlap across firms; they are never exposed
--     as cross-firm identifiers. APIs expose a char(36) PublicId where a row is addressable.
-- Money: DECIMAL(19,2) amounts in currency minor units of 2 decimals plus CurrencyCode
--   char(3) restricted to 'USD' until multi-currency is designed. Rates and quantities use
--   DECIMAL(19,4) and DECIMAL(12,4). Line amounts are rounded half away from zero (MySQL
--   ROUND on exact DECIMAL) to cents at the line; totals are sums of rounded lines.
--   No FLOAT or DOUBLE anywhere in money paths.
-- Dates: instants are datetime(6) UTC; legal and accounting dates are DATE with an explicit
--   TimeZone (IANA) or Jurisdiction column when the meaning depends on it.

-- ---------------------------------------------------------------- people and intake
-- Contacts and organizations are the existing `parties` table (PartyType Person|Organization)
-- with party_aliases and party_relationships. matter_parties links them to matters.
CREATE TABLE matter_parties (
    Id               char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId   char(36)     COLLATE ascii_general_ci NOT NULL,
    PartyId          char(36)     COLLATE ascii_general_ci NOT NULL,
    PartyRole        varchar(32)  COLLATE ascii_general_ci NOT NULL,
    IsClient         tinyint(1)   NOT NULL DEFAULT 0,
    RepresentedByPartyId char(36) COLLATE ascii_general_ci NULL,
    StartedAtUtc     datetime(6)  NOT NULL,
    EndedAtUtc       datetime(6)  NULL,
    RowVersion       int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_matter_parties PRIMARY KEY (Id),
    CONSTRAINT FK_matter_parties_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_matter_parties_parties FOREIGN KEY (PartyId) REFERENCES parties (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_matter_parties_rep FOREIGN KEY (RepresentedByPartyId) REFERENCES parties (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_matter_parties_Role CHECK (PartyRole IN
        ('Client','AdverseParty','OpposingCounsel','Insurer','Adjuster','Witness','Expert',
         'MedicalProvider','Lienholder','Owner','GeneralContractor','Subcontractor','Surety','Court','Other')),
    CONSTRAINT UX_matter_parties UNIQUE (MatterRecordId, PartyId, PartyRole),
    INDEX IX_matter_parties_PartyId (PartyId, PartyRole)          -- conflict search by party
) CHARACTER SET utf8mb4;

-- Ordinary domain table: people named on an inquiry before any engagement exists.
CREATE TABLE inquiry_contacts (
    Id              char(36)    COLLATE ascii_general_ci NOT NULL,
    IntakeRecordId  char(36)    COLLATE ascii_general_ci NOT NULL,
    PartyId         char(36)    COLLATE ascii_general_ci NOT NULL,
    ContactRole     varchar(32) COLLATE ascii_general_ci NOT NULL,  -- ProspectiveClient | AdverseParty | Other
    CreatedAtUtc    datetime(6) NOT NULL,
    CONSTRAINT PK_inquiry_contacts PRIMARY KEY (Id),
    CONSTRAINT FK_inquiry_contacts_intake FOREIGN KEY (IntakeRecordId) REFERENCES intake_records (Id) ON DELETE CASCADE,
    CONSTRAINT FK_inquiry_contacts_parties FOREIGN KEY (PartyId) REFERENCES parties (Id) ON DELETE RESTRICT,
    CONSTRAINT UX_inquiry_contacts UNIQUE (IntakeRecordId, PartyId, ContactRole),
    INDEX IX_inquiry_contacts_PartyId (PartyId)
) CHARACTER SET utf8mb4;

CREATE TABLE engagements (
    Id                     char(36)    COLLATE ascii_general_ci NOT NULL,
    IntakeRecordId         char(36)    COLLATE ascii_general_ci NOT NULL,
    MatterRecordId         char(36)    COLLATE ascii_general_ci NULL,
    ConflictDecisionId     char(36)    COLLATE ascii_general_ci NOT NULL,
    Status                 varchar(16) COLLATE ascii_general_ci NOT NULL DEFAULT 'Proposed',
    LetterDocumentVersionId char(36)   COLLATE ascii_general_ci NULL,
    SentAtUtc              datetime(6) NULL,
    SignedAtUtc            datetime(6) NULL,
    DeclinedAtUtc          datetime(6) NULL,
    CreatedByActorId       varchar(128) NOT NULL,
    CreatedAtUtc           datetime(6) NOT NULL,
    RowVersion             int         NOT NULL DEFAULT 1,
    CONSTRAINT PK_engagements PRIMARY KEY (Id),
    CONSTRAINT FK_engagements_intake FOREIGN KEY (IntakeRecordId) REFERENCES intake_records (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_engagements_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_engagements_conflict FOREIGN KEY (ConflictDecisionId) REFERENCES conflict_decisions (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_engagements_Status CHECK (Status IN ('Proposed','Sent','Signed','Declined','Withdrawn')),
    CONSTRAINT CK_engagements_Signed CHECK (Status <> 'Signed' OR SignedAtUtc IS NOT NULL),
    INDEX IX_engagements_Status_Created (Status, CreatedAtUtc)
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- deadlines
-- Legal deadlines are calendar DATES in a jurisdiction, not instants. calendar_entries keeps
-- reminders and scheduling; a deadline references its source rule and verifier.
CREATE TABLE deadlines (
    Id                 char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId     char(36)     COLLATE ascii_general_ci NOT NULL,
    DeadlineKind       varchar(48)  COLLATE ascii_general_ci NOT NULL,
    DueDate            date         NOT NULL,
    DueTimeLocal       time         NULL,
    TimeZone           varchar(64)  COLLATE ascii_general_ci NOT NULL,
    Jurisdiction       varchar(64)  NOT NULL,
    RuleReference      varchar(200) NULL,
    TriggerDate        date         NULL,
    Status             varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Unverified',
    ResponsibleActorId varchar(128) NOT NULL,
    VerifiedByActorId  varchar(128) NULL,
    VerifiedAtUtc      datetime(6)  NULL,
    CompletedAtUtc     datetime(6)  NULL,
    CalendarEntryId    char(36)     COLLATE ascii_general_ci NULL,
    CreatedAtUtc       datetime(6)  NOT NULL,
    RowVersion         int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_deadlines PRIMARY KEY (Id),
    CONSTRAINT FK_deadlines_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_deadlines_calendar FOREIGN KEY (CalendarEntryId) REFERENCES calendar_entries (Id) ON DELETE SET NULL,
    CONSTRAINT CK_deadlines_Status CHECK (Status IN ('Unverified','Verified','Completed','Vacated')),
    CONSTRAINT CK_deadlines_Verified CHECK (Status = 'Unverified' OR Status = 'Vacated' OR VerifiedAtUtc IS NOT NULL),
    INDEX IX_deadlines_Status_DueDate (Status, DueDate, Id),
    INDEX IX_deadlines_Responsible_Status_Due (ResponsibleActorId, Status, DueDate),
    INDEX IX_deadlines_Matter_Due (MatterRecordId, DueDate)
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- publication and messaging
CREATE TABLE client_updates (
    Id               char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId   char(36)     COLLATE ascii_general_ci NOT NULL,
    Title            varchar(200) NOT NULL,
    Body             text         NOT NULL,                      -- confidential
    Status           varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Draft',
    AuthorActorId    varchar(128) NOT NULL,
    ApprovedByActorId varchar(128) NULL,
    PublishedAtUtc   datetime(6)  NULL,
    WithdrawnAtUtc   datetime(6)  NULL,
    CreatedAtUtc     datetime(6)  NOT NULL,
    RowVersion       int          NOT NULL DEFAULT 1,
    CONSTRAINT PK_client_updates PRIMARY KEY (Id),
    CONSTRAINT FK_client_updates_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_client_updates_Status CHECK (Status IN ('Draft','Approved','Published','Withdrawn')),
    CONSTRAINT CK_client_updates_Published CHECK (Status NOT IN ('Published','Withdrawn')
        OR (PublishedAtUtc IS NOT NULL AND ApprovedByActorId IS NOT NULL)),
    INDEX IX_client_updates_Matter_Status_Published (MatterRecordId, Status, PublishedAtUtc)
) CHARACTER SET utf8mb4;

-- Explicit audience; no row means not visible. Never a bare is_visible flag.
CREATE TABLE client_update_audiences (
    ClientUpdateId  char(36)     COLLATE ascii_general_ci NOT NULL,
    AudienceActorId varchar(128) NOT NULL,
    AddedAtUtc      datetime(6)  NOT NULL,
    CONSTRAINT PK_client_update_audiences PRIMARY KEY (ClientUpdateId, AudienceActorId),
    CONSTRAINT FK_client_update_audiences_updates FOREIGN KEY (ClientUpdateId) REFERENCES client_updates (Id) ON DELETE CASCADE,
    CONSTRAINT FK_client_update_audiences_users FOREIGN KEY (AudienceActorId) REFERENCES firm_users (ActorId),
    INDEX IX_client_update_audiences_Actor (AudienceActorId, ClientUpdateId)
) CHARACTER SET utf8mb4;

CREATE TABLE conversations (
    Id              char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId  char(36)     COLLATE ascii_general_ci NOT NULL,
    Kind            varchar(16)  COLLATE ascii_general_ci NOT NULL,   -- ClientPortal | Internal
    Subject         varchar(200) NOT NULL,
    LastMessageAtUtc datetime(6) NULL,
    CreatedAtUtc    datetime(6)  NOT NULL,
    CONSTRAINT PK_conversations PRIMARY KEY (Id),
    CONSTRAINT FK_conversations_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_conversations_Kind CHECK (Kind IN ('ClientPortal','Internal')),
    INDEX IX_conversations_Matter_Last (MatterRecordId, LastMessageAtUtc)
) CHARACTER SET utf8mb4;

CREATE TABLE conversation_participants (
    ConversationId char(36)     COLLATE ascii_general_ci NOT NULL,
    ActorId        varchar(128) NOT NULL,
    JoinedAtUtc    datetime(6)  NOT NULL,
    RemovedAtUtc   datetime(6)  NULL,
    CONSTRAINT PK_conversation_participants PRIMARY KEY (ConversationId, ActorId),
    CONSTRAINT FK_conversation_participants_conv FOREIGN KEY (ConversationId) REFERENCES conversations (Id) ON DELETE CASCADE,
    CONSTRAINT FK_conversation_participants_users FOREIGN KEY (ActorId) REFERENCES firm_users (ActorId),
    INDEX IX_conversation_participants_Actor (ActorId, RemovedAtUtc, ConversationId)
) CHARACTER SET utf8mb4;

CREATE TABLE messages (
    Id             bigint       NOT NULL AUTO_INCREMENT,
    PublicId       char(36)     COLLATE ascii_general_ci NOT NULL,
    ConversationId char(36)     COLLATE ascii_general_ci NOT NULL,
    SenderActorId  varchar(128) NOT NULL,
    Body           text         NOT NULL,                        -- confidential
    SentAtUtc      datetime(6)  NOT NULL,
    RedactedAtUtc  datetime(6)  NULL,
    CONSTRAINT PK_messages PRIMARY KEY (Id),
    CONSTRAINT UX_messages_PublicId UNIQUE (PublicId),
    CONSTRAINT FK_messages_conversations FOREIGN KEY (ConversationId) REFERENCES conversations (Id) ON DELETE RESTRICT,
    -- Keyset pagination: WHERE ConversationId = ? AND Id < ? ORDER BY Id DESC LIMIT n.
    INDEX IX_messages_Conversation_Id (ConversationId, Id)
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- documents and evidence
-- matter_documents (existing) remains the logical document. Its StorageKey becomes legacy
-- after versions exist (expand/contract). Versions are immutable: runtime user has no UPDATE
-- or DELETE privilege on document_versions.
CREATE TABLE document_versions (
    Id              char(36)     COLLATE ascii_general_ci NOT NULL,
    DocumentId      char(36)     COLLATE ascii_general_ci NOT NULL,
    VersionNumber   int          NOT NULL,
    StorageKey      varchar(512) COLLATE ascii_general_ci NOT NULL,  -- opaque, firm-prefixed
    StorageVersionId varchar(256) COLLATE ascii_general_ci NULL,     -- object-store version id
    ContentSha256   binary(32)   NOT NULL,
    SizeBytes       bigint       NOT NULL,
    ContentType     varchar(128) COLLATE ascii_general_ci NOT NULL,
    ScanStatus      varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Pending',
    UploadedByActorId varchar(128) NOT NULL,
    UploadedAtUtc   datetime(6)  NOT NULL,
    CONSTRAINT PK_document_versions PRIMARY KEY (Id),
    CONSTRAINT FK_document_versions_documents FOREIGN KEY (DocumentId) REFERENCES matter_documents (Id) ON DELETE RESTRICT,
    CONSTRAINT UX_document_versions_Doc_Version UNIQUE (DocumentId, VersionNumber),
    CONSTRAINT CK_document_versions_Size CHECK (SizeBytes >= 0),
    CONSTRAINT CK_document_versions_Scan CHECK (ScanStatus IN ('Pending','Clean','Infected','Failed'))
) CHARACTER SET utf8mb4;

-- Explicit sharing of an exact version to a portal audience.
CREATE TABLE document_shares (
    Id                char(36)     COLLATE ascii_general_ci NOT NULL,
    DocumentVersionId char(36)     COLLATE ascii_general_ci NOT NULL,
    AudienceActorId   varchar(128) NOT NULL,
    SharedByActorId   varchar(128) NOT NULL,
    SharedAtUtc       datetime(6)  NOT NULL,
    RevokedAtUtc      datetime(6)  NULL,
    ActiveMarker      tinyint AS (CASE WHEN RevokedAtUtc IS NULL THEN 1 ELSE NULL END) STORED,
    CONSTRAINT PK_document_shares PRIMARY KEY (Id),
    CONSTRAINT FK_document_shares_versions FOREIGN KEY (DocumentVersionId) REFERENCES document_versions (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_document_shares_users FOREIGN KEY (AudienceActorId) REFERENCES firm_users (ActorId),
    CONSTRAINT UX_document_shares_Active UNIQUE (DocumentVersionId, AudienceActorId, ActiveMarker),
    INDEX IX_document_shares_Audience (AudienceActorId, ActiveMarker, DocumentVersionId)
) CHARACTER SET utf8mb4;

CREATE TABLE preservation_holds (
    Id               char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId   char(36)     COLLATE ascii_general_ci NOT NULL,
    DocumentId       char(36)     COLLATE ascii_general_ci NULL,        -- NULL = whole matter
    Reason           varchar(500) NOT NULL,
    IssuedByActorId  varchar(128) NOT NULL,
    IssuedAtUtc      datetime(6)  NOT NULL,
    ReleasedByActorId varchar(128) NULL,
    ReleasedAtUtc    datetime(6)  NULL,
    CONSTRAINT PK_preservation_holds PRIMARY KEY (Id),
    CONSTRAINT FK_preservation_holds_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_preservation_holds_documents FOREIGN KEY (DocumentId) REFERENCES matter_documents (Id) ON DELETE RESTRICT,
    INDEX IX_preservation_holds_Matter_Released (MatterRecordId, ReleasedAtUtc)
) CHARACTER SET utf8mb4;

CREATE TABLE evidence_items (
    Id               char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId   char(36)     COLLATE ascii_general_ci NOT NULL,
    Label            varchar(200) NOT NULL,
    Description      varchar(2000) NULL,
    OriginalDocumentVersionId char(36) COLLATE ascii_general_ci NULL,
    PhysicalLocation varchar(200) NULL,
    CreatedAtUtc     datetime(6)  NOT NULL,
    CONSTRAINT PK_evidence_items PRIMARY KEY (Id),
    CONSTRAINT FK_evidence_items_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_evidence_items_versions FOREIGN KEY (OriginalDocumentVersionId) REFERENCES document_versions (Id) ON DELETE RESTRICT,
    INDEX IX_evidence_items_Matter (MatterRecordId)
) CHARACTER SET utf8mb4;

-- Append-only, hash-chained per evidence item; tampering breaks the chain on verification.
CREATE TABLE custody_events (
    Id              bigint       NOT NULL AUTO_INCREMENT,
    EvidenceItemId  char(36)     COLLATE ascii_general_ci NOT NULL,
    EventType       varchar(32)  COLLATE ascii_general_ci NOT NULL,
    ActorId         varchar(128) NOT NULL,
    OccurredAtUtc   datetime(6)  NOT NULL,
    Notes           varchar(1000) NULL,
    PreviousEventSha256 binary(32) NULL,
    EventSha256     binary(32)   NOT NULL,
    CONSTRAINT PK_custody_events PRIMARY KEY (Id),
    CONSTRAINT FK_custody_events_items FOREIGN KEY (EvidenceItemId) REFERENCES evidence_items (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_custody_events_Type CHECK (EventType IN ('Received','Transferred','Copied','Examined','Produced','Returned','Destroyed')),
    INDEX IX_custody_events_Item_Id (EvidenceItemId, Id)
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- fees, time, invoices
CREATE TABLE fee_agreements (
    Id             char(36)    COLLATE ascii_general_ci NOT NULL,
    MatterRecordId char(36)    COLLATE ascii_general_ci NOT NULL,
    CreatedAtUtc   datetime(6) NOT NULL,
    CONSTRAINT PK_fee_agreements PRIMARY KEY (Id),
    CONSTRAINT FK_fee_agreements_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    INDEX IX_fee_agreements_Matter (MatterRecordId)
) CHARACTER SET utf8mb4;

CREATE TABLE fee_agreement_versions (
    Id                 char(36)     COLLATE ascii_general_ci NOT NULL,
    FeeAgreementId     char(36)     COLLATE ascii_general_ci NOT NULL,
    VersionNumber      int          NOT NULL,
    FeeType            varchar(16)  COLLATE ascii_general_ci NOT NULL,
    CurrencyCode       char(3)      COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    DefaultHourlyRate  decimal(19,4) NULL,
    ContingencyPercent decimal(7,4) NULL,       -- 33.3333 = 33.3333 percent
    FlatFeeAmount      decimal(19,2) NULL,
    TermsJson          json         NOT NULL,   -- structured terms snapshot
    SignedDocumentVersionId char(36) COLLATE ascii_general_ci NULL,
    EffectiveFrom      date         NOT NULL,
    Status             varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Draft',
    CreatedAtUtc       datetime(6)  NOT NULL,
    CONSTRAINT PK_fee_agreement_versions PRIMARY KEY (Id),
    CONSTRAINT FK_fee_agreement_versions_agreements FOREIGN KEY (FeeAgreementId) REFERENCES fee_agreements (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_fee_agreement_versions_doc FOREIGN KEY (SignedDocumentVersionId) REFERENCES document_versions (Id),
    CONSTRAINT UX_fee_agreement_versions UNIQUE (FeeAgreementId, VersionNumber),
    CONSTRAINT CK_fee_agreement_versions_Type CHECK (FeeType IN ('Hourly','Contingency','Flat','Hybrid')),
    CONSTRAINT CK_fee_agreement_versions_Currency CHECK (CurrencyCode = 'USD'),
    CONSTRAINT CK_fee_agreement_versions_Pct CHECK (ContingencyPercent IS NULL OR (ContingencyPercent > 0 AND ContingencyPercent < 100)),
    CONSTRAINT CK_fee_agreement_versions_Status CHECK (Status IN ('Draft','Active','Superseded'))
) CHARACTER SET utf8mb4;

CREATE TABLE time_entries (
    Id              char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId  char(36)      COLLATE ascii_general_ci NOT NULL,
    ActorId         varchar(128)  NOT NULL,
    WorkDate        date          NOT NULL,                        -- firm-local date
    DurationMinutes int           NOT NULL,
    HourlyRate      decimal(19,4) NOT NULL,
    Amount          decimal(19,2) NOT NULL,
    CurrencyCode    char(3)       COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    Narrative       varchar(4000) NOT NULL,                        -- privileged work product
    IsBillable      tinyint(1)    NOT NULL DEFAULT 1,
    Status          varchar(16)   COLLATE ascii_general_ci NOT NULL DEFAULT 'Draft',
    InvoiceLineId   char(36)      COLLATE ascii_general_ci NULL,
    CreatedAtUtc    datetime(6)   NOT NULL,
    RowVersion      int           NOT NULL DEFAULT 1,
    CONSTRAINT PK_time_entries PRIMARY KEY (Id),
    CONSTRAINT FK_time_entries_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_time_entries_Duration CHECK (DurationMinutes > 0 AND DurationMinutes <= 1440),
    CONSTRAINT CK_time_entries_Amount CHECK (Amount = ROUND(HourlyRate * DurationMinutes / 60, 2)),
    CONSTRAINT CK_time_entries_Currency CHECK (CurrencyCode = 'USD'),
    CONSTRAINT CK_time_entries_Status CHECK (Status IN ('Draft','Submitted','Billed','WrittenOff')),
    INDEX IX_time_entries_Matter_Status_Date (MatterRecordId, Status, WorkDate),
    INDEX IX_time_entries_Actor_Date (ActorId, WorkDate)
) CHARACTER SET utf8mb4;

CREATE TABLE expenses (
    Id             char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId char(36)      COLLATE ascii_general_ci NOT NULL,
    IncurredDate   date          NOT NULL,
    Category       varchar(48)   COLLATE ascii_general_ci NOT NULL,
    Description    varchar(1000) NOT NULL,
    Amount         decimal(19,2) NOT NULL,
    CurrencyCode   char(3)       COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    ReceiptDocumentVersionId char(36) COLLATE ascii_general_ci NULL,
    Status         varchar(16)   COLLATE ascii_general_ci NOT NULL DEFAULT 'Draft',
    InvoiceLineId  char(36)      COLLATE ascii_general_ci NULL,
    CreatedByActorId varchar(128) NOT NULL,
    CreatedAtUtc   datetime(6)   NOT NULL,
    RowVersion     int           NOT NULL DEFAULT 1,
    CONSTRAINT PK_expenses PRIMARY KEY (Id),
    CONSTRAINT FK_expenses_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_expenses_receipt FOREIGN KEY (ReceiptDocumentVersionId) REFERENCES document_versions (Id),
    CONSTRAINT CK_expenses_Amount CHECK (Amount > 0),
    CONSTRAINT CK_expenses_Currency CHECK (CurrencyCode = 'USD'),
    CONSTRAINT CK_expenses_Status CHECK (Status IN ('Draft','Submitted','Billed','WrittenOff')),
    INDEX IX_expenses_Matter_Status (MatterRecordId, Status)
) CHARACTER SET utf8mb4;

CREATE TABLE invoices (
    Id                    char(36)      COLLATE ascii_general_ci NOT NULL,
    InvoiceNumber         varchar(32)   COLLATE ascii_general_ci NOT NULL,
    MatterRecordId        char(36)      COLLATE ascii_general_ci NOT NULL,
    BillToPartyId         char(36)      COLLATE ascii_general_ci NOT NULL,
    FeeAgreementVersionId char(36)      COLLATE ascii_general_ci NULL,   -- snapshot reference
    CurrencyCode          char(3)       COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    Status                varchar(16)   COLLATE ascii_general_ci NOT NULL DEFAULT 'Draft',
    IssueDate             date          NULL,
    DueDate               date          NULL,
    Subtotal              decimal(19,2) NOT NULL DEFAULT 0,
    TaxTotal              decimal(19,2) NOT NULL DEFAULT 0,
    Total                 decimal(19,2) NOT NULL DEFAULT 0,
    AmountPaid            decimal(19,2) NOT NULL DEFAULT 0,
    BalanceDue            decimal(19,2) AS (Total - AmountPaid) STORED,
    -- 1 while collectible, else NULL: turns the two-status queue filter into one equality so
    -- the queue index returns rows already ordered by DueDate (validated: avoids scan + sort).
    OpenMarker            tinyint AS (CASE WHEN Status IN ('Issued','PartiallyPaid') THEN 1 ELSE NULL END) STORED,
    IssuedAtUtc           datetime(6)   NULL,
    VoidedAtUtc           datetime(6)   NULL,
    CreatedAtUtc          datetime(6)   NOT NULL,
    RowVersion            int           NOT NULL DEFAULT 1,
    CONSTRAINT PK_invoices PRIMARY KEY (Id),
    CONSTRAINT UX_invoices_Number UNIQUE (InvoiceNumber),
    CONSTRAINT FK_invoices_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_invoices_parties FOREIGN KEY (BillToPartyId) REFERENCES parties (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_invoices_fee_version FOREIGN KEY (FeeAgreementVersionId) REFERENCES fee_agreement_versions (Id),
    CONSTRAINT CK_invoices_Currency CHECK (CurrencyCode = 'USD'),
    CONSTRAINT CK_invoices_Status CHECK (Status IN ('Draft','Issued','PartiallyPaid','Paid','Void')),
    CONSTRAINT CK_invoices_Total CHECK (Total = Subtotal + TaxTotal),
    CONSTRAINT CK_invoices_Paid CHECK (AmountPaid >= 0 AND AmountPaid <= Total),
    CONSTRAINT CK_invoices_Issued CHECK (Status = 'Draft' OR (IssueDate IS NOT NULL AND DueDate IS NOT NULL)),
    INDEX IX_invoices_Open_DueDate (OpenMarker, DueDate, Id),
    INDEX IX_invoices_Matter_Status (MatterRecordId, Status),
    INDEX IX_invoices_BillTo_Open (BillToPartyId, OpenMarker)
) CHARACTER SET utf8mb4;

-- Ordinary domain table: no tenant column in a firm database.
CREATE TABLE invoice_lines (
    Id             char(36)      COLLATE ascii_general_ci NOT NULL,
    InvoiceId      char(36)      COLLATE ascii_general_ci NOT NULL,
    LineNumber     int           NOT NULL,
    LineType       varchar(16)   COLLATE ascii_general_ci NOT NULL,
    SourceTimeEntryId char(36)   COLLATE ascii_general_ci NULL,
    SourceExpenseId   char(36)   COLLATE ascii_general_ci NULL,
    Description    varchar(1000) NOT NULL,
    Quantity       decimal(12,4) NOT NULL,
    UnitPrice      decimal(19,4) NOT NULL,
    Amount         decimal(19,2) NOT NULL,
    CONSTRAINT PK_invoice_lines PRIMARY KEY (Id),
    CONSTRAINT FK_invoice_lines_invoices FOREIGN KEY (InvoiceId) REFERENCES invoices (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_invoice_lines_time FOREIGN KEY (SourceTimeEntryId) REFERENCES time_entries (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_invoice_lines_expense FOREIGN KEY (SourceExpenseId) REFERENCES expenses (Id) ON DELETE RESTRICT,
    CONSTRAINT UX_invoice_lines_Number UNIQUE (InvoiceId, LineNumber),
    CONSTRAINT UX_invoice_lines_Time UNIQUE (SourceTimeEntryId),        -- an entry is billed once
    CONSTRAINT UX_invoice_lines_Expense UNIQUE (SourceExpenseId),
    CONSTRAINT CK_invoice_lines_Type CHECK (LineType IN ('Time','Expense','Flat','Adjustment')),
    CONSTRAINT CK_invoice_lines_Amount CHECK (LineType = 'Adjustment' OR Amount = ROUND(Quantity * UnitPrice, 2))
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- payments
CREATE TABLE ledger_accounts (
    Id            char(36)     COLLATE ascii_general_ci NOT NULL,
    Code          varchar(16)  COLLATE ascii_general_ci NOT NULL,
    Name          varchar(120) NOT NULL,
    AccountType   varchar(16)  COLLATE ascii_general_ci NOT NULL,
    IsTrust       tinyint(1)   NOT NULL DEFAULT 0,
    BankAccountRef varchar(128) COLLATE ascii_general_ci NULL,       -- reference, never full number
    CurrencyCode  char(3)      COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    IsActive      tinyint(1)   NOT NULL DEFAULT 1,
    CONSTRAINT PK_ledger_accounts PRIMARY KEY (Id),
    CONSTRAINT UX_ledger_accounts_Code UNIQUE (Code),
    CONSTRAINT CK_ledger_accounts_Type CHECK (AccountType IN ('Asset','Liability','Equity','Revenue','Expense')),
    CONSTRAINT CK_ledger_accounts_Currency CHECK (CurrencyCode = 'USD')
) CHARACTER SET utf8mb4;

CREATE TABLE payments (
    Id                 char(36)      COLLATE ascii_general_ci NOT NULL,
    Provider           varchar(32)   COLLATE ascii_general_ci NOT NULL,
    ProviderAccountRef varchar(128)  COLLATE ascii_general_ci NOT NULL,  -- the firm's merchant account
    ProviderPaymentRef varchar(128)  COLLATE ascii_general_ci NOT NULL,
    PayerPartyId       char(36)      COLLATE ascii_general_ci NULL,
    DepositAccountId   char(36)      COLLATE ascii_general_ci NOT NULL,  -- operating or trust
    Method             varchar(16)   COLLATE ascii_general_ci NOT NULL,
    Amount             decimal(19,2) NOT NULL,
    AmountAllocated    decimal(19,2) NOT NULL DEFAULT 0,
    AmountRefunded     decimal(19,2) NOT NULL DEFAULT 0,
    CurrencyCode       char(3)       COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    Status             varchar(16)   COLLATE ascii_general_ci NOT NULL,
    ReceivedAtUtc      datetime(6)   NOT NULL,
    RowVersion         int           NOT NULL DEFAULT 1,
    CONSTRAINT PK_payments PRIMARY KEY (Id),
    CONSTRAINT UX_payments_Provider UNIQUE (Provider, ProviderAccountRef, ProviderPaymentRef),
    CONSTRAINT FK_payments_payer FOREIGN KEY (PayerPartyId) REFERENCES parties (Id),
    CONSTRAINT FK_payments_deposit FOREIGN KEY (DepositAccountId) REFERENCES ledger_accounts (Id),
    CONSTRAINT CK_payments_Amounts CHECK (Amount > 0 AND AmountAllocated >= 0 AND AmountRefunded >= 0
        AND AmountAllocated + AmountRefunded <= Amount),
    CONSTRAINT CK_payments_Currency CHECK (CurrencyCode = 'USD'),
    CONSTRAINT CK_payments_Status CHECK (Status IN ('Pending','Succeeded','Failed','Refunded','PartiallyRefunded','Disputed')),
    INDEX IX_payments_Received (ReceivedAtUtc),
    INDEX IX_payments_Unallocated (Status, ReceivedAtUtc)
) CHARACTER SET utf8mb4;

CREATE TABLE payment_allocations (
    Id             char(36)      COLLATE ascii_general_ci NOT NULL,
    PaymentId      char(36)      COLLATE ascii_general_ci NOT NULL,
    InvoiceId      char(36)      COLLATE ascii_general_ci NOT NULL,
    Amount         decimal(19,2) NOT NULL,
    AllocatedByActorId varchar(128) NOT NULL,
    AllocatedAtUtc datetime(6)   NOT NULL,
    ReversedAtUtc  datetime(6)   NULL,
    CONSTRAINT PK_payment_allocations PRIMARY KEY (Id),
    CONSTRAINT FK_payment_allocations_payments FOREIGN KEY (PaymentId) REFERENCES payments (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_payment_allocations_invoices FOREIGN KEY (InvoiceId) REFERENCES invoices (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_payment_allocations_Amount CHECK (Amount > 0),
    INDEX IX_payment_allocations_Invoice (InvoiceId, ReversedAtUtc),
    INDEX IX_payment_allocations_Payment (PaymentId, ReversedAtUtc)
) CHARACTER SET utf8mb4;

CREATE TABLE refunds (
    Id               char(36)      COLLATE ascii_general_ci NOT NULL,
    PaymentId        char(36)      COLLATE ascii_general_ci NOT NULL,
    ProviderRefundRef varchar(128) COLLATE ascii_general_ci NULL,
    Amount           decimal(19,2) NOT NULL,
    Status           varchar(16)   COLLATE ascii_general_ci NOT NULL,
    RequestedByActorId varchar(128) NOT NULL,
    CreatedAtUtc     datetime(6)   NOT NULL,
    RowVersion       int           NOT NULL DEFAULT 1,
    CONSTRAINT PK_refunds PRIMARY KEY (Id),
    CONSTRAINT FK_refunds_payments FOREIGN KEY (PaymentId) REFERENCES payments (Id) ON DELETE RESTRICT,
    CONSTRAINT UX_refunds_Provider UNIQUE (PaymentId, ProviderRefundRef),
    CONSTRAINT CK_refunds_Amount CHECK (Amount > 0),
    CONSTRAINT CK_refunds_Status CHECK (Status IN ('Requested','Succeeded','Failed'))
) CHARACTER SET utf8mb4;

CREATE TABLE disputes (
    Id                char(36)      COLLATE ascii_general_ci NOT NULL,
    PaymentId         char(36)      COLLATE ascii_general_ci NOT NULL,
    ProviderDisputeRef varchar(128) COLLATE ascii_general_ci NOT NULL,
    Amount            decimal(19,2) NOT NULL,
    ReasonCode        varchar(64)   COLLATE ascii_general_ci NOT NULL,
    Status            varchar(16)   COLLATE ascii_general_ci NOT NULL,
    OpenedAtUtc       datetime(6)   NOT NULL,
    ClosedAtUtc       datetime(6)   NULL,
    CONSTRAINT PK_disputes PRIMARY KEY (Id),
    CONSTRAINT FK_disputes_payments FOREIGN KEY (PaymentId) REFERENCES payments (Id) ON DELETE RESTRICT,
    CONSTRAINT UX_disputes_Provider UNIQUE (PaymentId, ProviderDisputeRef),
    CONSTRAINT CK_disputes_Status CHECK (Status IN ('Open','Won','Lost','Closed'))
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- ledger and trust
CREATE TABLE journal_entries (
    Id              char(36)     COLLATE ascii_general_ci NOT NULL,
    EntryNumber     bigint       NOT NULL,
    EffectiveDate   date         NOT NULL,
    Status          varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Draft',
    SourceType      varchar(32)  COLLATE ascii_general_ci NOT NULL,
    SourceId        char(36)     COLLATE ascii_general_ci NULL,
    ReversesEntryId char(36)     COLLATE ascii_general_ci NULL,
    IdempotencyKey  varchar(128) COLLATE ascii_general_ci NOT NULL,
    Memo            varchar(500) NULL,
    CreatedByActorId varchar(128) NOT NULL,
    PostedByActorId varchar(128) NULL,
    PostedAtUtc     datetime(6)  NULL,
    CreatedAtUtc    datetime(6)  NOT NULL,
    CONSTRAINT PK_journal_entries PRIMARY KEY (Id),
    CONSTRAINT UX_journal_entries_Number UNIQUE (EntryNumber),
    CONSTRAINT UX_journal_entries_Idempotency UNIQUE (IdempotencyKey),
    CONSTRAINT UX_journal_entries_Reverses UNIQUE (ReversesEntryId),       -- reversed at most once
    CONSTRAINT FK_journal_entries_reverses FOREIGN KEY (ReversesEntryId) REFERENCES journal_entries (Id),
    CONSTRAINT CK_journal_entries_Status CHECK (Status IN ('Draft','Posted')),
    CONSTRAINT CK_journal_entries_Posted CHECK (Status = 'Draft' OR (PostedAtUtc IS NOT NULL AND PostedByActorId IS NOT NULL)),
    INDEX IX_journal_entries_Effective (Status, EffectiveDate),
    INDEX IX_journal_entries_Source (SourceType, SourceId)
) CHARACTER SET utf8mb4;

CREATE TABLE journal_lines (
    Id              bigint        NOT NULL AUTO_INCREMENT,
    JournalEntryId  char(36)      COLLATE ascii_general_ci NOT NULL,
    LineNumber      int           NOT NULL,
    LedgerAccountId char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId  char(36)      COLLATE ascii_general_ci NULL,
    ClientPartyId   char(36)      COLLATE ascii_general_ci NULL,      -- trust beneficiary
    Debit           decimal(19,2) NOT NULL DEFAULT 0,
    Credit          decimal(19,2) NOT NULL DEFAULT 0,
    CurrencyCode    char(3)       COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    CONSTRAINT PK_journal_lines PRIMARY KEY (Id),
    CONSTRAINT FK_journal_lines_entries FOREIGN KEY (JournalEntryId) REFERENCES journal_entries (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_journal_lines_accounts FOREIGN KEY (LedgerAccountId) REFERENCES ledger_accounts (Id),
    CONSTRAINT FK_journal_lines_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_journal_lines_parties FOREIGN KEY (ClientPartyId) REFERENCES parties (Id) ON DELETE RESTRICT,
    CONSTRAINT UX_journal_lines_Entry_Line UNIQUE (JournalEntryId, LineNumber),
    CONSTRAINT CK_journal_lines_Side CHECK (Debit >= 0 AND Credit >= 0 AND ((Debit = 0) <> (Credit = 0))),
    CONSTRAINT CK_journal_lines_Currency CHECK (CurrencyCode = 'USD'),
    -- Account activity by date is served through the entry; trust activity by matter/client:
    INDEX IX_journal_lines_Account (LedgerAccountId, JournalEntryId),
    INDEX IX_journal_lines_Trust (LedgerAccountId, MatterRecordId, ClientPartyId, Id)
) CHARACTER SET utf8mb4;

-- Current balance per trust account, matter, and client. Updated in the posting transaction
-- with SELECT ... FOR UPDATE; the CHECK makes an overdraft impossible to commit.
CREATE TABLE trust_subledger_balances (
    TrustAccountId  char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId  char(36)      COLLATE ascii_general_ci NOT NULL,
    ClientPartyId   char(36)      COLLATE ascii_general_ci NOT NULL,
    CurrencyCode    char(3)       COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    Balance         decimal(19,2) NOT NULL DEFAULT 0,
    LastJournalEntryId char(36)   COLLATE ascii_general_ci NULL,
    RowVersion      int           NOT NULL DEFAULT 1,
    CONSTRAINT PK_trust_subledger_balances PRIMARY KEY (TrustAccountId, MatterRecordId, ClientPartyId, CurrencyCode),
    CONSTRAINT FK_trust_subledger_accounts FOREIGN KEY (TrustAccountId) REFERENCES ledger_accounts (Id),
    CONSTRAINT FK_trust_subledger_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_trust_subledger_parties FOREIGN KEY (ClientPartyId) REFERENCES parties (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_trust_subledger_NonNegative CHECK (Balance >= 0)
) CHARACTER SET utf8mb4;

CREATE TABLE trust_reconciliations (
    Id               char(36)      COLLATE ascii_general_ci NOT NULL,
    TrustAccountId   char(36)      COLLATE ascii_general_ci NOT NULL,
    StatementDate    date          NOT NULL,
    StatementBalance decimal(19,2) NOT NULL,
    BookBalance      decimal(19,2) NOT NULL,
    SubledgerTotal   decimal(19,2) NOT NULL,
    Status           varchar(16)   COLLATE ascii_general_ci NOT NULL DEFAULT 'Draft',
    PreparedByActorId varchar(128) NOT NULL,
    ApprovedByActorId varchar(128) NULL,
    ApprovedAtUtc    datetime(6)   NULL,
    CreatedAtUtc     datetime(6)   NOT NULL,
    RowVersion       int           NOT NULL DEFAULT 1,
    CONSTRAINT PK_trust_reconciliations PRIMARY KEY (Id),
    CONSTRAINT FK_trust_reconciliations_accounts FOREIGN KEY (TrustAccountId) REFERENCES ledger_accounts (Id),
    CONSTRAINT UX_trust_reconciliations_Period UNIQUE (TrustAccountId, StatementDate),
    CONSTRAINT CK_trust_reconciliations_Status CHECK (Status IN ('Draft','Balanced','Exception','Approved')),
    CONSTRAINT CK_trust_reconciliations_Approver CHECK (ApprovedByActorId IS NULL OR ApprovedByActorId <> PreparedByActorId),
    CONSTRAINT CK_trust_reconciliations_ThreeWay CHECK (Status NOT IN ('Balanced','Approved')
        OR (StatementBalance = BookBalance AND BookBalance = SubledgerTotal))
) CHARACTER SET utf8mb4;

CREATE TABLE reconciliation_items (
    Id               bigint        NOT NULL AUTO_INCREMENT,
    ReconciliationId char(36)      COLLATE ascii_general_ci NOT NULL,
    ItemType         varchar(24)   COLLATE ascii_general_ci NOT NULL,   -- OutstandingCheck | DepositInTransit | BankError | Unmatched
    JournalLineId    bigint        NULL,
    BankReference    varchar(128)  NULL,
    Amount           decimal(19,2) NOT NULL,
    ResolvedAtUtc    datetime(6)   NULL,
    CONSTRAINT PK_reconciliation_items PRIMARY KEY (Id),
    CONSTRAINT FK_reconciliation_items_recon FOREIGN KEY (ReconciliationId) REFERENCES trust_reconciliations (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_reconciliation_items_line FOREIGN KEY (JournalLineId) REFERENCES journal_lines (Id),
    INDEX IX_reconciliation_items_Recon (ReconciliationId, ItemType)
) CHARACTER SET utf8mb4;

-- Version-bound approvals: an approval row references (Id, ContentSha256) of the exact
-- version it approved. Any change produces a new version with a new hash, so old approvals
-- cannot be attached to it by the database.
CREATE TABLE trust_disbursement_requests (
    Id             char(36)      COLLATE ascii_general_ci NOT NULL,
    TrustAccountId char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId char(36)      COLLATE ascii_general_ci NOT NULL,
    ClientPartyId  char(36)      COLLATE ascii_general_ci NOT NULL,
    PayeePartyId   char(36)      COLLATE ascii_general_ci NOT NULL,
    Amount         decimal(19,2) NOT NULL,
    CurrencyCode   char(3)       COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    ContentSha256  binary(32)    NOT NULL,
    Status         varchar(16)   COLLATE ascii_general_ci NOT NULL DEFAULT 'Pending',
    JournalEntryId char(36)      COLLATE ascii_general_ci NULL,
    RequestedByActorId varchar(128) NOT NULL,
    CreatedAtUtc   datetime(6)   NOT NULL,
    RowVersion     int           NOT NULL DEFAULT 1,
    CONSTRAINT PK_trust_disbursement_requests PRIMARY KEY (Id),
    CONSTRAINT UX_trust_disbursement_requests_IdHash UNIQUE (Id, ContentSha256),
    CONSTRAINT FK_trust_disbursement_requests_balance FOREIGN KEY (TrustAccountId, MatterRecordId, ClientPartyId, CurrencyCode)
        REFERENCES trust_subledger_balances (TrustAccountId, MatterRecordId, ClientPartyId, CurrencyCode),
    CONSTRAINT FK_trust_disbursement_requests_payee FOREIGN KEY (PayeePartyId) REFERENCES parties (Id),
    CONSTRAINT FK_trust_disbursement_requests_entry FOREIGN KEY (JournalEntryId) REFERENCES journal_entries (Id),
    CONSTRAINT CK_trust_disbursement_requests_Amount CHECK (Amount > 0),
    CONSTRAINT CK_trust_disbursement_requests_Status CHECK (Status IN ('Pending','Approved','Rejected','Disbursed','Cancelled'))
) CHARACTER SET utf8mb4;

CREATE TABLE trust_disbursement_approvals (
    Id               char(36)     COLLATE ascii_general_ci NOT NULL,
    RequestId        char(36)     COLLATE ascii_general_ci NOT NULL,
    ApprovedContentSha256 binary(32) NOT NULL,
    ApproverActorId  varchar(128) NOT NULL,
    Decision         varchar(16)  COLLATE ascii_general_ci NOT NULL,
    DecidedAtUtc     datetime(6)  NOT NULL,
    CONSTRAINT PK_trust_disbursement_approvals PRIMARY KEY (Id),
    CONSTRAINT FK_trust_disbursement_approvals_version FOREIGN KEY (RequestId, ApprovedContentSha256)
        REFERENCES trust_disbursement_requests (Id, ContentSha256) ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT UX_trust_disbursement_approvals UNIQUE (RequestId, ApproverActorId),
    CONSTRAINT CK_trust_disbursement_approvals_Decision CHECK (Decision IN ('Approved','Rejected'))
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- settlements and liens (PI)
CREATE TABLE liens (
    Id                char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId    char(36)      COLLATE ascii_general_ci NOT NULL,
    LienholderPartyId char(36)      COLLATE ascii_general_ci NOT NULL,
    LienType          varchar(24)   COLLATE ascii_general_ci NOT NULL,
    AssertedAmount    decimal(19,2) NOT NULL,
    NegotiatedAmount  decimal(19,2) NULL,
    Status            varchar(16)   COLLATE ascii_general_ci NOT NULL DEFAULT 'Asserted',
    CreatedAtUtc      datetime(6)   NOT NULL,
    RowVersion        int           NOT NULL DEFAULT 1,
    CONSTRAINT PK_liens PRIMARY KEY (Id),
    CONSTRAINT FK_liens_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_liens_parties FOREIGN KEY (LienholderPartyId) REFERENCES parties (Id),
    CONSTRAINT CK_liens_Type CHECK (LienType IN ('Medical','Medicare','Medicaid','ERISA','Attorney','Mechanics','Other')),
    CONSTRAINT CK_liens_Status CHECK (Status IN ('Asserted','Negotiating','Resolved','Released','Disputed')),
    INDEX IX_liens_Matter_Status (MatterRecordId, Status)
) CHARACTER SET utf8mb4;

CREATE TABLE settlements (
    Id             char(36)    COLLATE ascii_general_ci NOT NULL,
    MatterRecordId char(36)    COLLATE ascii_general_ci NOT NULL,
    Status         varchar(16) COLLATE ascii_general_ci NOT NULL DEFAULT 'Negotiating',
    CreatedAtUtc   datetime(6) NOT NULL,
    RowVersion     int         NOT NULL DEFAULT 1,
    CONSTRAINT PK_settlements PRIMARY KEY (Id),
    CONSTRAINT FK_settlements_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_settlements_Status CHECK (Status IN ('Negotiating','Agreed','Funded','Distributed','Closed'))
) CHARACTER SET utf8mb4;

CREATE TABLE settlement_versions (
    Id             char(36)      COLLATE ascii_general_ci NOT NULL,
    SettlementId   char(36)      COLLATE ascii_general_ci NOT NULL,
    VersionNumber  int           NOT NULL,
    GrossAmount    decimal(19,2) NOT NULL,
    CurrencyCode   char(3)       COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    ContentSha256  binary(32)    NOT NULL,          -- hash of canonical version + lines
    Status         varchar(16)   COLLATE ascii_general_ci NOT NULL DEFAULT 'Draft',
    CreatedByActorId varchar(128) NOT NULL,
    CreatedAtUtc   datetime(6)   NOT NULL,
    CONSTRAINT PK_settlement_versions PRIMARY KEY (Id),
    CONSTRAINT FK_settlement_versions_settlements FOREIGN KEY (SettlementId) REFERENCES settlements (Id) ON DELETE RESTRICT,
    CONSTRAINT UX_settlement_versions UNIQUE (SettlementId, VersionNumber),
    CONSTRAINT UX_settlement_versions_IdHash UNIQUE (Id, ContentSha256),
    CONSTRAINT CK_settlement_versions_Gross CHECK (GrossAmount > 0),
    CONSTRAINT CK_settlement_versions_Status CHECK (Status IN ('Draft','PendingApproval','Approved','Superseded'))
) CHARACTER SET utf8mb4;

CREATE TABLE settlement_distribution_lines (
    SettlementVersionId char(36)      COLLATE ascii_general_ci NOT NULL,
    LineNumber          int           NOT NULL,
    LineKind            varchar(24)   COLLATE ascii_general_ci NOT NULL,
    PayeePartyId        char(36)      COLLATE ascii_general_ci NOT NULL,
    LienId              char(36)      COLLATE ascii_general_ci NULL,
    Amount              decimal(19,2) NOT NULL,
    CONSTRAINT PK_settlement_distribution_lines PRIMARY KEY (SettlementVersionId, LineNumber),
    CONSTRAINT FK_settlement_lines_versions FOREIGN KEY (SettlementVersionId) REFERENCES settlement_versions (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_settlement_lines_payee FOREIGN KEY (PayeePartyId) REFERENCES parties (Id),
    CONSTRAINT FK_settlement_lines_lien FOREIGN KEY (LienId) REFERENCES liens (Id),
    CONSTRAINT CK_settlement_lines_Kind CHECK (LineKind IN ('AttorneyFee','CostReimbursement','LienPayment','ClientNet','Other')),
    CONSTRAINT CK_settlement_lines_Amount CHECK (Amount >= 0)
) CHARACTER SET utf8mb4;

CREATE TABLE settlement_approvals (
    Id                    char(36)     COLLATE ascii_general_ci NOT NULL,
    SettlementVersionId   char(36)     COLLATE ascii_general_ci NOT NULL,
    ApprovedContentSha256 binary(32)   NOT NULL,
    ApproverKind          varchar(16)  COLLATE ascii_general_ci NOT NULL,   -- Attorney | Client
    ApproverActorId       varchar(128) NOT NULL,
    SignatureDocumentVersionId char(36) COLLATE ascii_general_ci NULL,
    ApprovedAtUtc         datetime(6)  NOT NULL,
    CONSTRAINT PK_settlement_approvals PRIMARY KEY (Id),
    CONSTRAINT FK_settlement_approvals_version FOREIGN KEY (SettlementVersionId, ApprovedContentSha256)
        REFERENCES settlement_versions (Id, ContentSha256) ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT UX_settlement_approvals UNIQUE (SettlementVersionId, ApproverKind, ApproverActorId),
    CONSTRAINT CK_settlement_approvals_Kind CHECK (ApproverKind IN ('Attorney','Client'))
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- reliability
CREATE TABLE import_batches (
    Id             char(36)     COLLATE ascii_general_ci NOT NULL,
    SourceSystem   varchar(64)  COLLATE ascii_general_ci NOT NULL,
    FileSha256     binary(32)   NOT NULL,
    Status         varchar(16)  COLLATE ascii_general_ci NOT NULL DEFAULT 'Staged',
    RowsTotal      int          NOT NULL DEFAULT 0,
    RowsApplied    int          NOT NULL DEFAULT 0,
    RowsRejected   int          NOT NULL DEFAULT 0,
    StartedByActorId varchar(128) NOT NULL,
    CreatedAtUtc   datetime(6)  NOT NULL,
    CompletedAtUtc datetime(6)  NULL,
    CONSTRAINT PK_import_batches PRIMARY KEY (Id),
    CONSTRAINT UX_import_batches_File UNIQUE (SourceSystem, FileSha256),
    CONSTRAINT CK_import_batches_Status CHECK (Status IN ('Staged','Validating','Applying','Applied','Failed','RolledBack'))
) CHARACTER SET utf8mb4;

-- Source key to target id map; makes re-import idempotent and lets a batch be rolled back.
CREATE TABLE import_row_mappings (
    SourceSystem  varchar(64)  COLLATE ascii_general_ci NOT NULL,
    SourceType    varchar(48)  COLLATE ascii_general_ci NOT NULL,
    SourceKey     varchar(191) COLLATE ascii_general_ci NOT NULL,
    TargetId      char(36)     COLLATE ascii_general_ci NOT NULL,
    BatchId       char(36)     COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT PK_import_row_mappings PRIMARY KEY (SourceSystem, SourceType, SourceKey),
    CONSTRAINT FK_import_row_mappings_batches FOREIGN KEY (BatchId) REFERENCES import_batches (Id),
    INDEX IX_import_row_mappings_Batch (BatchId)
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- construction extension
CREATE TABLE construction_projects (
    Id               char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId   char(36)     COLLATE ascii_general_ci NOT NULL,
    ProjectName      varchar(200) NOT NULL,
    SiteAddress      varchar(400) NULL,
    Jurisdiction     varchar(64)  NOT NULL,
    IsPublicWork     tinyint(1)   NOT NULL DEFAULT 0,
    OwnerPartyId     char(36)     COLLATE ascii_general_ci NULL,
    CreatedAtUtc     datetime(6)  NOT NULL,
    CONSTRAINT PK_construction_projects PRIMARY KEY (Id),
    CONSTRAINT FK_construction_projects_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_construction_projects_owner FOREIGN KEY (OwnerPartyId) REFERENCES parties (Id),
    INDEX IX_construction_projects_Matter (MatterRecordId)
) CHARACTER SET utf8mb4;

CREATE TABLE construction_contracts (
    Id             char(36)      COLLATE ascii_general_ci NOT NULL,
    ProjectId      char(36)      COLLATE ascii_general_ci NOT NULL,
    ContractTier   varchar(16)   COLLATE ascii_general_ci NOT NULL,      -- Prime | Subcontract | Supply
    PayorPartyId   char(36)      COLLATE ascii_general_ci NOT NULL,
    PayeePartyId   char(36)      COLLATE ascii_general_ci NOT NULL,
    ContractSum    decimal(19,2) NOT NULL,
    CurrencyCode   char(3)       COLLATE ascii_general_ci NOT NULL DEFAULT 'USD',
    ExecutedDate   date          NULL,
    DocumentVersionId char(36)   COLLATE ascii_general_ci NULL,
    CONSTRAINT PK_construction_contracts PRIMARY KEY (Id),
    CONSTRAINT FK_construction_contracts_projects FOREIGN KEY (ProjectId) REFERENCES construction_projects (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_construction_contracts_payor FOREIGN KEY (PayorPartyId) REFERENCES parties (Id),
    CONSTRAINT FK_construction_contracts_payee FOREIGN KEY (PayeePartyId) REFERENCES parties (Id),
    CONSTRAINT FK_construction_contracts_doc FOREIGN KEY (DocumentVersionId) REFERENCES document_versions (Id),
    CONSTRAINT CK_construction_contracts_Tier CHECK (ContractTier IN ('Prime','Subcontract','Supply'))
) CHARACTER SET utf8mb4;

CREATE TABLE change_orders (
    Id             char(36)      COLLATE ascii_general_ci NOT NULL,
    ContractId     char(36)      COLLATE ascii_general_ci NOT NULL,
    Number         varchar(32)   NOT NULL,
    Amount         decimal(19,2) NOT NULL,          -- may be negative (deductive change)
    TimeImpactDays int           NULL,
    Status         varchar(16)   COLLATE ascii_general_ci NOT NULL,
    RequestedDate  date          NULL,
    ApprovedDate   date          NULL,
    CONSTRAINT PK_change_orders PRIMARY KEY (Id),
    CONSTRAINT FK_change_orders_contracts FOREIGN KEY (ContractId) REFERENCES construction_contracts (Id) ON DELETE RESTRICT,
    CONSTRAINT UX_change_orders_Number UNIQUE (ContractId, Number),
    CONSTRAINT CK_change_orders_Status CHECK (Status IN ('Proposed','Approved','Rejected','Disputed'))
) CHARACTER SET utf8mb4;

-- Statutory notices (preliminary notice, mechanics lien, stop payment, bond claim).
CREATE TABLE construction_notices (
    Id            char(36)    COLLATE ascii_general_ci NOT NULL,
    ProjectId     char(36)    COLLATE ascii_general_ci NOT NULL,
    NoticeType    varchar(24) COLLATE ascii_general_ci NOT NULL,
    ServedDate    date        NULL,
    RecordedDate  date        NULL,
    RecordingRef  varchar(128) NULL,
    DeadlineId    char(36)    COLLATE ascii_general_ci NULL,
    CONSTRAINT PK_construction_notices PRIMARY KEY (Id),
    CONSTRAINT FK_construction_notices_projects FOREIGN KEY (ProjectId) REFERENCES construction_projects (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_construction_notices_deadlines FOREIGN KEY (DeadlineId) REFERENCES deadlines (Id),
    CONSTRAINT CK_construction_notices_Type CHECK (NoticeType IN ('Preliminary','MechanicsLien','StopPayment','BondClaim','Other'))
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- personal injury extension
CREATE TABLE pi_incidents (
    Id             char(36)     COLLATE ascii_general_ci NOT NULL,
    MatterRecordId char(36)     COLLATE ascii_general_ci NOT NULL,
    IncidentType   varchar(32)  COLLATE ascii_general_ci NOT NULL,
    IncidentDate   date         NOT NULL,
    IncidentTimeLocal time      NULL,
    TimeZone       varchar(64)  COLLATE ascii_general_ci NOT NULL,
    Jurisdiction   varchar(64)  NOT NULL,
    LocationDescription varchar(400) NULL,
    LimitationsDeadlineId char(36) COLLATE ascii_general_ci NULL,
    CONSTRAINT PK_pi_incidents PRIMARY KEY (Id),
    CONSTRAINT FK_pi_incidents_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_pi_incidents_deadlines FOREIGN KEY (LimitationsDeadlineId) REFERENCES deadlines (Id),
    INDEX IX_pi_incidents_Matter (MatterRecordId)
) CHARACTER SET utf8mb4;

CREATE TABLE insurance_policies (
    Id                 char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId     char(36)      COLLATE ascii_general_ci NOT NULL,
    CarrierPartyId     char(36)      COLLATE ascii_general_ci NOT NULL,
    CoverageType       varchar(32)   COLLATE ascii_general_ci NOT NULL,
    PolicyNumber       varchar(64)   NULL,                 -- sensitive
    ClaimNumber        varchar(64)   NULL,                 -- sensitive
    LimitPerPerson     decimal(19,2) NULL,
    LimitPerOccurrence decimal(19,2) NULL,
    CONSTRAINT PK_insurance_policies PRIMARY KEY (Id),
    CONSTRAINT FK_insurance_policies_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_insurance_policies_carrier FOREIGN KEY (CarrierPartyId) REFERENCES parties (Id),
    INDEX IX_insurance_policies_Matter (MatterRecordId)
) CHARACTER SET utf8mb4;

-- Health information. Restricted permission to read; never in logs, search snippets, or notifications.
CREATE TABLE treatment_episodes (
    Id               char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId   char(36)      COLLATE ascii_general_ci NOT NULL,
    ProviderPartyId  char(36)      COLLATE ascii_general_ci NOT NULL,
    StartDate        date          NOT NULL,
    EndDate          date          NULL,
    Summary          text          NULL,
    BilledAmount     decimal(19,2) NULL,
    PaidAmount       decimal(19,2) NULL,
    CONSTRAINT PK_treatment_episodes PRIMARY KEY (Id),
    CONSTRAINT FK_treatment_episodes_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_treatment_episodes_provider FOREIGN KEY (ProviderPartyId) REFERENCES parties (Id),
    CONSTRAINT CK_treatment_episodes_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate),
    INDEX IX_treatment_episodes_Matter_Start (MatterRecordId, StartDate)
) CHARACTER SET utf8mb4;

CREATE TABLE medical_record_requests (
    Id                char(36)    COLLATE ascii_general_ci NOT NULL,
    MatterRecordId    char(36)    COLLATE ascii_general_ci NOT NULL,
    ProviderPartyId   char(36)    COLLATE ascii_general_ci NOT NULL,
    AuthorizationDocumentVersionId char(36) COLLATE ascii_general_ci NULL,
    RequestedDate     date        NOT NULL,
    Status            varchar(16) COLLATE ascii_general_ci NOT NULL,
    ReceivedDocumentVersionId char(36) COLLATE ascii_general_ci NULL,
    CONSTRAINT PK_medical_record_requests PRIMARY KEY (Id),
    CONSTRAINT FK_medical_record_requests_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_medical_record_requests_provider FOREIGN KEY (ProviderPartyId) REFERENCES parties (Id),
    CONSTRAINT FK_medical_record_requests_auth FOREIGN KEY (AuthorizationDocumentVersionId) REFERENCES document_versions (Id),
    CONSTRAINT FK_medical_record_requests_recv FOREIGN KEY (ReceivedDocumentVersionId) REFERENCES document_versions (Id),
    CONSTRAINT CK_medical_record_requests_Status CHECK (Status IN ('Requested','FollowUp','Received','Refused')),
    INDEX IX_medical_record_requests_Status (Status, RequestedDate)
) CHARACTER SET utf8mb4;

CREATE TABLE damage_items (
    Id             char(36)      COLLATE ascii_general_ci NOT NULL,
    MatterRecordId char(36)      COLLATE ascii_general_ci NOT NULL,
    Category       varchar(32)   COLLATE ascii_general_ci NOT NULL,
    Description    varchar(500)  NOT NULL,
    Amount         decimal(19,2) NOT NULL,
    SupportingDocumentVersionId char(36) COLLATE ascii_general_ci NULL,
    CONSTRAINT PK_damage_items PRIMARY KEY (Id),
    CONSTRAINT FK_damage_items_matters FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_damage_items_doc FOREIGN KEY (SupportingDocumentVersionId) REFERENCES document_versions (Id),
    CONSTRAINT CK_damage_items_Category CHECK (Category IN ('MedicalExpense','LostWages','PropertyDamage','FutureCare','NonEconomic','Other')),
    INDEX IX_damage_items_Matter (MatterRecordId)
) CHARACTER SET utf8mb4;

-- ---------------------------------------------------------------- audit index and ledger triggers
CREATE INDEX IX_audit_events_Object ON audit_events (ObjectType, ObjectId, OccurredAtUtc);

-- Ledger triggers live in 13-firm-ledger-triggers.sql (they need a privileged creation step).
