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
| 7 | 2026-10-14 03:25 EDT (UTC−04:00) | D-20261014-3 | Founder | Add switchable global background images |
| 8 | 2026-10-14 03:51 EDT (UTC−04:00) | D-20261014-4 | Founder | Render app backgrounds on a dedicated shell layer |
| 9 | 2026-10-14 03:58 EDT (UTC−04:00) | D-20261014-5 | Founder | Expand background catalog with many style choices |
| 10 | 2026-10-14 04:06 EDT (UTC−04:00) | D-20261014-6 | Founder | Temporarily pin Skyline and hide picker UI |
| 11 | 2026-10-14 04:10 EDT (UTC−04:00) | D-20261014-7 | Founder | Warm only About and Practice hero overlays |
| 12 | 2026-10-14 04:16 EDT (UTC−04:00) | D-20261014-8 | Founder | Increase hero vibrancy for About, Practice, and Home |
| 13 | 2026-10-14 04:18 EDT (UTC−04:00) | D-20261014-9 | Founder | Dial back hero vibrancy to a balanced midpoint |
| 14 | 2026-10-14 04:34 EDT (UTC−04:00) | D-20261014-10 | Founder | Match About and Practice hero balance to Contact and Attorneys |
| 15 | 2026-10-14 04:43 EDT (UTC−04:00) | D-20261014-11 | Founder | Centralize signed-in page header styling in authenticated shell CSS |

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

## D-20261014-3 — Add switchable global background images

- **When:** 2026-10-14 03:25 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added a global background picker with multiple law-themed image options and persistence. The selected image applies to the entire app via shared body background styling.
- **Why:** The founder liked the home hero background image style and asked for similar imagery across all pages, with the ability to switch between options.
- **Good, because:** The same visual style now carries across pages, and users can switch backgrounds without code changes. Selection persists while navigating.
- **Bad, because:** Remote background images can fail to load or shift appearance if the source changes. A busy image can also reduce contrast if overlay tuning is not maintained.
- **Not done:** No admin settings page for theme management. No offline/local image fallback.
- **Evidence:** Browser session on `http://127.0.0.1:5173/` and `/contact`; background picker options were clickable and `localStorage` key `matterforge.app.background-theme` updated to the selected theme.

## D-20261014-4 — Render app backgrounds on a dedicated shell layer

- **When:** 2026-10-14 03:51 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Keep the background option picker, but render the selected background on `.app-shell::before` with a lighter overlay instead of composing a heavy overlay directly in the root CSS variable.
- **Why:** The founder reported that selecting options appeared to do nothing. The selection state was updating, but the previous overlay opacity made image differences hard to see.
- **Good, because:** Background changes are now visibly distinct while still preserving readability. The selected background applies across Home and Contact with the same persisted theme id.
- **Bad, because:** A lighter overlay can reduce text contrast if future card or section backgrounds become more transparent.
- **Not done:** No automatic contrast analyzer and no per-theme overlay tuning controls.
- **Evidence:** Browser session on `http://127.0.0.1:5173/` and `/contact`; `localStorage` and CSS variable updates matched selected options, and visual changes were confirmed in screenshots.

## D-20261014-5 — Expand background catalog with many style choices

- **When:** 2026-10-14 03:58 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Expanded the Background picker from a small set to a broad catalog of 23 options, including corporate office interiors, downtown city imagery, legal motifs, and art-forward options. Added panel scrolling so the larger catalog remains usable.
- **Why:** The founder requested many more options to find a perfect style fit, specifically asking for corporate office, downtown, law-focused, and law/art-beauty crossover backgrounds.
- **Good, because:** The visual direction is now tunable without code edits, with enough variety for different moods while keeping the same app structure.
- **Bad, because:** More remote images increase chance of inconsistent availability or visual quality drift if external hosts change assets.
- **Not done:** No curation workflow or per-firm profile defaults yet.
- **Evidence:** Browser session on `http://127.0.0.1:5173/`; 23 options were present and the options panel confirmed scrollable (`scrollHeight` greater than `clientHeight`).

## D-20261014-6 — Temporarily pin Skyline and hide picker UI

- **When:** 2026-10-14 04:06 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Background is pinned to City Skyline at Night for all pages. The background picker dropdown UI is commented out, while the full catalog stays in code for later feedback sessions.
- **Why:** The founder selected City Skyline at Night as the preferred look and asked to pause user switching while collecting stakeholder feedback.
- **Good, because:** Visual direction stays consistent during review, and the selector can be restored quickly without rebuilding the catalog.
- **Bad, because:** Live comparison by end users is paused until the picker is re-enabled.
- **Not done:** No role-based control or settings-page toggle for showing/hiding the picker.
- **Evidence:** Browser session on `http://127.0.0.1:5173/`; no picker button rendered, and `--app-shell-background-image` matched the skyline URL.

## D-20261014-7 — Warm only About and Practice hero overlays

- **When:** 2026-10-14 04:10 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Updated only `marketing-hero--about` and `marketing-hero--practice` overlay gradients to reduce dull gray/white wash and add warmer accent tone.
- **Why:** The founder said those two headers looked too gray and white and felt visually dull.
- **Good, because:** About and Practice heroes now keep readability while showing more warmth and depth, without changing hero styles on other pages.
- **Bad, because:** Warmer overlays can require future contrast checks if base imagery changes.
- **Not done:** Attorneys and Contact hero overlays were intentionally unchanged.
- **Evidence:** CSS computed backgrounds for `.marketing-hero--about` and `.marketing-hero--practice` in browser reflect the new radial + warm linear gradients.

## D-20261014-8 — Increase hero vibrancy for About, Practice, and Home

- **When:** 2026-10-14 04:16 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Applied a stronger color pass to About and Practice hero overlays and increased Home hero aura intensity (stronger accent gradients, brighter image presence, and richer glow).
- **Why:** The founder said Practice was still too dull and asked for the same improvement on About plus a more vibrant aura on Home.
- **Good, because:** The hero areas now present more visual life and color energy while preserving text readability and page-level scope.
- **Bad, because:** More intense overlays and glow can become visually heavy if future content blocks become more transparent.
- **Not done:** No hero style changes were made to Attorneys or Contact.
- **Evidence:** Frontend build passed; computed styles in browser show updated gradient stacks on `marketing-hero--about`, `marketing-hero--practice`, and `home-hero`.

## D-20261014-9 — Dial back hero vibrancy to a balanced midpoint

- **When:** 2026-10-14 04:18 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Reduced the intensity from the prior vibrant pass on Home, About, and Practice hero treatments, keeping a middle ground between dull and overly vivid.
- **Why:** The founder said the latest pass was too vibrant and asked to split the difference.
- **Good, because:** The three hero sections keep more life than the original gray tone while avoiding visual overpowering.
- **Bad, because:** Fine-tuning hero mood now depends on small opacity/gradient increments, which can require repeated visual reviews.
- **Not done:** No changes to other page hero variants (Attorneys, Contact, Not Found).
- **Evidence:** Frontend build passed; CSS values for `home-hero`, `home-hero__image`, `home-hero__glow`, `marketing-hero--about`, and `marketing-hero--practice` were reduced from the previous pass.

## D-20261014-10 — Match About and Practice hero balance to Contact and Attorneys

- **When:** 2026-10-14 04:34 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replaced the About and Practice hero overlay stacks with the same neutral top-to-bottom overlay profile used by Contact and Attorneys, removing the extra radial accent layers that made those two pages stand out too strongly.
- **Why:** The founder compared screenshot pairs and asked for the same balanced level of vibrancy/neutrality from Contact and Attorneys to be applied to Practice Areas and About.
- **Good, because:** Hero tone now feels consistent across marketing pages, and the two previously standout headers no longer overpower surrounding content.
- **Bad, because:** Matching overlays across pages can reduce unique page mood cues that were previously created through warmer page-specific accents.
- **Not done:** No changes were made to Home, Contact, Attorneys, or Not Found hero styling in this decision.
- **Evidence:** Frontend build passed and stylesheet diagnostics were clean after the CSS normalization.

## D-20261014-11 — Centralize signed-in page header styling in authenticated shell CSS

- **When:** 2026-10-14 04:43 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added centralized workspace header selectors in `AppShell.css` under `.app-shell--authenticated` for page `<h1>` and the immediately following `.lead`, and removed one page-level utility override (`TasksCalendarPage`) so it inherits the shared rule.
- **Why:** The founder asked for a way to apply signed-in header styling across all authorized pages without repeating utility-class edits page by page.
- **Good, because:** Header tone/scale updates for signed-in pages now happen in one place and flow to workspace pages that follow the standard `.page` structure.
- **Bad, because:** A page that needs intentionally different heading presentation now has to opt out or override the shared selector to avoid unintentional uniformity.
- **Not done:** No global conversion to a dedicated shared React header component in this step.
- **Evidence:** Frontend build passed and edited-file diagnostics reported no linter errors.
