# Cursor Prompt (Strict v3): PlatformOwner vs PlatformOps Security Split

Use this prompt exactly as written. This is an implementation runbook, not a brainstorming prompt.

## 0) Mission

Implement a secure platform-role split in the existing multi-tenant SaaS:

- `PlatformOwner`: full control-plane authority.
- `PlatformOps`: limited control-plane authority for operations, **without** lifecycle/provisioning/admin escalation.

This split must be server-enforced with least privilege and explicit deny paths. UI visibility controls are helpful but never sufficient.

## 1) Hard safety rules

1. Run and capture:
   - `git status --short --branch`
   - `git branch --show-current`
2. Preserve all existing uncommitted work.
3. Do not switch branches, reset, stash, clean, or discard files.
4. No commits/pushes/PRs.

## 2) Non-negotiable security constraints

1. Tenant isolation remains unchanged and fail-closed.
2. Platform role checks are independent from tenant membership checks.
3. `PlatformOps` must never be able to:
   - finalize approvals/rejections,
   - provision/reprovision schemas,
   - suspend/reactivate firms,
   - grant/revoke platform-owner privileges.
4. All sensitive transitions must stay auditable.

## 3) Required implementation slices

## Slice A: Data model and compatibility

- Add `users.platform_role` with values:
  - `platform_owner`
  - `platform_ops`
  - `null`
- Keep backward compatibility for existing `is_platform_administrator` flag during migration.
- Backfill strategy:
  - existing admin flag true and null role -> default owner behavior (compatibility path).

### Gate A (stop-if-fail)
- Stop if migration can orphan existing platform access unexpectedly.

## Slice B: Permission model

- Add explicit platform permission config (owner vs ops).
- Add server-side helpers on `User`:
  - role predicates,
  - `hasPlatformPermission(permission)`.
- Include compatibility fallback for existing admin-flag users with no explicit role.

### Gate B (stop-if-fail)
- Stop if any platform action still depends only on a boolean admin flag.

## Slice C: Middleware and policy enforcement

- Keep/adjust platform-admin-access middleware to require platform role/access.
- Add platform-permission middleware for action-level control.
- Update `FirmPolicy` to action-specific platform permissions.
- Split domain actions if needed:
  - domain challenge management (ops allowed),
  - domain verify/revoke finalize (owner only).

### Gate C (stop-if-fail)
- Stop if `PlatformOps` can execute owner-only endpoints through any route.

## Slice D: Routes and controllers

- Apply route-level platform permission middleware to `/api/platform/*`.
- Ensure each endpoint has a clear permission boundary.
- Ensure index/list routes use read/view policies (not create policy checks).
- Keep sanitized errors for forbidden access (403).

### Gate D (stop-if-fail)
- Stop if any platform endpoint lacks explicit permission mapping.

## Slice E: Auth payload and frontend role awareness

- Return `platformRole` + `platformPermissions` in auth identity payload.
- Update frontend types/session handling.
- Keep platform-admin dashboard routing behavior aligned with role.
- Hide owner-only actions in UI for ops users (defense-in-depth; server remains source of truth).

### Gate E (stop-if-fail)
- Stop if frontend can present an owner action without server deny handling.

## Slice F: Tests

Extend existing feature tests (no redundant framework):

1. Owner can perform owner-only actions.
2. Ops is forbidden from owner-only actions.
3. Ops can perform allowed operational actions.
4. Non-platform users forbidden on platform routes.
5. Tenant isolation tests remain passing after role split.

Required checks:

- `php artisan test --compact tests/Feature/FirmTenancyTest.php tests/Feature/TenantMysqlProvisioningTest.php`
- `vendor/bin/pint --dirty --format agent`
- `npm run build` in `vue-frontend`

### Gate F (stop-if-fail)
- Stop on any failing required check; fix or document blocker before proceeding.

## 4) Suggested permission map

- `platform.firms.read` (owner, ops)
- `platform.firms.create` (owner, ops)
- `platform.firms.memberships.manage` (owner, ops)
- `platform.firms.domains.manage` (owner, ops)
- `platform.firms.branding.manage` (owner, ops)
- `platform.firms.verifications.record` (owner, ops)
- `platform.firms.review.finalize` (owner only)
- `platform.firms.lifecycle.manage` (owner only)
- `platform.firms.provision` (owner only)
- `platform.admins.manage` (owner only; future endpoints)

## 5) Documentation updates required

- Update architecture documentation for role split and boundaries.
- Update runbook/planning logs:
  - `ProgressPlan.md`
  - `SLICE_HISTORY.md`
  - `Markdown/planning/decision-log.md`

Claims must match executed evidence only.

## 6) Final report format

1. Branch and safety checks
2. Platform role/permission model implemented
3. Route/policy enforcement map
4. Owner vs ops behavior outcomes
5. Files changed
6. Checks run and results
7. Remaining risks/blockers
8. Integrity confirmation (no destructive git ops, no commit/push)

