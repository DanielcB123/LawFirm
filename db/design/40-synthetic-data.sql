-- Synthetic data generator for DISPOSABLE validation databases only. All names are fabricated.
-- Requires session variables set by the caller:
--   @scale  multiplier for row counts (1.0 = "medium firm" profile, see capacity doc)
--   @firm   short label embedded in synthetic text ('A' or 'B')
-- Ids are deterministic (zz_sid(type, n)), so two firm databases deliberately contain
-- OVERLAPPING local ids for different rows. Isolation must not depend on id uniqueness.
-- Distributions use modular arithmetic so runs are reproducible.

SET @now = TIMESTAMP('2026-10-12 00:00:00');
SET @today = DATE(@now);
SET @n_parties = FLOOR(20000 * @scale), @n_clients = FLOOR(3000 * @scale),
    @n_matters = FLOOR(5000 * @scale),  @n_tasks = FLOOR(100000 * @scale),
    @n_deadlines = FLOOR(25000 * @scale), @n_docs = FLOOR(60000 * @scale),
    @n_convs = FLOOR(8000 * @scale),    @n_msgs = FLOOR(300000 * @scale),
    @n_hot = FLOOR(20000 * @scale),     @n_invoices = FLOOR(20000 * @scale),
    @n_payments = FLOOR(15000 * @scale), @n_allocs = FLOOR(18000 * @scale),
    @n_entries = FLOOR(30000 * @scale), @n_intake = FLOOR(6000 * @scale),
    @n_consults = FLOOR(8000 * @scale), @n_outbox = FLOOR(200000 * @scale),
    @n_webhooks = FLOOR(30000 * @scale), @n_audit = FLOOR(100000 * @scale),
    @n_staff_grants = FLOOR(2000 * @scale);

DROP FUNCTION IF EXISTS zz_sid;
DELIMITER $$
CREATE FUNCTION zz_sid(t INT, n INT) RETURNS char(36) CHARSET ascii DETERMINISTIC NO SQL
RETURN CONCAT(LPAD(HEX(t), 8, '0'), '-0000-4000-8000-', LPAD(HEX(n), 12, '0'))$$
DELIMITER ;

DROP TABLE IF EXISTS zz_digits, zz_nums;
CREATE TABLE zz_digits (d int PRIMARY KEY);
INSERT INTO zz_digits VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9);
CREATE TABLE zz_nums (n int PRIMARY KEY)
SELECT 1 + a.d + 10*b.d + 100*c.d + 1000*d.d + 10000*e.d + 100000*f.d AS n
FROM zz_digits a, zz_digits b, zz_digits c, zz_digits d, zz_digits e, zz_digits f;

-- Staff: u-s-001..015 attorneys, 016..030 paralegals, 031..040 admins.
INSERT INTO firm_users (ActorId, UserKind, DisplayName, RoleKey, Status, CreatedAtUtc)
SELECT CONCAT('u-s-', LPAD(n, 3, '0')), 'Staff', CONCAT('Synthetic Staff ', @firm, n),
       CASE WHEN n <= 15 THEN 'Attorney' WHEN n <= 30 THEN 'Paralegal' ELSE 'Admin' END, 'Active', @now
FROM zz_nums WHERE n <= 40;

INSERT INTO parties (Id, PartyType, DisplayName, PrimaryEmail, PrimaryPhone, IsRestricted, CreatedAtUtc)
SELECT zz_sid(2, n), IF(n % 5 = 0, 'Organization', 'Person'), CONCAT('Synthetic Party ', @firm, '-', n),
       NULL, NULL, n % 50 = 0, @now - INTERVAL n MINUTE
FROM zz_nums WHERE n <= @n_parties;

INSERT INTO firm_users (ActorId, UserKind, DisplayName, RoleKey, Status, ClientPartyId, CreatedAtUtc)
SELECT CONCAT('u-c-', n), 'Client', CONCAT('Synthetic Client ', @firm, n), 'Client', 'Active', zz_sid(2, n), @now
FROM zz_nums WHERE n <= @n_clients;

-- Matters: 30% Active, 60% Closed, 10% Pending; 15 attorneys; two thirds have a paralegal.
INSERT INTO matters (Id, MatterNumber, Title, PracticeArea, Stage, Status, ResponsibleAttorneyActorId,
    ResponsibleAttorneyDisplayName, ParalegalActorId, ParalegalDisplayName, LinkedIntakeRecordId, CreatedByActorId, CreatedAtUtc)
SELECT zz_sid(1, n), CONCAT('MAT-', LPAD(n, 6, '0')), CONCAT('Synthetic matter ', @firm, '-', n),
       ELT(1 + n % 3, 'ConstructionLaw', 'Litigation', 'PersonalInjury'), 'Open',
       CASE WHEN n % 10 < 3 THEN 'Active' WHEN n % 10 < 9 THEN 'Closed' ELSE 'Pending' END,
       CONCAT('u-s-', LPAD(1 + n % 15, 3, '0')), 'Synthetic Attorney',
       IF(n % 3 = 0, NULL, CONCAT('u-s-', LPAD(16 + n % 15, 3, '0'))), IF(n % 3 = 0, NULL, 'Synthetic Paralegal'),
       NULL, 'u-s-031', @now - INTERVAL ((n * 37) % 1500) DAY - INTERVAL n SECOND
FROM zz_nums WHERE n <= @n_matters;

INSERT INTO matter_assignments (Id, MatterRecordId, ActorId, AssignmentRole, AssignedByActorId, AssignedAtUtc)
SELECT zz_sid(3, n), zz_sid(1, n), CONCAT('u-s-', LPAD(1 + n % 15, 3, '0')), 'ResponsibleAttorney', 'u-s-031', @now
FROM zz_nums WHERE n <= @n_matters;
INSERT INTO matter_assignments (Id, MatterRecordId, ActorId, AssignmentRole, AssignedByActorId, AssignedAtUtc)
SELECT zz_sid(4, n), zz_sid(1, n), CONCAT('u-s-', LPAD(16 + n % 15, 3, '0')), 'Paralegal', 'u-s-031', @now
FROM zz_nums WHERE n <= @n_matters AND n % 3 <> 0;

INSERT INTO matter_access_grants (Id, MatterRecordId, ActorId, GrantType, GrantedByActorId, GrantedAtUtc)
SELECT zz_sid(5, n), zz_sid(1, 1 + (n * 7) % @n_matters), CONCAT('u-c-', n), 'Client', 'u-s-001', @now
FROM zz_nums WHERE n <= @n_clients;
INSERT IGNORE INTO matter_access_grants (Id, MatterRecordId, ActorId, GrantType, GrantedByActorId, GrantedAtUtc, RevokedByActorId, RevokedAtUtc)
SELECT zz_sid(6, n), zz_sid(1, 1 + (n * 13) % @n_matters), CONCAT('u-s-', LPAD(1 + n % 30, 3, '0')), 'Staff', 'u-s-001', @now,
       IF(n % 5 = 0, 'u-s-001', NULL), IF(n % 5 = 0, @now, NULL)
FROM zz_nums WHERE n <= @n_staff_grants;

-- Tasks: 12.5% Open, 12.5% InProgress, 75% Completed; due dates mostly in the past.
INSERT INTO matter_tasks (Id, MatterRecordId, Title, Description, Status, Priority, AssigneeActorId, AssigneeDisplayName,
    DueAtUtc, IsDeadlineVerified, CompletedAtUtc, CreatedByActorId, CreatedByDisplayName, CreatedAtUtc)
SELECT zz_sid(7, n), zz_sid(1, 1 + n % @n_matters), CONCAT('Synthetic task ', n), NULL,
       CASE n % 8 WHEN 0 THEN 'Open' WHEN 1 THEN 'InProgress' ELSE 'Completed' END,
       ELT(1 + n % 3, 'Low', 'Normal', 'High'),
       CONCAT('u-s-', LPAD(1 + n % 30, 3, '0')), 'Synthetic Staff',
       @now + INTERVAL ((n * 31) % 730 - 600) DAY, 0,
       IF(n % 8 > 1, @now - INTERVAL 1 DAY, NULL), 'u-s-031', 'Synthetic Admin', @now - INTERVAL 800 DAY
FROM zz_nums WHERE n <= @n_tasks;

INSERT INTO deadlines (Id, MatterRecordId, DeadlineKind, DueDate, TimeZone, Jurisdiction, Status, ResponsibleActorId,
    VerifiedByActorId, VerifiedAtUtc, CreatedAtUtc)
SELECT zz_sid(8, n), zz_sid(1, 1 + n % @n_matters), ELT(1 + n % 4, 'Filing', 'Discovery', 'Limitations', 'Notice'),
       @today + INTERVAL ((n * 17) % 900 - 600) DAY, 'America/Chicago', 'Synthetic-Jurisdiction',
       CASE WHEN (n * 17) % 900 < 600 AND n % 10 < 9 THEN 'Completed' WHEN n % 2 = 0 THEN 'Verified' ELSE 'Unverified' END,
       CONCAT('u-s-', LPAD(1 + n % 15, 3, '0')),
       IF(n % 2 = 0 OR ((n * 17) % 900 < 600 AND n % 10 < 9), 'u-s-001', NULL),
       IF(n % 2 = 0 OR ((n * 17) % 900 < 600 AND n % 10 < 9), @now, NULL), @now
FROM zz_nums WHERE n <= @n_deadlines;

INSERT INTO matter_documents (Id, MatterRecordId, FileName, DocumentType, StorageKey, UploadedByActorId, UploadedByDisplayName,
    UploadedAtUtc, ReviewStatus)
SELECT zz_sid(9, n), zz_sid(1, 1 + n % @n_matters), CONCAT('synthetic-', n, '.pdf'), ELT(1 + n % 4, 'Pleading', 'Correspondence', 'Evidence', 'Contract'),
       NULL, 'u-s-016', 'Synthetic Paralegal', @now - INTERVAL ((n * 11) % 1500) DAY - INTERVAL n SECOND,
       IF(n % 4 = 0, 'Pending', 'Approved')
FROM zz_nums WHERE n <= @n_docs;
INSERT INTO document_versions (Id, DocumentId, VersionNumber, StorageKey, ContentSha256, SizeBytes, ContentType, ScanStatus, UploadedByActorId, UploadedAtUtc)
SELECT zz_sid(12, n), zz_sid(9, n), 1, CONCAT('firm/', @firm, '/', zz_sid(12, n)), UNHEX(SHA2(CONCAT(@firm, n, 'v1'), 256)),
       10000 + n % 900000, 'application/pdf', 'Clean', 'u-s-016', @now
FROM zz_nums WHERE n <= @n_docs;
INSERT INTO document_versions (Id, DocumentId, VersionNumber, StorageKey, ContentSha256, SizeBytes, ContentType, ScanStatus, UploadedByActorId, UploadedAtUtc)
SELECT zz_sid(13, n), zz_sid(9, n), 2, CONCAT('firm/', @firm, '/', zz_sid(13, n)), UNHEX(SHA2(CONCAT(@firm, n, 'v2'), 256)),
       10000 + n % 900000, 'application/pdf', 'Clean', 'u-s-016', @now
FROM zz_nums WHERE n <= @n_docs AND n % 2 = 0;

INSERT INTO conversations (Id, MatterRecordId, Kind, Subject, CreatedAtUtc)
SELECT zz_sid(10, n), zz_sid(1, 1 + n % @n_matters), IF(n % 2 = 0, 'ClientPortal', 'Internal'), CONCAT('Synthetic thread ', n), @now
FROM zz_nums WHERE n <= @n_convs;
-- One hot conversation (id n=1) holds @n_hot messages; the rest spread evenly.
INSERT INTO messages (PublicId, ConversationId, SenderActorId, Body, SentAtUtc)
SELECT zz_sid(11, n), IF(n <= @n_hot, zz_sid(10, 1), zz_sid(10, 2 + n % (@n_convs - 1))),
       CONCAT('u-s-', LPAD(1 + n % 30, 3, '0')), 'Synthetic message body', @now - INTERVAL (@n_msgs - n) SECOND
FROM zz_nums WHERE n <= @n_msgs;

INSERT INTO ledger_accounts (Id, Code, Name, AccountType, IsTrust, CurrencyCode) VALUES
 (zz_sid(20, 1), '1000', 'Operating bank', 'Asset', 0, 'USD'),
 (zz_sid(20, 2), '1050', 'Client trust bank', 'Asset', 1, 'USD'),
 (zz_sid(20, 3), '2050', 'Client trust liability', 'Liability', 1, 'USD'),
 (zz_sid(20, 4), '1200', 'Accounts receivable', 'Asset', 0, 'USD'),
 (zz_sid(20, 5), '4000', 'Fee revenue', 'Revenue', 0, 'USD');

-- Invoices: 70% Paid, 15% Issued, 5% PartiallyPaid, 5% Draft, 5% Void.
INSERT INTO invoices (Id, InvoiceNumber, MatterRecordId, BillToPartyId, CurrencyCode, Status, IssueDate, DueDate,
    Subtotal, TaxTotal, Total, AmountPaid, CreatedAtUtc)
SELECT zz_sid(30, n), CONCAT('INV-', LPAD(n, 7, '0')), zz_sid(1, 1 + n % @n_matters), zz_sid(2, 1 + n % @n_clients), 'USD',
       CASE WHEN n % 20 < 14 THEN 'Paid' WHEN n % 20 < 17 THEN 'Issued' WHEN n % 20 = 17 THEN 'PartiallyPaid'
            WHEN n % 20 = 18 THEN 'Draft' ELSE 'Void' END,
       IF(n % 20 = 18, NULL, @today - INTERVAL ((n * 3) % 700) DAY),
       IF(n % 20 = 18, NULL, @today - INTERVAL ((n * 3) % 700) DAY + INTERVAL 30 DAY),
       100 + (n % 50) * 25, 0, 100 + (n % 50) * 25,
       CASE WHEN n % 20 < 14 THEN 100 + (n % 50) * 25 WHEN n % 20 = 17 THEN ROUND((100 + (n % 50) * 25) / 2, 2) ELSE 0 END,
       @now
FROM zz_nums WHERE n <= @n_invoices;
INSERT INTO invoice_lines (Id, InvoiceId, LineNumber, LineType, Description, Quantity, UnitPrice, Amount)
SELECT zz_sid(31, n), zz_sid(30, 1 + (n - 1) % @n_invoices), 1 + FLOOR((n - 1) / @n_invoices), 'Flat',
       'Synthetic line', 1, 25, 25
FROM zz_nums WHERE n <= @n_invoices * 4;

INSERT INTO payments (Id, Provider, ProviderAccountRef, ProviderPaymentRef, PayerPartyId, DepositAccountId, Method, Amount,
    CurrencyCode, Status, ReceivedAtUtc)
SELECT zz_sid(32, n), 'synthetic-pay', CONCAT('acct_synthetic_', @firm), CONCAT('pay_', n), zz_sid(2, 1 + n % @n_clients),
       zz_sid(20, 1), 'Card', 500, 'USD', 'Succeeded', @now - INTERVAL n MINUTE
FROM zz_nums WHERE n <= @n_payments;
INSERT INTO payment_allocations (Id, PaymentId, InvoiceId, Amount, AllocatedByActorId, AllocatedAtUtc, ReversedAtUtc)
SELECT zz_sid(33, n), zz_sid(32, 1 + n % @n_payments), zz_sid(30, 1 + n % @n_invoices), 10, 'u-s-031', @now,
       IF(n % 50 = 0, @now, NULL)
FROM zz_nums WHERE n <= @n_allocs;

-- Journal: every third entry is a trust deposit tied to a matter and client.
INSERT INTO journal_entries (Id, EntryNumber, EffectiveDate, Status, SourceType, IdempotencyKey, CreatedByActorId, CreatedAtUtc)
SELECT zz_sid(40, n), n, @today - INTERVAL ((n * 7) % 730) DAY, 'Draft', 'Synthetic', CONCAT('syn-', n), 'u-s-031', @now
FROM zz_nums WHERE n <= @n_entries;
INSERT INTO journal_lines (JournalEntryId, LineNumber, LedgerAccountId, MatterRecordId, ClientPartyId, Debit, Credit, CurrencyCode)
SELECT zz_sid(40, n), 1, IF(n % 3 = 0, zz_sid(20, 2), zz_sid(20, 1)),
       IF(n % 3 = 0, zz_sid(1, 1 + n % @n_matters), NULL), IF(n % 3 = 0, zz_sid(2, 1 + n % @n_clients), NULL),
       100 + n % 900, 0, 'USD'
FROM zz_nums WHERE n <= @n_entries;
INSERT INTO journal_lines (JournalEntryId, LineNumber, LedgerAccountId, MatterRecordId, ClientPartyId, Debit, Credit, CurrencyCode)
SELECT zz_sid(40, n), 2, IF(n % 3 = 0, zz_sid(20, 3), zz_sid(20, 5)),
       IF(n % 3 = 0, zz_sid(1, 1 + n % @n_matters), NULL), IF(n % 3 = 0, zz_sid(2, 1 + n % @n_clients), NULL),
       0, 100 + n % 900, 'USD'
FROM zz_nums WHERE n <= @n_entries;
UPDATE journal_entries SET Status = 'Posted', PostedAtUtc = @now, PostedByActorId = 'u-s-031';
INSERT INTO trust_subledger_balances (TrustAccountId, MatterRecordId, ClientPartyId, CurrencyCode, Balance)
SELECT zz_sid(20, 2), MatterRecordId, ClientPartyId, 'USD', SUM(Debit) - SUM(Credit)
FROM journal_lines WHERE LedgerAccountId = zz_sid(20, 2)
GROUP BY MatterRecordId, ClientPartyId;

INSERT INTO intake_records (Id, PracticeArea, IntakeType, Stage, ProspectiveClientName, OwnerActorId, OwnerDisplayName,
    NextAction, NextActionDueAtUtc, CreatedByActorId, CreatedAtUtc)
SELECT zz_sid(50, n), ELT(1 + n % 3, 'ConstructionLaw', 'Litigation', 'PersonalInjury'), 'Web',
       CASE WHEN n % 10 = 0 THEN 'New' WHEN n % 10 = 1 THEN 'Contacted' WHEN n % 10 = 2 THEN 'ConsultationScheduled'
            WHEN n % 10 = 3 THEN 'UnderReview' WHEN n % 10 < 7 THEN 'Accepted' WHEN n % 10 < 9 THEN 'Declined' ELSE 'ReferredElsewhere' END,
       CONCAT('Synthetic Prospect ', @firm, n), CONCAT('u-s-', LPAD(16 + n % 15, 3, '0')), 'Synthetic Paralegal',
       'Follow up', @now + INTERVAL ((n * 13) % 60 - 30) DAY, 'u-s-031', @now - INTERVAL (n % 700) DAY
FROM zz_nums WHERE n <= @n_intake;

INSERT INTO consultation_requests (Id, FullName, Email, Phone, PracticeArea, Message, PreferredAtUtc, TimeZone, Status, CreatedAtUtc, UpdatedAtUtc)
SELECT zz_sid(51, n), CONCAT('Synthetic Requester ', @firm, n), CONCAT('requester', n, '@example.invalid'), NULL,
       ELT(1 + n % 3, 'ConstructionLaw', 'Litigation', 'PersonalInjury'), NULL,
       @now + INTERVAL ((n * 19) % 120 - 60) DAY, 'America/Chicago',
       CASE WHEN n % 20 = 0 THEN 'New' WHEN n % 20 = 1 THEN 'Contacted' WHEN n % 20 = 2 THEN 'Scheduled'
            WHEN n % 20 = 3 THEN 'InReview' WHEN n % 20 < 18 THEN 'Closed' ELSE 'Cancelled' END,
       @now - INTERVAL (n % 700) DAY, @now
FROM zz_nums WHERE n <= @n_consults;

-- Outbox: 1% pending backlog, 99% done.
INSERT INTO outbox_events (EventId, EventType, AggregateType, AggregateId, PayloadJson, Status, Attempt, AvailableAtUtc, ProcessedAtUtc, CreatedAtUtc)
SELECT zz_sid(60, n), 'matter.updated', 'matter', zz_sid(1, 1 + n % @n_matters), JSON_OBJECT('v', 1),
       IF(n % 100 = 0, 'Pending', 'Done'), IF(n % 100 = 0, 0, 1),
       @now - INTERVAL (@n_outbox - n) SECOND, IF(n % 100 = 0, NULL, @now - INTERVAL (@n_outbox - n) SECOND), @now
FROM zz_nums WHERE n <= @n_outbox;

INSERT INTO webhook_receipts (Id, Provider, ProviderAccountRef, EventId, EventType, PayloadSha256, SignatureVerified, ReceivedAtUtc, ProcessedAtUtc, Outcome)
SELECT zz_sid(61, n), 'synthetic-pay', CONCAT('acct_synthetic_', @firm), CONCAT('evt_', n), 'payment.succeeded',
       UNHEX(SHA2(CONCAT('evt', n), 256)), 1, @now - INTERVAL n MINUTE,
       IF(n % 200 = 0, NULL, @now - INTERVAL n MINUTE), IF(n % 200 = 0, NULL, 'Applied')
FROM zz_nums WHERE n <= @n_webhooks;

INSERT INTO audit_events (Id, Action, ObjectType, ObjectId, ActorId, ActorDisplayName, CorrelationId, MetadataJson, OccurredAtUtc)
SELECT zz_sid(62, n), 'matter.viewed', 'matter', zz_sid(1, 1 + n % @n_matters), CONCAT('u-s-', LPAD(1 + n % 40, 3, '0')),
       'Synthetic Staff', CONCAT('corr-', n), '{}', @now - INTERVAL n MINUTE
FROM zz_nums WHERE n <= @n_audit;

DROP TABLE zz_nums, zz_digits;
DROP FUNCTION zz_sid;
