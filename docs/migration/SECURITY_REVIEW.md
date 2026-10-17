# Security Review (Migration Baseline)

## Scope in this session

- Baseline risk posture from current implementation and startup behavior.
- Migration-sensitive security controls to preserve or improve.

## Current observations

- Current app enforces JWT-based auth and policy checks.
- Matter-level access logic is centralized and test-covered in current stack.
- Demo users are hardcoded and intentionally development-only when enabled.
- Startup fails fast without DB connectivity (no silent fallback DB).
- Config files do not contain active secrets in tracked values.

## Risks requiring explicit migration handling

1. **Authorization regression risk**
   - Matter-level authorization must be reproduced server-side in Laravel.
2. **Tenant isolation gap**
   - Current implementation is single-tenant; true multi-tenant isolation not implemented.
3. **Placeholder module ambiguity**
   - Some UI/pages imply capabilities (billing/accounts/integrations) that are not fully backed by domain logic.
4. **Public endpoint abuse controls**
   - Existing rate limiting patterns must be preserved/improved in Laravel.
5. **Audit consistency**
   - Audit write guarantees and immutability need explicit implementation plan.

## Migration security requirements

- Re-implement authorization with policies/gates and integration tests.
- Enforce least-privilege data exposure in API resources.
- Preserve or improve rate limiting on public write endpoints.
- Validate request payloads via Form Requests.
- Prevent mass-assignment vulnerabilities in Eloquent models.
- Apply CSRF/session controls for cookie-based flows (if used).
- Ensure uploads are validated and stored safely when file pipeline is migrated.
- Prevent secrets from being embedded in images or tracked files.

## Verification requirements (to be completed)

- Unauthorized read/write regression tests for major resources.
- Matter-scope bypass attempts rejected.
- Cross-user data access denied.
- Sensitive fields omitted from unauthorized contexts.
- Security logs avoid exposing sensitive payload data.
