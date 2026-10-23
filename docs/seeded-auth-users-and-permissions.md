# Seeded Authenticated Users and Permissions

This document summarizes the seeded users and role permissions currently configured in the Laravel migration backend.

## Source of truth

- `laravel-backend/database/seeders/DatabaseSeeder.php`
- `laravel-backend/config/roles.php`
- `laravel-backend/routes/api.php`

## Seeded authenticated users

All seeded users share the same seeded password:

- `Passw0rd!`

The following users are created (or updated) by the database seeder:

| Name | Email | Role |
|---|---|---|
| Sandra Whitmore | `partner@matterforge.local` | Partner |
| Avery Attorney | `attorney@matterforge.local` | Attorney |
| Paige Paralegal | `paralegal@matterforge.local` | Paralegal |
| Aria Accountant | `accountant@matterforge.local` | Accountant |
| Liam Legal Assistant | `assistant@matterforge.local` | LegalAssistant |
| Ira Admin | `adminit@matterforge.local` | AdminIT |
| Brian and Sandra Firm Partner | `lawfirmbrianandsandra@matterforge.com` | Partner |
| Casey Client | `client@matterforge.local` | Client |

## Role-to-permission matrix

Permissions are seeded from `config/roles.php` and stored per user in the `permissions` attribute.

| Role | Granted Permissions |
|---|---|
| Partner | `contacts.read`, `contacts.manage`, `intake.read`, `intake.manage`, `consultations.read`, `consultations.manage`, `calendar.read`, `calendar.manage`, `matters.read`, `tasks.read`, `tasks.manage`, `users.manage` |
| Attorney | `contacts.read`, `contacts.manage`, `intake.read`, `intake.manage`, `consultations.read`, `consultations.manage`, `calendar.read`, `calendar.manage`, `matters.read`, `tasks.read`, `tasks.manage` |
| Paralegal | `contacts.read`, `intake.read`, `consultations.read`, `calendar.read`, `matters.read`, `tasks.read` |
| Accountant | `contacts.read` |
| LegalAssistant | `contacts.read`, `intake.read`, `intake.manage`, `consultations.read`, `consultations.manage`, `calendar.read`, `calendar.manage`, `matters.read`, `tasks.read`, `tasks.manage` |
| AdminIT | `contacts.read`, `consultations.read`, `calendar.read`, `matters.read`, `tasks.read` |
| Client | (no seeded permissions) |

## What these permissions currently gate

Authenticated routes in `routes/api.php` apply permission middleware as follows:

- `contacts.read` -> `GET /api/tenant/contacts`
- `contacts.manage` -> `POST /api/tenant/contacts`
- `intake.read` -> `GET /api/tenant/intake`
- `intake.manage` -> `POST /api/tenant/intake`
- `consultations.read` -> `GET /api/tenant/consultations`
- `consultations.manage` -> `POST /api/tenant/consultations/{consultation}/status`
- `calendar.read` -> `GET /api/tenant/calendar/entries`
- `calendar.manage` -> `POST /api/tenant/calendar/entries`, `POST /api/tenant/calendar/entries/{entry}/complete`
- `matters.read` -> `GET /api/tenant/matters`
- `tasks.read` -> `GET /api/tenant/tasks`
- `tasks.manage` -> `POST /api/tenant/tasks`, `POST /api/tenant/tasks/{task}/status`
- `users.manage` -> `GET /api/tenant/users`, `POST /api/tenant/users`, `PUT /api/tenant/users/{user}/membership`

The middleware returns `403 Forbidden` if the authenticated user lacks the required permission.
