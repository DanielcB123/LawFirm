# Unified provider integrations

[Home](../README.md) · [Architecture](../architecture/README.md) · [Payments](../billing/payments-and-ledger.md)

> **Current implementation (audited 2026-10-12):** No provider integrations, SDKs, or webhook endpoints exist. See G51, G63 in the [gap map](../delivery/implementation-gap-map.md).

“One application” can orchestrate specialist services without requiring staff or clients to re-enter the matter in multiple tools. Decide which provider is authoritative for each resource and how updates are reconciled. Embedded flows are conditional on provider capabilities and licensing.

| Capability | Internal responsibility | Provider boundary |
|---|---|---|
| Payments | Invoices, allocation, authorized destination, ledger, reconciliation | Tokenized collection, payout/event state |
| E-signature | Agreement version, signer scope, evidence retention | Signature execution and completion evidence |
| Email/SMS | Recipient authorization, minimal message, retries | Delivery and bounce status |
| Calendars | Bookings, legal deadline records, verified internal state | Availability/event synchronization |
| Legal research | Matter context, saved citations/notes, entitlement checks | Licensed content, search/citator functionality |
| E-filing/courts | Approved filings, status, receipts, deadline review | Court-specific accepted submission and docket data |
| Accounting | Approved finance mapping and reconciliation | General accounting where outside product scope |

## Integration contract
Each adapter documents supported capability, terms, permissions, firm-specific credentials, required payload, idempotency, timeouts, retryability, webhook verification, source of truth, failure states, quotas, data location/retention, offboarding, and test environment. Distinguish “sent,” “accepted,” “completed,” and “failed.” A retry must not duplicate charges, filings, signatures, or notices.

Keep external work outside database transactions using outbox/inbox patterns. Persist provider event identity; acknowledge after durable receipt; process retryably. Invalid signatures, unrecognized accounts, or mismatched amounts go to a restricted exception queue.

## Legal research specifically
Do not assume LexisNexis Protégé or any other paid product exposes an unrestricted API or permits redistribution. Verify API availability, entitlement, permitted display/caching, user licensing, and contract in a dedicated provider decision before implementation. Until verified, provide an internal research workspace with citation/source references and approved attachments, not a fabricated API adapter. Research API procurement was not completed for this blueprint.

## Acceptance
A failed provider leaves coherent internal pending state. Replayed/out-of-order callbacks cause no duplicated effects. Revoked tenant credentials cannot be used by delayed jobs. Users do not see a false “filed,” “signed,” or “paid” status based merely on request submission.
