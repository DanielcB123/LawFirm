# Trust accounting and settlement distribution

[Home](../README.md) · [Ledger](payments-and-ledger.md) · [Sources](../research/sources.md)

## Legal reference freshness
The State Bar's current linked rules PDF is effective March 7, 2025 and lists **1.15 Safekeeping Property**, while older trust guides refer to 1.14. Cite the current text and its effective date; do not copy obsolete numbering into product policies. Rules 1.03, 1.04, 1.05, 1.15, and 1.18 are relevant review inputs [S12]. The software requirements here are proposed controls, not a determination of the firm's legal obligations.

Firm counsel/accountant must approve trust procedures, permitted destinations, record retention, fee treatment, reconciliation, authorization, and jurisdiction-specific configuration before real trust funds flow. Do not treat every payment as earned revenue or every retainer as non-refundable.

## Trust ledger controls
Separate operating and trust accounts; track client/third-party beneficiary subledgers and matter attribution. Record receipt source, ownership, cleared status, purpose, and supporting evidence. Fees/costs transfer only under approved agreements and authority. Preserve disputed money under the applicable process. No commingling through an undifferentiated wallet.

Require three-way reconciliation: adjusted bank statement balance, trust control ledger, and sum of individual beneficiary balances. Record outstanding deposits/checks, differences, reviewer, reconciliation date, attachments, and resolved exceptions. Monthly reconciliation is a proposed baseline informed by State Bar guidance [S13], subject to current rules and firm's approved policy.

## Settlement workflow
Offer recorded → attorney review → client authority/acceptance recorded → executed agreement → funds received → cleared-funds verification → lien/cost/fee review → approved distribution → disbursement → reconciliation → closing statement. These are independent checkpoints; receipt does not automatically unlock distribution.

Version gross proceeds, fee calculation basis, expenses, liens, third-party payments, reserves, and client net. Preserve exact fee-agreement version and supporting documents. Require review when amounts, recipients, destinations, or terms change. Client acknowledgment and authorized settlement consent have distinct meanings and evidence.

A posting/disbursement references the approved version. Concurrent changes invalidate approval. Detect duplicate disbursement commands with stable idempotency references. Require separation of duties appropriate to firm size; document any approved small-firm exception with a compensating review.

## Acceptance before production
Test exact balances, partial settlements, multiple clients/beneficiaries, disputed liens, bounced funds, chargebacks, reversal, unclaimed funds workflow, changed bank details, duplicate requests, simultaneous disbursements, and full restore. Reconciliation evidence and policy sign-off are required before this module handles real trust money.
