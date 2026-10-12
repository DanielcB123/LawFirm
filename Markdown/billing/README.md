# Billing and money

[Home](../README.md) · [Ledger and payments](payments-and-ledger.md) · [Trust and settlement](trust-and-settlement.md)

> **Current implementation (audited 2026-10-12):** Absent. Only permission constants (`billing.read.own`, `billing.manage`) and static demo endpoints exist (`backend/Features/Authorization/AuthorizationDemoEndpoints.cs`). See G50–G53 in the [gap map](../delivery/implementation-gap-map.md).

## Three distinct domains
**SaaS subscription billing:** the platform bills law firms for software. **Legal billing:** the firm bills clients under its fee agreements. **Trust/settlement accounting:** the firm safeguards and distributes money belonging to clients/others. Never combine these balances or merchant accounts.

Support hourly, flat, contingency, and mixed arrangements through versioned fee agreements. Define rate snapshots, time entry approval, costs/expenses, invoice review, discounts, credits, write-offs, tax applicability if any, partial payments, refunds, and disputes. Do not assume a contingency percentage or prescribe how costs are deducted.

Clients see authorized invoices, statements, receipts, payment status, and any approved trust information. Legal invoices may contain sensitive descriptions; their access policy can differ from matter documents and from access granted to third-party payers.

A payment processor handles card/bank collection under approved terms. The platform stores permitted provider references, never card numbers/CVV or bank-login credentials. Destination account mapping, fees, chargebacks, and reversals require explicit review before trust payments are enabled.

## Minimum finance acceptance
Invoice calculations are exact and reproducible. Issued invoices capture immutable agreement/rate data. Duplicate payment callbacks do not double-credit. Failed payments remain unpaid. Refunds and disputes update allocations with a clear audit trail. Provider settlement, bank clearing, and money available for distribution are separate states.

If native trust accounting is not yet proven, use an integrated validated system and surface its workflow/status in the app. Do not launch a fake trust ledger merely to present an all-in-one demo. See [Roadmap](../delivery/roadmap-and-acceptance.md) for the release gate.
