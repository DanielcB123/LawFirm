# Cursor Prompt (Strict v2): Implement SaaS-First Root With Tenant Hosts (Option B)

Use this prompt exactly as written. This is an implementation runbook, not a brainstorming prompt.

## 0) Mission (do not reinterpret)

Implement Option B host architecture:

- `www.<saas-domain>` (or local root host) = SaaS marketing + onboarding acquisition
- `app.<saas-domain>` = shared sign-in/discovery shell
- `<firm>.<saas-domain>` and verified custom domains = tenant public site + tenant workspace entry
- Every firm must keep a canonical fallback platform subdomain even when custom domain is live

Keep one shared Laravel backend and one shared workspace codebase. Keep one schema/database per firm. Do not introduce a shared `tenant_id` architecture.

---

## 1) Hard safety and execution rules (must pass before any edits)

Run these commands first:

1. `git status --short --branch`
2. `git branch --show-current`
3. inspect tracked diff + untracked files

### Preflight pass criteria (all required)

- [ ] Current branch is `feature/firm-tenant-schema-foundation`
- [ ] Existing uncommitted work is preserved and understood
- [ ] No destructive git actions are planned (`reset`, `clean`, `stash`, forced checkout)

### STOP-IF-FAIL gate

If branch is not `feature/firm-tenant-schema-foundation`, stop immediately and report:

1. current branch
2. `git status --short --branch` output summary
3. why work cannot continue without user direction

Do not switch branches automatically.

---

## 2) Non-negotiable architecture invariants

These are hard constraints. Do not violate any.

1. Tenant selection in production comes from server-verified host mapping only.
2. Never trust caller-supplied `X-Firm-Id`, query tenant selectors, route slug tenant selectors, or browser storage as tenant authority.
3. Domain ownership proof is separate from DNS routing readiness and TLS readiness.
4. Domain validity does not grant membership.
5. Membership in Firm A never grants Firm B access.
6. Unknown/unverified/suspended/mismatched hosts fail closed without tenant enumeration.
7. Platform-admin actions remain platform-admin-only.
8. Keep runtime tenant context/connection reset guarantees on requests, jobs, and exception paths.

---

## 3) Required document audit before design changes

Read and reconcile at minimum:

- `AGENTS.md`
- `ProgressPlan.md`
- latest relevant `SLICE_HISTORY.md` entries
- `Markdown/README.md`
- `Markdown/architecture/README.md`
- `Markdown/architecture/firm-tenant-schema-foundation.md`
- `Markdown/delivery/implementation-gap-map.md`
- `Markdown/delivery/multitenant-acceptance-matrix.md`
- `Markdown/operations/docker-and-laravel-command-runbook.md`
- `Markdown/planning/decision-log.md`
- `Markdown/planning/open-decisions.md`

### Audit checklist

- [ ] Verified docs match current repository behavior
- [ ] Conflicts between docs and code are listed explicitly
- [ ] Existing tenancy/domain/public-site patterns are inventory-complete

### STOP-IF-FAIL gate

If evidence conflicts with this prompt, stop and produce:

1. conflict list (prompt claim vs repository evidence)
2. smallest safe implementation adjustment
3. explicit risk of proceeding without adjustment

Proceed only after documenting the conflict resolution approach.

---

## 4) Phased implementation plan with hard gates

Execute in order. Do not skip a gate.

## Phase 1 - Host surface separation

Implement explicit host responsibilities:

- root host -> SaaS platform shell
- app host -> shared sign-in/discovery
- tenant hosts -> tenant public/workspace entry

### Phase 1 checklist

- [ ] Local root host no longer defaults to arbitrary tenant public site
- [ ] Explicit platform IA routes exist (onboarding CTA + firm sign-in guidance)
- [ ] Tenant rendering still requires verified host mapping
- [ ] Canonical tenant fallback URL model remains active

### STOP-IF-FAIL gate

Stop if any of these occur:

- root host can reach tenant data without tenant host resolution
- tenant host behavior depends on non-host tenant selector in production path
- fallback tenant URL removed or no longer resolvable

If stopped, report failing behavior and touched files before continuing.

## Phase 2 - Discovery shell and multi-membership flow

Implement membership-aware post-auth flow on app host.

### Phase 2 checklist

- [ ] Shared sign-in authenticates identity without pre-authorizing a tenant
- [ ] One-membership users route only to that authorized firm host
- [ ] Multi-membership users are forced through explicit firm selection
- [ ] Selection is server-authorized and redirect-safe
- [ ] No client-side tenant claim can bypass membership checks

### STOP-IF-FAIL gate

Stop if:

- a user can deep-link into an unauthorized tenant host after login
- membership enforcement differs across API and workspace pages

## Phase 3 - Domain ownership + host resolution hardening

Complete/validate TXT verification lifecycle and host-only production resolution.

### Phase 3 checklist

- [ ] Host normalization handles case, trailing dot, IDN/punycode, malformed input
- [ ] Reserved hosts cannot be tenant-claimed
- [ ] Global hostname uniqueness is enforced
- [ ] TXT challenge uses random, expiring, single-use token
- [ ] Stored challenge representation is non-reusable in plaintext form
- [ ] Reissue/revoke/reverify lifecycle is auditable and safe
- [ ] Verified ownership is distinct from routing/TLS readiness
- [ ] Unknown/unverified hosts fail closed

### STOP-IF-FAIL gate

Stop if:

- spoofed host headers can influence tenant selection
- unverified domain can resolve to tenant context
- one hostname can be linked to multiple firms

## Phase 4 - Public-site extension safety model

Preserve one backend while allowing distinct firm public sites.

### Phase 4 checklist

- [ ] `public_site_key` (or equivalent) is validated server-side
- [ ] At least one neutral site and one materially distinct built-in site are supported
- [ ] Missing/invalid key fails to neutral or unavailable page (never another tenant site)
- [ ] Shared auth, tenancy routing, and workspace APIs are not forked per site
- [ ] Unsafe script/HTML/runtime code injection is rejected

### STOP-IF-FAIL gate

Stop if:

- invalid key can render another firm's branding/data
- custom site path can bypass shared auth/tenant enforcement

## Phase 5 - Onboarding lifecycle and activation gates

Ensure firm activation is explicit and state-gated.

### Phase 5 checklist

- [ ] Prospect/onboarding lead is distinct from active tenant state
- [ ] Human review outcome is recorded (actor/time/reason/evidence reference)
- [ ] Provisioning state is explicit and retry-safe
- [ ] Domain ownership verified before tenant routing is enabled
- [ ] Initial firm admin membership is explicit
- [ ] Suspend/reactivate states enforce access consequences
- [ ] Audit trail exists for all privileged transitions

### STOP-IF-FAIL gate

Stop if:

- admin form submission directly grants active tenant access without full gates
- provisioning failure can still mark tenant/domain usable

## Phase 6 - Security and runtime boundary verification

### Phase 6 checklist

- [ ] Tenant context always resets after requests/jobs/exceptions
- [ ] No tenant secrets/challenge secrets leak in API payloads/logs/docs
- [ ] Platform-admin routes are isolated and protected
- [ ] Firm admins cannot perform platform-admin transitions

### STOP-IF-FAIL gate

Stop if any cross-tenant read/write path succeeds in test or manual verification.

---

## 5) Testing protocol (must be evidence-backed)

Extend existing tenancy tests; do not invent parallel frameworks.

Target files:

- `tests/Feature/FirmTenancyTest.php`
- `tests/Feature/TenantMysqlProvisioningTest.php`
- additional focused host-surface tests as required

### Mandatory test checklist

- [ ] Root host resolves SaaS shell, not tenant public shell
- [ ] Verified tenant hosts resolve only their own active firm
- [ ] Reserved host claim attempts are rejected
- [ ] Host normalization/uniqueness edge cases are covered
- [ ] Unverified/pending/revoked domains cannot route tenant data
- [ ] Membership checks are independent of domain validity
- [ ] Multi-membership selection cannot escalate into unauthorized tenant
- [ ] Canonical fallback tenant URL remains operational
- [ ] Cross-tenant data isolation remains intact after host-surface changes

### Required command evidence

Run and report exact commands actually executed, for example:

- `docker compose config` (if available)
- `php artisan test --compact tests/Feature/FirmTenancyTest.php tests/Feature/TenantMysqlProvisioningTest.php`
- `vendor/bin/pint --dirty --format agent`
- `npm run build` in `vue-frontend`

Never claim checks that were not run.

### STOP-IF-FAIL gate

If a required test fails:

1. stop adding new features,
2. fix failure or document blocker,
3. rerun affected checks,
4. continue only after pass or explicit blocker report.

---

## 6) Documentation completion protocol

Update only authoritative sources impacted by implemented behavior:

- `Markdown/architecture/firm-tenant-schema-foundation.md`
- affected architecture/delivery/operations/planning docs
- `ProgressPlan.md`
- `SLICE_HISTORY.md`
- `Markdown/planning/decision-log.md`

### Documentation checklist

- [ ] Architecture reflects actual final behavior (not intended behavior)
- [ ] Gap map and acceptance matrix reflect closed/open items accurately
- [ ] Runbook includes local host simulation and DNS/TLS go-live distinctions
- [ ] Risks/blockers are explicit and non-marketing
- [ ] No claim of production readiness without verified controls

### STOP-IF-FAIL gate

Stop if documentation and code disagree on:

- tenant resolution source of truth,
- activation gates,
- credential isolation level,
- test status.

Resolve disagreement before completion.

---

## 7) Prohibited actions

Do not:

1. create/switch branches automatically
2. commit, push, or open PR
3. run destructive production migrations
4. claim browser verification unless browser automation/manual browser checks were actually run
5. claim per-tenant DB isolation is complete if shared runtime fallback remains enabled for production

---

## 8) Final completion report template (copy exactly)

Use this exact structure in the final response:

1. **Branch and safety checks**
   - current branch
   - working-tree handling confirmation
   - confirmation no destructive git ops were run
2. **Host-surface implementation**
   - behavior of root host, app host, tenant hosts, fallback URL
3. **Domain verification and host resolution**
   - normalization, uniqueness, TXT lifecycle, routing gates
4. **Membership and authorization behavior**
   - single/multi-membership flow and enforcement boundaries
5. **Public-site extension model**
   - how site keys map, failure behavior, safe extension path
6. **Files changed**
   - exact file list
7. **Checks run and results**
   - exact commands + pass/fail
8. **Open blockers and risks**
   - production blockers, deferred items, legal/ops decisions needed
9. **Integrity confirmation**
   - no commit/push/reset/stash/clean/destructive operations performed

---

## 9) Definition of done (strict)

This prompt is done only when all are true:

- [ ] All phase gates passed or explicitly blocked with evidence
- [ ] Required tests executed with truthful results
- [ ] Docs updated to match real implementation
- [ ] Security invariants preserved
- [ ] Final report delivered in required format

If any item is false, status is **not done**.

