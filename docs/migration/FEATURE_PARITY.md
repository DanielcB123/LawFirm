# Feature Parity Tracker

Status values:

- `Discovered` - confirmed in current app.
- `Migrating` - being implemented in Laravel/Vue stack.
- `Migrated` - implemented in new stack.
- `Verified` - migrated and verified against baseline behavior.
- `Gap/Absent` - not currently implemented in existing app.

## Current application feature status (source baseline)

| Feature area | Current app status | Evidence | Migration status |
|---|---|---|---|
| Public marketing pages | Discovered | `frontend/src/pages/public/*` | Pending |
| Contact page and consultation request flow | Discovered | `ContactPage.tsx`, `ConsultationsEndpoints.cs` | Pending |
| Authentication (JWT + demo users) | Discovered | `Features/Auth`, `Infrastructure/Auth` | Migrating (Sanctum token login/me slice live) |
| Workspace navigation and dashboard pages | Discovered | `AppRoutes.tsx`, workspace pages | Pending |
| Contacts/parties management | Discovered | `PartiesEndpoints.cs`, `ContactsPage.tsx` | Migrating (Laravel contacts vertical slice live) |
| Conflict checks and decisions | Discovered | `ConflictEndpoints.cs` | Pending |
| Calendar/task entries | Discovered | `CalendarEndpoints.cs`, `TasksCalendarPage.tsx` | Pending |
| Intake pipeline | Discovered | `IntakeEndpoints.cs` | Migrating (list/create vertical slice live) |
| Matter management and tabs | Discovered | `MattersEndpoints.cs`, `MatterWorkspace*` | Pending |
| Matter access grants and client portal API | Discovered | `MatterAccessGrantEndpoints`, `ClientPortalEndpoints` | Pending |
| Billing/accounts/integrations pages | Partially discovered (UI scaffolding, limited/no backend depth) | workspace pages + placeholder modules | Pending |
| Document binary storage/download pipeline | Gap/Absent (metadata-focused) | matter document model and pages | Pending |
| Trust accounting/payments | Gap/Absent | placeholder endpoint modules | Pending |
| Multitenancy (runtime) | Gap/Absent in implementation | docs/design artifacts only | Pending |

## New-stack vertical slice status

- Docker Compose stack is running with:
  - `backend` on `127.0.0.1:8080`
  - `frontend` on `127.0.0.1:5174`
  - `mysql` on `127.0.0.1:3307:3306`
- Laravel runtime now consistently uses MySQL via:
  - `DB_HOST=mysql`
  - `DB_PORT=3306`
  - tracked docker env file `laravel-backend/.env.docker`.
- Verified workflow:
  - `GET /api/health` reports `"database":"up"`
  - `POST /api/auth/login` returns bearer token for seeded demo user
  - `GET /api/identity/me` returns authenticated actor profile via Sanctum token
  - unauthenticated `GET /api/contacts` now returns 401 JSON (`Unauthenticated.`)
  - unauthorized `POST /api/intake` without token returns 401
  - authenticated user without `intake.manage` receives 403 on intake create
  - authenticated user with `intake.manage` can `POST /api/intake`, then `GET /api/intake` returns created record
  - `GET /api/contacts` returns list
  - `POST /api/contacts` creates row
  - subsequent `GET /api/contacts` returns created row.
- Known limitation:
  - role/permission scope is currently user-row JSON + middleware checks; it does not yet replicate full legacy claim policy set, role branching, or matter-level authorization semantics.

## Workflow trace status

- End-to-end trace depth in progress; each major flow will be documented as:
  - happy path
  - validation failure path
  - unauthorized path
  - side effects (DB/audit/storage)

## Confirmed workflow traces (initial)

1. Public consultation request:
   - `ContactPage` -> `POST /api/consultations` -> validation + rate limiting + slot lock ->
     `consultation_requests` row + `audit_events` entry.
2. Intake acceptance gate:
   - intake stage change request to `Accepted` requires approved conflict decision tied to the same conflict check.
3. Matter resource authorization:
   - workspace actions -> `matters-v2` routes -> `IMatterAccessService` checks ->
     allow, 403 (writable-but-forbidden), or not-found masking.
4. Client portal document submission:
   - portal route -> client grant validation -> metadata-only document row with server-controlled storage key.

## Planned next traces

1. Sign-in -> workspace access.
2. Intake -> convert to matter -> matter tab operations.
3. Calendar create/update/ack/complete, including matter-reference write constraints.
4. Conflict-check run -> decision record path.
5. Contacts search/create/relationship path with restricted visibility behavior.
