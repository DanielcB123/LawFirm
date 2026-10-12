# Security requirements and verification

[Home](../README.md) · [Confidentiality](confidentiality-and-access.md) · [Payments](../billing/payments-and-ledger.md)

> **Current implementation (audited 2026-10-12):** JWT bearer with issuer, audience, and lifetime validation, plus permission policies (`backend/Infrastructure/Auth/*`). Demo users are active in every environment; secrets are tracked in `appsettings*.json`; there is no MFA, revocation, rate limiting, or security test. Existing middleware and policies are not evidence that a path is secure. See G03–G05 and G10–G16 in the [gap map](../delivery/implementation-gap-map.md).
>
> **Update 2026-10-12 (F1, uncommitted):** matter-level authorization and restricted-party masking are enforced and tested on MySQL for a single firm ([matter access policy](matter-access-policy.md)). Demo users require an explicit Development opt-in. Tracked config holds no secrets; historical values still need rotation. Still absent: MFA, token revocation, rate limiting, ethical walls, cross-firm controls.
>
> **Multi-firm (design only, 2026-10-12):** [threat model and control matrix](multitenant-threat-model.md); [acceptance matrix](../delivery/multitenant-acceptance-matrix.md).

Use a documented threat model and traceable controls informed by OWASP ASVS and its authorization/upload guidance [S15–S17]. Map applicable requirements to actual implementation and evidence; no compliance badge is implied.

## Threats and required controls

| Threat | Required controls | Evidence |
|---|---|---|
| Cross-firm/matter leakage | Trusted context, database grants, server authorization, scoped search/cache/jobs | Negative boundary tests across every surface |
| Account takeover | Staff MFA, secure authentication/recovery, rate limits, session rotation/revocation | Auth and recovery tests; no helpdesk bypass |
| Overbroad staff/operator access | Least privilege, ethical walls, time-limited support grants | Role matrix, access logs, revocation tests |
| Malicious uploads | Quarantine, validation, scanning, safe preview/download | Malware/scan-outage paths |
| Injection/XSS/CSRF | Parameterized queries, output encoding, validation, appropriate CSRF and CSP | Relevant application tests and review |
| Financial fraud | Verified destinations, dual review, immutable postings, reconciliation | Concurrency and approval-change tests |
| Secret leakage | Secrets manager, restricted credentials, redacted telemetry | Repository scanning and log review |
| Ransomware/data loss | Protected backups, isolated recovery, holds, versioning | Restore rehearsal and access separation |
| Provider/AI data exposure | Scoped contracts/credentials, minimized payloads, retention terms | Integration review and denied-access tests |

## Baseline
TLS in transit; encryption at rest; managed key lifecycle; least-privileged infrastructure roles; separate environments; dependency patching and scanning; secret scanning; secure cookies/tokens; restrictive CORS; size/rate limits; authorized exports; audit sensitive access and changes. Avoid unsafe HTML previews and server-side URL fetching without SSRF controls.

Choose authentication consistent with the existing stack and deployment. Cookie authentication needs HttpOnly/Secure/appropriate SameSite and CSRF defenses. Token-based APIs require correct issuer/audience/lifetime validation and safe storage. Authentication does not replace object-level authorization.

## Client data handling
No production narratives, attachments, medical records, financial credentials, or identity details in source code, fixtures, screenshots, analytics, Cursor prompts, or third-party AI tools without an approved process. Use synthetic fixtures. Do not log message bodies or raw provider payloads. Limit audit content while retaining useful access evidence.

## Release gates
Threat model, tenant/matter boundary evidence, dependency/secrets checks, file handling tests, backup restore, financial correctness, and operational ownership before real case data. Significant new integrations or AI features require an updated review. Risks are tracked with owner, mitigation, evidence, and resolution date.
