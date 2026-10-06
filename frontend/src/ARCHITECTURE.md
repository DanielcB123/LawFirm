# Frontend Structure (Minimal, Production-Style)

## Edit Guide

- Page-level content and layout: `src/pages/HomePage.tsx`
- Reusable UI component: `src/components/HealthStatusCard.tsx`
- Feature API call for health endpoint: `src/services/healthApi.ts`
- Shared HTTP helper behavior: `src/services/apiClient.ts`
- App shell wiring: `src/app/AppShell.tsx`

## Data Flow

`HomePage` -> `HealthStatusCard` -> `healthApi` -> `apiClient` -> `/api/health`
