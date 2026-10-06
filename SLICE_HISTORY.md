# Slice History

## 2026-10-04 - Slice S1 + S2 foundation

Implemented initial vertical slice for:

- identity and actor profile APIs
- authorization boundaries for contact/conflict workflows
- immutable audit trail records for write actions
- consistent error contract with correlation IDs
- contact/organization graph with aliases and relationships
- conflict search and attorney/admin decision workflow with retained snapshots
- frontend workspace flows for sign-in, actor context, parties, and conflict review

### Backend highlights

- Added persistence layer with SQLite EF Core context:
  - `backend/Infrastructure/Persistence/LawFirmDbContext.cs`
  - `backend/Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`
- Added error contract middleware and correlation IDs:
  - `backend/Infrastructure/Errors/*`
- Added immutable audit event writer:
  - `backend/Infrastructure/Audit/AuditTrailService.cs`
- Added endpoint modules:
  - `backend/Features/Identity/IdentityEndpoints.cs`
  - `backend/Features/Parties/PartiesEndpoints.cs`
  - `backend/Features/Conflicts/ConflictEndpoints.cs`
- Updated auth permissions/policies and demo users for contacts/conflicts workflows.

### Frontend highlights

- Added auth provider and route protection:
  - `frontend/src/features/auth/AuthContext.tsx`
  - `frontend/src/features/auth/RequireAuth.tsx`
- Added workspace pages:
  - `frontend/src/pages/WorkspaceLoginPage.tsx`
  - `frontend/src/pages/WorkspaceHomePage.tsx`
  - `frontend/src/pages/PartiesPage.tsx`
  - `frontend/src/pages/ConflictChecksPage.tsx`
- Added API services and types for auth/parties/conflicts:
  - `frontend/src/services/authApi.ts`
  - `frontend/src/services/partiesApi.ts`
  - `frontend/src/services/conflictsApi.ts`
  - `frontend/src/types/auth.ts`
  - `frontend/src/types/parties.ts`
- Updated shared API client to parse backend error envelopes and correlation IDs.

### Validation run

- Backend: `dotnet build -o ./.build-out` (success)
- Frontend: `npm run build` (success)

### Known operational notes

- A separately running backend process can lock default `bin/Debug` outputs.
- Build validation was run with alternate output path for backend.

## 2026-10-04 - Staff Tasks & Calendar slice

Implemented authorized staff-only calendar/task tracking for non-client users.

### Backend highlights

- Added calendar permissions/policies:
  - `calendar.read`
  - `calendar.manage`
- Added calendar persistence model:
  - `CalendarEntry` in `backend/Infrastructure/Persistence/LawFirmDbContext.cs`
- Added calendar API endpoints in `backend/Features/Calendar/CalendarEndpoints.cs`:
  - `GET /api/calendar/entries`
  - `POST /api/calendar/entries`
  - `POST /api/calendar/entries/{id}/acknowledge`
  - `POST /api/calendar/entries/{id}/complete`
- Added audit events for create/acknowledge/complete transitions.

### Frontend highlights

- Added `Tasks & Calendar` workspace route/page:
  - `frontend/src/pages/TasksCalendarPage.tsx`
- Added calendar service/types:
  - `frontend/src/services/calendarApi.ts`
  - `frontend/src/types/calendar.ts`
- Added authenticated nav link to calendar workspace:
  - `frontend/src/components/SiteNav.tsx`

### Validation run

- Backend: `dotnet build -o ./.build-out` (success)
- Frontend: `npm run build` (success)

## 2026-10-04 - Slice N1 Intake Pipeline + Matter Shell

Implemented the first intake-to-matter production workflow slice with seeded data for manual testing.

### Backend highlights

- Added intake + matter shell persistence models in `backend/Infrastructure/Persistence/LawFirmDbContext.cs`:
  - `IntakeRecord`, `IntakeStageHistory`, `IntakeNote`
  - `MatterRecord`, `MatterStageHistory`
  - `IntakeStages` constants
- Added MySQL-safe table creation for new entities in `backend/Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`.
- Added intake permissions/policies:
  - `intake.read`
  - `intake.manage`
  - policy wiring in `backend/Infrastructure/Auth/AuthServiceCollectionExtensions.cs`
- Added intake API in `backend/Features/Intake/IntakeEndpoints.cs`:
  - `GET /api/intake`
  - `GET /api/intake/{intakeId}`
  - `POST /api/intake`
  - `POST /api/intake/{intakeId}/stage`
  - `POST /api/intake/{intakeId}/notes`
  - `POST /api/intake/{intakeId}/convert-to-matter`
- Implemented stage transition rules and acceptance gate requiring an approved conflict decision for `Accepted`.
- Added matter shell API in `backend/Features/Matters/MattersEndpoints.cs`:
  - `GET /api/matters-v2`
  - `GET /api/matters-v2/{matterId}`
- Added development seeding in `backend/Infrastructure/Persistence/DevelopmentDataSeeder.cs` and wired startup seed call in `backend/Program.cs`.

### Frontend highlights

- Added intake board/detail flows:
  - `frontend/src/pages/IntakeBoardPage.tsx`
  - `frontend/src/pages/IntakeDetailPage.tsx`
  - `frontend/src/services/intakeApi.ts`
  - `frontend/src/types/intake.ts`
- Added matter list + workspace shell:
  - `frontend/src/pages/MattersPage.tsx`
  - `frontend/src/pages/MatterWorkspacePage.tsx`
  - `frontend/src/pages/MatterWorkspaceTabs.tsx`
  - `frontend/src/services/mattersApi.ts`
  - `frontend/src/types/matters.ts`
- Added workspace routing for intake/matters and nested matter tabs in `frontend/src/app/AppShell.tsx`.
- Updated permission-aware nav links for intake/matters in `frontend/src/components/SiteNav.tsx`.
- Added kanban/workspace tab styling in `frontend/src/app/AppShell.css`.

### Validation run

- Backend: `dotnet build -o ./.build-out` (success)
- Frontend: `npm run build` (success)
- Lints: `ReadLints` on edited backend/frontend files (no errors)

### Current limits

- Matter tab sections for tasks/documents are placeholders for Slice N2 deeper functionality.
- Intake board is stage-grouped kanban but does not yet support drag/drop stage transitions.

## 2026-10-04 - Slice N2 Matter Workspace Tasks + Documents + Timeline Actions

Implemented the second workspace slice to support matter execution workflows (task/deadline operations, document index metadata, and timeline actions) with seed data for manual validation.

### Backend highlights

- Extended persistence model in `backend/Infrastructure/Persistence/LawFirmDbContext.cs`:
  - `MatterTask`
  - `MatterDocument`
  - `MatterTimelineAction`
- Added MySQL-safe table creation in `backend/Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`:
  - `matter_tasks`
  - `matter_documents`
  - `matter_timeline_actions`
- Expanded `backend/Features/Matters/MattersEndpoints.cs` with new endpoints:
  - tasks:
    - `GET /api/matters-v2/{matterId}/tasks`
    - `POST /api/matters-v2/{matterId}/tasks`
    - `POST /api/matters-v2/{matterId}/tasks/{taskId}`
    - `DELETE /api/matters-v2/{matterId}/tasks/{taskId}`
  - documents:
    - `GET /api/matters-v2/{matterId}/documents`
    - `POST /api/matters-v2/{matterId}/documents`
    - `POST /api/matters-v2/{matterId}/documents/{documentId}/review`
    - `DELETE /api/matters-v2/{matterId}/documents/{documentId}`
  - timeline actions:
    - `GET /api/matters-v2/{matterId}/timeline`
    - `POST /api/matters-v2/{matterId}/timeline-actions`
- Added audit events for all new write actions (task/document/timeline).

### Frontend highlights

- Expanded matter types in `frontend/src/types/matters.ts` for:
  - tasks + create/update payloads
  - document metadata + review payloads
  - timeline actions + create payload
- Expanded API service in `frontend/src/services/mattersApi.ts` for all N2 endpoints.
- Reworked `frontend/src/pages/MatterWorkspaceTabs.tsx`:
  - **Tasks tab**: add/list/update status/verify deadline/delete tasks
  - **Documents tab**: add/list/review/delete document metadata entries
  - **Timeline tab**: add/list custom timeline actions and display stage history context

### Seed data updates

- Extended `backend/Infrastructure/Persistence/DevelopmentDataSeeder.cs` with:
  - sample matter tasks
  - sample matter documents
  - sample matter timeline actions

### Validation run

- Backend: `dotnet build -o ./.build-out` (success)
- Frontend: `npm run build` (success)
- Lints: `ReadLints` on edited backend/frontend files (no errors)

### Current limits

- Document handling is metadata index only in this slice (no binary upload/storage pipeline yet).
- Task updates currently use a full-field post model from UI quick actions rather than granular PATCH semantics.
