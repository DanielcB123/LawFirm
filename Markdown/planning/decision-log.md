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
| 16 | 2026-10-14 04:52 EDT (UTC−04:00) | D-20261014-12 | Founder | Greatly reduce Home hero left burgundy gradient weight |
| 17 | 2026-10-14 04:55 EDT (UTC−04:00) | D-20261014-13 | Founder | Shift Home hero left accent from burgundy to blue |
| 18 | 2026-10-14 04:58 EDT (UTC−04:00) | D-20261014-14 | Founder | Remove remaining Home hero red cast with full blue rebalance |
| 19 | 2026-10-14 05:00 EDT (UTC−04:00) | D-20261014-15 | Founder | Force Home hero plain blue and disable image/glow for isolation test |
| 20 | 2026-10-14 05:03 EDT (UTC−04:00) | D-20261014-16 | Founder | Restore full Home hero visuals with blue-only tint layers |
| 21 | 2026-10-14 05:06 EDT (UTC−04:00) | D-20261014-17 | Founder | Annotate Home hero CSS controls for manual tuning |
| 22 | 2026-10-16 23:59 EDT (UTC−04:00) | D-20261016-1 | Founder | Start a dedicated `docs/migration` workspace and branch for Laravel/Vue/Docker migration |
| 23 | 2026-10-17 15:34 EDT (UTC−04:00) | D-20261017-5 | Founder | Restore Vue shell styling from pre-migration React snapshot and repair malformed compose line |
| 24 | 2026-10-17 15:56 EDT (UTC−04:00) | D-20261017-6 | Founder | Normalize compose command YAML to reduce false duplicate-key diagnostics |
| 25 | 2026-10-17 16:00 EDT (UTC−04:00) | D-20261017-7 | Founder | Use inline command arrays and explicit volume maps in compose to clear duplicate-key lint |
| 26 | 2026-10-17 22:20 EDT (UTC−04:00) | D-20261017-8 | Founder | Rebuild route-level Vue parity and consultation queue flow with seeded role matrix |
| 27 | 2026-10-17 23:46 EDT (UTC−04:00) | D-20261017-9 | Founder | Enforce single shared legacy-parity consultation modal across public CTA flows |
| 28 | 2026-10-18 03:45 EDT (UTC−04:00) | D-20261018-1 | Founder | Make Laravel container startup deterministic and profile-driven |
| 29 | 2026-10-18 03:58 EDT (UTC−04:00) | D-20261018-2 | Founder | Bind consultation modal host with top-level refs to avoid always-truthy nested ref conditionals |
| 30 | 2026-10-18 04:05 EDT (UTC−04:00) | D-20261018-3 | Founder | Document Docker profile validation and operator commands in architecture README |
| 31 | 2026-10-18 04:12 EDT (UTC−04:00) | D-20261018-4 | Founder | Publish seeded auth user and permission matrix in docs |
| 32 | 2026-10-18 04:36 EDT (UTC−04:00) | D-20261018-5 | Founder | Implement Laravel/Vue workspace parity for calendar, matters, tasks, and editable consultation triage |
| 33 | 2026-10-18 04:44 EDT (UTC−04:00) | D-20261018-6 | Founder | Canonicalize Docker/Artisan command ownership and annotate command use cases in-place |
| 34 | 2026-10-18 05:00 EDT (UTC−04:00) | D-20261018-7 | Founder | Move Docker/Artisan command workflow into a dedicated operations runbook and index it from architecture/home |
| 35 | 2026-10-19 15:28 EDT (UTC−04:00) | D-20261019-10 | Founder | Laravel control plane plus one MySQL schema per firm, without moving existing matter tables yet |
| 36 | 2026-10-19 18:39 EDT (UTC−04:00) | D-20261019-12 | Founder | Adopt SaaS-first outer layer recommendation and document implementation options |
| 37 | 2026-10-19 20:35 EDT (UTC−04:00) | D-20261019-11 | Founder | Enforce DNS TXT domain verification and move workspace routes/data to tenant schemas |
| 38 | 2026-10-20 11:28 EDT (UTC−04:00) | D-20261020-1 | Founder | Package Option B as a run-ready implementation prompt and replace the brainstorming artifact |
| 39 | 2026-10-20 11:39 EDT (UTC−04:00) | D-20261020-2 | Founder | Publish strict v2 Option B prompt with phase checklists and stop-if-fail gates |
| 40 | 2026-10-20 11:49 EDT (UTC−04:00) | D-20261020-3 | Founder | Execute strict v2 Option B host-surface rollout with platform/app shell behavior and fallback-host continuity |
| 41 | 2026-10-23 10:57 EDT (UTC−04:00) | D-20261023-1 | Founder | Preserve frontend port when redirecting to tenant hosts in local dev login and firm-selection flows |
| 42 | 2026-10-23 11:04 EDT (UTC−04:00) | D-20261023-2 | Founder | Keep platform-admin sign-ins on SaaS admin routes and navigation on platform hosts |
| 43 | 2026-10-23 11:15 EDT (UTC−04:00) | D-20261023-3 | Founder | Remove cross-host post-login redirects on platform host to prevent localStorage token loss across origins |
| 44 | 2026-10-23 13:01 EDT (UTC−04:00) | D-20261023-4 | Founder | Split platform administration into PlatformOwner and PlatformOps with action-level backend permission enforcement |
| 45 | 2026-10-23 15:36 EDT (UTC−04:00) | D-20261023-5 | Founder | Let firm partners manage firm users in-tenant with role-based permissions and add firm-branded seeded login |

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

## D-20261014-12 — Greatly reduce Home hero left burgundy gradient weight

- **When:** 2026-10-14 04:52 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Reduced the Home hero's left-side accent radial intensity from `var(--accent) 21%` to `6%` (with a slightly wider fade) to remove the heavy burgundy cast on the left side of the header.
- **Why:** The founder requested removing or significantly reducing the burgundy gradient on the left part of the Home page header.
- **Good, because:** The Home hero reads cleaner and less color-heavy while preserving the established header layout and typography contrast.
- **Bad, because:** If reduced too far, the hero can lose visual depth and feel flatter relative to the rest of the page palette.
- **Not done:** No changes to About, Practice, Contact, or Attorneys hero overlays in this decision.
- **Evidence:** Frontend build passed and stylesheet diagnostics were clean after the gradient reduction.

## D-20261014-13 — Shift Home hero left accent from burgundy to blue

- **When:** 2026-10-14 04:55 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replaced the Home hero left radial accent hue with a muted blue (`#4b82c7` blend) instead of the prior burgundy-tinted accent.
- **Why:** The founder changed direction and requested a blueish left-side hue on the Home header instead of burgundy.
- **Good, because:** The Home hero now has a cooler tonal balance and avoids the warm-heavy left edge.
- **Bad, because:** The cooler accent can diverge from warm tones used in other headers, which may need future cross-page harmonization.
- **Not done:** No layout, typography, or non-Home hero overlays were changed in this decision.
- **Evidence:** Frontend build passed and edited-file diagnostics reported no linter issues.

## D-20261014-14 — Remove remaining Home hero red cast with full blue rebalance

- **When:** 2026-10-14 04:58 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replaced the Home hero's remaining warm components with blue-neutral values: right-side radial accent switched to blue and base linear gradient moved from maroon/beige tinting to cool blue/light-blue tinting.
- **Why:** The founder observed the header still looked gray/red and asked for a blue replacement rather than red.
- **Good, because:** The Home header now consistently reflects a cool blue direction instead of mixed warm-cool tones.
- **Bad, because:** Cooler overlays can make warm CTA accents feel more prominent and may need minor harmonization tuning later.
- **Not done:** No changes were made to other page hero overlays in this decision.
- **Evidence:** Frontend build passed and stylesheet diagnostics remained clean after the hue rebalance.

## D-20261014-15 — Force Home hero plain blue and disable image/glow for isolation test

- **When:** 2026-10-14 05:00 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Set `.home-hero` to a solid blue background (`#6f98cf`) and disabled `.home-hero__image` plus `.home-hero__glow`.
- **Why:** The founder still observed a red-gray hue and requested a plain-blue test to verify color behavior without layered gradients or image tinting.
- **Good, because:** This isolates hue behavior completely and confirms whether any warmth is coming from hero layers versus other page surfaces.
- **Bad, because:** The Home hero loses depth/brand texture while this test mode is active.
- **Not done:** No changes to non-Home hero styles or CTA component styles.
- **Evidence:** Frontend build passed and edited-file diagnostics remained lint-clean.

## D-20261014-16 — Restore full Home hero visuals with blue-only tint layers

- **When:** 2026-10-14 05:03 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Re-enabled the full Home hero layer stack (background gradients, Lady Liberty image overlay, and glow) and set all tint layers to cool blue values only.
- **Why:** The founder confirmed the plain-blue test worked and requested returning to the original beautiful hero composition while keeping blue instead of red.
- **Good, because:** The hero regains depth and visual character without the prior red cast.
- **Bad, because:** Multiple blue overlay layers can still need minor opacity tweaks on different displays to keep perfect text contrast.
- **Not done:** No changes were made to non-Home hero pages in this decision.
- **Evidence:** Frontend build passed and stylesheet diagnostics remained clean after restoration.

## D-20261014-17 — Annotate Home hero CSS controls for manual tuning

- **When:** 2026-10-14 05:06 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added inline comments in the Home hero stylesheet block documenting how to adjust blue intensity, gradient depth, image overlay opacity, and glow spread/strength.
- **Why:** The founder asked for explicit guidance in-file to manually iterate on visual balance quickly.
- **Good, because:** Designers/developers can tune values in place without re-deriving which layer controls which visual result.
- **Bad, because:** If future refactors change layer behavior but comments are not updated, the guidance can drift from actual rendering.
- **Not done:** No visual values were changed in this decision.
- **Evidence:** Frontend build passed and CSS diagnostics stayed lint-clean after comment-only edits.

## D-20261016-1 — Start dedicated migration docs workspace and branch

- **When:** 2026-10-16 23:59 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Begin Laravel/Vue/Docker migration with:
  - isolated branch `migration/laravel-vue-docker-phase1`,
  - persistent migration documentation folder `docs/migration/`,
  - baseline command evidence recorded before application rewrite.
- **Why:** The migration scope is broad and high-risk. Work needs explicit audit coverage, reproducible checkpoints, and clear separation from current `main` behavior while parity is established.
- **Good, because:** Baseline outcomes, open blockers, and Docker/port constraints are now tracked in one place and can be resumed across sessions without restarting discovery.
- **Bad, because:** Documentation overhead is front-loaded and slows immediate feature rewriting; stale tracking is possible if updates are skipped.
- **Not done:** Full file-by-file deep review and visual screenshot baseline are still in progress and marked as pending in `docs/migration/`.
- **Evidence:** Added migration documents and updated handoff artifacts in the same session; baseline command outputs captured in `docs/migration/PROGRESS.md`.

## D-20261017-1 — Use Sanctum token auth for initial Laravel migration slice

- **When:** 2026-10-17 EDT. Not committed.
- **Decided by:** Lead migration engineer.
- **Status:** accepted.
- **What:** Implement initial authenticated API vertical slice with Laravel Sanctum (`/api/auth/login`, `/api/identity/me`) and protect `/api/contacts` behind `auth:sanctum`.
- **Why:** The migration needed an end-to-end secured workflow early (not just anonymous health/data endpoints) to validate Docker runtime, DB access, and auth middleware behavior with realistic API usage.
- **Good, because:** Authenticated and unauthorized API paths are now explicitly exercised and verified in the new stack while retaining required MySQL Docker networking (`127.0.0.1:3307:3306`, app internal `mysql:3306`).
- **Bad, because:** This is not legacy-permission parity yet; role/permission claims and policy-level behavior from the .NET app are still pending and could diverge if not addressed next.
- **Not done:** No tenant isolation model or role/permission policy mapping has been implemented in Laravel yet.
- **Evidence:** `docs/migration/PROGRESS.md` command log showing 401 unauthorized, successful login/token issuance, successful identity read, and authenticated contact create/list.

## D-20261017-2 — Enforce permission middleware on migrated contacts/intake routes

- **When:** 2026-10-17 EDT. Not committed.
- **Decided by:** Lead migration engineer.
- **Status:** accepted.
- **What:** Added explicit permission middleware checks (`permission:*`) and applied them to migrated contacts/intake routes, with seeded users representing authorized and read-only roles.
- **Why:** Authentication-only guards were insufficient for parity direction; migrated workflows needed behavior-level authorization and test coverage for `401` and `403` paths.
- **Good, because:** Route behavior now distinguishes unauthenticated (`401`) from authenticated-but-forbidden (`403`) access and has automated tests (`IntakeApiTest`) plus runtime verification.
- **Bad, because:** Current permission source is user JSON fields and does not yet mirror full legacy policy composition or matter-scoped access rules.
- **Not done:** No complete role-policy catalog migration yet; no matter-level policy enforcement in Laravel for other modules.
- **Evidence:** `docs/migration/PROGRESS.md` and `laravel-backend/tests/Feature/IntakeApiTest.php`.

## D-20261017-3 — Hard-remove legacy .NET/React runtime paths

- **When:** 2026-10-17 EDT. Not committed.
- **Decided by:** User instruction.
- **Status:** accepted.
- **What:** Removed legacy runtime paths (`backend` contents, `frontend`, old .NET test projects, `db/design`, and root .NET solution/manifests) to keep Laravel/Vue/Docker as the only active runtime stack.
- **Why:** User explicitly requested immediate cleanup of no-longer-needed code/folders after upgrade.
- **Good, because:** Lowers maintenance overhead and ambiguity by removing obsolete runtime implementations from day-to-day working tree.
- **Bad, because:** Eliminates in-branch side-by-side reference implementation and increases reliance on git history/previous branches for forensic comparisons.
- **Not done:** Empty top-level `backend/` directory could not be removed in automation due external process lock; directory contents are already deleted.
- **Evidence:** `git status` deletion set and runtime verification in `docs/migration/PROGRESS.md`.

## D-20261017-4 — Prioritize visual parity shell for migrated Vue frontend

- **When:** 2026-10-17 EDT. Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replace the temporary plain-styled Vue migration page with a branded layout and CSS system modeled on prior React-era design language (hero treatment, warm palette, card styling, and typography), while preserving existing Laravel auth/contacts/intake behavior.
- **Why:** The founder flagged the current migrated frontend presentation as unacceptable versus pre-migration quality and requested restoration of the previous look-and-feel direction.
- **Good, because:** Demonstrations now communicate migration progress without a major perceived UX regression, improving usability and stakeholder confidence during transition.
- **Bad, because:** Styling parity at the shell level can mask remaining route/component parity gaps and may require additional refactors when full multi-page Vue architecture is introduced.
- **Not done:** Full page-by-page parity (public marketing routes and workspace route ecosystem) is still pending.
- **Evidence:** `docker compose exec -T frontend npm run build` passed; lint diagnostics clean for `vue-frontend/src/App.vue` and `vue-frontend/src/style.css`; runtime checks confirmed backend health on `http://localhost:8080/up`.

## D-20261017-5 — Restore Vue shell styling from pre-migration snapshot and repair compose corruption

- **When:** 2026-10-17 15:34 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Ported the Vue migration UI toward the pre-migration React visual shell by reworking `vue-frontend/src/App.vue` and `vue-frontend/src/style.css` using commit `a7352dc1d4e3778b9309fc5a435f81c646b3d293` as design reference, and recreated `docker-compose.yml` to remove a malformed accidental `frontend` `working_dir` line.
- **Why:** The founder reported the current migrated frontend quality as unacceptable compared with the pre-migration UI and requested immediate visual restoration while keeping Laravel/Vue/Docker runtime direction.
- **Good, because:** Demo and stakeholder-facing UI now better matches known product quality without interrupting existing migrated workflows (auth/contacts/intake).
- **Bad, because:** Visual shell parity can create false confidence if route-level parity and deeper interaction parity are not completed next; manual CSS carry-over can also diverge from future design-token centralization unless consolidated.
- **Not done:** Full multi-route Vue parity (`about`, `practice`, `attorneys`, `contact`, and signed-in route architecture) remains pending.
- **Evidence:** `npm run build` in `vue-frontend` passed; `docker compose config` passed with clean `frontend` service configuration.

## D-20261017-6 — Normalize compose command YAML to reduce false duplicate-key diagnostics

- **When:** 2026-10-17 15:56 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Rewrote `docker-compose.yml` command declarations into explicit YAML list form (`command: [sh, -lc, ...]` equivalent multi-line list style) for both backend and frontend services.
- **Why:** The editor kept reporting `DUPLICATE_KEY` around `frontend.working_dir` even though `docker compose config` successfully parsed the file; list-form commands reduce folded-scalar ambiguity in YAML language tooling.
- **Good, because:** Compose intent is clearer and less sensitive to parser edge-cases when editing long shell chains.
- **Bad, because:** IDE diagnostics can still remain stale after file corrections, which can mislead future edits unless runtime validation (`docker compose config`) is used as confirmation.
- **Not done:** No service runtime behavior change and no additional stack startup checks beyond compose model validation in this decision.
- **Evidence:** `docker compose -f docker-compose.yml config` succeeded after normalization while Cursor diagnostics still showed false-positive duplicate-key warnings.

## D-20261017-7 — Use inline command arrays and explicit volume maps in compose to clear duplicate-key lint

- **When:** 2026-10-17 16:00 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replaced multi-line list command declarations with inline command arrays for `backend` and `frontend` services, and declared top-level volumes as explicit empty maps (`{}`).
- **Why:** The frontend block continued showing duplicate-key YAML diagnostics around `working_dir`/`command`; a tighter, unambiguous structure was needed to satisfy both Compose and editor parsing.
- **Good, because:** The same file now validates in `docker compose config` and in editor diagnostics without YAML errors.
- **Bad, because:** Long shell commands inside inline arrays are less human-readable and can become unwieldy as bootstrap logic grows.
- **Not done:** No behavioral/runtime change testing beyond compose model rendering and lint diagnostics.
- **Evidence:** `docker compose -f docker-compose.yml config` succeeded and Cursor `ReadLints` returned no errors for `docker-compose.yml`.

## D-20261017-8 — Rebuild route-level Vue parity and consultation queue flow with seeded role matrix

- **When:** 2026-10-17 22:20 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replaced the single-page Vue migration shell with route-based public/workspace architecture, restored consultation booking UX via a Contact modal, added Laravel consultation queue APIs, and seeded role-based personas (Partner, Attorney, Paralegal, Accountant, LegalAssistant, AdminIT, Client) using a centralized role-to-permission matrix.
- **Why:** The founder requested top-to-bottom migration continuation: scroll-animated public pages, attorney/background pages, real sign-in flow, and consultation intake-to-queue behavior parity with the pre-migration experience.
- **Good, because:** The new stack now demonstrates an end-to-end flow from public booking to authenticated internal queue handling while preserving the Laravel/Vue/Docker runtime direction.
- **Bad, because:** Workspace parity is still partial—calendar, matters, and several signed-in modules are currently navigable placeholders and can create a false sense of full parity if not completed quickly.
- **Not done:** Full behavior-level migration for calendar/matters/modules beyond contacts/intake/consultations; no E2E browser automation tests were added in this slice.
- **Evidence:** `npm run build` passed in `vue-frontend`; `php artisan test --compact` passed (11 tests); `docker compose config` passed; lint diagnostics clean.

## D-20261017-9 — Enforce single shared legacy-parity consultation modal across public CTA flows

- **When:** 2026-10-17 23:46 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replaced route/navigation-based consultation entry with a single globally mounted modal controller and rewrote the modal to legacy-style behavior (large two-column scheduler, 42-day slot grid, month navigation, close semantics) while wiring all consultation-intake public CTAs to this same component.
- **Why:** Founder requested strict migration parity for the consultation modal experience and consistency across every relevant public CTA, with no redesign drift.
- **Good, because:** Public intake UX is now consistent and reusable across pages, reducing implementation drift and restoring expected legacy flow from multiple entry points.
- **Bad, because:** Strict parity introduces a denser component with more interaction logic, which can raise maintenance cost if later redesign efforts diverge from legacy interaction assumptions.
- **Not done:** GIF capture was not produced in this decision; screenshot evidence was captured instead.
- **Evidence:** frontend build and backend tests passed; lint diagnostics clean; Playwright screenshots captured for home/contact/other public CTA modal launches.

## D-20261018-1 — Make Laravel container startup deterministic and profile-driven

- **When:** 2026-10-18 03:45 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replaced runtime bootstrap chaining in `docker-compose.yml` with explicit orchestration primitives: a multi-stage Laravel image with built-in dependencies, a `migrate` one-shot service, a `backend-dev` service for bind-mounted development, and a separate immutable `backend` service for the `prod` profile. Added `docker/laravel/entrypoint.sh` to enforce lock-hash-based dependency sync in dev and stable startup modes (`web`, `dev-web`, `migrate`).
- **Why:** `docker compose up -d` previously failed healthchecks when package discovery/cache state drifted from the mounted `vendor` volume and running container lifecycle, causing class-not-found bootstrap failures despite healthy infra dependencies.
- **Good, because:** Startup behavior is now deterministic by design, migration responsibility is explicit, and dependency drift is proactively detected in dev before healthchecks fail.
- **Bad, because:** Profiles add operational complexity (developers must use the correct `--profile` command), and a separate migration job means teams must keep runtime/DB workflow conventions synchronized.
- **Not done:** Full `prod` profile runtime smoke start was not executed in this decision; validation focused on compose model correctness and the active `dev` profile runtime.
- **Evidence:** `docker compose config`, `docker compose --profile dev config`, and `docker compose --profile prod config` all passed; `docker compose --profile dev up -d --build` completed with `backend-dev` healthy and `migrate` exited `0`; `curl http://localhost:8080/up` returned HTTP 200.

## D-20261018-2 — Bind consultation modal host with top-level refs to avoid always-truthy nested ref conditionals

- **When:** 2026-10-18 03:58 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Refactored `ConsultationModalHost.vue` from object-member template bindings (`modal.isOpen`, `modal.closeModal`) to destructured top-level bindings (`isOpen`, `closeModal`) returned from `useConsultationModal()`.
- **Why:** The public site loaded with the consultation modal already open and non-dismissable because Vue treated the nested ref object in `v-if` as truthy, keeping the modal permanently mounted.
- **Good, because:** Modal lifecycle now follows explicit boolean state and close interactions (`X`, backdrop, cancel, escape) can unmount the component as designed.
- **Bad, because:** This bug class is subtle and can reappear if future composable hosts expose refs through nested plain objects and templates reuse member expressions in conditional rendering.
- **Not done:** No browser automation test was added in this decision; validation focused on successful frontend build and direct causal code fix.
- **Evidence:** `docker compose --profile dev exec -T frontend npm run build` passed after host binding update.

## D-20261018-3 — Document Docker profile validation and operator commands in architecture README

- **When:** 2026-10-18 04:05 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added a dedicated `Docker profile rollout verification` section to `Markdown/architecture/README.md` containing the exact validation checklist outcomes and the canonical run/stop commands for `dev` and `prod` compose profiles.
- **Why:** The founder requested an explicit, centralized architecture-level record of the validated profile workflow so operators can reliably start, verify, and stop the stack using the new profile split.
- **Good, because:** Operational onboarding is clearer, verification evidence is easy to find, and profile usage expectations are now codified in architecture docs.
- **Bad, because:** This section can drift from reality if compose commands or profile wiring change without synchronized documentation updates.
- **Not done:** No additional runtime checks were rerun in this decision; the documentation captures checks already executed and verified in the prior deterministic-bootstrap slice.
- **Evidence:** `Markdown/architecture/README.md` now includes the requested validation bullets and command list verbatim in the new section.

## D-20261018-4 — Publish seeded auth user and permission matrix in docs

- **When:** 2026-10-18 04:12 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added `docs/seeded-auth-users-and-permissions.md` to document all seeded authenticated users, role types, permission sets, and permission-gated authenticated routes.
- **Why:** Developers and QA needed a quick reference for local login personas and authorization coverage without repeatedly inspecting seeder and config files.
- **Good, because:** Auth scenario setup is faster, role expectations are explicit, and permission-route mapping is easier to verify during manual and automated testing.
- **Bad, because:** Documentation can become stale if user seeds, role permission mappings, or route middleware requirements change and this file is not updated.
- **Not done:** No runtime validation or test run was added in this decision because the slice is documentation-only and source-backed.
- **Evidence:** `docs/seeded-auth-users-and-permissions.md` was added with content sourced from `laravel-backend/database/seeders/DatabaseSeeder.php`, `laravel-backend/config/roles.php`, and `laravel-backend/routes/api.php`.

## D-20261018-5 — Implement workspace parity for calendar, matters, tasks, and editable consultation triage

- **When:** 2026-10-18 04:36 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replace authenticated workspace placeholders with functional Laravel/Vue modules for Calendar (month/week/day tabs + create/complete actions), Matters (read list + staffing/open-task counts), and Tasks (queue + status updates + create flow), while upgrading Consultation Queue records from one-click scheduling to full per-record status/notes draft-and-save editing.
- **Why:** Founder requested migration parity depth beyond contact/intake basics so signed-in legal staff can use day-to-day operational pages similar to the pre-migration behavior and Clio-style workspace expectations.
- **Good, because:** The migrated stack now supports a real authenticated workflow from public consultation submission to internal triage, scheduling, and task/matter operations without placeholder dead ends.
- **Bad, because:** Expanding permissions, schema, and route surface area in one slice increases drift risk if future pages do not follow the same permission and test discipline.
- **Not done:** Matter detail tabs (documents/timeline action editing), activity feed parity, and deeper calendar verification/acknowledgment semantics are still pending.
- **Evidence:** `php artisan test --compact` passed (18 tests, 79 assertions), `vendor/bin/pint --dirty --format agent` passed, `npm run build` in `vue-frontend` passed, and edited-file lint diagnostics were clean.

## D-20261018-6 — Canonicalize Docker/Artisan command ownership and annotate command use cases in-place

- **When:** 2026-10-18 04:44 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Declared a canonical command source-of-truth map and added use-case comments above command declarations in `docker-compose.yml` and `docker/laravel/entrypoint.sh`, plus a commented command catalog in `Markdown/architecture/README.md` for compose up/down, migrate, seed, and cache/optimize clears.
- **Why:** Founder asked where operational commands live and requested explicit comments above command lines so the team can quickly know when to use each command.
- **Good, because:** Operational commands are now discoverable in one place and command definitions are self-explaining at point-of-use.
- **Bad, because:** Comments can drift from runtime reality if compose service names, profiles, or entrypoint mode behavior change without synchronized updates.
- **Not done:** No runtime command behavior was changed in this decision; only command comments and runbook discoverability were updated.
- **Evidence:** Updated `docker-compose.yml`, `docker/laravel/entrypoint.sh`, and `Markdown/architecture/README.md` with command-use annotations and command catalog blocks.

## D-20261018-7 — Move Docker/Artisan command workflow into dedicated operations runbook

- **When:** 2026-10-18 05:00 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added `Markdown/operations/docker-and-laravel-command-runbook.md` as the canonical operator command-sequence reference and linked it from `Markdown/README.md` and `Markdown/architecture/README.md`.
- **Why:** Founder asked for command order and command-location documentation to be easy to find in a more suitable dedicated file rather than buried in general architecture narrative.
- **Good, because:** Daily Docker/Artisan workflows now have one focused runbook with exact order, profile context, and use-case comments.
- **Bad, because:** Maintaining both architecture-level references and operations runbook content introduces synchronization risk if one is updated without the other.
- **Not done:** No compose profile wiring or container command behavior was changed in this decision; this is documentation placement and discoverability only.
- **Evidence:** Added `Markdown/operations/docker-and-laravel-command-runbook.md`, updated `Markdown/README.md` operations index row, and added runbook link in `Markdown/architecture/README.md`.

## D-20261018-8 — Shift workspace calendar to week-first time-grid with slot-driven event creation

- **When:** 2026-10-18 05:22 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Reworked the Vue workspace calendar page to default to week view, display a 30-minute time-grid for week/day scheduling, keep a traversable mini month calendar pinned on the left, and open a larger event-detail modal when users double-click any grid slot.
- **Why:** Founder requested a Clio-like calendar interaction model where time slots are visually explicit, month traversal is always accessible, and event capture happens in a dedicated form surface launched directly from the grid.
- **Good, because:** Scheduling is now more precise and faster to use, with direct slot-targeted event creation and clear date navigation context.
- **Bad, because:** The denser interactive layout raises UI complexity and may need additional mobile/overflow tuning for very busy weeks.
- **Not done:** No browser automation capture or manual screenshot verification was run in this decision; validation was build/lint plus source-level behavior inspection.
- **Evidence:** `npm run build` in `vue-frontend` succeeded; `ReadLints` for edited calendar files reported no diagnostics; updated files: `vue-frontend/src/pages/workspace/WorkspaceCalendarPage.vue`, `vue-frontend/src/style.css`, `ProgressPlan.md`, `SLICE_HISTORY.md`.

## D-20261018-9 — Make Docker local-start docs command-order-safe for PowerShell onboarding

- **When:** 2026-10-18 17:18 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Updated `Markdown/operations/docker-and-laravel-command-runbook.md` and `Markdown/architecture/README.md` to add a copy/paste-safe local startup sequence, explicit command order rationale, and guardrails against pasting markdown fence markers into PowerShell.
- **Why:** Local startup troubleshooting showed repeated operator failures caused by out-of-order commands (`down` before `exec` workflow completion, `exec` against stopped containers) and accidental pasting of markdown fences (```` ``` ```` / ```` ```bash ````) into terminal sessions.
- **Good, because:** The runbook now explains both the exact sequence and the dependency logic behind each step, reducing avoidable startup/support loops for developers onboarding to profile-based compose workflows.
- **Bad, because:** Documentation clarity alone does not enforce runtime correctness; if compose service/profile names change and docs are not updated at the same time, these explicit instructions can become confidently wrong.
- **Not done:** No compose profile wiring, entrypoint logic, or application runtime behavior was changed in this decision; this is documentation-only hardening.
- **Evidence:** Updated files: `Markdown/operations/docker-and-laravel-command-runbook.md`, `Markdown/architecture/README.md`, `ProgressPlan.md`, `SLICE_HISTORY.md`.

## D-20261019-1 — Add explicit begin/finish runbook sections with optional volume teardown

- **When:** 2026-10-19 03:55 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Updated `Markdown/operations/docker-and-laravel-command-runbook.md` to add a clearly labeled `Start here (begin local dev)` section and a clearly labeled `When finished (clean shutdown)` section with both standard shutdown and optional full cleanup (`down --remove-orphans --volumes`), plus explicit warning that volume teardown resets project-local DB data.
- **Why:** Repeated local-env support questions showed ambiguity around where startup begins, how to finish cleanly, and how to guarantee no compose-managed containers/processes remain after development.
- **Good, because:** Developers now have one obvious entry point and one obvious exit point in the runbook, reducing forgotten running containers and reducing environment drift across sessions.
- **Bad, because:** Adding stronger cleanup commands increases accidental data-reset risk if `--volumes` is run without intent or context.
- **Not done:** No compose YAML/service behavior changed; this is documentation and operator guidance only.
- **Evidence:** Updated files: `Markdown/operations/docker-and-laravel-command-runbook.md`, `ProgressPlan.md`, `SLICE_HISTORY.md`.

## D-20261019-2 — Enable Tailwind in Vue frontend and enforce polling-based HMR for Docker dev

- **When:** 2026-10-19 03:59 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added Tailwind CSS support to the Vue frontend (`tailwindcss`, `@tailwindcss/vite`, stylesheet import) and hardened Vite dev-server watch/HMR settings for Docker-on-Windows by enabling polling and explicit host/port config, with matching frontend container polling environment variables.
- **Why:** Founder requested save-triggered frontend style hot loading and Tailwind availability in the migrated Vue stack; prior local flow had intermittent file-watch/update issues common to bind-mounted Docker development on Windows.
- **Good, because:** CSS/UI iteration is now faster and more reliable in local Docker sessions, and Tailwind utilities are available for incremental component-level styling.
- **Bad, because:** Running Tailwind alongside the existing large custom stylesheet can temporarily increase styling overlap and maintenance complexity until targeted cleanup is done.
- **Not done:** No broad style refactor to Tailwind utilities was performed in this decision; this slice only enables the toolchain and reliability settings.
- **Evidence:** Updated files: `vue-frontend/vite.config.ts`, `vue-frontend/src/style.css`, `docker-compose.yml`, `vue-frontend/package.json`, `vue-frontend/package-lock.json`, `ProgressPlan.md`, `SLICE_HISTORY.md`.

## D-20261019-3 — Route workspace login through public shell for unauth navigation parity

- **When:** 2026-10-19 04:59 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Moved `workspace-login` route under `PublicShell` children (path remains `/workspace/login`) so `SiteNav` and `SiteFooter` render on the sign-in page; adjusted `WorkspaceLoginPage.vue` markup to remove nested `<main>` wrapper under the shell.
- **Why:** Founder requested the workspace sign-in page to show the same navigation experience as other non-authenticated/public routes.
- **Good, because:** Unauthenticated UX is now consistent across public and sign-in routes, and users can navigate away from sign-in using the same top-nav patterns.
- **Bad, because:** Future changes to public shell layout/styles will also affect workspace sign-in, which may require route-specific spacing or visual overrides.
- **Not done:** No new route-level visual redesign was done for sign-in; this decision changes shell composition/routing only.
- **Evidence:** Updated files: `vue-frontend/src/router/index.ts`, `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue`, `ProgressPlan.md`, `SLICE_HISTORY.md`; `npm run build` passed and lint diagnostics were clean.

## D-20261019-4 — Apply sticky footer shell behavior to public and authenticated layouts

- **When:** 2026-10-19 05:06 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added shell-level layout rules so footer is pinned to viewport bottom on short pages in both app contexts: public shell now uses flex-column with expandable main, and workspace shell now includes shared `SiteFooter` with explicit grid footer row.
- **Why:** Founder identified a UX issue where footer appeared directly after short sign-in content instead of at page bottom, and requested this behavior consistently across authenticated and non-authenticated pages.
- **Good, because:** Page framing is now consistent and visually stable on sparse routes, matching common UI expectations for both login and workspace pages.
- **Bad, because:** Adding a global footer to workspace shell can introduce extra vertical density on some data-heavy pages and may require future spacing refinements.
- **Not done:** No page-specific footer redesign or responsive spacing tuning was done in this decision.
- **Evidence:** Updated files: `vue-frontend/src/style.css`, `vue-frontend/src/layout/WorkspaceShell.vue`, `ProgressPlan.md`, `SLICE_HISTORY.md`; `npm run build` passed and lint diagnostics were clean.

## D-20261019-5 — Enrich attorney profile UX with animated transitions and home partner deep links

- **When:** 2026-10-19 05:13 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added profile-route transition animations in the public shell, enhanced attorney detail page content structure (highlight chips + expectation panel + action grouping), and converted Home partner spotlight cards to deep links into `/attorneys/:slug` detail pages.
- **Why:** Founder requested a more exciting, engaging profile experience and consistent navigation paths from both Attorneys and Home partner cards into detailed team-member pages.
- **Good, because:** Team-profile navigation now feels more intentional and premium, with better page energy and stronger conversion paths to consultation actions.
- **Bad, because:** Additional transition and styling layers increase CSS/UI complexity and may need accessibility tuning for reduced-motion users.
- **Not done:** Reduced-motion preference overrides were not implemented in this slice; non-attorney operations-team entries on Attorneys page remain list items rather than full profile cards.
- **Evidence:** Updated files: `vue-frontend/src/pages/public/HomePage.vue`, `vue-frontend/src/pages/public/AttorneyProfilePage.vue`, `vue-frontend/src/layout/PublicShell.vue`, `vue-frontend/src/style.css`, `ProgressPlan.md`, `SLICE_HISTORY.md`; `npm run build` passed and lint diagnostics were clean.

## D-20261019-6 — Roll back route-wrapper transitions to fix blank public pages and keep profile-local animation

- **When:** 2026-10-19 05:22 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Removed route-wrapper transition composition from `PublicShell` and replaced it with a localized attorney-profile card entrance animation to preserve visual liveliness without destabilizing public route rendering.
- **Why:** Founder reported blank content across all top-nav public routes after transition integration; the issue pointed to interaction risk between shell-level route transitions and reveal lifecycle behavior.
- **Good, because:** Restores reliable rendering across public pages while keeping the requested animated experience on attorney detail pages.
- **Bad, because:** Cross-route transition effects are deferred and may require a more nuanced implementation tied to reveal observer timing.
- **Not done:** No reimplementation of global route transitions was done in this corrective slice.
- **Evidence:** Updated files: `vue-frontend/src/layout/PublicShell.vue`, `vue-frontend/src/pages/public/AttorneyProfilePage.vue`, `vue-frontend/src/style.css`, `ProgressPlan.md`, `SLICE_HISTORY.md`; `npm run build` passed and lint diagnostics were clean.

## D-20261019-7 — Upgrade authenticated consultations queue into modern triage workspace

- **When:** 2026-10-19 05:31 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Redesigned `/workspace/consultations` with animated status summary tiles, richer per-request cards, status-pill signaling, client-side search, and improved notes editing ergonomics while preserving existing permission-gated status update flow.
- **Why:** Founder asked where unauthenticated contact appointment requests are surfaced for legal assistants/paralegals and requested a visually modern, easy-to-manage queue for qualifying potential matters.
- **Good, because:** Intake staff can now scan and act on incoming requests faster with clearer visual hierarchy, quicker filtering, and more actionable card-level context.
- **Bad, because:** Expanded styling and page-level logic increase maintenance complexity until this queue is split into smaller reusable components.
- **Not done:** No backend schema/field expansion (e.g., follow-up deadlines, explicit urgency flags, assignment filters) was introduced in this decision.
- **Evidence:** Updated files: `vue-frontend/src/pages/workspace/ConsultationsQueuePage.vue`, `vue-frontend/src/style.css`, `ProgressPlan.md`, `SLICE_HISTORY.md`; `npm run build` passed and lint diagnostics were clean.

## D-20261019-8 — Fix authenticated consultations queue invisibility by initializing reveal observer in workspace shell

- **When:** 2026-10-19 05:37 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added `useScrollReveal(() => route.fullPath)` to `WorkspaceShell.vue` so `[data-reveal]` sections in authenticated routes transition into visible state.
- **Why:** Founder reported that newly submitted public consultation requests did not appear in workspace queue; backend data check confirmed records existed, isolating issue to front-end reveal lifecycle.
- **Good, because:** Restores queue visibility immediately and prevents similar hidden-content issues for future reveal-animated workspace sections.
- **Bad, because:** Shell-level reveal observation broadens animation scope, which could require threshold/performance tuning on very dense authenticated pages.
- **Not done:** No backend/API behavior changes were made; this decision addresses front-end rendering visibility only.
- **Evidence:** DB query confirmed fresh rows in `consultation_requests`; updated `vue-frontend/src/layout/WorkspaceShell.vue`; `npm run build` passed and lint diagnostics were clean.

## D-20261019-9 — Remove reveal-animation gating from critical consultations queue containers

- **When:** 2026-10-19 05:40 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Removed `data-reveal` attributes from consultations queue stats/toolbar/list containers so queue content never depends on reveal observer timing to become visible.
- **Why:** Founder screenshot showed non-zero queue counts with no visible records, indicating hidden content rather than missing data.
- **Good, because:** Operational triage data is now visible by default and cannot appear blank due to animation lifecycle timing.
- **Bad, because:** This reduces motion effects on that page, so visual polish is slightly lower than fully animated entry states.
- **Not done:** No API/backend changes were needed; issue was presentation-layer visibility only.
- **Evidence:** Updated `vue-frontend/src/pages/workspace/ConsultationsQueuePage.vue`; `npm run build` passed and lint diagnostics were clean.

## D-20261019-10 — Database per firm on the Laravel stack, existing modules left in place

- **When:** 2026-10-19 15:28 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder, through the multi-tenant foundation request.
- **Status:** accepted for this branch.
- **What:** Add a control-plane firm registry, human review states, explicit schema provisioning, per-request tenant connections, and public branding. Keep contacts, matters, consultations, tasks, and calendar on the current database until a later move.
- **Why:** ADR-002 already chose a database per firm, and the running code is Laravel rather than the ASP.NET design. A shared `tenant_id` column would not meet that decision. Moving every existing table in the same branch would rewrite working workspace flows.
- **Good, because:** New tenant routes fail closed, two schemas cannot read each other's notes, and a failed provision does not mark the firm active.
- **Bad, because:** The existing workspace APIs are still one shared database. A signed-in user with the legacy permission can read that data without a firm schema. The runtime MySQL account is also granted DML on every provisioned schema, so a wrong connection string is not stopped by a per-firm account.
- **Not done:** Per-firm database users, placement cells, MFA, login throttling, tenant deletion, and moving legal records into the firm schema.
- **Evidence:** [firm tenant schema foundation](../architecture/firm-tenant-schema-foundation.md). `php artisan test --compact` passed (30 tests, 203 assertions), including `TenantMysqlProvisioningTest` against MySQL 8.0.36 on port 3307.

## D-20261019-12 — Choose SaaS-first outer layer and document implementation options

- **When:** 2026-10-19 18:39 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted recommendation for planning.
- **What:** Documented a SaaS-first outer-layer direction where platform root host handles law-firm onboarding while verified tenant domains/subdomains handle firm public sites and firm workspace entry.
- **Why:** Root-host firm-first behavior makes multi-firm SaaS onboarding and brand separation confusing as tenant count grows.
- **Good, because:** Clarifies product boundary (`platform` vs `tenant`), improves customer-acquisition flow, and aligns host-routing/security expectations with verified-domain tenancy.
- **Bad, because:** Requires IA/UX split and additional surface-area coordination (platform shell, tenant shell, support routing) before full rollout.
- **Not done:** No full implementation flip of `localhost:5173`/production root behavior in this planning-only decision.
- **Evidence:** Planning artifact was superseded by `doc/saas-first-root-and-tenant-hosts-implementation-prompt.md`, which captures the execution-ready specification.

## D-20261019-11 — Enforce DNS TXT domain verification and tenant-route data isolation

- **When:** 2026-10-19 20:35 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added domain-claim lifecycle with DNS TXT challenge issuance/reissue/verification/revocation, switched workspace APIs to `/api/tenant/*` middleware-backed routes, moved core workspace tables into tenant migrations, and introduced control-plane storage for per-firm runtime DB credentials with local fallback behavior.
- **Why:** The previous slice still trusted manually marked domains, left confidential workspace records in shared tables, and could not express production-ready host verification and runtime-credential boundaries.
- **Good, because:** Host-based routing now depends on verified active domains, public intake fails closed on unknown hosts, and workspace writes/reads for contacts/intake/consultations/matters/tasks/calendar now run in firm schemas by default.
- **Bad, because:** Local fallback to a shared runtime DB account can still be enabled when user provisioning privileges are missing, so DB-level isolation is not fully guaranteed until fallback is disabled in production and per-firm DB users are enforced.
- **Not done:** Full decommission of legacy shared workspace rows and their tables; polished platform-admin UX; DNS/TLS provider automation.
- **Evidence:** `tests/Feature/FirmTenancyTest.php`, `tests/Feature/TenantMysqlProvisioningTest.php`, and `../docs/firm-onboarding-domain-routing-and-tenant-workspaces.md`.

## D-20261020-1 — Replace brainstorming doc with execution prompt for Option B rollout

- **When:** 2026-10-20 11:28 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Replaced the brainstorming/options document with a detailed Cursor prompt that is ready to execute Option B implementation work (SaaS root, app shell, tenant hosts, fallback URL, rollout slices, testing requirements, and completion-report contract).
- **Why:** The founder approved Option B and asked for a detailed prompt artifact that can be fed directly into Cursor to start implementation.
- **Good, because:** Reduces ambiguity during execution and gives the implementation run a stable contract for host responsibilities, lifecycle gates, safety checks, and test evidence.
- **Bad, because:** Prompt fidelity can drift from repository reality over time if docs/code change and the prompt is reused without a fresh audit pass.
- **Not done:** No runtime code changes, no route/controller changes, and no test runs were performed in this packaging decision.
- **Evidence:** `doc/saas-first-root-and-tenant-hosts-implementation-prompt.md`.

## D-20261020-2 — Publish strict v2 execution prompt with hard phase gates

- **When:** 2026-10-20 11:39 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added a second, stricter prompt variant that keeps the same Option B target architecture but enforces copy/paste-ready execution controls: explicit preflight checks, phase checklists, mandatory stop-if-fail gates, prohibited actions, and a required final report template.
- **Why:** The founder requested a stricter implementation prompt to reduce execution ambiguity and force auditable checkpoints during tenancy/domain-routing rollout.
- **Good, because:** Improves execution discipline and makes implementation status easier to validate objectively at each phase.
- **Bad, because:** Heavier gates can slow iteration when rapid prototyping would otherwise be acceptable in early exploratory work.
- **Not done:** No runtime code changes or test runs were performed; this is prompt packaging only.
- **Evidence:** `doc/saas-first-root-and-tenant-hosts-implementation-prompt-strict-v2.md`.

## D-20261020-3 — Execute strict v2 host-surface rollout and discovery shell behavior

- **When:** 2026-10-20 11:49 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Implemented SaaS-first platform host behavior (`MatterForge` platform messaging), app-host multi-membership firm selector flow, local-only explicit firm header selection guardrails, and automatic platform-managed fallback tenant host creation during firm registration.
- **Why:** The strict v2 prompt required moving from planning artifacts to enforceable runtime behavior: platform root/app surfaces must be distinct from tenant host surfaces while preserving secure host-verified tenancy boundaries.
- **Good, because:** Clarifies acquisition vs tenant-entry surfaces, improves multi-firm account usability, and preserves operational resilience through canonical fallback tenant URLs.
- **Bad, because:** Local/dev host simulation and multi-host verification now depend on explicit hosts-file setup and environment variable alignment; if misconfigured, routing behavior can appear inconsistent.
- **Not done:** No browser automation/manual browser proof was captured in this slice; DNS/TLS production go-live remains an operations workflow.
- **Evidence:** `laravel-backend/config/tenancy.php`, `laravel-backend/app/Tenancy/FirmRegistry.php`, `vue-frontend/src/features/host/surface.ts`, `vue-frontend/src/pages/workspace/FirmSelectorPage.vue`, and check results in `SLICE_HISTORY.md` entry `2026-10-20 - Strict v2 Option B host-surface and discovery implementation`.

## D-20261022-1 — Keep tenant hosts public-first for guests and harden workspace login redirects

- **When:** 2026-10-22 14:45 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Updated frontend route-guard and sign-in behavior so unauthenticated requests to protected workspace routes on tenant hosts are redirected to tenant public home, and workspace login redirect values are constrained to safe internal `/workspace*` paths. Also set browser title from resolved public branding so tenant surfaces show firm identity (`{Firm Name} | MatterForge`).
- **Why:** Founder requested firm-subdomain behavior where non-authenticated visitors see the firm's prospective-customer page and asked for implementation aligned with best-practice security posture.
- **Good, because:** Preserves a clear tenant public surface for guest traffic and reduces open-redirect risk in post-login navigation.
- **Bad, because:** Guests following protected workspace deep links on tenant hosts now need one extra click to reach sign-in if they were expecting immediate login.
- **Not done:** No backend tenancy middleware or API authorization contracts changed; no manual multi-host browser validation was run in this slice.
- **Evidence:** `vue-frontend/src/router/index.ts`, `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue`, `vue-frontend/src/features/branding/api.ts`; `npm run build` passed; `ReadLints` on edited files reported no diagnostics.

## D-20261022-2 — Add self-service onboarding with two-reviewer approvals and local-only fast-track gates

- **When:** 2026-10-22 15:35 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added applicant-facing firm onboarding APIs/UI (`/register-firm`, `/onboarding/status`), revision-bound onboarding verification/review tables, two-distinct-reviewer approval enforcement for onboarding applications, and a server-side local/testing-only fast-track path (`FIRM_ONBOARDING_DEV_FAST_TRACK`) that reuses existing provisioning services without fabricating reviewer approvals.
- **Why:** Founder requested rapid local register -> provision -> dashboard onboarding while preserving normal production-grade verification, two-person approval, and tenant-isolation controls.
- **Good, because:** Developers can fast-track test firms locally under explicit guarded config, while normal onboarding remains reviewable, auditable, and fail-closed when verification/provider prerequisites are missing.
- **Bad, because:** The control-plane workflow and state model become more complex (revisioned applications, verifications, review quorum), which increases operational/training burden for platform reviewers.
- **Not done:** No external verification-provider integration was implemented; provider mode intentionally reports unavailable when unconfigured. No browser automation capture was run for the new onboarding pages.
- **Evidence:** `laravel-backend/database/migrations/2026_10_22_185837_create_firm_onboarding_tables.php`, `app/Tenancy/FirmOnboardingService.php`, `app/Tenancy/FirmReviewService.php`, `app/Tenancy/OnboardingFastTrackGuard.php`, `app/Http/Controllers/Api/FirmOnboardingController.php`, `tests/Feature/FirmOnboardingApiTest.php`, `vue-frontend/src/pages/public/RegisterFirmPage.vue`, `vue-frontend/src/pages/public/OnboardingStatusPage.vue`; tests/build checks recorded in `SLICE_HISTORY.md`.

## D-20261023-1 — Preserve local frontend port when redirecting from platform host to tenant host

- **When:** 2026-10-23 10:57 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Updated platform-host login and firm-selection redirect builders to append the current browser port (`window.location.port`) when redirecting to tenant hosts.
- **Why:** Local sign-in was redirecting to `http://<tenant-host>/workspace` (port 80) instead of `http://<tenant-host>:5173/workspace`, producing browser `ERR_CONNECTION_REFUSED`.
- **Good, because:** Restores reliable local multi-host flow without requiring a reverse proxy on port 80.
- **Bad, because:** Redirect logic now depends on browser port state; if future environments require protocol/port transformations, this helper may need environment-specific mapping.
- **Not done:** No backend tenancy logic changed and no browser automation capture was run in this fix slice.
- **Evidence:** `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue`, `vue-frontend/src/pages/workspace/FirmSelectorPage.vue`; `npm run build` passed; `ReadLints` on edited files reported no diagnostics.

## D-20261023-2 — Keep platform-admin sign-in on SaaS admin surface

- **When:** 2026-10-23 11:04 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Updated platform-host auth routing so platform admins land on `/workspace/platform/firms` instead of being redirected to tenant-host member flows; updated workspace shell to present platform-admin navigation options rather than law-firm practice operations on platform-admin surface.
- **Why:** Founder reported that signing in at root as SaaS admin incorrectly landed on a tenant customer/public page, which conflicts with the SaaS control-plane UX goal.
- **Good, because:** Platform admins now stay in the SaaS-management context and immediately see client-firm management options.
- **Bad, because:** Platform-admin navigation now diverges from firm-workspace navigation, so route-level documentation and operator onboarding must keep both surfaces clear.
- **Not done:** No browser automation capture was run in this fix slice.
- **Evidence:** `vue-frontend/src/router/index.ts`, `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue`, `vue-frontend/src/layout/WorkspaceShell.vue`; `npm run build` passed; `ReadLints` on edited files reported no diagnostics.

## D-20261023-3 — Remove cross-host login redirects on platform host to preserve auth continuity

- **When:** 2026-10-23 11:15 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Removed tenant-host redirects after platform-host login and firm selection so auth/session remain on the same origin; single-firm and multi-firm selections now set firm context and continue on platform-host workspace routes.
- **Why:** Founder reproduced that sign-in still landed on firm public pages. Root cause was cross-origin redirect with bearer token stored in `localStorage`, which is origin-scoped and therefore unavailable on the redirected tenant host.
- **Good, because:** Prevents silent auth loss and stabilizes platform-host sign-in behavior for both SaaS admin and firm-member accounts during local development.
- **Bad, because:** This local-first routing model does not exercise full cross-host authenticated navigation; production SSO/cookie-domain strategy still needs explicit design if cross-host auth handoff is required.
- **Not done:** No browser automation capture was run in this fix slice.
- **Evidence:** `vue-frontend/src/pages/workspace/WorkspaceLoginPage.vue`, `vue-frontend/src/pages/workspace/FirmSelectorPage.vue`; `npm run build` passed; `ReadLints` on edited files reported no diagnostics.

## D-20261023-4 — Split platform administration into PlatformOwner and PlatformOps with action-level enforcement

- **When:** 2026-10-23 13:01 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added explicit platform roles (`platform_owner`, `platform_ops`) and an action-level permission map, persisted role state on `users.platform_role`, enforced route-level permission middleware on every `api/platform` endpoint, and aligned `FirmPolicy`/controller authorization methods to permissioned actions instead of broad boolean admin checks.
- **Why:** The prior single `is_platform_administrator` gate gave every platform admin full control over review/provision/lifecycle/domain-finalization actions, which exceeded least-privilege requirements and increased operator-risk blast radius.
- **Good, because:** PlatformOps can now perform day-to-day onboarding operations (read/create firms, manage memberships/domains, branding, verification records) without receiving owner-only authority (review finalization, provisioning, lifecycle transitions, domain finalize/revoke), and enforcement is server-side before controller logic runs.
- **Bad, because:** Role/permission matrix management is now a first-class control-plane concern; if permissions drift from policy/route wiring, users can experience confusing 403s until docs and tests are kept synchronized.
- **Not done:** MFA/step-up auth and out-of-band approval workflows are not yet implemented; this decision narrows authorization scope but does not replace stronger auth factors.
- **Evidence:** `laravel-backend/config/platform.php`, `laravel-backend/app/Http/Middleware/RequirePlatformPermission.php`, `laravel-backend/routes/api.php`, `laravel-backend/app/Policies/FirmPolicy.php`, `laravel-backend/tests/Feature/FirmTenancyTest.php`; test/build checks in `SLICE_HISTORY.md`.

## D-20261023-5 — Let firm partners manage firm users in-tenant with role-based permissions

- **When:** 2026-10-23 15:36 EDT (UTC−04:00). Not committed.
- **Decided by:** Founder.
- **Status:** accepted.
- **What:** Added partner-only tenant user-management APIs and a workspace settings page so firm partners can create users and assign role-based access (`Partner`, `Attorney`, `Paralegal`, etc.) inside their own firm context; also expanded seed data with a firm-branded partner login (`lawfirmbrianandsandra@matterforge.com`).
- **Why:** Founder requested that SaaS-client law firms onboard their own staff directly, with explicit page/input access control and without requiring platform-admin intervention for day-to-day team setup.
- **Good, because:** Day-to-day identity onboarding can now happen inside the firm workspace, and enforcement is fail-closed at API level using tenant context + partner membership checks to prevent cross-firm user management.
- **Bad, because:** Role permissions are still user-level today (not per-membership override), so multi-firm users with different intended firm roles need a future membership-scoped permission model to avoid over-broad effective permissions.
- **Not done:** Full sidebar-level dynamic page hiding across all workspace routes remains follow-up; current security enforcement is API-first with settings-page role matrix visibility.
- **Evidence:** `laravel-backend/app/Http/Controllers/Api/TenantUserController.php`, `laravel-backend/routes/api.php`, `laravel-backend/config/roles.php`, `vue-frontend/src/pages/workspace/WorkspaceUsersPage.vue`, `laravel-backend/tests/Feature/FirmTenancyTest.php`, and runbook/auth docs updates.
