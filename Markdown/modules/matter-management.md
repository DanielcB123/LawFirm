# Matter work, deadlines, and accountability

[Home](../README.md) · [Construction](../practice-areas/construction-law.md) · [PI](../practice-areas/personal-injury.md)

> **Current implementation (audited 2026-10-12):** Matter shell, stage history, tasks with attorney verification flag, and timeline actions (`backend/Features/Matters/MattersEndpoints.cs`). Staff calendar with deadline type, source, and verification fields (`backend/Features/Calendar/CalendarEndpoints.cs`). Gaps: no assignment-based access; the calendar verifier is taken from the request body; no jurisdiction, trigger, or rule version; no date-only deadlines; hard deletes. See G10, G30, G31, and G33 in the [gap map](../delivery/implementation-gap-map.md).

Each matter has a unique firm number, practice area, jurisdiction, parties/roles, responsible attorney, team, engagement, phase, assignments, and access policy. Track tasks, dependencies, requests, calendar events, deadlines, evidence, communications, expenses, and decisions in one workspace.

Tasks have owner, due date, state, dependencies, completion evidence, and escalation rules. Owner changes and completion are auditable. Supervisors see stale matters, overdue work, blocked intake, documents awaiting review, and missing financial checks.

## Legal deadlines
Store deadline type, jurisdiction, governing source, triggering event, source date, computation inputs, time zone/date semantics, calculated proposal, attorney-verified date, reviewer, and rule version. Manual override requires recorded rationale. Changes to trigger facts flag dependent deadlines for re-review.

Do not hard-code a single Texas limitation period or assume all court calendars share holiday rules. Rules vary by claim, court, jurisdiction, service method, exceptions, and facts. Automated date proposals are not authoritative until reviewed under the firm's policy. Alert failures need monitoring and escalation; reminders alone do not satisfy reliable calendaring.

## Practice workflow templates
Version templates; capture the version applied to a matter. Template changes do not silently alter active matters. Support deviations with a reason. Keep phase transitions separate from irreversible financial postings and client publication.

Acceptance: overdue work is visible; deadline provenance is retrievable; changing a trigger flags dependent calculations; a restricted staff member cannot access a matter through a dashboard count/search/export; closing a matter checks outstanding tasks, final finances, notices, exports, and holds.
