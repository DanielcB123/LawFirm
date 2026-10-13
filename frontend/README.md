# MatterForge frontend

React 19 + TypeScript + Vite 8 single-page app: the public firm website and the staff workspace. Folder layout and conventions: [solution structure](../Markdown/architecture/solution-structure.md#frontend-frontendsrc).

## Commands

| Command | What it does |
|---|---|
| `npm install` | Install dependencies |
| `npm run dev` | Dev server; proxies `/api` to `http://localhost:5073` (see `vite.config.ts`) |
| `npm run build` | Type-check (`tsc -b`) and production build |
| `npm run lint` | ESLint, including the feature-boundary import rule |
| `npm test` | Vitest, run once (`npm run test:watch` to watch) |

## Configuration

Copy `.env.example` to `.env.local` to override settings. `VITE_API_BASE_URL` is empty by default, so requests use same-origin `/api` paths. `VITE_` variables are bundled into the browser, so never put secrets in them.
