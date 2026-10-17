# Target Architecture (Proposed)

## Decision (current)

Use **Laravel API + separate Vue 3 application** (not Inertia) for this migration.

### Rationale

- Existing app is already split into backend API and frontend SPA concerns.
- Feature parity work benefits from preserving explicit API boundary and contracts.
- Enables independent frontend evolution while preserving current UI design behavior.
- Better alignment with existing test style (direct API and UI behavior checks).

## Target stack

- Backend: Laravel (PHP 8.3+), MySQL.
- Frontend: Vue 3 + Vite + TypeScript.
- Development runtime: Docker Compose only (no XAMPP requirement).
- Optional queue/cache components added only when justified by migrated behavior.

## Proposed directory shape (additive, non-destructive)

- `laravel-backend/` - Laravel application.
- `vue-frontend/` - Vue 3 SPA preserving existing visual design.
- `docker/` - Dockerfiles and helper scripts if needed.
- `docs/migration/` - migration evidence and planning.
- Existing `.NET` and React app remains intact during migration comparison period.

## Authentication and authorization strategy

- Laravel Sanctum personal access tokens for migration-phase API auth.
- Route-level permission middleware in Laravel (`permission:*`) for behavior-level access control beyond authentication.
- Recreate role/permission model from existing backend policies (in progress; initial contacts/intake permissions mapped).
- Implement server-side matter-level access checks equivalent to current behavior.
- Do not trust client-supplied tenant or matter membership claims.

## Database strategy

- Primary dev DB service: Compose MySQL.
- Port mapping requirement:
  - host: `127.0.0.1:3307`
  - container: `3306`
  - mapping: `127.0.0.1:3307:3306`
- App container DB connection:
  - host `mysql` (Compose service name)
  - port `3306`
- Windows DB client connection:
  - host `127.0.0.1`
  - port `3307`

## Docker strategy

- Compose services (initial):
  - `backend` (Laravel PHP runtime),
  - `frontend` (Vue + Vite dev server),
  - `mysql` (persistent data volume),
  - optional `queue` worker once needed.
- Include health checks and dependency readiness.
- Avoid destructive startup commands or automatic DB resets.

## Testing strategy

- Backend:
  - unit tests for core domain logic and policies,
  - feature tests for endpoint behavior and validation,
  - authorization regression tests (including unauthorized access).
- Frontend:
  - unit/component tests for key interactions,
  - build/type/lint checks.
- Integration:
  - representative end-to-end workflows against Compose stack.
