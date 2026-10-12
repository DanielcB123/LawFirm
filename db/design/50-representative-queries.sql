-- Representative queries for index validation. Run against a synthetic firm database
-- (output of 40-synthetic-data.sql) by the firm RUNTIME user, which only needs SELECT.
-- Each query is printed with EXPLAIN ANALYZE (MySQL 8.0.18+): estimated plan plus actual
-- rows and loops on this data. Timings are from a developer workstation container and are
-- not performance claims.

-- GUID variables carry the column's charset and collation (ascii_general_ci). A user variable
-- assigned from a plain literal is utf8mb4; comparing it with an ascii column converts the
-- COLUMN and disables its index (see Q0). EF Core/MySqlConnector inline parameters as
-- literals, which take the column collation, but hand-written SQL must not rely on that.
SET @now = TIMESTAMP('2026-10-12 00:00:00'), @today = DATE('2026-10-12');
SET @attorney = 'u-s-003', @paralegal = 'u-s-016';
SET @matter = _ascii'00000001-0000-4000-8000-000000000004' COLLATE ascii_general_ci;
SET @client = _ascii'00000002-0000-4000-8000-000000000004' COLLATE ascii_general_ci;
SET @trust  = _ascii'00000014-0000-4000-8000-000000000002' COLLATE ascii_general_ci;
SET @hot_conversation = _ascii'0000000a-0000-4000-8000-000000000001' COLLATE ascii_general_ci;
SET @document = _ascii'00000009-0000-4000-8000-000000000004' COLLATE ascii_general_ci;
SET @invoice = _ascii'0000001e-0000-4000-8000-000000000011' COLLATE ascii_general_ci;
SET @matter_utf8mb4 = '00000001-0000-4000-8000-000000000004';

SELECT '=== Q0 PITFALL: utf8mb4 user variable against ascii_general_ci GUID column';
EXPLAIN ANALYZE
SELECT Id, Title FROM matter_tasks WHERE MatterRecordId = @matter_utf8mb4;

SELECT '=== Q0b plain string literal (how client-side parameter substitution arrives)';
EXPLAIN ANALYZE
SELECT Id, Title FROM matter_tasks WHERE MatterRecordId = '00000001-0000-4000-8000-000000000004';

SELECT '=== Q1a authorized active matters, CURRENT shape (MatterAccessService: OR + EXISTS)';
EXPLAIN ANALYZE
SELECT m.Id, m.MatterNumber, m.Title, m.CreatedAtUtc
FROM matters m
WHERE m.Status = 'Active'
  AND (m.ResponsibleAttorneyActorId = @attorney OR m.ParalegalActorId = @attorney
       OR EXISTS (SELECT 1 FROM matter_access_grants g
                  WHERE g.MatterRecordId = m.Id AND g.ActorId = @attorney
                    AND g.GrantType = 'Staff' AND g.RevokedAtUtc IS NULL))
ORDER BY m.CreatedAtUtc DESC, m.Id DESC
LIMIT 50;

SELECT '=== Q1b authorized active matters, PROPOSED shape (assignment/grant id set, ethical wall)';
EXPLAIN ANALYZE
SELECT m.Id, m.MatterNumber, m.Title, m.CreatedAtUtc
FROM matters m
JOIN (SELECT MatterRecordId FROM matter_assignments WHERE ActorId = @attorney AND ActiveMarker = 1
      UNION
      SELECT MatterRecordId FROM matter_access_grants
      WHERE ActorId = @attorney AND GrantType = 'Staff' AND RevokedAtUtc IS NULL) s
  ON s.MatterRecordId = m.Id
WHERE m.Status = 'Active'
  AND NOT EXISTS (SELECT 1 FROM ethical_walls w
                  WHERE w.MatterRecordId = m.Id AND w.ScreenedActorId = @attorney AND w.ActiveMarker = 1)
ORDER BY m.CreatedAtUtc DESC, m.Id DESC
LIMIT 50;

SELECT '=== Q1c firm-wide active matters, keyset page 2 (matters.read.all)';
SET @cursor_created = (SELECT CreatedAtUtc FROM matters WHERE Status = 'Active' ORDER BY CreatedAtUtc DESC, Id DESC LIMIT 1 OFFSET 49);
SET @cursor_id = (SELECT Id FROM matters WHERE Status = 'Active' ORDER BY CreatedAtUtc DESC, Id DESC LIMIT 1 OFFSET 49);
EXPLAIN ANALYZE
SELECT m.Id, m.MatterNumber, m.Title, m.CreatedAtUtc
FROM matters m
WHERE m.Status = 'Active'
  AND (m.CreatedAtUtc < @cursor_created OR (m.CreatedAtUtc = @cursor_created AND m.Id < @cursor_id))
ORDER BY m.CreatedAtUtc DESC, m.Id DESC
LIMIT 50;

SELECT '=== Q1d client portal matters (ClientMatters: Client grant)';
EXPLAIN ANALYZE
SELECT m.Id, m.MatterNumber, m.Title
FROM matters m
WHERE EXISTS (SELECT 1 FROM matter_access_grants g
              WHERE g.MatterRecordId = m.Id AND g.ActorId = 'u-c-4'
                AND g.GrantType = 'Client' AND g.RevokedAtUtc IS NULL);

SELECT '=== Q2a my overdue open tasks';
EXPLAIN ANALYZE
SELECT Id, MatterRecordId, Title, DueAtUtc
FROM matter_tasks
WHERE AssigneeActorId = @paralegal AND Status IN ('Open', 'InProgress') AND DueAtUtc < @now
ORDER BY DueAtUtc, Id
LIMIT 50;

SELECT '=== Q2b open tasks on one matter';
EXPLAIN ANALYZE
SELECT Id, Title, Status, DueAtUtc
FROM matter_tasks
WHERE MatterRecordId = @matter AND Status IN ('Open', 'InProgress')
ORDER BY DueAtUtc;

SELECT '=== Q2c firm deadlines due in the next 14 days';
EXPLAIN ANALYZE
SELECT Id, MatterRecordId, DeadlineKind, DueDate, Status
FROM deadlines
WHERE Status IN ('Unverified', 'Verified') AND DueDate BETWEEN @today AND @today + INTERVAL 14 DAY
ORDER BY DueDate, Id
LIMIT 100;

SELECT '=== Q2d my deadlines, overdue or due within 30 days';
EXPLAIN ANALYZE
SELECT Id, MatterRecordId, DueDate, Status
FROM deadlines
WHERE ResponsibleActorId = @attorney AND Status IN ('Unverified', 'Verified') AND DueDate < @today + INTERVAL 30 DAY
ORDER BY DueDate;

SELECT '=== Q3a documents on a matter, newest first';
EXPLAIN ANALYZE
SELECT Id, FileName, DocumentType, UploadedAtUtc
FROM matter_documents
WHERE MatterRecordId = @matter
ORDER BY UploadedAtUtc DESC, Id DESC
LIMIT 50;

SELECT '=== Q3b current version of a document';
EXPLAIN ANALYZE
SELECT Id, VersionNumber, StorageKey, ContentSha256
FROM document_versions
WHERE DocumentId = @document
ORDER BY VersionNumber DESC
LIMIT 1;

SELECT '=== Q4a conversation page, keyset (older than cursor)';
SET @msg_cursor = (SELECT MAX(Id) - 15000 FROM messages WHERE ConversationId = @hot_conversation);
EXPLAIN ANALYZE
SELECT Id, PublicId, SenderActorId, SentAtUtc
FROM messages
WHERE ConversationId = @hot_conversation AND Id < @msg_cursor
ORDER BY Id DESC
LIMIT 50;

SELECT '=== Q4c keyset page in an ORDINARY conversation (messages interleaved across the whole id range)';
SET @normal_conversation = _ascii'0000000a-0000-4000-8000-000000000002' COLLATE ascii_general_ci;
SET @normal_cursor = (SELECT Id FROM messages WHERE ConversationId = @normal_conversation ORDER BY Id DESC LIMIT 1 OFFSET 10);
EXPLAIN ANALYZE
SELECT Id, PublicId, SenderActorId, SentAtUtc
FROM messages
WHERE ConversationId = @normal_conversation AND Id < @normal_cursor
ORDER BY Id DESC
LIMIT 20;

SELECT '=== Q4b conversation page, OFFSET 15000 (anti-pattern for comparison)';
EXPLAIN ANALYZE
SELECT Id, PublicId, SenderActorId, SentAtUtc
FROM messages
WHERE ConversationId = @hot_conversation
ORDER BY Id DESC
LIMIT 50 OFFSET 15000;

SELECT '=== Q5a overdue receivables, two-status IN (comparison)';
EXPLAIN ANALYZE
SELECT Id, InvoiceNumber, MatterRecordId, DueDate, BalanceDue
FROM invoices
WHERE Status IN ('Issued', 'PartiallyPaid') AND DueDate < @today
ORDER BY DueDate, Id
LIMIT 50;

SELECT '=== Q5a2 overdue receivables via OpenMarker (design)';
EXPLAIN ANALYZE
SELECT Id, InvoiceNumber, MatterRecordId, DueDate, BalanceDue
FROM invoices
WHERE OpenMarker = 1 AND DueDate < @today
ORDER BY DueDate, Id
LIMIT 50;

SELECT '=== Q5b active allocations for one invoice';
EXPLAIN ANALYZE
SELECT a.Id, a.PaymentId, a.Amount
FROM payment_allocations a
WHERE a.InvoiceId = @invoice AND a.ReversedAtUtc IS NULL;

SELECT '=== Q5c open invoices for one client';
EXPLAIN ANALYZE
SELECT Id, InvoiceNumber, BalanceDue
FROM invoices
WHERE BillToPartyId = @client AND OpenMarker = 1;

SELECT '=== Q6a trust activity for one matter and client';
EXPLAIN ANALYZE
SELECT l.Id, e.EntryNumber, e.EffectiveDate, l.Debit, l.Credit
FROM journal_lines l
JOIN journal_entries e ON e.Id = l.JournalEntryId
WHERE l.LedgerAccountId = @trust AND l.MatterRecordId = @matter AND l.ClientPartyId = @client
  AND e.Status = 'Posted'
ORDER BY l.Id;

SELECT '=== Q6b trust book balance as of a statement date (reconciliation input)';
EXPLAIN ANALYZE
SELECT SUM(l.Debit - l.Credit)
FROM journal_lines l
JOIN journal_entries e ON e.Id = l.JournalEntryId
WHERE l.LedgerAccountId = @trust AND e.Status = 'Posted' AND e.EffectiveDate <= @today;

SELECT '=== Q6c trust subledger total (reconciliation input)';
EXPLAIN ANALYZE
SELECT SUM(Balance) FROM trust_subledger_balances WHERE TrustAccountId = @trust;

SELECT '=== Q7a intake work queue';
EXPLAIN ANALYZE
SELECT Id, ProspectiveClientName, Stage, NextActionDueAtUtc
FROM intake_records
WHERE Stage IN ('New', 'Contacted', 'ConsultationScheduled', 'UnderReview')
ORDER BY NextActionDueAtUtc, Id
LIMIT 50;

SELECT '=== Q7b new consultation requests by preferred time';
EXPLAIN ANALYZE
SELECT Id, FullName, PreferredAtUtc
FROM consultation_requests
WHERE Status = 'New'
ORDER BY PreferredAtUtc, Id
LIMIT 50;

SELECT '=== Q8a outbox claim batch (plan only; real claim uses FOR UPDATE SKIP LOCKED)';
EXPLAIN FORMAT=TREE
SELECT Id FROM outbox_events
WHERE Status = 'Pending' AND AvailableAtUtc <= @now
ORDER BY AvailableAtUtc, Id
LIMIT 100
FOR UPDATE SKIP LOCKED;
EXPLAIN ANALYZE
SELECT Id FROM outbox_events
WHERE Status = 'Pending' AND AvailableAtUtc <= @now
ORDER BY AvailableAtUtc, Id
LIMIT 100;

SELECT '=== Q8b webhook dedup probe';
EXPLAIN ANALYZE
SELECT Id, ProcessedAtUtc FROM webhook_receipts
WHERE Provider = 'synthetic-pay' AND ProviderAccountRef = 'acct_synthetic_A' AND EventId = 'evt_12345';

SELECT '=== Q8c unprocessed webhook receipts';
EXPLAIN ANALYZE
SELECT Id, EventType FROM webhook_receipts
WHERE ProcessedAtUtc IS NULL
ORDER BY ReceivedAtUtc
LIMIT 100;
