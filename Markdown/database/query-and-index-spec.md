# Query and index specification

[Home](../README.md) · [Database](README.md) · [Data dictionary](data-dictionary.md) · [ADR-002](ADR-002-multitenant-architecture.md) · [Queries](../../db/design/50-representative-queries.sql) · [Validation log](../../db/design/validation/mysql-8.0.36-validation.log)

**Status:** design, validated on a disposable MySQL 8.0.36 container (`mysql:8.0.36` image, InnoDB, REPEATABLE-READ, binary logging on, `max_connections = 151`) through Docker 20.10.17 on the development workstation, 2026-10-12.

Plans come from `EXPLAIN ANALYZE`, run as the firm **runtime** account after `ANALYZE TABLE`. Timings are single runs on a workstation container. They show plan shape and rows read; they are **not performance results**.

## Data used (synthetic firm A, deterministic generator)
[40-synthetic-data.sql](../../db/design/40-synthetic-data.sql) at scale 1.0:

| Table | Rows | Distribution |
|---|---|---|
| matters | 5,000 | 30% Active, 60% Closed, 10% Pending; 15 attorneys (334 each); two thirds have a paralegal |
| matter_assignments / matter_access_grants | 8,334 / 3,000 client grants plus up to 2,000 staff grants (duplicates skipped by `INSERT IGNORE`; 20% of staff grants revoked) | — |
| matter_tasks | 100,000 | 12.5% Open, 12.5% InProgress, 75% Completed; 30 assignees; due dates mostly past |
| deadlines | 25,000 | 15,004 Completed, 5,831 Unverified, 4,165 Verified |
| matter_documents / document_versions | 60,000 / 90,000 | Half have version 2 |
| messages | 300,000 | One "hot" conversation with 20,000 messages contiguous in id; the rest spread over 7,999 conversations |
| invoices / invoice_lines | 20,000 / 80,000 | 70% Paid, 15% Issued, 5% PartiallyPaid, 5% Draft, 5% Void |
| payments / allocations | 15,000 / 18,000 | 2% of allocations reversed |
| journal entries / lines | 30,000 / 60,000 | One third are trust deposits by (matter, client); 5,000 trust subledger rows |
| intake / consultations | 6,000 / 8,000 | 40% of intake in open stages; 5% of consultations New |
| outbox / webhook receipts / audit | 200,000 / 30,000 / 100,000 | 1% outbox pending; 0.5% of webhooks unprocessed |

Total data plus indexes: about 506 MB. Firm B is the same generator at scale 0.1, with **overlapping local ids**.

**Limitations.**
- The data is uniform and modular: no hot attorneys, no long-tail matters, short text.
- The run used one firm at a time, with no concurrent load.
- Small tables fit in the buffer pool.
- Optimizer choices can change with data size and statistics, so these plans must be re-checked on realistic volumes before they become acceptance evidence for production.

## Finding: GUID collation decides index use
`char(36) ascii_general_ci` id columns compared with a **utf8mb4** user variable caused a full table scan: Q0 read 100,000 rows. A plain literal used the index: Q0b read 20 rows. The column is converted to the variable's character set, so its index can't be used.

Client-side parameter substitution (the MySqlConnector default) sends literals, which take the column's collation. The F1 code path is therefore expected to be fine, but this has **not been checked with query logging**. Hand-written SQL, stored routines, and tools must bind ids as `_ascii'…' COLLATE ascii_general_ci`. The acceptance matrix tracks a check that captures the SQL the app actually sends (AT-IDX-02).

## Queries

| Id | Query | Filter → sort → page | Index (column order) | Observed plan (rows read) | Rationale |
|---|---|---|---|---|---|
| Q1a | Authorized active matters, **current** `MatterAccessService` shape | `Status = 'Active' AND (attorney = @a OR paralegal = @a OR EXISTS grant)` → `CreatedAtUtc DESC, Id DESC` → LIMIT 50 | `IX_matters_Status_CreatedAtUtc (Status, CreatedAtUtc, Id)` plus grant probe on `(MatterRecordId, ActorId, ActiveMarker)` | Reverse index walk over Active, filtering: 436 rows and 386 dependent grant probes for 50 results | Cost grows with the number of **active matters in the firm**, not with the user's own matters. If a user has few matters it walks every active matter. Acceptable for one small firm only |
| Q1b | The same, **proposed** shape | UNION of the user's active assignments and staff grants, then join `matters`, then anti-join `ethical_walls` → sort → LIMIT 50 | `IX_matter_assignments_Actor_Active (ActorId, ActiveMarker, MatterRecordId)` covering; `IX_matter_access_grants_ActorId_RevokedAtUtc`; PK; `IX_ethical_walls_Actor_Active` | 334 + 67 index rows, 386 PK lookups, sort of 167 | Cost grows with the **user's** assignments. The sort is bounded by that set. It replaces the OR across two columns, which no single index serves |
| Q1c | Firm-wide active matters (`matters.read.all`), keyset page 2 | `Status = 'Active' AND (CreatedAtUtc, Id) < cursor` → DESC → LIMIT 50 | `(Status, CreatedAtUtc, Id)` | Range scan, 50 rows | The OR-expanded keyset predicate is range-optimized. Don't use the `(a, b) < (x, y)` row constructor without checking the plan |
| Q1d | Client portal matters | Client grant by actor | `IX_matter_access_grants_ActorId_RevokedAtUtc` | 1 index row, 1 PK lookup | Existing index suffices |
| Q2a | My overdue open tasks | `Assignee = @a AND Status IN (Open, InProgress) AND DueAtUtc < now` → `DueAtUtc, Id` → LIMIT 50 | `(AssigneeActorId, Status, DueAtUtc)` | Two ranges, 684 rows, then a sort | An IN on the second column prevents an ordered read. The sort of a few hundred rows is acceptable. If a user's open tasks grow large, add `OpenMarker` as was done for invoices (a STORED generated column rebuilds the table on an existing table; plan it as a migration) |
| Q2b | Open tasks on one matter | `MatterRecordId = @m AND Status IN (…)` → `DueAtUtc` | `(MatterRecordId, Status, DueAtUtc)` | Range, 0–20 rows | It makes the existing `IX_matter_tasks_MatterRecordId` redundant (a left prefix); drop that in a contract step |
| Q2c | Firm deadlines in the next 14 days | `Status IN (Unverified, Verified) AND DueDate BETWEEN` → `DueDate, Id` → LIMIT 100 | `(Status, DueDate, Id)` | Two ranges, 416 rows, then a sort | Acceptable; the window bounds the rows |
| Q2d | My deadlines due or overdue within 30 days | `Responsible = @a AND Status IN (…) AND DueDate < d` | `(ResponsibleActorId, Status, DueDate)` | 56 rows | — |
| Q3a | Documents on a matter, newest first | `MatterRecordId = @m` → `UploadedAtUtc DESC, Id DESC` | `(MatterRecordId, UploadedAtUtc, Id)` | Reverse index lookup, 12 rows, no sort | Without it, the first run did a table scan (60,000 rows) |
| Q3b | Current version of a document | `DocumentId = @d` → `VersionNumber DESC` LIMIT 1 | `UX_document_versions_Doc_Version (DocumentId, VersionNumber)` | 1 row | The unique key doubles as the access path |
| Q4a, Q4c | Conversation page, keyset | `ConversationId = @c AND Id < cursor` → `Id DESC` LIMIT n | `(ConversationId, Id)` | Ordinary conversation (Q4c): index range, 20 rows. Hot conversation (Q4a): the optimizer chose a reverse primary-key range instead, correct only because that conversation's ids are contiguous | Watch this plan choice when real data is skewed. Pin with an index hint only if production plans show the primary-key path on interleaved conversations |
| Q4b | Conversation page by OFFSET 15000 | — | Same | 15,050 index rows read for 50 results | **Don't use OFFSET** for message history. The cost grows linearly with depth |
| Q5a, Q5a2 | Overdue receivables | Q5a: `Status IN (Issued, PartiallyPaid) AND DueDate < today`. Q5a2: `OpenMarker = 1 AND DueDate < today`; both → `DueDate, Id` → LIMIT 50 | `IX_invoices_Open_DueDate (OpenMarker, DueDate, Id)` | Q5a: full table scan of 20,000 rows plus sort. Q5a2: ordered index range that stopped at 50 rows | A generated `OpenMarker` turns a two-value IN into equality, so the index returns rows already ordered |
| Q5b | Active allocations of an invoice | `InvoiceId = @i AND ReversedAtUtc IS NULL` | `(InvoiceId, ReversedAtUtc)` | 1 row | — |
| Q5c | A client's open invoices | `BillToPartyId = @p AND OpenMarker = 1` | `(BillToPartyId, OpenMarker)` | Index lookup | — |
| Q6a | Trust activity for a matter and client | `Account = @trust AND Matter = @m AND Client = @p` → `Id` | `IX_journal_lines_Trust (LedgerAccountId, MatterRecordId, ClientPartyId, Id)` | 2 rows plus PK join to entries | The client ledger card |
| Q6b | Trust book balance as of a date | All posted trust lines with `EffectiveDate <= d` | `IX_journal_lines_Trust` prefix | 10,000 lines plus 10,000 PK probes | Linear in trust activity. Acceptable for a periodic reconciliation; if it grows, add monthly closed balances (a future design item) |
| Q6c | Subledger total | `TrustAccountId = @t` | PK prefix | 5,000 rows | Compared with Q6b and the bank statement in `trust_reconciliations` (three-way CHECK) |
| Q7a | Intake work queue | `Stage IN (4 open stages)` → `NextActionDueAtUtc, Id` LIMIT 50 | `(Stage, NextActionDueAtUtc, Id)` candidate | Table scan of 6,000 rows plus sort: the optimizer judged 40% selectivity too low | Acceptable at this size. If intake grows, use an `OpenMarker` like invoices. The candidate index could be dropped if it never wins (write cost without benefit) |
| Q7b | New consultations by preferred time | `Status = 'New'` → `PreferredAtUtc, Id` | `(Status, PreferredAtUtc, Id)` | Ordered index lookup, 50 rows | Replaces the separate Status and PreferredAtUtc indexes (both become redundant for this path) |
| Q8a | Outbox claim | `Status = 'Pending' AND AvailableAtUtc <= now` → `AvailableAtUtc, Id` LIMIT 100 `FOR UPDATE SKIP LOCKED` | `(Status, AvailableAtUtc, Id)` covering | Covering range, 100 rows. Concurrent dispatchers claimed disjoint rows (R3) | Done rows must be archived, or the Pending prefix stays narrow but the index keeps growing |
| Q8b | Webhook dedup | Exact `(Provider, ProviderAccountRef, EventId)` | Unique key | Constant lookup | The insert itself is the dedup: duplicates fail with 1062 (L15) |
| Q8c | Unprocessed webhooks | `ProcessedAtUtc IS NULL` → `ReceivedAtUtc` | `(ProcessedAtUtc, ReceivedAtUtc)` | 100 rows | — |

## Composite-index behavior, selectivity, cost
- **Leftmost prefix.** An index serves equality on leading columns, then one range or an ordering. An IN on a middle column produces several ranges whose union must be sorted (Q2a, Q2c). Putting equality columns first and the sort column last is the rule used throughout.
- **Selectivity.** The optimizer prefers a scan when a predicate keeps a large share of rows and the index isn't covering (Q5a at 19%, Q7a at 40%). Generated marker columns make queues selective.
- **Write amplification and size.** Each secondary index adds one B-tree write per insert. On `matter_tasks`, index size (51 MB) exceeded data size (22 MB) in the fixture. Every index here is tied to a query in the table above. The redundant ones to drop in a contract step are:
  - `IX_matter_tasks_MatterRecordId`;
  - `IX_matter_documents_MatterRecordId`;
  - `IX_consultation_requests_Status`, superseded by the composite index;
  - possibly `IX_intake_records_Stage_NextActionDue`.
- **Pooled variant.** Every index would gain a `FirmId` prefix (see [20-pooled-alternative.sql](../../db/design/20-pooled-alternative.sql)). This was not measured.

## Partitioning
- **Verified on 8.0.36:** a partitioned InnoDB table can't declare foreign keys, and no foreign key can reference a partitioned table. Both give ERROR 1506 (E1, E2). Every unique key must include the partitioning columns, ERROR 1503 (E3).
- So partitioning would mean dropping the FK constraints that carry the design's integrity rules. **Partitioning is not used** for firm tables. Archiving (outbox, audit) is done by moving rows, not by dropping partitions.
- **Partitioning isn't how firms are spread across servers.** Spreading is placement: firm databases are assigned to cells (servers) by the control-plane catalog. Partitioning splits one table inside one server. They solve different problems.
