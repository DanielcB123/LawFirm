# SaaS firm onboarding runbook

[Home](../README.md) · [Operations](reliability-and-recovery.md) · [Fleet provisioning](fleet-provisioning-and-migration-runbook.md) · [Provisioning baseline](../database/provisioning-and-migrations.md) · [Open decisions](../planning/open-decisions.md)

## Why this exists
This runbook is for onboarding a **new law firm tenant** to the SaaS platform (not a new legal client record inside an existing firm). It defines the minimum repeatable process, hard-stop gates, and evidence package required before a firm can go live.

## Current state warning
As of this date, core multi-firm runtime controls are still in design/implementation phases. This runbook is the operating standard to follow as those controls are implemented. If a required gate cannot be satisfied, onboarding must stop and remain in pre-production status.

## Roles and ownership
- **Platform owner:** approves onboarding start and go-live.
- **Implementation lead:** runs technical provisioning and validation.
- **Security owner:** validates access model, credentials, and controls.
- **Data/privacy owner:** validates contractual and policy prerequisites.
- **Firm admin sponsor:** confirms firm configuration and acceptance.

No one person should both provision and independently sign off all security checks.

## Onboarding lifecycle

### Phase 0: Intake and qualification (required before any provisioning)
1. Confirm legal entity name, billing contact, and designated firm admin(s).
2. Confirm contract status, data processing terms, and incident contacts.
3. Confirm approved hosting region/data residency expectations.
4. Confirm firm timezone(s), business hours, and initial practice areas.
5. Confirm whether migration of existing firm data is in scope or out of scope.

**Exit gate:** signed onboarding packet with named approvers.

### Phase 1: Security and configuration prerequisites
1. Generate onboarding operation id and tracking ticket.
2. Prepare firm slug and proposed domain mappings.
3. Generate least-privilege secret references (never commit clear-text secrets).
4. Define initial role/membership matrix for firm admins and staff.
5. Confirm required controls that must be active before go-live:
   - trusted firm resolution,
   - firm context routing,
   - membership enforcement,
   - audit capture,
   - recovery path evidence.

**Exit gate:** prerequisites checklist approved by platform and security owners.

### Phase 2: Technical provisioning
Use the operational sequence in `fleet-provisioning-and-migration-runbook.md`:
1. Create firm catalog entry and placement.
2. Create firm database and migration/runtime accounts.
3. Apply schema migrations and runtime grants.
4. Initialize `firm_identity`.
5. Configure firm-scoped object storage prefix and access scope.
6. Configure and verify firm domain.
7. Create first admin invitation and activation path.
8. Mark firm active only after verification probes pass.

**Exit gate:** provisioning operation is complete with no unresolved failed step.

### Phase 3: Validation and acceptance testing
Run and archive evidence for:
1. **Routing/isolation checks:** unknown host fails closed; no cross-firm data access.
2. **Authorization checks:** firm admin/staff/client permissions align to policy.
3. **Operational checks:** health/ready endpoints, migration state, logs/metrics visibility.
4. **Recovery checks:** backup metadata exists and restore rehearsal status is documented.
5. **Functional smoke checks:** sign-in, core workflow entrypoints, and error handling.

**Exit gate:** acceptance matrix shows required onboarding tests as pass.

### Phase 4: Go-live
1. Confirm go-live window and support coverage.
2. Enable production routing for approved domain(s).
3. Reconfirm admin access and emergency contacts.
4. Publish onboarding completion record and artifact bundle.

**Exit gate:** platform owner and firm admin sponsor approve cutover.

### Phase 5: Post-go-live stabilization (first 14 days)
1. Monitor error rate, auth failures, and routing anomalies daily.
2. Review audit-event health and alert quality.
3. Confirm no unresolved critical onboarding defects.
4. Hold stabilization review with firm admin sponsor.

**Exit gate:** stabilization review signed and archived.

## Hard-stop conditions
Stop onboarding immediately if any of these occur:
- Trusted firm resolution is bypassable or inconsistent.
- Cross-firm data leakage is observed or cannot be ruled out.
- Required secrets are missing, exposed, or unmanaged.
- Migration drift or grant drift is detected and unresolved.
- Recovery evidence is missing for the target environment.
- Critical security findings are open without accepted risk sign-off.

## Required evidence package per firm
Store one onboarding record bundle containing:
1. Onboarding intake packet and approvals.
2. Provisioning operation log and step outcomes.
3. Configuration manifest (redacted, no raw secrets).
4. Test evidence for routing, auth, and functional smoke checks.
5. Recovery/backup evidence statement.
6. Go-live approval record and stabilization notes.

## Quick execution checklist
- [ ] Intake and legal packet approved.
- [ ] Security prerequisites approved.
- [ ] Firm catalog, placement, DB, and identity initialized.
- [ ] Domain verified and routing checks pass.
- [ ] Isolation and authorization tests pass.
- [ ] Recovery evidence attached.
- [ ] Go-live approvals signed.
- [ ] Stabilization review completed.

## Known blockers right now
Before this runbook can be fully executed for production onboarding, these platform gaps must be closed in code/runtime:
- `G01` and `G02` (control plane plus trusted per-request firm context),
- `G03` and `G04` (persistent identity/membership and hardened auth),
- `G43` (outbox/worker reliability),
- `G60` through `G62` (recovery, observability, and deployment operations).

Track status in `Markdown/delivery/implementation-gap-map.md` and `ProgressPlan.md`.
