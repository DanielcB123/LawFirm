# Repository Inventory and Review Coverage

This document tracks audit coverage and file disposition. It is intentionally explicit about what is reviewed vs still pending.

## Coverage key

- `Reviewed` - content inspected in this migration effort.
- `Partially reviewed` - inspected for purpose/integration only; needs deeper behavior review.
- `Generated/excluded` - generated, lockfile, binary, or third-party artifact; integration points reviewed instead.
- `Pending` - not yet fully inspected in this migration effort.

## Tracked file census

- Source of truth: `git ls-files`
- Count at baseline: 271 tracked files

## Reviewed files (current session)

| Path | Responsibility | Dependencies/callers | Data/auth implications | Target counterpart | Status | Notes |
|---|---|---|---|---|---|---|
| `AGENTS.md` | repository work rules | all contributors/agents | documentation process and handoff integrity | migration governance | Reviewed | enforced as session constraints |
| `ProgressPlan.md` | current state and roadmap | root planning docs | risk and verification claims | migration baseline input | Reviewed | treated as advisory, verified against code |
| `SLICE_HISTORY.md` | append-only change log | root planning docs | evidence history | migration baseline input | Reviewed | used for preexisting-state trace |
| `README.md` | runtime and setup overview | developers | indicates current run commands and DB expectations | future migration README alignment | Reviewed | documents .NET/React stack |
| `MatterForge.sln` | .NET solution boundary | `backend*` projects | defines current build surface | legacy stack reference | Reviewed | backend + tests only |
| `global.json` | .NET SDK pin | dotnet toolchain | build reproducibility | legacy compatibility reference | Reviewed | SDK `10.0.300` |
| `Directory.Packages.props` | NuGet version pinning | all csproj files | dependency/security surface | migration dependency mapping | Reviewed | EF/Pomelo/Testcontainers versions |
| `backend/Program.cs` | API startup pipeline | all backend modules | auth, persistence init, rate limiter | Laravel bootstrap mapping | Reviewed | applies persistence initialization before serving |
| `backend/Infrastructure/Endpoints/EndpointModuleExtensions.cs` | endpoint auto-registration | all `IEndpointModule` implementations | route lifecycle | Laravel route provider mapping | Reviewed | maps all modules under `/api` |
| `backend/Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs` | DB connection and schema initialization | `LawFirmDbContext`, MySQL | migration safety, startup failure behavior | Laravel DB connection + migration bootstrap | Reviewed | blocks unmanaged legacy schema auto-modification |
| `backend/Infrastructure/Auth/AuthServiceCollectionExtensions.cs` | authn/authz policy wiring | JWT + policies + claims | permission model enforcement | Laravel guards/policies mapping | Reviewed | demo users enabled only in Development |
| `backend/Infrastructure/Persistence/LawFirmDbContext.cs` | EF data model and constraints | all feature endpoints | core schema, FK/index rules, field limits | Laravel migration/model design source | Reviewed | one-firm schema; no tenant column |
| `backend/Infrastructure/Persistence/DevelopmentDataSeeder.cs` | synthetic dev seed data | startup dev path | synthetic legal-like records | Laravel seeder parity source | Reviewed | includes contacts/intake/matters/docs/consultations |
| `backend/Infrastructure/Auth/DemoUserStore.cs` | demo identities and permissions | `AuthEndpoints` | role/permission matrix and login behavior | Laravel auth seeding/fixtures source | Reviewed | in-memory hardcoded users |
| `backend/Infrastructure/Access/MatterAccessService.cs` | matter-level access decisions | matter and portal endpoints | matter authorization boundaries | Laravel policy/service parity source | Reviewed | supports staff/client grant-based access |
| `backend/Features/Auth/AuthEndpoints.cs` | login and current-user API | auth store + token service | sign-in availability and token claims | Laravel auth controller/API | Reviewed | returns 503 when demo auth disabled |
| `backend/Features/Parties/PartiesEndpoints.cs` | contacts CRUD/search relations | `LawFirmDbContext`, audit | restricted-party visibility and write auditing | Laravel contact module | Reviewed | validates party type and relationship rules |
| `backend/Features/Conflicts/ConflictEndpoints.cs` | conflict check + decision workflow | parties + audit + db | restricted match masking and retained snapshots | Laravel conflict module | Reviewed | approximate matching and decision persistence |
| `backend/Features/Intake/IntakeEndpoints.cs` | intake pipeline and conversion | conflicts + matters + audit | stage transition gates and accepted-to-matter path | Laravel intake module | Reviewed | requires approved decision bound to intake conflict check |
| `backend/Features/Matters/MattersEndpoints.cs` | matter read/write and child resources | matter access service + audit | staff read/write boundaries, document metadata ops | Laravel matter module | Reviewed | task/document/timeline operations implemented |
| `backend/Features/Matters/MatterAccessGrantEndpoints.cs` | explicit matter grants | access service + audit | grant creation/revoke constraints | Laravel matter-grant module | Reviewed | responsible attorney grant-management restriction |
| `backend/Features/Calendar/CalendarEndpoints.cs` | calendaring CRUD/status | matter access + audit | matter-linked visibility/write checks; verify rules | Laravel calendar module | Reviewed | write requires writable matter reference |
| `backend/Features/Consultations/ConsultationsEndpoints.cs` | public booking and staff queue | rate limiter + named lock + db | anti-abuse/race controls + intake staff updates | Laravel consultation module | Reviewed | named MySQL lock for slot booking serialization |
| `backend/Features/ClientPortal/ClientPortalEndpoints.cs` | client-facing matter/document routes | matter access + audit | client-scope resource access and upload metadata | Laravel client portal module | Reviewed | separate reduced DTO surface for clients |
| `backend/Features/Health/HealthEndpoints.cs` | health/readiness endpoints | aspnet health checks | runtime observability path | Laravel health endpoint parity | Reviewed | includes `/health` and `/health/ready` |
| `backend/Features/Identity/IdentityEndpoints.cs` | actor profile endpoint | auth claims | claim exposure and correlation id | Laravel identity endpoint parity | Reviewed | returns sorted permissions list |
| `backend/Features/Authorization/AuthorizationDemoEndpoints.cs` | permission demo routes | auth policies | non-production policy examples | likely retire in migration | Reviewed | placeholder/demo scope |
| `backend/Features/Billing/BillingEndpoints.cs` | billing module scaffold | endpoint module loader | future auth-protected billing group | Laravel billing implementation target | Reviewed | placeholder only (`/billing-v2`) |
| `backend/Features/Documents/DocumentsEndpoints.cs` | documents module scaffold | endpoint module loader | future storage/access flow | Laravel documents implementation target | Reviewed | placeholder only (`/documents-v2`) |
| `backend/Features/Engagements/EngagementsEndpoints.cs` | engagements scaffold | endpoint module loader | future fee-agreement workflows | Laravel engagements target | Reviewed | placeholder only |
| `backend/Features/Payments/PaymentsEndpoints.cs` | payments scaffold | endpoint module loader | future payments/webhook surface | Laravel payments target | Reviewed | placeholder only |
| `backend/Features/Trust/TrustEndpoints.cs` | trust accounting scaffold | endpoint module loader | future trust-ledger controls | Laravel trust target | Reviewed | placeholder only |
| `backend/Features/Communications/CommunicationsEndpoints.cs` | communications scaffold | endpoint module loader | future secure messaging/publication | Laravel communications target | Reviewed | placeholder only |
| `backend.IntegrationTests/Infrastructure/MySqlApiFixture.cs` | test runtime provisioning | Testcontainers + API host | verifies MySQL-backed integration behavior | migration test strategy input | Reviewed | uses disposable `mysql:8.0.36` |
| `frontend/src/app/AppRoutes.tsx` | SPA route map | all page components | client-visible feature surface | Vue router parity source | Reviewed | public + workspace route inventory |
| `frontend/src/app/AppShell.tsx` | root layout and global UX hooks | nav/footer/background | visual baseline and shell behavior | Vue app shell parity source | Reviewed | background pinned to skyline theme |
| `frontend/src/app/AppProviders.tsx` | browser/router/auth provider wiring | `BrowserRouter`, `AuthProvider` | auth context scope | Vue app bootstrap provider mapping | Reviewed | minimal provider layer |
| `frontend/src/components/layout/SiteNav.tsx` | public/workspace navigation and role-aware links | auth context + route map | permission-based UI exposure | Vue navigation parity source | Reviewed | signed-in sidebar and public nav behavior |
| `frontend/src/features/auth/AuthContext.tsx` | frontend auth session state | login/me API + localStorage | token persistence and bootstrap behavior | Vue auth composable/store source | Reviewed | localStorage key `lawfirm.auth.token` |
| `frontend/src/features/auth/RequireAuth.tsx` | protected-route gate | auth context + react-router | unauthenticated redirect behavior | Vue route guard parity source | Reviewed | redirects to `/workspace/login` |
| `frontend/src/lib/apiClient.ts` | shared API request/error abstraction | fetch + env base URL | error envelope handling and validation surfacing | Vue API client parity source | Reviewed | maps API errors to typed client errors |
| `frontend/vite.config.ts` | dev/build server behavior | Vite + React plugin | API proxy and local ports | Vite-in-Docker parity design source | Reviewed | proxies `/api` to `localhost:5073` |
| `frontend/src/main.tsx` | frontend entrypoint | app providers/shell | bootstrapping behavior | Vue entrypoint mapping | Reviewed | React StrictMode root |
| `frontend/.env.example` | frontend env contract | API client config | base URL behavior | Vue env contract source | Reviewed | empty `VITE_API_BASE_URL` by default |
| `frontend/src/pages/workspace/WorkspaceLoginPage.tsx` | login form page | auth context + api client | sign-in UX and error display | Vue login view parity source | Reviewed | prefilled demo credentials |
| `frontend/src/pages/workspace/WorkspaceHomePage.tsx` | dashboard/operations cockpit | calendar/intake/matters APIs | role-based metric visibility | Vue dashboard parity source | Reviewed | aggregates multiple API modules |
| `frontend/src/pages/workspace/ContactsPage.tsx` | contact directory and create form | parties API | restricted marker and search behavior | Vue contacts view parity | Reviewed | includes add + search |
| `frontend/src/pages/workspace/ConflictChecksPage.tsx` | conflict workflow UI | conflicts API | decision capture and rationale requirements | Vue conflicts view parity | Reviewed | shows correlation/check ids |
| `frontend/src/pages/workspace/IntakeBoardPage.tsx` | intake kanban and create flow | intake API | intake creation defaults and stage grouping | Vue intake board parity | Reviewed | owner defaults to signed-in actor |
| `frontend/src/pages/workspace/IntakeDetailPage.tsx` | intake detail/stage/notes/convert | intake API + routing | accepted-stage conversion and note history | Vue intake detail parity | Reviewed | accepts conflict decision id input |
| `frontend/src/pages/workspace/MattersPage.tsx` | matter list page | matters API | matter visibility listing | Vue matters list parity | Reviewed | links to nested workspace |
| `frontend/src/pages/workspace/MatterWorkspacePage.tsx` | matter shell and tab host | matters API + nested routes | matter detail exposure | Vue matter workspace parity | Reviewed | nested tab routes |
| `frontend/src/pages/workspace/MatterWorkspaceTabs.tsx` | task/doc/timeline tab interactions | matters API | task/doc/timeline side effects | Vue matter tabs parity | Reviewed | metadata-only document flow |
| `frontend/src/pages/workspace/TasksCalendarPage.tsx` | calendar month/week/day UI + modal CRUD | calendar API | matterReference write behavior, staff ownership fields | Vue calendar parity | Reviewed | modal create/edit/delete flow |
| `frontend/src/pages/workspace/WorkspaceConsultationsPage.tsx` | consultation queue operations | consultations API | intake permission-based queue access | Vue consultations parity | Reviewed | status + notes draft update |
| `frontend/src/pages/public/HomePage.tsx` | marketing home page | team data + health card | visual branding baseline | Vue public home parity | Reviewed | parallax/animation layers |
| `frontend/src/pages/public/ContactPage.tsx` | contact page + appointment modal | consultations API | public intake booking request surface | Vue contact parity | Reviewed | appointment calendar/time picker |
| `backend/appsettings.json` | default backend config | startup config | empty secrets placeholders | Laravel env mapping source | Reviewed | no secrets stored |
| `backend/appsettings.Development.json` | dev config overrides | startup config | migration-on-start in Development | Laravel local env strategy input | Reviewed | `ApplyMigrationsOnStartup=true` |
| `backend/Properties/launchSettings.json` | local launch profile | `dotnet run` | local host ports/env | Docker host-port planning input | Reviewed | `http://localhost:5073` |

## Partially reviewed (broad inspection complete, deep behavior review pending)

- Remaining backend feature files under `backend/Features/**` were route-enumerated; non-reviewed files still require full behavior inspection.
- Frontend pages and feature APIs under `frontend/src/pages/**` and `frontend/src/features/**` were route-surface mapped; behavior-by-component deep review still pending.
- Documentation under `Markdown/**` was partially reviewed for migration-relevant architecture/security/runtime assertions.

## Generated/excluded (integration points reviewed)

| Path/pattern | Reason |
|---|---|
| `frontend/package-lock.json` | generated dependency lock; reviewed as dependency pin artifact only |
| `backend/Infrastructure/Persistence/Migrations/*.Designer.cs` | EF-generated snapshot metadata; reviewed at integration level |
| `Markdown/archive/original-prompt/Law-Firm-SaaS-Blueprint.zip` | binary archive; treated as reference artifact |
| `db/design/validation/mysql-8.0.36-validation.log` | generated validation output log; reviewed for architecture evidence only |

## Pending deep review backlog

All first-party tracked files not listed as `Reviewed` above remain `Pending` or `Partially reviewed`.

Priority pending sets for next pass:

1. Backend endpoint implementations:
   - non-reviewed files in `backend/Features/**` (contracts and any unreviewed endpoints)
2. Backend contracts/entities and middleware:
   - `backend/Infrastructure/**` (remaining files)
3. Frontend feature/page behavior:
   - `frontend/src/pages/**`
   - `frontend/src/features/**`
   - `frontend/src/components/**`
4. Tests:
   - `backend.IntegrationTests/**` (remaining files)
   - `backend.UnitTests/**` (remaining files)
5. Migration/design SQL:
   - `db/design/*.sql`
6. Legacy architecture docs impacting behavior claims:
   - `Markdown/**` (remaining files)

## Open inventory questions

- Whether non-tracked local data stores (local MySQL databases, uploads, or dumps) exist and must be preserved before cutover.
- Whether any runtime configuration currently comes from user-secrets or environment variables that alter behavior from tracked defaults.

## Reviewed in this continuation slice

The following files were inspected during this continuation to reduce the pending backlog:

- Backend contracts and DTOs:
  - `backend/Features/Auth/LoginRequest.cs`
  - `backend/Features/Auth/LoginResponse.cs`
  - `backend/Features/Auth/UserProfileResponse.cs`
  - `backend/Features/Calendar/Contracts/CalendarContracts.cs`
  - `backend/Features/ClientPortal/Contracts/ClientPortalContracts.cs`
  - `backend/Features/Conflicts/Contracts/ConflictContracts.cs`
  - `backend/Features/Consultations/Contracts/ConsultationContracts.cs`
  - `backend/Features/Identity/Contracts/IdentityContracts.cs`
  - `backend/Features/Intake/Contracts/IntakeContracts.cs`
  - `backend/Features/Matters/Contracts/MatterAccessGrantContracts.cs`
  - `backend/Features/Matters/Contracts/MatterContracts.cs`
  - `backend/Features/Parties/Contracts/PartyContracts.cs`
- Backend infrastructure/authn/authz/error abstractions:
  - `backend/Infrastructure/Access/AccessServiceCollectionExtensions.cs`
  - `backend/Infrastructure/Access/ActorClaims.cs`
  - `backend/Infrastructure/Access/MatterAccessResults.cs`
  - `backend/Infrastructure/Access/PartyVisibility.cs`
  - `backend/Infrastructure/Audit/AuditTrailService.cs`
  - `backend/Infrastructure/Auth/AuthConstants.cs`
  - `backend/Infrastructure/Auth/AuthPermissions.cs`
  - `backend/Infrastructure/Auth/AuthPolicies.cs`
  - `backend/Infrastructure/Auth/AuthRoles.cs`
  - `backend/Infrastructure/Auth/DemoUser.cs`
  - `backend/Infrastructure/Auth/IDemoUserStore.cs`
  - `backend/Infrastructure/Auth/ITokenService.cs`
  - `backend/Infrastructure/Auth/JwtOptions.cs`
  - `backend/Infrastructure/Auth/PermissionAuthorizationHandler.cs`
  - `backend/Infrastructure/Auth/PermissionRequirement.cs`
  - `backend/Infrastructure/Auth/TokenResult.cs`
  - `backend/Infrastructure/Auth/TokenService.cs`
  - `backend/Infrastructure/Errors/ApiErrorResponse.cs`
  - `backend/Infrastructure/Errors/CorrelationIdMiddleware.cs`
  - `backend/Infrastructure/Errors/ErrorHandlingExtensions.cs`
  - `backend/Infrastructure/Errors/ExceptionHandlingMiddleware.cs`
  - `backend/Infrastructure/OpenApi/OpenApiServiceCollectionExtensions.cs`
  - `backend/Infrastructure/Outbox/IOutbox.cs`
  - `backend/Infrastructure/Outbox/OutboxDispatcher.cs`
  - `backend/Infrastructure/Payments/IPaymentGateway.cs`
  - `backend/Infrastructure/Signatures/ISignatureProvider.cs`
  - `backend/Infrastructure/Storage/IDocumentStorage.cs`
  - `backend/Infrastructure/Tenancy/IFirmContext.cs`
  - `backend/Infrastructure/Persistence/LawFirmDesignTimeDbContextFactory.cs`
- Frontend features/components:
  - `frontend/src/components/layout/SiteFooter.tsx`
  - `frontend/src/features/auth/*`
  - `frontend/src/features/billing/*`
  - `frontend/src/features/calendar/*`
  - `frontend/src/features/conflicts/*`
  - `frontend/src/features/consultations/*`
  - `frontend/src/features/health/*`
  - `frontend/src/features/intake/*`
  - `frontend/src/features/matters/*`
  - `frontend/src/features/parties/*`
  - `frontend/src/features/portal/*`
- Test suite files:
  - `backend.IntegrationTests/AuthenticationTests.cs`
  - `backend.IntegrationTests/CalendarMatterScopeTests.cs`
  - `backend.IntegrationTests/ClientPortalTests.cs`
  - `backend.IntegrationTests/MatterAccessTests.cs`
  - `backend.IntegrationTests/PartyVisibilityTests.cs`
  - `backend.IntegrationTests/SchemaSafetyTests.cs`
  - `backend.IntegrationTests/VulnerabilityHardeningTests.cs`
  - `backend.IntegrationTests/Infrastructure/HttpAssert.cs`
  - `backend.IntegrationTests/Infrastructure/TestData.cs`
  - `backend.UnitTests/Common/PageRequestTests.cs`

Remaining backlog is now concentrated in selected root config/docs SQL files and a final pass to mark every tracked first-party file with a non-ambiguous reviewed/excluded disposition.

## Reviewed in this continuation (page coverage closure + migration slice files)

- Frontend page set (`frontend/src/pages/**`) was completed in this pass, including:
  - Public pages: `AboutPage`, `AttorneyProfilePage`, `AttorneysPage`, `ContactPage`, `HomePage`, `PracticeAreasPage`, `NotFoundPage`.
  - Workspace pages: `ActivitiesPage`, `AccountsPage`, `BillingPage`, `CommunicationsPage`, `DocumentsPage`, `IntegrationsPage`, `ReportsPage`, `ResourceCentrePage`, `SettingsPage`, `WorkspaceWorklistsPage`.
- New migration-stack first-party files reviewed as implemented:
  - `laravel-backend/app/Models/IntakeRecord.php`
  - `laravel-backend/app/Http/Controllers/Api/IntakeController.php`
  - `laravel-backend/app/Http/Middleware/RequirePermission.php`
  - `laravel-backend/database/migrations/2026_10_17_010001_add_role_and_permissions_to_users_table.php`
  - `laravel-backend/database/migrations/2026_10_17_010101_create_intake_records_table.php`
  - `laravel-backend/tests/Feature/IntakeApiTest.php`

Residual backlog is now mainly root-level legacy docs/SQL and remaining first-party migration docs needing final disposition normalization.
