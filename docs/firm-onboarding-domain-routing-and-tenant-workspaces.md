# Firm Onboarding, Domain Routing, and Tenant Workspaces

## Scope of this slice

This implementation extends the branch `feature/firm-tenant-schema-foundation` to provide an operational onboarding path with explicit lifecycle controls, DNS TXT domain ownership verification, host-based tenant routing, tenant-scoped workspace data, and a platform-admin UI page.

This is still **not production-ready** because local fallback to a shared runtime DB account can still be enabled when MySQL user-management privileges are not available.

## What changed

### Platform onboarding workflow

- Added platform-admin list/query endpoint: `GET /api/platform/firms`.
- Added lifecycle transition endpoint: `POST /api/platform/firms/{firm}/reactivate`.
- Added Vue platform-admin page: `GET /workspace/platform/firms` (in-app route).
- Existing onboarding actions are now centralized in that page:
  - create pending firm,
  - approve/reject review,
  - provision tenant schema,
  - suspend/reactivate,
  - claim/verify/revoke domains,
  - save public branding/site key,
  - attach membership.

### Domain ownership verification and routing

- Domain input now uses strict normalization (`HostnameNormalizer`):
  - lowercase,
  - trailing-dot trimming,
  - IDNA ASCII conversion,
  - rejects credentials/paths/query/ports and malformed hostnames,
  - enforces reserved-hostname denial.
- Domain claims no longer auto-verify.
- Claiming a domain now issues a single-use, expiring DNS TXT challenge:
  - record name: `_mf-domain-challenge.<hostname>`
  - value: random `mfv1_...` token
  - stored as SHA-256 hash with expiry.
- Verification is server-side DNS TXT lookup only.
- Added endpoints:
  - `POST /api/platform/firms/{firm}/domains`
  - `POST /api/platform/firms/{firm}/domains/{domain}/reissue-challenge`
  - `POST /api/platform/firms/{firm}/domains/{domain}/verify`
  - `POST /api/platform/firms/{firm}/domains/{domain}/revoke`
- Host routing now requires verified and non-revoked domain rows.
- Public consultation endpoints now require host-resolved tenant context and fail safe (`404`) on unknown/unverified hosts.

### Public-site extension model

- Added explicit public-site key support (`public_site_key`) in branding/profile data.
- Allow-listed site keys:
  - `default_classic`
  - `modern_feature`
- Frontend extension registry is typed in `src/firms/extensions.ts`.
- Branding payload now includes `publicSiteKey`; frontend applies variant shell class.
- Variant behavior is code-owned; runtime branding cannot inject scripts/components.

### Tenant workspace data isolation

- Tenant migrations now include:
  - `contacts`
  - `intake_records`
  - `consultation_requests`
  - `matter_records`
  - `matter_tasks`
  - `calendar_entries`
- Workspace models now inherit `TenantModel` (fail-closed without tenant context).
- Workspace API routes moved under tenant middleware (`/api/tenant/...`) with auth and permissions.
- Public consultation create/slot routes use host-only tenant middleware.

### Runtime DB credential model

- Added control-plane credential store: `firm_database_credentials`.
- Provisioning now attempts per-firm runtime DB user creation/grants.
- Credentials are stored encrypted at rest.
- Runtime tenant connection establishes with the firm credential (when available).
- Local fallback can store/use a shared runtime account if user provisioning is blocked.

## Data boundaries

### Control plane (shared)

- users and platform identity state
- firms and lifecycle/provisioning state
- domain claim/verification metadata
- memberships
- public profile configuration
- provisioning events
- tenant DB credential metadata

### Tenant schema (per firm)

- firm identity marker
- workspace notes
- contacts
- intake records
- consultation requests (post host resolution)
- matters and tasks
- calendar entries

## Migration and backfill flow

### Command added

`php artisan tenants:backfill-workspace --firm=<firm-uuid> [--map-all-to-firm] [--apply]`

Behavior:

- Default mode is dry run (report only).
- Fails closed without explicit `--map-all-to-firm` mapping acknowledgement.
- `--apply` performs idempotent upserts into the selected tenant schema.
- Does not drop legacy shared tables.

## Docker and migration commands used in this slice

Commands executed in this implementation:

- `php artisan test --compact tests/Feature/FirmTenancyTest.php tests/Feature/TenantMysqlProvisioningTest.php`
- `npm run build` (in `vue-frontend`)
- `vendor/bin/pint app database routes tests bootstrap --format agent`

## DNS and go-live checklist (manual)

1. Claim domain from platform admin.
2. Publish TXT record:
   - host: `_mf-domain-challenge.<domain>`
   - value: challenge token returned by API/UI.
3. Wait for DNS propagation and run verify action.
4. Configure routing records (A/AAAA/CNAME) for web/API paths.
5. Provision and validate TLS certificate separately.
6. Confirm firm lifecycle is active and tenant provisioning is ready.
7. Confirm first firm-admin membership exists.
8. Confirm domain renders the intended public site key/layout.
9. Confirm sign-in on that host only reaches authorized firm tenant data.

TXT verification proves control of DNS only. It does **not** by itself prove traffic routing readiness or TLS readiness.

## Local verification guidance (two hostnames)

For local manual checks, map two hostnames to localhost in hosts file and create two firm-domain claims/verification rows for those hostnames. Then:

- host A serves firm A branding/site variant,
- host B serves firm B branding/site variant,
- authenticated user on host A cannot access host B tenant APIs/data.

Do not use this local hosts-file mechanism as a production trust model.

## Security controls in this slice

- Server-side domain normalization and reserved-host checks.
- DNS TXT challenge hash storage and expiry.
- Explicit verify/revoke lifecycle for domains.
- Tenant routing only from verified, active domain + valid lifecycle.
- Tenant models fail closed without context.
- Tenant context cleared on request end and queue loop boundaries.
- Platform-admin endpoints protected by auth + platform-admin middleware + policy checks.
- Public branding payload restricts exposed fields and site keys.

## Known limitations and deferred work

- Production must disable shared-runtime fallback and enforce per-firm DB user provisioning.
- Legacy shared workspace rows still exist until reconciled migration/cutover cleanup release.
- Platform-admin UI is minimal and operational; it is not a polished final product.
- No automated DNS provider or certificate automation is included.
- No MFA, no CSRF-cookie auth migration, and no login throttling in this slice.

## 2026-10-22 self-service onboarding additions

- Added applicant-facing onboarding API flow:
  - guest registration + firm application: `POST /api/onboarding/register`
  - authenticated application submission: `POST /api/onboarding/applications`
  - applicant status endpoint: `GET /api/onboarding/applications/current`
  - applicant revision update endpoint: `PUT /api/onboarding/applications/current`
- Added applicant local fast-track endpoint:
  - `POST /api/onboarding/applications/current/fast-track`
  - guarded by server-side config and environment checks only (`FIRM_ONBOARDING_DEV_FAST_TRACK=true` and `APP_ENV in [local, testing]`).
- Added local/testing fast-track command:
  - `php artisan onboarding:fast-track --firm="..." --email="..."`
- Added explicit onboarding verification and two-reviewer control-plane records:
  - `firm_onboarding_applications`
  - `firm_onboarding_verifications`
  - `firm_onboarding_reviews`
  - plus `firms.applicant_user_id` and `firms.onboarding_revision`.
- Added platform endpoint to record manual/provider verification outcomes:
  - `POST /api/platform/firms/{firm}/verifications`
  - provider mode fails closed when provider config is missing.
- Added frontend pages/routes:
  - `/register-firm`
  - `/onboarding/status`
  - plus platform-surface nav CTA for "Register your firm."

Normal path remains verification-gated and reviewer-gated. Fast-track does not fabricate reviewer approvals and does not mark `users.email_verified_at`.

