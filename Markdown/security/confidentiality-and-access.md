# Confidentiality, ethical walls, and permissions

[Home](../README.md) · [Portal](../modules/client-portal.md) · [Sources](../research/sources.md)

> **Current implementation (audited 2026-10-12):** Role and permission claims only. No matter grants, ethical walls, or audience model. Restricted parties are masked in conflict search but **not** in `/api/parties`. See G10–G15 in the [gap map](../delivery/implementation-gap-map.md).
>
> **Update 2026-10-12 (F1, uncommitted):** matter grants (Staff and Client, revocable) and server-side assignment checks now exist. Restricted parties are hidden in `/api/parties`, protected on alias and relationship writes, and masked with opaque ids in conflicts. All of this is verified on MySQL for a single firm ([matter access policy](matter-access-policy.md)). Ethical walls, an audience model, and intake scoping are still absent (G14, G15).

Texas Rule 1.05 addresses confidentiality and Rule 1.18 addresses prospective clients in the current linked rules [S12]. Confidentiality protection and attorney-client privilege are related but not identical; software labels do not establish privilege. Protect prospective-client records too.

## Access decision
An action requires an active identity, active firm membership or authorized portal grant, an allowed role/action, matter access, document/message audience permission, and any extra ethical-wall/classification constraint. Default deny. Firm membership alone does not grant every matter. Organization employees and family members do not automatically gain client authority.

| Role | Default access posture |
|---|---|
| Client | Specifically granted matters and published audience content |
| Representative | Explicit delegated scope and expiry |
| Attorney/paralegal | Assigned or expressly authorized matters, subject to walls |
| Finance | Financial records needed for duties; no blanket narrative access |
| Firm admin | Membership/configuration duties; matter access remains policy controlled |
| Platform support | Health metadata; approved, time-limited access when necessary |
| Investor | No legal records by default |

Implement ethical-wall membership and denial rules at server/query boundaries, including conflicts searching, previews, exports, analytics, notifications, and background work. Record policy precedence so a generic admin role cannot accidentally override a wall.

## Support access
Use requested scope, firm authorization under agreed support policy, reason, expiry, step-up authentication, complete audit, and post-access review. Do not add silent impersonation. Emergency access needs an explicit policy and accountability. The browser's “view as client” feature must use a controlled server path rather than simply hiding fields in the frontend.

## Retention and holds
Maintain per-record-class retention with governing source, jurisdiction, trigger, review, and deletion evidence. Legal holds block expiration/deletion across originals, derived files, search, and exports. Financial and matter records may have different schedules. Restore workflows must not erase holds or access revocations. Clarify backup expiration and residual copies in approved policy.

## Research and AI
Legal research entitlements do not imply redistribution rights. Keep firm/matter/audience filters in retrieval and recheck sources before generation. No cross-firm training or global confidential embeddings. Treat retrieved documents as untrusted instructions. Require citations, source access checks, and attorney review for draft legal work or client updates. AI is a later, separately gated capability.
