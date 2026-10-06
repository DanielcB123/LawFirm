# Authentication and Authorization Strategy (Law Firm)

## Recommendation from market best practices

For law-firm systems that hold privileged communications and case documents, the best practice is:

1. Use **OIDC/OAuth2 with a managed identity provider** (Auth0, Microsoft Entra External ID, Okta, Cognito).
2. Enforce **MFA for staff roles** (attorneys, paralegals, admins).
3. Offer at least one **phishing-resistant factor** (FIDO2/WebAuthn passkeys or security keys), especially for staff access.
4. Keep authorization in the API with **policy-based access control** and explicit permissions claims.

The current implementation in this repo uses local JWT auth for development speed and architecture clarity. For production, keep the same policy model and swap token issuance to a managed IdP.

## Why this approach

- OWASP ASVS v5 guidance emphasizes re-authentication rules, session termination controls, and strong authentication for higher-risk apps.
- NIST SP 800-63B guidance favors AAL2-style MFA and phishing-resistant options for stronger assurance.
- Microsoft ASP.NET Core guidance recommends policy-based authorization with role and claim requirements for maintainable APIs.

## Role and permission model

### Client
- `matters.read.own`
- `documents.upload`
- `billing.read.own`

### Paralegal
- `contacts.read`
- `contacts.manage`
- `conflicts.search`
- `calendar.read`
- `calendar.manage`
- `matters.read.assigned`
- `matters.update.assigned`
- `documents.upload`
- `documents.review`

### Attorney
- `contacts.read`
- `contacts.manage`
- `conflicts.search`
- `conflicts.review`
- `calendar.read`
- `calendar.manage`
- `matters.read.assigned`
- `matters.read.all`
- `matters.update.all`
- `documents.upload`
- `documents.review`
- `billing.manage`

### Admin
- `contacts.read`
- `contacts.manage`
- `conflicts.search`
- `conflicts.review`
- `calendar.read`
- `calendar.manage`
- `matters.read.all`
- `matters.update.all`
- `documents.review`
- `billing.manage`
- `users.manage`

## Operational controls to add before production

- Replace demo users with external IdP or persistent identity store.
- Rotate JWT signing keys using a managed secrets platform.
- Add refresh tokens with revocation and device/session management.
- Add audit logging for sign-in, authorization failures, and sensitive actions.
- Enforce least privilege by matter assignment and tenant/client boundaries.
- Add step-up auth for sensitive actions (billing changes, user role changes, case exports).
