# Client portal

[Home](../README.md) · [Publishing](communications.md) · [Access](../security/confidentiality-and-access.md)

> **Current implementation (audited 2026-10-12):** Absent. There is a `Client` role and a static `/api/portal/client` demo endpoint. Frontend workspace routes check only for a token. See G40 in the [gap map](../delivery/implementation-gap-map.md).

## Landing dashboard
Show authorized matters, responsible team and contact method, current approved phase, latest approved update with publication date, next expected update date, upcoming appointments, requested actions, shared documents, and permitted financial information. Give waiting periods a clear explanation where approved. A progress bar must not imply a predictable litigation outcome or fixed completion percentage.

## Matter workspace
Provide overview, timeline, documents, messages, appointments, action requests, billing/payments, and closing information. Support mobile devices, keyboard navigation, clear language, save/resume forms, loading/error/empty states, accessible validation, and explicit time zones. Treat WCAG 2.2 AA as a product acceptance target, not a claim of legal certification.

Client actions include scheduling/rescheduling permitted appointments, submitting intake, uploading requested files, acknowledging updates, completing signatures, paying authorized invoices/deposits, sending messages, and downloading approved materials. Receipt/acknowledgment does not imply legal approval or consent to settlement.

## Visibility boundary
No internal notes, conflict-search results, privileged strategy drafts, opposing-party private records, hidden evidence metadata, staff financial data, or another joint client's private communications are exposed by default. Separate client DTOs/read models from staff serializers. Every list, detail, count, search, preview, export, notification, and attachment endpoint enforces matter and audience access.

Joint clients and organizational representatives require explicit permissions. A payer may be permitted to see a bill without access to legal strategy. Revoking a representative removes future access and invalidates active sessions; define short-lived download limitations and record previous access.

## Acceptance examples
A client with two matters sees only their approved records. A guessed document ID or modified matter URL fails. An unpublished update never appears in search or email. An invited representative sees only specifically granted material. A revoked user cannot reuse a session to download a new URL. Staff can preview exactly what a selected client would see through a controlled, audited function.
