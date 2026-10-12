# Intake, conflict review, and consultations

[Home](../README.md) · [Journey](../product/client-journey.md) · [Matters](matter-management.md)

> **Current implementation (audited 2026-10-12):** Intake stage machine with a conflict-decision gate on `Accepted` and idempotent convert-to-matter (`backend/Features/Intake/IntakeEndpoints.cs`). Conflict search with restricted masking (`backend/Features/Conflicts/ConflictEndpoints.cs`). Public consultation booking (`backend/Features/Consultations/ConsultationsEndpoints.cs`). Gaps: booking is not concurrency-safe and uses UTC hours; the conflict decision is not bound to the intake; there is no engagement step. See gaps G20–G25 in the [gap map](../delivery/implementation-gap-map.md).

Collect minimal information before conflicts triage: contact, matter category, relevant parties, urgency, and preferred consultation time. Provide attorney-approved submission wording and confidentiality handling. Restrict unaccepted submissions; do not publish them to broad staff dashboards.

Scheduling supports attorney/service availability, buffers, time zones, cancellation windows, rescheduling, attendance/no-show, and confirmed booking status. Prevent double booking using transactional reservation or equivalent concurrency control. External-calendar synchronization can be delayed; display confirmed internal state and escalate conflicting external changes.

Version intake forms by practice area; preserve the exact submitted form and answers. Support save/resume, required documents, staff requests for missing information, and translation needs if established. Avoid excessive sensitive data in public forms and analytics.

Conflict review searches current/former clients, prospective clients, organizations, aliases, adverse parties, and relevant relationships within the firm. Matching suggests review; it does not automatically clear conflicts. Record reviewer, reviewed party set, decision, rationale classification, and any approved consent document. Restrict who can view conflict-search results.

An engagement references the fee-agreement version, required signers, signature evidence, attorney acceptance, and prerequisites. Booking and payment must not automatically mark representation active. Declined matters retain controlled records and policy-defined notices.

Acceptance: concurrent booking attempts produce one valid reservation; conflict matches require an authorized decision; an incomplete/unsigned engagement cannot silently become an active matter; a changed party set reopens appropriate review.
