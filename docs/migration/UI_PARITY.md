# UI Parity Baseline

## Objective

Preserve current visual design, layout, and branding while migrating to Vue 3.

## Current evidence captured

- Source route map reviewed in `frontend/src/app/AppRoutes.tsx`.
- Shared shell and global visual behavior reviewed in `frontend/src/app/AppShell.tsx`.
- Existing styling system confirmed in:
  - `frontend/src/app/AppShell.css`
  - `frontend/src/index.css`
- Public and workspace page sets identified.

## Screenshot baseline status

- Public-route screenshot baseline captured with Playwright at:
  - `docs/migration/screenshots/public-home-desktop.png`
  - `docs/migration/screenshots/public-about-desktop.png`
  - `docs/migration/screenshots/public-practice-desktop.png`
  - `docs/migration/screenshots/public-attorneys-desktop.png`
  - `docs/migration/screenshots/public-contact-desktop.png`
  - `docs/migration/screenshots/public-home-mobile.png`
  - `docs/migration/screenshots/public-about-mobile.png`
  - `docs/migration/screenshots/public-practice-mobile.png`
  - `docs/migration/screenshots/public-attorneys-mobile.png`
  - `docs/migration/screenshots/public-contact-mobile.png`
- Workspace-route screenshot baseline is still pending live authenticated capture.
  - Current blocker is reproducible: legacy backend startup fails before serving due MySQL auth error (`Access denied for user 'lawfirm_app'@... (using password: NO)`), so authenticated workspace pages cannot yet be exercised live.

## Planned parity capture procedure

1. Use synthetic/demo data only.
2. Capture desktop and mobile viewport screenshots for:
   - public pages,
   - auth/login,
   - key workspace modules (contacts, intake, matters, calendar, consultations).
3. Record route, viewport, and state for each image.
4. Compare Vue implementation against baseline before each feature is marked verified.

## Capture notes

- Initial mobile capture attempt via `--device="iPhone 13"` failed due missing WebKit runtime; resolved by using Chromium with explicit mobile viewport `390x844`.
- Public routes can be captured without backend runtime dependency.
- Authenticated workspace views require a running backend + login session and will be captured in a later baseline pass.
- Public baseline remains valid; workspace baseline currently relies on source-level route/component audit until backend credentials are corrected.

## Non-negotiable parity targets

- Navigation structure.
- Core layout spacing and hierarchy.
- Typography, color intent, and branding mood.
- Table/forms/dialog interactions.
- Loading, empty, success, and error states.
- Responsive behavior at representative breakpoints.
