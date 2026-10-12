# Secure communications and published updates

[Home](../README.md) · [Portal](client-portal.md) · [Confidentiality](../security/confidentiality-and-access.md)

> **Current implementation (audited 2026-10-12):** Absent. No messaging, publication model, outbox, or notification provider exists. Calendar `RemindersJson` is stored but never dispatched. See G41–G43 in the [gap map](../delivery/implementation-gap-map.md).

Use firm/matter-scoped conversations with explicit participants. Preserve message authorship, time, attachment versions, delivery/read state, and permitted correction history. A read receipt does not prove legal comprehension or consent. Provide an urgent contact path appropriate to firm policy; asynchronous portal messaging should not promise emergency monitoring.

## Publication workflow
Draft → review → approved → published → superseded/withdrawn. Record source version, audience, publisher, approver, and timestamps. Restricted attorney strategy cannot be turned into client content by an automated blanket synchronization. Separate the substantive approved update from task/event logs.

Staff own a configured update cadence. Show an honest last-published date and next-update expectation. Stale updates create staff reminders. If there is no meaningful movement, staff can publish a reviewed explanation of what the matter is waiting on rather than invent progress.

## Notifications
Publish a durable outbox event with content identifier and intended audience. Before sending, workers recheck recipient access and current publication state. Email/SMS default to minimal notices such as “A new update is available in your secure portal.” Do not include confidential case details, medical data, bills, or sensitive links in ordinary notifications.

Track provider accepted/delivered/bounced states separately. Deduplicate notifications; bound retries; monitor dead letters and escalation. Notification failure does not undo a published portal update. Critical messages require a firm-defined fallback process and evidence, not just best-effort delivery.

Acceptance: unpublished/revoked content is not notified; no cross-client message leak; reassigned/revoked recipients are rechecked at send time; stale updates appear in a staff queue; portal access remains useful when the notification provider is down.
