# ProgressPlan: Clio-Parity Buildout

## Purpose

This document is the working, detailed parity plan for building this law-firm platform to the point where the firm can operate without subscribing to Clio.

Goals:

1. Track what Clio offers at a capability level.
2. Track what this repository currently has.
3. Quantify each module as:
   - `Implemented`
   - `Partial`
   - `Not Implemented`
4. Define exactly what it takes (product + technical work) to close gaps.
5. Keep this file current during development.

---

## Benchmark references

Primary references used for parity scope (public docs/pages):

- [Clio Manage](https://www.clio.com/manage/)
- [Clio Features](https://www.clio.com/features/)
- [Clio Pricing / plans](https://www.clio.com/pricing/)
- [Clio Case Management](https://www.clio.com/features/case-management/)
- [Clio Online Intake Forms](https://www.clio.com/features/online-intake-forms/)
- [Clio Legal CRM](https://www.clio.com/features/legal-crm-software/)
- [Clio Payments](https://www.clio.com/features/payments/)
- [Clio Integrations](https://www.clio.com/features/integrations/)
- [Clio + QuickBooks integration](https://www.clio.com/app-directory/quickbooks-online/)
- [Clio + Microsoft 365 integration](https://www.clio.com/app-directory/microsoft-office-365/)
- [Clio + Google Drive integration](https://www.clio.com/app-directory/google-drive/)
- [Clio + DocuSign integration](https://www.clio.com/app-directory/sign-with-docusign/)

---

## How scoring works

- `Implemented`: production-relevant workflow exists in both backend and frontend for day-to-day use.
- `Partial`: foundational objects/endpoints/UI exist but significant workflow depth, controls, or integrations are missing.
- `Not Implemented`: no meaningful workflow exists yet.

Capability score bands:

- `100%`: parity-ready for that feature set.
- `40-80%`: partial with viable foundation.
- `0-30%`: mostly missing.

These scores are directional, not legal/compliance guarantees.

---

## Current architecture inventory (repo evidence)

### Backend modules present

- Auth and identity:
  - `backend/Features/Auth/AuthEndpoints.cs`
  - `backend/Features/Identity/IdentityEndpoints.cs`
  - `backend/Infrastructure/Auth/*`
- Parties/contacts:
  - `backend/Features/Parties/PartiesEndpoints.cs`
- Conflicts:
  - `backend/Features/Conflicts/ConflictEndpoints.cs`
- Calendar/tasks/deadlines:
  - `backend/Features/Calendar/CalendarEndpoints.cs`
- Intake:
  - `backend/Features/Intake/IntakeEndpoints.cs`
- Matters:
  - `backend/Features/Matters/MattersEndpoints.cs`
- Consultations:
  - `backend/Features/Consultations/ConsultationsEndpoints.cs`
- Persistence model:
  - `backend/Infrastructure/Persistence/LawFirmDbContext.cs`
- Audit trail:
  - `backend/Infrastructure/Audit/AuditTrailService.cs`

### Frontend modules present

- Public site and contact funnel:
  - `frontend/src/pages/HomePage.tsx`
  - `frontend/src/pages/AboutPage.tsx`
  - `frontend/src/pages/PracticeAreasPage.tsx`
  - `frontend/src/pages/AttorneysPage.tsx`
  - `frontend/src/pages/ContactPage.tsx`
- Authenticated workspace:
  - `frontend/src/pages/WorkspaceHomePage.tsx`
  - `frontend/src/pages/WorkspaceWorklistsPage.tsx`
  - `frontend/src/pages/TasksCalendarPage.tsx`
  - `frontend/src/pages/PartiesPage.tsx`
  - `frontend/src/pages/ConflictChecksPage.tsx`
  - `frontend/src/pages/IntakeBoardPage.tsx`
  - `frontend/src/pages/IntakeDetailPage.tsx`
  - `frontend/src/pages/MattersPage.tsx`
  - `frontend/src/pages/MatterWorkspacePage.tsx`
  - `frontend/src/pages/MatterWorkspaceTabs.tsx`
  - `frontend/src/pages/WorkspaceConsultationsPage.tsx`

---

## Clio parity matrix

## 1) Marketing website and lead capture

### What Clio-aligned firms usually need

- Modern public site, attorney bios, practice pages, contact CTA, consultation booking.
- Intake path from website into CRM/case lifecycle.

### Current status

- **Status:** `Implemented` for baseline website, `Partial` for advanced lead tooling.
- **Score:** `70%`

### Evidence

- Multi-page public site with modern UI:
  - `frontend/src/pages/HomePage.tsx`, `AboutPage.tsx`, `PracticeAreasPage.tsx`, `AttorneysPage.tsx`
- Contact + appointment request modal with available slots:
  - `frontend/src/pages/ContactPage.tsx`
  - `backend/Features/Consultations/ConsultationsEndpoints.cs`

### Missing for parity

- Campaign attribution and conversion tracking.
- SEO technical layer (structured data, metadata strategy, sitemap automation).
- Multi-step lead qualification and dynamic form logic.

---

## 2) Legal CRM / intake pipeline (Clio Grow-type)

### Expected Clio-class capability

- Online intake forms (public link/embed), custom fields, conditional questions.
- Pipeline stages with automation and follow-up reminders.
- Appointment scheduling tied to intake status.
- Lead-to-matter conversion with no duplicate entry.

### Current status

- **Status:** `Partial`
- **Score:** `55%`

### Evidence

- Intake records with stages and transition rules:
  - `backend/Features/Intake/IntakeEndpoints.cs`
- Stage history + notes:
  - `LawFirmDbContext` entities `IntakeStageHistory`, `IntakeNote`
- Kanban board and detail workflow:
  - `frontend/src/pages/IntakeBoardPage.tsx`
  - `frontend/src/pages/IntakeDetailPage.tsx`
- Convert accepted intake to matter:
  - `POST /intake/{id}/convert-to-matter`

### Missing for parity

- Public configurable intake form builder.
- Conditional logic, multilingual intake templates.
- Automated workflow engine (emails/reminders/stage triggers).
- Lead scoring and conversion analytics.
- Duplicate detection/merge tools for prospects.

### What it takes to close

1. Create `IntakeFormTemplate`, `IntakeSubmission`, `IntakeFieldDefinition` schema.
2. Build public form renderer + secure form submission endpoint.
3. Add rule engine for triggers:
   - "form submitted -> create intake -> assign owner -> send confirmation -> schedule reminder."
4. Add communications template system for intake emails/SMS.

---

## 3) Contacts and relationship graph

### Expected capability

- Person/org records, aliases, relationship mapping, restricted records.

### Current status

- **Status:** `Partial to strong`
- **Score:** `75%`

### Evidence

- Party creation/search/aliases/relationships:
  - `backend/Features/Parties/PartiesEndpoints.cs`
- UI create/search:
  - `frontend/src/pages/PartiesPage.tsx`

### Missing for parity

- Rich contact timelines and communication history.
- Conflict-safe redaction controls beyond current role checks.
- Bulk import/export and dedupe workflows.

---

## 4) Conflict checking and review

### Expected capability

- Search across parties/matters, snapshot evidence, legal review decision logs.

### Current status

- **Status:** `Partial to strong`
- **Score:** `80%`

### Evidence

- Conflict checks + stored snapshots + attorney decision + rationale:
  - `backend/Features/Conflicts/ConflictEndpoints.cs`
- Restricted match masking for unauthorized roles.
- UI workflow:
  - `frontend/src/pages/ConflictChecksPage.tsx`

### Missing for parity

- Cross-matter entity graph conflict depth (beyond current direct matching).
- Waiver templates and approval chains.
- Conflict packet generation and audit export.

---

## 5) Matter management and lifecycle

### Expected capability

- Matter shell, responsible team, stage/status, task/doc/timeline linkage.

### Current status

- **Status:** `Partial to strong`
- **Score:** `78%`

### Evidence

- Matter list/detail/history/tasks/doc metadata/timeline:
  - `backend/Features/Matters/MattersEndpoints.cs`
  - `frontend/src/pages/MattersPage.tsx`
  - `frontend/src/pages/MatterWorkspacePage.tsx`
  - `frontend/src/pages/MatterWorkspaceTabs.tsx`

### Missing for parity

- Matter budget vs actuals.
- Linked communications stream by matter.
- Rule-driven matter templates (auto-task packs by matter type).
- Deeper permissions at matter-level assignment boundaries.

---

## 6) Calendar, deadlines, and tasking

### Expected capability

- Legal calendaring, deadlines, ownership, reminders, completion/acknowledgment.

### Current status

- **Status:** `Partial to strong`
- **Score:** `82%`

### Evidence

- Calendar CRUD with reminders, owner/backup, ack/complete:
  - `backend/Features/Calendar/CalendarEndpoints.cs`
- Month/week/day UI with creation/edit/delete:
  - `frontend/src/pages/TasksCalendarPage.tsx`
- Workspace worklists tied to task state:
  - `frontend/src/pages/WorkspaceWorklistsPage.tsx`

### Missing for parity

- Court rule/deadline automation per jurisdiction.
- Two-way external calendar sync (Outlook/Google).
- Recurrence engine and dependency chains.

---

## 7) Consultations queue and intake operations

### Expected capability

- Public appointment booking + internal triage queue.

### Current status

- **Status:** `Implemented baseline`
- **Score:** `72%`

### Evidence

- Available slot generation and booking conflict checks:
  - `backend/Features/Consultations/ConsultationsEndpoints.cs`
- Public contact modal scheduler:
  - `frontend/src/pages/ContactPage.tsx`
- Internal queue with status/assignment/notes:
  - `frontend/src/pages/WorkspaceConsultationsPage.tsx`

### Missing for parity

- Appointment type definitions and service durations.
- Staff calendar conflict negotiation and reschedule workflows.
- Automated reminders/confirmations/no-show handling.

---

## 8) Document management and e-sign

### Expected capability

- Matter-linked document storage, versioning, review, generation from templates, e-sign.

### Current status

- **Status:** `Partial (metadata-heavy, file-light)`
- **Score:** `38%`

### Evidence

- Matter document metadata CRUD + review status:
  - `backend/Features/Matters/MattersEndpoints.cs`
  - `MatterDocuments` tab in `frontend/src/pages/MatterWorkspaceTabs.tsx`

### Missing for parity

- Actual binary upload/download pipeline and secure storage.
- Versioning and compare.
- Template-based document generation.
- Native e-sign flow and signer workflow.
- Document retention/hold policy controls.

### What it takes to close

1. Add document file pipeline:
   - object storage adapter (S3/Azure Blob/etc),
   - signed URL upload/download,
   - checksum and antivirus hooks.
2. Add `DocumentVersion` model.
3. Add template merge service (matter/contact fields -> document output).
4. Integrate e-sign provider API (DocuSign/Dropbox Sign equivalent).

---

## 9) Time tracking

### Expected capability

- Start/stop timers, manual time entries, billable flags, activity codes.

### Current status

- **Status:** `Not Implemented`
- **Score:** `0%`

### Evidence

- No time-entry entities/endpoints/UI in current code.

### What it takes to close

1. Add `TimeEntry` model linked to matter/contact/user.
2. Add timer state handling (active timer constraints per user).
3. Add approvals/edit locks by billing period.
4. Expose in matter UI and billing pipelines.

---

## 10) Billing, invoicing, accounting, trust

### Expected capability

- Bills, ledger accounts, trust vs operating separation, reconciliation, collections, financial reporting.

### Current status

- **Status:** `Not Implemented` (outside auth demo placeholders)
- **Score:** `5%`

### Evidence

- Permission constants include billing scopes:
  - `backend/Infrastructure/Auth/AuthPermissions.cs`
- Demo policy endpoints only:
  - `backend/Features/Authorization/AuthorizationDemoEndpoints.cs`
- No invoice/accounting/trust domain models in `LawFirmDbContext`.

### Missing for parity

- Billing entities and invoice lifecycle.
- Trust ledger + operating ledger architecture.
- Three-way reconciliation workflows.
- Payment application against invoices/trust requests.
- Financial statements and aging reports.

### What it takes to close

1. Accounting domain schema:
   - `Account`, `LedgerEntry`, `Invoice`, `InvoiceLine`, `Payment`, `TrustTransaction`, `ReconciliationBatch`.
2. Immutable ledger posting engine.
3. Billing workflow:
   - draft -> review -> approval -> issue -> payment -> write-off/refund.
4. Trust compliance controls:
   - no fee debits from trust,
   - trust/operating segregation,
   - audit reports.
5. Financial reporting APIs + exports.

---

## 11) Payments (Clio Payments equivalent)

### Expected capability

- Card/ACH/wallet acceptance, secure links, trust-safe routing, payment plans.

### Current status

- **Status:** `Not Implemented`
- **Score:** `0%`

### Evidence

- No PSP integration code, no payment endpoints, no tokenized payment method handling.

### What it takes to close

1. PSP integration abstraction (`IPaymentGateway`).
2. Payment intents + webhook processing.
3. Trust vs operating payout routing.
4. Refund/chargeback reconciliation rules.
5. Secure payment links + portal payment UX.

---

## 12) Client portal and communications

### Expected capability

- Client login, message threads, matter visibility, secure docs, invoices/payments.

### Current status

- **Status:** `Partial foundation only`
- **Score:** `20%`

### Evidence

- Client role exists in demo auth:
  - `backend/Infrastructure/Auth/DemoUserStore.cs`
- Placeholder client portal policy endpoint:
  - `backend/Features/Authorization/AuthorizationDemoEndpoints.cs`
- No client-facing portal routes/pages beyond public marketing site.

### Missing for parity

- Dedicated client portal UI.
- Client-scoped matter/documents/invoices access rules.
- Two-way messaging/audit trail.
- Portal notifications and document requests.

---

## 13) Reporting, dashboards, and analytics

### Expected capability

- Productivity, profitability, matter pipeline, collections, user workload.

### Current status

- **Status:** `Partial baseline dashboard`
- **Score:** `30%`

### Evidence

- Operational dashboard cards + worklists:
  - `frontend/src/pages/WorkspaceHomePage.tsx`
  - `frontend/src/pages/WorkspaceWorklistsPage.tsx`
- No dedicated reporting backend or KPI data warehouse layer.

### Missing for parity

- Saved/custom report builder.
- Profitability and realization metrics.
- Billing/collections analytics.
- Scheduled report distribution.

---

## 14) Automations and AI assistance

### Expected capability

- Trigger workflows, document/task automation, AI-supported drafting/summaries.

### Current status

- **Status:** `Mostly Not Implemented`
- **Score:** `10%`

### Evidence

- Manual workflows dominate all modules.
- No automation engine or AI services in backend/frontend modules.

### Missing for parity

- Rule engine and scheduler.
- Prompt-safe AI pipelines for summaries/drafts.
- AI-assisted bill drafting or event extraction.

---

## 15) Integrations ecosystem

### Expected capability

- Accounting sync, email/calendar sync, cloud storage, e-sign integrations.

### Current status

- **Status:** `Not Implemented`
- **Score:** `0%`

### Evidence

- No integration connectors present in current backend/frontend modules.

### Missing for parity

- Integration framework:
  - OAuth connection manager
  - job workers
  - sync status/error monitoring
- Initial connectors:
  - QuickBooks/Xero
  - Microsoft 365/Google calendar + contacts
  - Google Drive/OneDrive/Dropbox
  - DocuSign/Dropbox Sign

---

## 16) Security, compliance, and identity maturity

### Expected capability

- MFA/SSO, strong auditing, granular policies, data retention controls.

### Current status

- **Status:** `Partial`
- **Score:** `45%`

### Evidence

- JWT + permissions model + policy checks:
  - `backend/Infrastructure/Auth/*`
- Audit events captured on many write operations.
- Strategy doc acknowledges managed IdP direction:
  - `backend/AUTHORIZATION_STRATEGY.md`

### Missing for parity

- Enterprise SSO (OIDC/SAML) and enforced MFA.
- Session management and token revocation model.
- Field-level encryption and key management policy.
- Compliance playbooks (SOC2 controls, access reviews, retention schedules).

---

## Overall parity summary

- Strongest current areas:
  - intake-to-matter lifecycle foundations,
  - calendar/worklist operations,
  - conflicts workflow,
  - role/permission architecture.
- Largest gaps:
  - billing/accounting/trust,
  - payments,
  - client portal,
  - integrations,
  - advanced CRM automation,
  - document/e-sign depth.

Estimated current parity toward full Clio replacement:

- **Approximately 40-45%** for internal operations foundation.
- **Approximately 20-25%** for full business-critical parity once finance/payments/portal are weighted appropriately.

---

## Build roadmap to close gaps

## Phase 1 (0-8 weeks): Core reliability and workflow depth

1. Document binary storage + secure access URLs.
2. Public intake form templates + submission mapping.
3. Intake automation (assignment/reminder/stage triggers).
4. Calendar recurrence and dependency chains.
5. Hardening:
   - background jobs,
   - retry policies,
   - structured telemetry.

## Phase 2 (8-16 weeks): Finance foundation

1. Time entry system.
2. Billing/invoice core model.
3. Operating/trust ledger model + immutable postings.
4. Basic payment integration and invoice payment flow.
5. Financial reporting MVP (AR aging, trust balances, collections).

## Phase 3 (16-24 weeks): Client experience and integrations

1. Client portal (matters/docs/messages/bills/payments).
2. E-sign workflows and signature audit trail.
3. First integration wave:
   - calendar/email sync,
   - accounting sync,
   - cloud doc providers.
4. Reporting suite expansion.

## Phase 4 (24+ weeks): SaaS readiness

1. Multi-tenant data architecture and tenant isolation controls.
2. Tenant-level configuration, branding, and permissions.
3. Subscription/billing for SaaS customers.
4. Advanced compliance and security posture for external customers.

---

## Execution checklist (update continuously)

- [ ] Create `TimeEntry` domain and APIs.
- [ ] Create `Invoice` and `InvoiceLine` domain and APIs.
- [ ] Create trust/operating ledger domain.
- [ ] Implement payment provider abstraction + webhooks.
- [ ] Build client portal routes and backend scopes.
- [ ] Build integration framework and first connectors.
- [ ] Build intake template engine and workflow automations.
- [ ] Build document upload/versioning/e-sign.
- [ ] Add reporting backend and downloadable report endpoints.
- [ ] Add managed IdP + MFA + security hardening.

---

## Maintenance notes

When new features ship, update:

1. the relevant module score,
2. evidence file paths,
3. gaps removed/added,
4. roadmap phase completion status.

This file should stay truthful enough that a new developer can understand:

- where we are,
- what is missing,
- and what to build next to replace Clio-class subscriptions with this app.
