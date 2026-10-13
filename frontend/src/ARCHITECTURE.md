# Frontend structure

The full layout, rules, and the steps for adding a feature are in [Markdown/architecture/solution-structure.md](../../Markdown/architecture/solution-structure.md#frontend-frontendsrc).

## Where to edit

- Routes: `src/app/AppRoutes.tsx`
- Providers (router, auth): `src/app/AppProviders.tsx`
- Layout and navigation: `src/app/AppShell.tsx`, `src/components/layout/`
- A domain's API calls and types: `src/features/<name>/api.ts`, `types.ts`, exported through `index.ts`
- Screens: `src/pages/public/`, `src/pages/workspace/`
- HTTP behavior and error mapping: `src/lib/apiClient.ts`
- Environment variables: `src/config/env.ts`, `frontend/.env.example`

## Data flow

`pages/public/HomePage` → `features/health` (`HealthStatusCard` → `getHealthStatus`) → `lib/apiClient` → `/api/health`
