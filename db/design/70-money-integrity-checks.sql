-- Money and approval integrity checks. Run in a synthetic firm database by the firm RUNTIME
-- user with mysql --force (expected errors print and execution continues).
-- Requires the ledger accounts from 40-synthetic-data.sql.

SET @op = '00000014-0000-4000-8000-000000000001', @trust = '00000014-0000-4000-8000-000000000002',
    @liab = '00000014-0000-4000-8000-000000000003', @rev = '00000014-0000-4000-8000-000000000005';
SET @matter = '00000001-0000-4000-8000-000000000004', @client = '00000002-0000-4000-8000-000000000004';
SET @now = UTC_TIMESTAMP(6);

SELECT 'L1 balanced entry posts: expect Status = Posted' AS check_name;
INSERT INTO journal_entries (Id, EntryNumber, EffectiveDate, Status, SourceType, IdempotencyKey, CreatedByActorId, CreatedAtUtc)
VALUES ('aaaaaaaa-0000-4000-8000-000000000001', 9000001, '2026-10-12', 'Draft', 'Check', 'chk-1', 'u-s-031', @now);
INSERT INTO journal_lines (JournalEntryId, LineNumber, LedgerAccountId, Debit, Credit) VALUES
 ('aaaaaaaa-0000-4000-8000-000000000001', 1, @op, 100.00, 0),
 ('aaaaaaaa-0000-4000-8000-000000000001', 2, @rev, 0, 100.00);
UPDATE journal_entries SET Status = 'Posted', PostedAtUtc = @now, PostedByActorId = 'u-s-001'
WHERE Id = 'aaaaaaaa-0000-4000-8000-000000000001';
SELECT Status FROM journal_entries WHERE Id = 'aaaaaaaa-0000-4000-8000-000000000001';

SELECT 'L2 EXPECT ERROR 1644 journal_entry_unbalanced' AS check_name;
INSERT INTO journal_entries (Id, EntryNumber, EffectiveDate, Status, SourceType, IdempotencyKey, CreatedByActorId, CreatedAtUtc)
VALUES ('aaaaaaaa-0000-4000-8000-000000000002', 9000002, '2026-10-12', 'Draft', 'Check', 'chk-2', 'u-s-031', @now);
INSERT INTO journal_lines (JournalEntryId, LineNumber, LedgerAccountId, Debit, Credit) VALUES
 ('aaaaaaaa-0000-4000-8000-000000000002', 1, @op, 100.00, 0),
 ('aaaaaaaa-0000-4000-8000-000000000002', 2, @rev, 0, 90.00);
UPDATE journal_entries SET Status = 'Posted', PostedAtUtc = @now, PostedByActorId = 'u-s-001'
WHERE Id = 'aaaaaaaa-0000-4000-8000-000000000002';

SELECT 'L3 EXPECT ERROR 1644 journal_entry_not_draft: add line to posted entry' AS check_name;
INSERT INTO journal_lines (JournalEntryId, LineNumber, LedgerAccountId, Debit, Credit)
VALUES ('aaaaaaaa-0000-4000-8000-000000000001', 3, @op, 5.00, 0);

SELECT 'L4 EXPECT ERROR 1644 journal_entry_posted_immutable: edit posted entry' AS check_name;
UPDATE journal_entries SET Memo = 'edited' WHERE Id = 'aaaaaaaa-0000-4000-8000-000000000001';

SELECT 'L5 EXPECT ERROR 1644 journal_entry_posted_immutable: delete posted entry' AS check_name;
DELETE FROM journal_entries WHERE Id = 'aaaaaaaa-0000-4000-8000-000000000001';

SELECT 'L6 EXPECT ERROR 1644 journal_line_immutable: edit a line' AS check_name;
UPDATE journal_lines SET Debit = 101.00 WHERE JournalEntryId = 'aaaaaaaa-0000-4000-8000-000000000001' AND LineNumber = 1;

SELECT 'L7 EXPECT ERROR 1644 journal_entry_too_few_lines' AS check_name;
INSERT INTO journal_entries (Id, EntryNumber, EffectiveDate, Status, SourceType, IdempotencyKey, CreatedByActorId, CreatedAtUtc)
VALUES ('aaaaaaaa-0000-4000-8000-000000000003', 9000003, '2026-10-12', 'Draft', 'Check', 'chk-3', 'u-s-031', @now);
UPDATE journal_entries SET Status = 'Posted', PostedAtUtc = @now, PostedByActorId = 'u-s-001'
WHERE Id = 'aaaaaaaa-0000-4000-8000-000000000003';

SELECT 'L8 EXPECT ERROR 1062: idempotency key replay' AS check_name;
INSERT INTO journal_entries (Id, EntryNumber, EffectiveDate, Status, SourceType, IdempotencyKey, CreatedByActorId, CreatedAtUtc)
VALUES ('aaaaaaaa-0000-4000-8000-000000000004', 9000004, '2026-10-12', 'Draft', 'Check', 'chk-1', 'u-s-031', @now);

SELECT 'L9 EXPECT ERROR 3819: trust subledger overdraft' AS check_name;
UPDATE trust_subledger_balances SET Balance = Balance - (Balance + 0.01), RowVersion = RowVersion + 1
WHERE TrustAccountId = @trust AND MatterRecordId = @matter AND ClientPartyId = @client;

SELECT 'L10 version-bound approval: approve hash H1, expect 1 approval' AS check_name;
INSERT INTO trust_disbursement_requests (Id, TrustAccountId, MatterRecordId, ClientPartyId, PayeePartyId, Amount,
    ContentSha256, RequestedByActorId, CreatedAtUtc)
VALUES ('bbbbbbbb-0000-4000-8000-000000000001', @trust, @matter, @client, @client, 10.00,
        UNHEX(SHA2('request-v1', 256)), 'u-s-016', @now);
INSERT INTO trust_disbursement_approvals (Id, RequestId, ApprovedContentSha256, ApproverActorId, Decision, DecidedAtUtc)
VALUES ('cccccccc-0000-4000-8000-000000000001', 'bbbbbbbb-0000-4000-8000-000000000001', UNHEX(SHA2('request-v1', 256)), 'u-s-001', 'Approved', @now);
SELECT COUNT(*) AS approvals FROM trust_disbursement_approvals WHERE RequestId = 'bbbbbbbb-0000-4000-8000-000000000001';

SELECT 'L11 EXPECT ERROR 1452: approval that names a different content hash' AS check_name;
INSERT INTO trust_disbursement_approvals (Id, RequestId, ApprovedContentSha256, ApproverActorId, Decision, DecidedAtUtc)
VALUES ('cccccccc-0000-4000-8000-000000000002', 'bbbbbbbb-0000-4000-8000-000000000001', UNHEX(SHA2('request-v2', 256)), 'u-s-002', 'Approved', @now);

SELECT 'L12 EXPECT ERROR 1451: changing approved content while an approval references it' AS check_name;
UPDATE trust_disbursement_requests SET ContentSha256 = UNHEX(SHA2('request-v2', 256)), Amount = 9999.00
WHERE Id = 'bbbbbbbb-0000-4000-8000-000000000001';

SELECT 'L13 EXPECT ERROR 3819: time entry amount must equal rounded rate x duration' AS check_name;
INSERT INTO time_entries (Id, MatterRecordId, ActorId, WorkDate, DurationMinutes, HourlyRate, Amount, Narrative, CreatedAtUtc)
VALUES ('dddddddd-0000-4000-8000-000000000001', @matter, 'u-s-003', '2026-10-12', 6, 300.0000, 30.01, 'Synthetic', @now);

SELECT 'L14 time entry with correct amount: expect success; then EXPECT ERROR 1062 billing it twice' AS check_name;
INSERT INTO time_entries (Id, MatterRecordId, ActorId, WorkDate, DurationMinutes, HourlyRate, Amount, Narrative, CreatedAtUtc)
VALUES ('dddddddd-0000-4000-8000-000000000002', @matter, 'u-s-003', '2026-10-12', 6, 300.0000, 30.00, 'Synthetic', @now);
INSERT INTO invoice_lines (Id, InvoiceId, LineNumber, LineType, SourceTimeEntryId, Description, Quantity, UnitPrice, Amount)
VALUES ('eeeeeeee-0000-4000-8000-000000000001', '0000001e-0000-4000-8000-000000000013', 10, 'Time',
        'dddddddd-0000-4000-8000-000000000002', 'Synthetic', 0.1000, 300.0000, 30.00);
INSERT INTO invoice_lines (Id, InvoiceId, LineNumber, LineType, SourceTimeEntryId, Description, Quantity, UnitPrice, Amount)
VALUES ('eeeeeeee-0000-4000-8000-000000000002', '0000001e-0000-4000-8000-000000000014', 10, 'Time',
        'dddddddd-0000-4000-8000-000000000002', 'Synthetic', 0.1000, 300.0000, 30.00);

SELECT 'L15 EXPECT ERROR 1062: webhook replay in the same provider account' AS check_name;
INSERT INTO webhook_receipts (Id, Provider, ProviderAccountRef, EventId, EventType, PayloadSha256, SignatureVerified, ReceivedAtUtc)
VALUES ('ffffffff-0000-4000-8000-000000000001', 'synthetic-pay', 'acct_synthetic_B', 'evt_1', 'payment.succeeded', UNHEX(SHA2('x', 256)), 1, @now);

SELECT 'L16 same event id under a different provider account is a different event: expect success' AS check_name;
INSERT INTO webhook_receipts (Id, Provider, ProviderAccountRef, EventId, EventType, PayloadSha256, SignatureVerified, ReceivedAtUtc)
VALUES ('ffffffff-0000-4000-8000-000000000002', 'synthetic-pay', 'acct_other', 'evt_1', 'payment.succeeded', UNHEX(SHA2('x', 256)), 1, @now);

SELECT 'L17 EXPECT ERROR 3819: unverified webhook signature cannot be recorded' AS check_name;
INSERT INTO webhook_receipts (Id, Provider, ProviderAccountRef, EventId, EventType, PayloadSha256, SignatureVerified, ReceivedAtUtc)
VALUES ('ffffffff-0000-4000-8000-000000000003', 'synthetic-pay', 'acct_synthetic_B', 'evt_unsigned', 'payment.succeeded', UNHEX(SHA2('x', 256)), 0, @now);

SELECT 'L18 EXPECT ERROR 3819: payment over-allocation' AS check_name;
UPDATE payments SET AmountAllocated = Amount + 0.01 WHERE Id = '00000020-0000-4000-8000-000000000001';
