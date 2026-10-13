# Agent instructions

At the start of each repository work session, read ProgressPlan.md and the latest relevant SLICE_HISTORY.md entries. Inspect source and applicable domain documentation before relying on their claims. At the end of each session that changes code, configuration, tests, documentation, architecture decisions, or verified project status, update ProgressPlan.md with the current state and append a truthful SLICE_HISTORY.md entry. Synchronize affected gap-map, roadmap, and domain documents. Record failed, blocked, partial, and verification-pending outcomes as carefully as successful outcomes. Before a planned pause or handoff, record the current task, changed files, completed checks, unresolved failures, and exact next action. Never claim undocumented work is complete. Never include secrets or confidential client data in handoff files. A read-only session with no new findings does not require a fabricated history entry.

## Where things are

- Handoff state: [ProgressPlan.md](ProgressPlan.md), then [SLICE_HISTORY.md](SLICE_HISTORY.md).
- Canonical design and domain documentation: [Markdown/README.md](Markdown/README.md). The original prompt and blueprint zip are archived read-only in `Markdown/archive/original-prompt/` (formerly the misspelled root `Markldown/` folder); do not edit or delete them.
- Solution and folder conventions (backend and frontend): [Markdown/architecture/solution-structure.md](Markdown/architecture/solution-structure.md).
- Matter access policy: [Markdown/security/matter-access-policy.md](Markdown/security/matter-access-policy.md).
- Database baseline and migrations: [Markdown/database/migration-baseline.md](Markdown/database/migration-baseline.md).
- Integration tests (real MySQL in a disposable container, Docker required): `backend.IntegrationTests/`. Unit tests (no database): `backend.UnitTests/`. Frontend tests: `npm test` in `frontend/`.
- NuGet versions are set only in `Directory.Packages.props`; project files reference packages without versions.
