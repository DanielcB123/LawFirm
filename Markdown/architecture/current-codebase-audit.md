# Current codebase audit

[Home](../README.md) · [Architecture](README.md) · [Stack and runtime](stack-and-runtime.md) · [Gap map](../delivery/implementation-gap-map.md) · [ADR-001](../database/ADR-001-tenancy.md)

**Audit date:** 2026-10-12. **Commit inspected:** `61e5134` (branch head, plus untracked `Markdown/`). **Method:** static source reading, configuration inspection (key names only; secret values were not printed), local builds, and lint. The API was **not** run against a live MySQL server, and no HTTP request was exercised.

> **Status update, 2026-10-12 (slice F1, uncommitted working tree).** The findings below are kept as they stood at `61e5134`. Since then, integration tests against MySQL 8.0.36 verify these fixes:
> - Matter routes are now scoped by assignment or grant. Clients are rejected on staff routes and use `/api/portal/matters`. Admin access is now assignment-or-grant only.
> - `/api/parties` hides restricted parties, and conflict masking no longer exposes the real party id.
> - `DemoUserStore` is registered only through an explicit Development opt-in.
> - Tracked config holds no secrets, and the fallback connection string is removed. Earlier commits still contain credentials, which have not been rotated.
> - `EnsureCreated` and the raw DDL were replaced by EF Core migrations. Unmanaged databases are refused.
> - An integration test project exists.
>
> Details: [matter access policy](../security/matter-access-policy.md), [migration baseline](../database/migration-baseline.md), [SLICE_HISTORY.md](../../SLICE_HISTORY.md).

## Evidence labels

| Label | Meaning |
|---|---|
| **Inspected** | Behavior read directly in source at the cited path |
| **Inferred** | Consequence reasoned from inspected code; not executed |
| **Untested** | Assumption that no test or runtime run confirms |
| **Verified** | Confirmed by a command run during this audit (build/lint) |

No automated tests exist in the repository, so no behavior below is test-verified.

## Repository inventory

| Area | Finding | Evidence |
|---|---|---|
| Root | `backend/`, `frontend/`, `Markldown/` (prompt + blueprint zip; folder name is misspelled and was left untouched), `README.md`, `ProgressPlan.md`, `SLICE_HISTORY.md`, `.gitignore` | Inspected |
| Instructions | No `AGENTS.md`, `.cursor/rules`, or contributor guide found | Inspected (glob) |
| Backend entry | `backend/Program.cs` — minimal API host, Swagger in Development only, HTTPS redirection, authN/authZ, endpoint modules under `/api` | Inspected |
| Backend manifest | `backend/EnterpriseKnowledgeAssistant.Api.csproj` — `net8.0`; JwtBearer 8.0.11, Pomelo.EntityFrameworkCore.MySql 8.0.3, Swashbuckle 6.8.1. No lockfile (`packages.lock.json` absent) | Inspected |
| Frontend entry | `frontend/src/main.tsx` → `AuthProvider` → `AppShell`. `frontend/src/App.tsx` is the unused Vite template | Inspected |
| Frontend manifest | `frontend/package.json` + `package-lock.json` — React 19.2, react-router-dom 6.30, Vite 8.2, TypeScript ~6.0, ESLint 10 | Inspected |
| Database / ORM | MySQL via Pomelo; server version pinned in code to 8.0.36 | Inspected: `backend/Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs` |
| Migrations | **Absent.** `EnsureCreatedAsync()` followed by raw `CREATE TABLE IF NOT EXISTS` for later tables | Inspected (same file) |
| Tests | **Absent** (no test project, no `*.test.*`/`*.spec.*`) | Inspected (glob + `git ls-files`) |
| CI / deployment | **Absent** (no `.github/`, Dockerfile, compose, scripts) | Inspected |
| Environment examples | **Absent.** Real-looking config lives in tracked `backend/appsettings.json` and `appsettings.Development.json` | Inspected |
| Existing docs | [README.md](../../README.md), [ProgressPlan.md](../../ProgressPlan.md), [SLICE_HISTORY.md](../../SLICE_HISTORY.md), [backend/AUTHORIZATION_STRATEGY.md](../../backend/AUTHORIZATION_STRATEGY.md), [backend/DEV_AUTH_CREDENTIALS.md](../../backend/DEV_AUTH_CREDENTIALS.md), [frontend/src/ARCHITECTURE.md](../../frontend/src/ARCHITECTURE.md), [frontend/README.md](../../frontend/README.md) | Inspected |

## Audit coverage by module

| Module | Files | Depth |
|---|---|---|
| Host, error handling, correlation IDs | `Program.cs`, `Infrastructure/Errors/*` | Fully read |
| Auth / identity | `Infrastructure/Auth/*`, `Features/Auth/*`, `Features/Identity/IdentityEndpoints.cs` | Fully read |
| Authorization demo routes | `Features/Authorization/AuthorizationDemoEndpoints.cs` | Fully read |
| Persistence model + seed | `LawFirmDbContext.cs`, `PersistenceServiceCollectionExtensions.cs`, `DevelopmentDataSeeder.cs` | Fully read |
| Audit trail | `Infrastructure/Audit/AuditTrailService.cs` | Fully read |
| Parties, conflicts, calendar, intake, matters, consultations | `Features/*/*Endpoints.cs` | Fully read |
| Health, weather template | `Features/Health`, `Features/WeatherForecast` | Endpoint mapping read; service body not read |
| Frontend routing/auth/HTTP client | `app/AppShell.tsx`, `features/auth/*`, `services/*`, `vite.config.ts` | Fully read |
| Frontend pages (17 files) | `src/pages/*` | **Partial**: read via service calls, permission greps, and route wiring; render/state logic not reviewed line by line |
| Styling, public marketing content | `*.css`, `data/teamMembers.ts`, marketing pages | Not reviewed beyond a firm-name grep |
| Generated/vendor | `node_modules`, `bin`, `obj`, `dist`, `.build-out` | Excluded |

## Representative paths, end to end

### Sign-in — present, demo-grade
`WorkspaceLoginPage` → `authApi.loginWithPassword` → `POST /api/auth/login` (`Features/Auth/AuthEndpoints.cs`) → `DemoUserStore.FindByEmail` / `VerifyPassword` → `TokenService.CreateAccessToken` (HS256, claims `sub`, `email`, `name`, `role`, one `permission` claim per permission, `jti`). Frontend stores the token in `localStorage` (`features/auth/AuthContext.tsx`) and calls `GET /api/identity/me` on boot. **Inspected.**

- Users are eight hard-coded accounts with plaintext passwords hashed at startup (`Infrastructure/Auth/DemoUserStore.cs`). `IDemoUserStore` is registered unconditionally in `Program.cs` — **not** gated to Development. **Inspected.** A production deployment would accept the documented demo passwords. **Inferred.**
- Token validation checks issuer, audience, signing key, and lifetime with 30 s skew (`AuthServiceCollectionExtensions.cs`). **Inspected.** No refresh tokens, revocation, session store, MFA, lockout, or login rate limiting. Logout only clears `localStorage`. **Inspected.**
- Permissions are frozen into the token at issue time; a role change takes effect only after expiry (60 min base config; the Development file sets a three-digit value). **Inferred.**

### Tenant selection — absent
No firm/tenant entity, claim, header, hostname resolution, or per-request connection selection exists. One connection string (`ConnectionStrings:LawFirm`) serves all requests. The demo users and seed data use a fictional `parkerreedlaw.com` domain. **Inspected.** The current system is effectively **single-firm, single-database**.

### Matter read/write — present, assignment not enforced
`MattersPage`/`MatterWorkspaceTabs` → `mattersApi` → `/api/matters-v2/*` (`Features/Matters/MattersEndpoints.cs`).

- Every read route requires policy `MatterReadAssigned`; writes require `MatterUpdateAssigned`. `PermissionAuthorizationHandler` checks only that the permission claim exists. **No query filters by responsible attorney, paralegal, or any grant.** `ListMatters` returns up to 200 matters from the whole firm. **Inspected.** Any paralegal can read and modify every matter's tasks and timeline. **Inferred.**
- Admin users hold `matters.read.all` but not `matters.read.assigned`, and no route uses `MatterReadAll`. Admins are therefore denied every `/matters-v2` read route and all task/timeline writes, yet they can still review and delete document metadata through `DocumentReview`. Meanwhile the frontend worklist treats either permission as sufficient (`pages/WorkspaceWorklistsPage.tsx:70`). **Inferred; not exercised.**
- Clients hold `documents.upload`. `POST /api/matters-v2/{matterId}/documents` requires only that policy plus `MatterExists`, so a client can attach metadata (including an arbitrary `StorageKey` string) to **any** matter whose GUID they know. **Inferred from inspected code.**
- Tasks, documents, and calendar entries are hard-deleted (`Remove` + `SaveChanges`). **Inspected.**
- `GenerateMatterNumberAsync` (`Features/Intake/IntakeEndpoints.cs`) reads all numbers and takes max+1; concurrent conversions collide on the unique index and surface as a 500. **Inferred.**

### Intake and conflicts — present, gate is weakly bound
- Stage machine with allowed transitions; moving to `Accepted` requires an existing `Approved` `ConflictDecision` (`IntakeEndpoints.ChangeStage`). **Inspected.** The decision is **not** checked to belong to this intake, its parties, or a recent check — any approved decision ID satisfies the gate. **Inspected.**
- `ConvertToMatter` requires `Accepted` and is idempotent per intake. There is no engagement or fee-agreement step, and no signature. **Inspected.**
- `GET /api/intake` exposes all prospective-client records to anyone with `intake.read` (all staff). **Inspected.**
- Conflict search loads **all** parties into memory and masks restricted matches for non-attorney/admin roles (`Features/Conflicts/ConflictEndpoints.cs`). **Inspected.** The stored snapshot is the masked view when a paralegal runs it, and the attorney decision copies that snapshot. **Inspected.**
- `GET /api/parties` (`Features/Parties/PartiesEndpoints.cs`) returns restricted parties **unmasked** to any `contacts.read` holder, bypassing the conflict masking. **Inspected.**

### Consultations (public booking) — present, not concurrency-safe
`ContactPage` → `GET /api/consultations/available-slots` and `POST /api/consultations` — both **anonymous** (`Features/Consultations/ConsultationsEndpoints.cs`). **Inspected.**

- Conflict detection is check-then-insert with no unique constraint or lock; two simultaneous requests for one slot can both succeed. **Inferred.**
- Slots are generated as 09:00–16:30 **UTC** on weekdays regardless of the firm's time zone (05:00–12:30 in US Eastern daylight time). **Inspected.**
- No rate limit, CAPTCHA, or size limit beyond column lengths. The response echoes the created record. **Inspected.**
- A consultation does not create an intake record or party. **Inspected.**

### Calendar and deadlines — partial provenance
`CalendarEntry` stores `DeadlineType`, `SourceReference`, `VerifiedAtUtc/ByActorId/ByDisplayName`, `OverrideReason`, `TimeZone`, and `IsAllDay` (`LawFirmDbContext.cs`). **Inspected.** Verification fields are **taken from the request body** in create/update, so a caller can attribute verification to another person or time. **Inspected.** `MatterReference` is free text, not a foreign key. No jurisdiction, trigger fact, rule version, or date-only representation exists. `MatterTask` verification uses the authenticated actor. **Inspected.**

### Document access — metadata only
No upload, storage, scanning, download, or preview path exists. `MatterDocument.StorageKey` is a caller-supplied string; seeds contain fictional keys. `/api/documents/upload` is a demo stub. **Inspected.**

### Background jobs, notifications — absent
No hosted services, queues, outbox, email/SMS, or scheduler. Calendar `RemindersJson` is stored but nothing dispatches reminders. **Inspected (no `IHostedService`, no mail packages).**

### Payments / webhooks — absent
No billing entities, PSP SDKs, or webhook routes. `/api/billing/*` are demo stubs. **Inspected.**

### Client portal — absent
`/api/portal/client` returns a static object. No client-facing routes exist. `RequireAuth` checks only for a token, so a client can open `/workspace/*` pages. The API rejects most calls except document metadata creation (see above). **Inspected/Inferred.**

## Cross-cutting observations

| Topic | Finding | Label |
|---|---|---|
| Audit trail | `AuditTrailService.RecordAsync` calls its own `SaveChangesAsync` after the domain save, so the two writes are not atomic. A failure after commit returns 500 even though the change persisted | Inspected / Inferred |
| Audit immutability | `audit_events` is an ordinary mutable table; [SLICE_HISTORY.md](../../SLICE_HISTORY.md) describes it as "immutable", which the code does not enforce | Inspected |
| Audit content | `party.created` metadata stores `DisplayName`; `conflict.check_created` stores the raw query (prospective names) | Inspected |
| Correlation ID | Accepted from client header without length bound; persisted into `varchar(128)` | Inspected; overflow behavior untested |
| Secrets | JWT signing key and DB credentials are in tracked `appsettings*.json`; a fallback connection string with a password is hard-coded in `PersistenceServiceCollectionExtensions.cs` | Inspected (values not printed) |
| Time | Instants are `DateTimeOffset` stored as `datetime(6)` (offset not retained). Calendar keeps an IANA zone name separately. No date-only type for legal deadlines | Inspected |
| Money | No money fields exist yet | Inspected |
| CORS / hosts | No CORS policy (frontend uses Vite proxy); `AllowedHosts` is a wildcard | Inspected |
| Schema evolution | `EnsureCreated` never alters existing tables; new columns on existing tables would not deploy | Inspected / Inferred |
| Legacy doc drift | `SLICE_HISTORY.md` S1 says SQLite; the code uses MySQL. `ProgressPlan.md` scores are self-assessed and do not reflect the authorization gaps above | Inspected |

## Verification run during this audit

| Command | Result |
|---|---|
| `dotnet build -o ./.build-out` (SDK 10.0.300, target net8.0) | **Verified:** success, 0 warnings, 0 errors |
| `npm run build` (Node 22.22.3) | **Verified:** success |
| `npm run lint` | **Verified failing:** 7 errors, 3 warnings (`react-hooks/set-state-in-effect`, `exhaustive-deps`; e.g. `pages/TasksCalendarPage.tsx:130`) |
| Runtime API/DB exercise | Not performed (no MySQL instance verified at `localhost:3307`) |

## Reusable foundations
Endpoint-module pattern (`IEndpointModule`), consistent error envelope with correlation ID, policy/permission vocabulary, intake stage machine, conflict snapshot + decision record, matter/task/document/timeline schema with stage history, calendar provenance fields, audit writer interface, typed frontend service layer, and permission-aware navigation. The single-database schema is a usable starting point for a per-firm data plane (see [ADR-001](../database/ADR-001-tenancy.md)).

## Unread or blocked areas
Frontend page internals and CSS (partial); weather template service body; runtime behavior against MySQL; hosting/deployment environment (none in repository); any external systems the firm uses today (requires [first-firm discovery](../product/first-firm.md)).
