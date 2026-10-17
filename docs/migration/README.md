# Migration Workspace

This folder tracks the migration from the current .NET/React application to a production-minded Laravel + Vue 3 + Docker Compose stack.

## Objective

- Preserve existing working business behavior and UI appearance while replacing the runtime stack.
- Make Docker the required development runtime (no XAMPP dependency).
- Standardize local MySQL access as `127.0.0.1:3307 -> mysql:3306`.

## Current phase

- Phase 1 and Phase 2 in progress:
  - safe baseline established,
  - full-repository audit started,
  - inventory and architecture mapping underway.

## Document map

- `PROGRESS.md` - session-by-session checkpoint, commands, results, blockers, next actions.
- `INVENTORY.md` - file and directory coverage with review disposition.
- `CURRENT_ARCHITECTURE.md` - evidence-based description of current runtime behavior.
- `FEATURE_PARITY.md` - discovered features and current migration disposition.
- `TARGET_ARCHITECTURE.md` - selected migration target architecture and rationale.
- `DATA_MIGRATION.md` - database conversion strategy and validation plan.
- `SECURITY_REVIEW.md` - current findings, required fixes, unresolved risks.
- `UI_PARITY.md` - visual baseline strategy and parity checkpoints.
- `PLAN.md` - ordered implementation plan with dependencies and acceptance criteria.

## Scope guardrails

- Do not claim full migration complete until all tracked features are implemented and validated.
- Do not claim visual parity without direct viewport-based evidence.
- Do not claim tenant isolation/security completion without explicit tests and enforcement evidence.
- Do not delete legacy implementation until replacement workflows are verified and recoverable.
