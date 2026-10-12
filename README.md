# Law Firm Unified Platform

This repository is building a **single system for law-firm operations** so the firm does not need:

- one product for the public website,
- a second product for intake/CRM,
- and a third product for practice management and operations.

The product direction is to consolidate those workflows into one app that can eventually be offered as multi-tenant SaaS for other firms.

## Why this exists

Many firms run a fragmented stack:

- marketing website + contact forms,
- a legal CRM/intake product,
- a legal practice management system,
- separate accounting and payment tools,
- disconnected document and e-sign systems.

This project is intentionally taking the opposite path:

- modern public site for lead generation,
- internal legal operations workspace,
- intake-to-matter lifecycle in one system,
- role-based access across legal staff and clients,
- extensible architecture for future billing, trust accounting, portal, and integrations.

## Product strategy

The baseline benchmark is Clio's combined capability footprint across:

- Clio Manage (practice management),
- Clio Grow (legal CRM/intake),
- Clio Payments (legal payment + trust-aware flows),
- and ecosystem integrations.

We are **not embedding Clio**. We are replicating and adapting core module patterns so this firm can operate inside its own branded platform.

## Current codebase snapshot

### Frontend

- React + TypeScript + Vite
- Route-based app with:
  - public pages (marketing + contact + appointment request),
  - authenticated workspace pages (calendar, parties, conflicts, intake, matters, consultations).
- Role/permission-aware navigation and protected routes.

### Backend

- ASP.NET Core minimal APIs
- EF Core persistence (`LawFirmDbContext`) with modules for:
  - parties and aliases,
  - conflict checks and decisions,
  - calendar entries,
  - intake records/stage history/notes,
  - matters/tasks/documents/timeline,
  - consultation requests,
  - audit events.
- JWT auth with policy/permission model.

## Current implemented modules (high-level)

- Public website + content + animated UX
- Authentication and role-based authorization
- Contacts/parties management
- Conflict check + legal review decision recording
- Staff calendar/task scheduling
- Intake pipeline with stage transitions and notes
- Intake conversion to matter shell
- Matter workspace tabs (overview, tasks, docs metadata, timeline)
- Consultation request flow from contact page + internal queue
- Audit logging hooks on major write operations

## What is intentionally not complete yet

These major categories are still early or missing compared with Clio-scale systems:

- legal billing and invoicing engine,
- trust accounting and reconciliation workflows,
- payments processing,
- client self-service portal,
- document generation/e-sign,
- workflow automation/rules engine,
- reporting/analytics suite,
- integration marketplace and sync connectors,
- multi-office/multi-entity accounting controls,
- enterprise security controls (SSO/MFA/step-up auth),
- tenant architecture for SaaS distribution.

## Source-of-truth planning artifact

Use `ProgressPlan.md` in the repository root as the ongoing parity tracker.

`ProgressPlan.md` includes:

- Clio capability map,
- current implementation status in this repository,
- evidence pointers to code,
- gap analysis,
- technical roadmap to close gaps,
- and priority sequencing.

Update it as each module is implemented so progress remains measurable over time.

## Running locally

### Backend

- Open `backend`
- Configure database connection in `backend/appsettings.json`
- Run the API (development environment seeds demo data)

### Frontend

- Open `frontend`
- Install dependencies:
  - `npm install`
- Run dev server:
  - `npm run dev`

## Development credentials (local only)

Demo users and passwords are listed in `backend/DEV_AUTH_CREDENTIALS.md`.

These are for development/testing only and must never be used in production.

## References used for capability benchmarking

- [Clio Manage](https://www.clio.com/manage/)
- [Clio Features](https://www.clio.com/features/)
- [Clio Pricing / plan capability matrix](https://www.clio.com/pricing/)
- [Clio case management features](https://www.clio.com/features/case-management/)
- [Clio online intake forms](https://www.clio.com/features/online-intake-forms/)
- [Clio legal CRM](https://www.clio.com/features/legal-crm-software/)
- [Clio Payments](https://www.clio.com/features/payments/)
- [Clio integrations overview](https://www.clio.com/features/integrations/)
