# Reliability, incident handling, and recovery

[Home](../README.md) · [Provisioning](../database/provisioning-and-migrations.md) · [Roadmap](../delivery/roadmap-and-acceptance.md)

> **Current implementation (audited 2026-10-12):** No hosting, backups, monitoring, or runbooks exist in the repository. Health endpoints `/api/health` and `/api/health/ready` exist, but no checks are registered. MySQL restore semantics are in [ADR-001](../database/ADR-001-tenancy.md). See G60–G62 in the [gap map](../delivery/implementation-gap-map.md).

## Operational objectives
Set explicit RPO (acceptable lost-data interval), RTO (recovery time), availability, and notification response targets with the firm and hosting owner. Example planning targets of 15-minute RPO and four-hour RTO are hypotheses, not promises; do not sell them until tested and supported. Budget recovery infrastructure, object versions, and staffed incident response.

Monitor application errors, latency, tenant routing, connection pressure, failed migrations, outbox lag, failed/late notifications, scanner health, appointment conflicts, reconciliation exceptions, and deadline reminder execution. Record correlation IDs and sanitized scope; case names and narratives do not belong in dashboards by default.

## Single-firm restoration
Cloud snapshots may restore an entire instance/cluster even with a database per firm [S8]. Restore into an isolated environment, extract only the selected firm's legal database, verify ownership/counts/checksums, recover corresponding object versions, rebuild search, and reconcile financial events/provider state. Preserve current membership revocations, holds, and routing controls rather than blindly rolling the control plane back.

Freeze affected writes or use an independently proven reconciliation plan. Identify jobs/webhooks already processed after the restore point and prevent replayed double payments/disbursements. Restore ledger data with provider/bank reconciliation, not by assuming a recovered database proves the money balance.

## Backups
Encrypt backups, separate backup permissions from runtime credentials, test restoration, define retention/immutability, and verify object lifecycle does not destroy held or recovery-needed evidence. Database and object backup metadata must allow consistent recovery. A backup-success message is not proof of recovery.

## Incidents
Triage scope and impact; contain access or unsafe writes; preserve evidence; identify affected firms; communicate verified facts and next update time; recover; validate access and finances; review root cause. Keep firm notification obligations and contacts documented; legal disclosure decisions belong to authorized firm/platform counsel.

## Acceptance
Rehearse loss of the first firm's database with objects and posted finances, verify another firm remains unaffected, and measure actual RPO/RTO. Exercise wrong routing, corrupted file, compromised member, provider outage, and failed deployment. Record actions, discrepancies, owner, and follow-up. Runbook ownership is mandatory for production.
