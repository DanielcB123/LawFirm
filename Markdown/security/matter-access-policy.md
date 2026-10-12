# Matter access policy (single firm)

[Home](../README.md) · [Security](README.md) · [Confidentiality and access](confidentiality-and-access.md) · [Gap map](../delivery/implementation-gap-map.md) · [Codebase audit](../architecture/current-codebase-audit.md)

Status: **Implemented—verified within stated scope** (2026-10-12, uncommitted working tree). Scope is one firm in one database. Nothing here isolates one firm from another; that is the multitenancy work in [ADR-001](../database/ADR-001-tenancy.md).

Enforcement lives in one place: `backend/Infrastructure/Access/MatterAccessService.cs` (`IMatterAccessService`), with uniform denial responses in `MatterAccessResults.cs` and restricted-party rules in `PartyVisibility.cs`. Route policies `MatterStaffRead`, `MatterStaffWrite`, and `ClientMatterPortal` are in `backend/Infrastructure/Auth/AuthServiceCollectionExtensions.cs`. Every matter-owned query starts from `ReadableMatters`, `WritableMatters`, or `ClientMatters`, so filtering happens in SQL rather than after loading.

## Rules

1. **Staff read** a matter when they hold role Attorney, Paralegal, or Admin and either `matters.read.all`, or `matters.read.assigned` plus one of: responsible attorney, assigned paralegal, or an unrevoked `Staff` grant.
2. **Staff write** follows the same rule with `matters.update.all` / `matters.update.assigned`. Writes are checked on the server for every write route, including child resources.
3. **Clients** reach a matter only through an unrevoked `Client` grant, only through `/api/portal/matters/*`, and only with role Client plus `matters.read.own`. The staff routes return 403 to clients regardless of grants.
4. **Grant types do not cross over.** A `Client` grant gives no staff-route access; a `Staff` grant gives no portal access.
5. **Admin is not unrestricted.** Demo Admin accounts now hold `matters.read.assigned` and `matters.update.assigned`, not the firm-wide permissions. An admin sees matter content only when assigned or granted. This is tested.
6. **Attorneys keep firm-wide access** (`matters.read.all`, `matters.update.all`) because `backend/AUTHORIZATION_STRATEGY.md` already assigned it. This is a **pending firm decision**: ethical walls (G15) do not exist yet, so a firm-wide attorney is not walled off from anything.
7. **Grant management.** Only an Attorney who can write the matter and is its responsible attorney, or holds `matters.update.all`, can create or revoke grants. A duplicate active grant returns 409. Revocation is idempotent and keeps the row (revoked time and actor). Create and revoke are audited.
8. **Restricted parties** are visible only with `contacts.read.restricted` (demo Attorney and Admin). For everyone else, restricted parties are absent from the directory, cannot be aliased or linked (404), and appear in conflict results only as a masked match with a fresh random id per result, "Restricted Party", type "Restricted", and a generic reason. The stored conflict snapshot holds the same masked form.

## Denial responses

| Situation | Response |
|---|---|
| No or invalid token | 401 |
| Wrong role or missing permission for the route (for example a client on `/matters-v2`, staff on `/portal/matters`) | 403 from policy, before any data lookup |
| Matter the caller cannot read, on staff or portal routes | 404 `matter_not_found`, identical to a nonexistent id |
| Child resource whose matter the caller cannot read | 404 `matter_not_found` |
| Child id that exists but under a different matter than the route | 404 `matter_task_not_found` / `matter_document_not_found` (lookups filter by both ids) |
| Matter readable but not writable | 403 `matter_write_forbidden` |
| Readable matter, caller not allowed to manage grants | 403 `matter_grant_forbidden` |
| Calendar entry linked to an unreadable matter | 404 `calendar_entry_not_found` |
| Calendar entry linked to a readable, non-writable matter | 403 `matter_write_forbidden` |
| Calendar create/update naming a matter that is missing or not writable | 400 validation error on `matterReference`, identical for both cases |
| Restricted party targeted by alias/relationship without permission | 404 `party_not_found` |

## Role and action matrix (demo permission sets)

"Assigned" means responsible attorney, assigned paralegal, or unrevoked Staff grant. "Granted" for clients means an unrevoked Client grant.

| Action | Client | Paralegal | Attorney | Admin |
|---|---|---|---|---|
| List/read staff matter view and tasks, documents, timeline, grants | 403 | Assigned only | All | Assigned only |
| Create/update/delete tasks, timeline actions | 403 | Assigned only | All | Assigned only |
| Create document metadata (staff route; needs `documents.upload`) | 403 | Assigned only | All | 403 (no `documents.upload`) |
| Review/delete document metadata (needs `documents.review`) | 403 | Assigned only | All | Assigned only |
| Create/revoke access grants | 403 | 403 | Responsible attorney, or holder of `update.all` | 403 |
| Portal: list/read own matters (minimal fields) | Granted only | 403 | 403 | 403 |
| Portal: submit document metadata (no storage key accepted; status Pending) | Granted only | 403 | 403 | 403 |
| Portal: list documents | Own uploads on granted matters | 403 | 403 | 403 |
| Calendar list | Route requires `calendar.read`, which clients lack | Unlinked entries plus entries on readable matters | All | Unlinked plus readable |
| Calendar create/update/delete/acknowledge/complete | — | Unlinked entries or writable matters | All | Unlinked or writable |
| Party directory, restricted entries | — | Hidden | Visible | Visible |
| Conflict search, restricted matches | — | Masked | Full | Full |

## Inventory of matter-owned paths (inspected 2026-10-12)

| Path | Matter data | Enforcement now |
|---|---|---|
| `GET /api/matters-v2` | List | `ReadableMatters` in SQL, then filters, `Take(200)` |
| `GET /api/matters-v2/{id}` | Detail + stage history | `ReadableMatters` |
| `GET/POST /api/matters-v2/{id}/tasks`, `POST/DELETE .../tasks/{taskId}` | Tasks | Read or write check, then lookup by matter id and task id |
| `GET/POST /api/matters-v2/{id}/documents`, `POST .../review`, `DELETE .../{documentId}` | Document metadata | Read or write check plus `documents.*` policy |
| `GET /api/matters-v2/{id}/timeline`, `POST .../timeline-actions` | Timeline | Read or write check |
| `GET/POST /api/matters-v2/{id}/access-grants`, `POST .../{grantId}/revoke` | Grants | Read check / write + grant-manager check |
| `GET /api/portal/matters`, `GET .../{id}`, `GET/POST .../{id}/documents` | Client view | `ClientMatters` |
| `GET/POST/PUT/DELETE /api/calendar/entries*` | `MatterReference` (matter number as text) | Resolved against matters; see rules above |
| `POST /api/intake/{id}/convert-to-matter` | Creates a matter | `intake.manage` only. Creates, does not expose content; returns id and number of an existing linked matter to intake managers. Not changed. |
| `GET /api/parties`, `POST /api/parties/{id}/aliases`, `POST /api/parties/relationships`, `POST /api/conflicts/checks` | Party data (not matter-linked in schema) | `PartyVisibility` |
| Dashboards, worklists, counts | Frontend derives them from `/matters-v2` and `/calendar` responses (`WorkspaceHomePage.tsx`, `WorkspaceWorklistsPage.tsx`) | Inherit server filtering; no separate count endpoint exists |
| Search, exports, reports, file download | No endpoints exist | Nothing to enforce yet; each new one must start from `IMatterAccessService` |
| Static `/api/matters/*`, `/api/documents/*`, `/api/portal/client|staff` in `Features/Authorization/AuthorizationDemoEndpoints.cs` | None (static JSON) | Permission policies only; return no records |

## Evidence

`backend.IntegrationTests/` (44 tests) against `mysql:8.0.36` in a disposable Testcontainers container, schema created only by EF Core migrations. Before enforcement: 26 of 43 failed. After: 44 of 44 passed, including one added afterwards for the development seeder. Results are recorded in [SLICE_HISTORY.md](../../SLICE_HISTORY.md).

## Known gaps (not closed by this work)

- No persistent user directory: grant `actorId` values are not validated against real identities (G03).
- No ethical walls or deny precedence (G15). Firm-wide attorneys see everything.
- Intake records are not scoped (G14).
- Calendar linkage is by matter-number text, not a foreign key. Unlinked or free-text references stay visible to all staff (G30).
- Calendar verifier fields are still taken from the request body (G30).
- Staff document metadata still accepts a caller-supplied `StorageKey` (G32).
- Audit metadata for party creation includes the display name, including for restricted parties (G16).
- Conflict match scores are shown for masked matches; a score could hint at name similarity.
- Token claims are trusted until expiry; revoking a grant takes effect on the next request, but changing a user's role or permissions does not until a new token (G03/G04).
