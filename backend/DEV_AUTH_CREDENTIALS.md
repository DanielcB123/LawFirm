# Development Auth Credentials

These demo accounts are defined in `backend/Infrastructure/Auth/DemoUserStore.cs` for local testing only. They are fictional and their passwords are public by design.

**Off by default.** The store is registered only when `Authentication:DemoUsers:Enabled` is `true` and the environment is Development (set it with `dotnet user-secrets`; see the root README). Enabled in any other environment, startup fails. Not enabled, `POST /api/auth/login` returns 503 `sign_in_unavailable`.

Matter visibility depends on assignment and grants, not just role. In seeded data, `MAT-2026-0001` is assigned to Jordan Attorney (`u-attorney-002`) and Priya Paralegal (`u-paralegal-001`), and Casey Client (`u-client-001`) holds its Client grant. Admins see no matters until granted.

**Cleanup.** The accounts live in code, not the database, so disabling the flag removes them. Rows that reference demo actor ids (seeded matters, grants, audit events) exist only in databases the Development seeder ran against. Do not delete them from a database you have not identified; discard the whole development database instead.

## Client

- Email: `client@parkerreedlaw.com`
- Password: `ClientPass!123`
- Role: `Client`

- Email: `maria.client@parkerreedlaw.com`
- Password: `ClientPass!456`
- Role: `Client`

## Paralegal

- Email: `paralegal@parkerreedlaw.com`
- Password: `ParalegalPass!123`
- Role: `Paralegal`

- Email: `dylan.paralegal@parkerreedlaw.com`
- Password: `ParalegalPass!456`
- Role: `Paralegal`

## Attorney

- Email: `attorney@parkerreedlaw.com`
- Password: `AttorneyPass!123`
- Role: `Attorney`

- Email: `jordan.attorney@parkerreedlaw.com`
- Password: `AttorneyPass!456`
- Role: `Attorney`

## Admin

- Email: `admin@parkerreedlaw.com`
- Password: `AdminPass!123`
- Role: `Admin`

- Email: `sam.admin@parkerreedlaw.com`
- Password: `AdminPass!456`
- Role: `Admin`

---

Do not use these credentials outside local development/test environments.
