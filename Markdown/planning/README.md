# Planning

[Home](../README.md) · [Decision log](decision-log.md) · [Open decisions](open-decisions.md) · [Database evolution](../database/schema-evolution.md)

This folder is the place to see **what was decided, when, and by whom**, in order. Product domain notes stay in their own folders (`product/`, `modules/`, `billing/`, `security/`). Delivery checklists stay in `delivery/`. Database shape stays in `database/`.

## How a decision is recorded

The shape follows [Markdown Architectural Decision Records (MADR)](https://adr.github.io/madr/), which keeps a date, the decision-makers, the options, and the consequences. This repository uses **one chronological log** instead of one file per decision, so a later choice cannot be filed where it looks earlier than an earlier choice.

Every new choice is appended to [decision-log.md](decision-log.md). Required fields:

| Field | Rule |
|---|---|
| Id | `D-YYYYMMDD-n`. Never reuse or renumber. |
| When | Local date and clock time with the offset, for example `2026-10-13 23:15 EDT (UTC−04:00)`. Date-only is allowed only when the clock time was not recorded, and the entry must say so. |
| Decided by | The person or role who made the choice. Use Founder, Technical owner, or Unassigned. Do not invent a personal name. |
| Status | `accepted`, `proposed`, or `superseded by D-…` |
| What / Why | The choice and the reason |
| Good, because | The benefit that follows from the choice, or that was observed |
| Bad, because | The future downfall |
| Not done | What a reader must not treat as built |
| Evidence | The check that was run, or "not exercised" |

Order is the `When` timestamp, oldest first. The index at the top of the decision log is rebuilt from those timestamps. If two entries share a timestamp, the higher id is later, and the entry says why the clock did not separate them.

Unresolved questions stay in [open-decisions.md](open-decisions.md) until someone decides them. A decision moves into the log. It is not deleted from the open list without a log id that closes it.

Slice checks remain in [SLICE_HISTORY.md](../../SLICE_HISTORY.md). Current handoff state remains in [ProgressPlan.md](../../ProgressPlan.md).
