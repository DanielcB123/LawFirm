# Migration Plan (Dependency-Ordered)

## Phase A - Baseline and audit closure

1. Complete file-by-file review coverage in `INVENTORY.md`.
2. Complete workflow traces in `FEATURE_PARITY.md`.
3. Capture UI screenshot baseline with synthetic data.
4. Record unresolved unknowns (data retention, local DB provenance, auth edge cases).

Acceptance:

- No relevant first-party file remains unclassified.
- Core workflows have explicit current-state traces.

## Phase B - Docker-first development runtime

1. Introduce Compose stack for:
   - Laravel backend runtime,
   - Vue frontend runtime,
   - MySQL service.
2. Enforce MySQL mapping: `127.0.0.1:3307:3306`.
3. Document host-vs-container addressing:
   - containerized app -> `mysql:3306`,
   - Windows DB client -> `127.0.0.1:3307`.
4. Add health checks and persistent DB/upload volumes.

Acceptance:

- `docker compose up` brings up all required services.
- No dependency on XAMPP services for project runtime.

## Phase C - Vertical slice migration

1. Bootstrap Laravel project structure.
2. Bootstrap Vue 3 project structure.
3. Implement one end-to-end workflow:
   - authentication + one representative domain flow (likely intake or matters).
4. Verify API/DB/UI flow works inside Docker.

Acceptance:

- One workflow fully usable in Laravel+Vue stack via Compose.
- Feature tests and frontend checks pass for slice scope.

## Phase D - Incremental feature migration

Migrate in this sequence:

1. Auth and permission scaffolding.
2. Contacts and conflicts.
3. Intake and consultations.
4. Matters and matter access grants.
5. Calendar/tasks.
6. Remaining workspace modules and placeholders with honest status.

Acceptance:

- Each migrated module has parity notes and verification evidence.

## Phase E - Data migration and hardening

1. Finalize schema mapping and import scripts.
2. Validate conversion with sanitized copy.
3. Security and authorization regression testing.
4. Operational docs update for local setup, testing, and runbooks.

Acceptance:

- Data conversion validation complete for defined datasets.
- Security/access tests cover unauthorized and cross-boundary paths.

## Phase F - Legacy retirement planning

1. Keep old stack available during parity verification.
2. Retire legacy only after new stack parity is verified and recoverable backup exists.

Acceptance:

- Explicit sign-off checklist completed before any destructive retirement action.
