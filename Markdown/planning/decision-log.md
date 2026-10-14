# Decision log

[Planning](README.md) · [Open decisions](open-decisions.md) · [Progress plan](../../ProgressPlan.md) · [Slice history](../../SLICE_HISTORY.md) · [Schema evolution](../database/schema-evolution.md)

Append-only. Oldest first. The index below is the order of the `When` timestamps. Rules: [planning README](README.md). Work before 2026-10-13 22:40 EDT is in the slice history without this shape and is not backfilled.

## Index

| Order | When | Id | Decided by | Choice |
|---|---|---|---|---|
| 1 | 2026-10-13 22:40 EDT (UTC−04:00) | D-20261013-1 | Founder | Signed-in Parties becomes Contacts |
| 2 | 2026-10-13 22:55 EDT (UTC−04:00) | D-20261013-2 | Founder | Full Clio-shaped sidebar, empty money pages stay empty |
| 3 | 2026-10-13 23:05 EDT (UTC−04:00) | D-20261013-3 | Founder | Start this decision log |
| 4 | 2026-10-13 23:15 EDT (UTC−04:00) | D-20261013-4 | Founder | Put planning and database history in two folders |
| 5 | 2026-10-14 02:35 EDT (UTC−04:00) | D-20261014-1 | Founder | Widen the public appointment request dialog |
| 6 | 2026-10-14 02:59 EDT (UTC−04:00) | D-20261014-2 | Founder | Scale up the appointment dialog and prevent overlap |

## D-20261013-1 — Signed-in Parties becomes Contacts

- **When:** 2026-10-13 22:40 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** The signed-in menu label Parties is Contacts. The page is `/workspace/contacts`. `/workspace/parties` redirects there. The directory shows name, person or organization, email, phone, and aliases. The API and database record remain a party (`/api/parties`, `frontend/src/features/parties`).
- **Why:** The founder compared the signed-in menu with Clio’s sidebar. Clio calls this destination Contacts. Staff looking for a person or company were landing on a screen that talked about parties.
- **Good, because:** A signed-in attorney can open Contacts and see the seeded people and organizations with email and phone. Search still filters that directory. Observed 2026-10-13 as Avery Attorney: Alicia Warren and Northgate Builders LLC; search "Northgate" returned the organization.
- **Bad, because:** The screen says Contact and the stored record is a Party. A later change could add a second contacts table and split the conflict graph from the directory. The redirect also hides the old path, so a bookmark that still says `/workspace/parties` will not show that the name changed.
- **Not done:** No party schema change. No contact merge, import, or relationship editor beyond what the parties API already did.
- **Evidence:** Browser session on `http://localhost:5173` against the running API. Slice history entry `2026-10-13 - Signed-in sidebar and Contacts directory`.

## D-20261013-2 — Full Clio-shaped sidebar, with honest empty destinations

- **When:** 2026-10-13 22:55 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** The signed-in menu now lists Dashboard, Calendar, Tasks, Matters, Contacts, Activities, Billing, Accounts, Documents, Communications, Reports, Integrations, and Settings, then a Practice group for Intake, Consultations, and Conflicts. Resource centre is in the footer. Each item has a page under `frontend/src/pages/workspace/`.
- **Why:** The founder asked for every Clio sidebar option, a few destinations this firm needs beyond that list, and a page for each option. Intake, Consultations, and Conflicts are the extra destinations because those workflows already exist and Clio does not show them as top-level items in the screenshot.
- **Good, because:** Activities, Documents, Communications, and Reports show records the API already stores. Observed as Avery Attorney: two timeline rows, two matter files, two consultation messages, and report counts of 0 open tasks, 1 active matter, 2 intake records, and 2 contacts. Billing, Accounts, and Integrations open and say that no invoices, balances, or connections exist.
- **Bad, because:**
  - Billing, Accounts, and Integrations can be mistaken for working books. A later edit that prints `$0.00` or a Connect button that does not persist would look like a ledger or a live integration.
  - Documents lists metadata already saved on a matter. It does not download a file. Someone can treat that list as the document system and skip the binary pipeline in gap G32.
  - Communications is the consultation queue under another name. Building matter messages on that list would mix consultation requests with client communications (gaps G41 and G42).
  - Reports are live counts, not a report product. They must not be cited as billing, trust, or compliance reports.
  - Activities and Documents ask for every open matter, then one request per matter. That will get slow as the matter list grows.
  - Settings shows the current session. It is not a firm administration screen.
  - The menu is taller than a laptop screen, so Practice items and Settings sit in the scroll area.
  - Billing is visible to anyone with `billing.manage` or `billing.read.own`, including a client. Accounts stays limited to `billing.manage`.
- **Not done:** No invoice, trust ledger, payment, file download, secure matter message, or outside integration. Gap-map rows G32, G41, G42, G50, G51, and G52 stay open.
- **Evidence:** Browser session on `http://localhost:5173` as Avery Attorney, 2026-10-13. Slice history entry `2026-10-13 - Full signed-in sidebar`.

## D-20261013-3 — Start a decision log separate from the slice history

- **When:** 2026-10-13 23:05 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted. The file created here later moved; see D-20261013-4. This entry is not rewritten to hide that it was first written at `Markdown/delivery/decision-log.md`.
- **What:** Added a decision log and linked it from the README, AGENTS.md, ProgressPlan, and the slice history. Gap-map rows G32, G41, G42, G50, and G52 say the new workspace screens do not close those gaps.
- **Why:** The founder asked for a look-back record of what changed, when, why, the benefit, and the possible future downfall. The slice history had the change and the check, and it did not have the benefit or the downfall.
- **Good, because:** A later session can read D-20261013-1 and D-20261013-2 before adding a contacts table, a fake balance, or a download button.
- **Bad, because:** Two logs can drift if a session updates only one of them. Older slices were left in their original shape.
- **Not done:** No rewrite of slice history entries before 2026-10-13 22:40 EDT.
- **Evidence:** Documentation only.

## D-20261013-4 — Planning folder and database history live apart

- **When:** 2026-10-13 23:15 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Planning records live in `Markdown/planning/`. Database evolution and the UML class diagram live in `Markdown/database/`. The decision log and the open-decision list moved here. Short pointers remain at the old `delivery/` paths. The implemented class-to-table map is [uml-class-diagrams.md](../database/uml-class-diagrams.md). The ordered schema history is [schema-evolution.md](../database/schema-evolution.md).
- **Why:** The founder said the notes were scattered and that decision order was hard to see. [MADR](https://adr.github.io/madr/) records a date and the decision-makers with each choice. [Evolutionary database design](https://www.martinfowler.com/articles/evodb.html) keeps schema changes as an ordered history next to the database, not inside the product decision log. A UML class diagram maps classes to tables, attributes to columns, and associations to foreign keys, so it sits beside the database notes and links both ways.
- **Good, because:** The index at the top of this file is sorted by timestamp, so D-20261013-1 is visibly before D-20261013-2. Database shape has its own timeline, tied to the EF migration ids.
- **Bad, because:** Pointers at the old paths can be edited as if they were still the record. The record is this file. A second database folder was not created; adding one later would split the schema history from `Markdown/database/`.
- **Not done:** Domain documents were not moved. Older slice entries were not rewritten. No new SQL migration.
- **Evidence:** Documentation only. Link targets checked by path while writing.

## D-20261014-1 — Widen the public appointment request dialog

- **When:** 2026-10-14 02:35 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** The Contact page “Request appointment” dialog uses its own wider card. Contact fields and the message sit on the left. The month calendar and time choices sit beside them. The shared calendar-task dialog stays on the narrower card.
- **Why:** The founder said the request dialog was tall and narrow, so the whole request was not visible without scrolling the dialog.
- **Good, because:** On a 1440×900 viewport the card was 1040px wide and 554px tall, with no internal scroll. Choosing a date, choosing 10:30 AM, typing a name, and moving to the next month all still worked.
- **Bad, because:** A window narrower than about 860px stacks the columns again, and that stacked dialog still scrolls. The wider card also covers more of the contact page behind it.
- **Not done:** No change to how slots are stored or how a request is submitted. The workspace calendar dialog was not restyled.
- **Evidence:** Browser session on `http://127.0.0.1:5173/contact` against the already running API. Slice history entry `2026-10-14 - Appointment request dialog layout`.

## D-20261014-2 — Scale up the appointment dialog and prevent overlap

- **When:** 2026-10-14 02:59 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Keep the current appointment dialog design, increase its overall visual scale, and add responsive guardrails so the “Available appointment time” section does not overlap the basic contact inputs.
- **Why:** The founder asked to preserve the current restyle but make it larger overall and remove overlap seen between the appointment picker area and the input area.
- **Good, because:** The card now scales up to 1220px with larger type and controls. On a 1400×900 viewport, the card measured 1220×611 with no internal scroll and no input-vs-picker overlap.
- **Bad, because:** The wider/larger card covers more of the page behind the modal, and the one-column switch now happens earlier (under 1200px), so medium windows lose the side-by-side layout.
- **Not done:** No API or submission behavior changed. No workspace modal styles changed.
- **Evidence:** Browser session on `http://127.0.0.1:5173/contact` against the already running API. Slice history entry `2026-10-14 - Appointment dialog scale and overlap guard`.
