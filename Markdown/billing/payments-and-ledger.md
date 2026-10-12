# Payments, ledger correctness, and financial cybersecurity

[Home](../README.md) · [Billing](README.md) · [Trust](trust-and-settlement.md)

## Money representation
Use fixed decimal or integer minor units, currency, defined rounding, and explicit conversion rules. Never use binary floating point for posted money. A payment can allocate across invoices; an invoice can receive multiple payments. Track unallocated funds explicitly. A processor payout is not the same entity as a client payment.

## Ledger posting
Journal entries have effective date, recorded timestamp, currency, source, posting state, actor, and idempotency reference. Lines reference accounts and any required beneficiary/matter subledger. Debit and credit totals must balance within the same currency at posting. Post atomically; pending drafts may be editable, posted entries are immutable and corrected with reversing/new entries.

Prevent negative allowed trust subledger balances under concurrent withdrawals using appropriate locking or serializable execution and retry. Define account mappings and whether invoice issuance is accrual, cash-only reporting, or another accounting policy with the firm's accountant. Do not silently infer a general ledger from payment notifications.

## Provider flow
Create an internal payment intent scoped to firm, merchant account, payer, amount/currency, destination, and invoice/version. Use an idempotency key for provider requests. Verify signed webhooks with appropriate timestamp/replay checks; store a durable receipt and unique provider/account/event identifier before applying effects. Webhooks can arrive out of order and repeat. Re-fetch authoritative provider state when needed; reconcile instead of assuming callback order.

A browser success redirect does not prove payment. A verified event can still represent pending rather than cleared funds. Provider outage results in pending/retryable state, bounded backoff, visible operational alerts, and a safe client message.

## Security
Use hosted/tokenized payment capture where possible and validate PCI scope with the chosen provider. Encrypt secrets; restrict provider credentials by firm/account; verify destination changes; require step-up authentication and dual approval for sensitive disbursement changes. Never send money solely because an email supplies new bank details. Call-back verification uses previously verified contact information under firm policy.

Mask permitted payment data, audit changes, separate finance roles, and monitor abnormal refunds/destination changes. Webhook endpoints require authenticity and account-to-tenant mapping even when not using user login. Do not trust a tenant_id in provider metadata alone.

## Acceptance
Repeated callback → one financial effect. Two simultaneous withdrawals cannot overspend. Wrong merchant/firm mapping fails. Provider amount mismatch is quarantined. Partial payment/refund/dispute allocation reconciles. Stale approval cannot authorize changed payment destination or amount. No secrets or payment payload details appear in routine logs.
