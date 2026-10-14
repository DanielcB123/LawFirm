# Naming and rename record

[Home](../README.md) · [Vision and scope](vision-and-scope.md) · [First firm](first-firm.md) · [Open decisions](../planning/open-decisions.md)

## Status (2026-10-12): two separate facts

| Item | Status | Evidence |
|---|---|---|
| **Branding** | "MatterForge" is a **working product name only**. No trademark search, domain check, or legal clearance has been done. Not cleared for public use. | Founder instruction; no clearance record exists |
| **Repository folder** | **Not renamed.** Still `D:\Documents\Projects\LearnCSharpDotNet`. `D:\Documents\Projects\MatterForge` does not exist. | Checked with `Test-Path` this session |

The folder was not renamed because doing so was not safe at the time:
- the API process (`EnterpriseKnowledgeAssistant.Api.exe`) was running from `backend\bin\Debug\net8.0`;
- a Vite dev server was running from `frontend`;
- the Cursor workspace was open on the folder.

Renaming a folder that is in use can fail partway or break the open workspace. No duplicate copy of the repository was created as a shortcut.

### Manual folder rename (owner action)
1. Stop project terminals and running application processes.
2. Close the current Cursor workspace.
3. Rename LearnCSharpDotNet to MatterForge in File Explorer.
4. Reopen D:\Documents\Projects\MatterForge in Cursor.

Nothing in tracked first-party files contains the string `LearnCSharpDotNet`, so the rename needs no code change. There is no `.code-workspace` file and no `.cursor` folder. Cursor's own per-project agent data is keyed by path, so earlier chat history may appear under the old path.

## Inventory and classification
Rule: change only user-visible product branding; record everything else.

| Occurrence | Class | Action |
|---|---|---|
| `README.md` title; `Markdown/README.md` title | Product branding | **Changed** to MatterForge, with a working-name note |
| `frontend/index.html` `<title>` (`frontend`) | Product branding | **Changed** to `MatterForge` |
| `backend/Program.cs` Swagger document title | Product branding | **Changed** to `MatterForge API` |
| "Brian & Sandra Law" / "BSL" (`SiteNav.tsx`, `SiteFooter.tsx`, `AboutPage.tsx`, `HomePage.tsx`) | Fictional demo **firm** branding: the tenant's brand, not the product's | Unchanged. It becomes per-firm configuration when multi-firm branding exists |
| `parkerreedlaw.com` in demo e-mails (`ContactPage.tsx`, `WorkspaceLoginPage.tsx`, `DemoUserStore`, `DEV_AUTH_CREDENTIALS.md`) | Fictional demo data | Unchanged |
| Namespace and project `EnterpriseKnowledgeAssistant.Api` (csproj, `.http`, all `.cs`) | Technical identifier | Unchanged; bounded follow-up below |
| Test `RootNamespace` `EnterpriseKnowledgeAssistant.IntegrationTests` | Technical identifier | Unchanged; follow-up |
| Solution file `MatterForge.sln` and test namespace `EnterpriseKnowledgeAssistant.UnitTests` (both added 2026-10-12, slice R1) | New build artifacts | The solution uses the working name because it is new and only groups projects; rename the file freely if the brand changes. The unit-test namespace follows the existing technical prefix |
| `LawFirmDbContext`, migration designer files | Technical identifier (migration snapshot references the type) | Unchanged |
| Connection-string key `ConnectionStrings:LawFirm` | Configuration contract (environments and user secrets depend on it) | Unchanged |
| `UserSecretsId` `lawfirm-platform-api-dev` | Developer secret store location | Unchanged. Changing it orphans local secrets |
| localStorage key `lawfirm.auth.token` | Client state contract | Unchanged. Changing it signs every user out |
| `package.json` `name: "frontend"` | Package identifier | Unchanged |
| JWT issuer `EnterpriseKnowledgeAssistant`, audience `EnterpriseKnowledgeAssistant.Frontend` | **Authentication identifiers** | **Must not change** for branding. Changing them invalidates tokens and must be a coordinated security change |
| Git remote `origin https://github.com/DanielcB123/LawFirm.git` | External resource | Unchanged |
| Names of any existing local or deployed databases (supplied through secrets or configuration; values not printed here) | Existing data stores | Unchanged |
| `SLICE_HISTORY.md` entries, the Clio parity tracker | Historical record | Unchanged. History is not rewritten |

## Bounded follow-up: technical rename (not started)
Possible later, as its own slice, after branding clearance:
- rename the namespace `EnterpriseKnowledgeAssistant.*` to `MatterForge.*`;
- rename the project and assembly names.

The slice should be one mechanical commit with no behavior change. It must keep the following unchanged:
- JWT issuer and audience;
- `ConnectionStrings:LawFirm`;
- `UserSecretsId`;
- the localStorage key;
- database names;
- the Git remote.

Acceptance: the build passes, the full integration suite passes, and `dotnet ef migrations list` still shows the same migrations. Migration designer files reference the DbContext type by name, so they need regenerating or careful editing.

## Open naming decisions
- Clearance of "MatterForge" (trademark and domain) before any public use.
- Confirmation of the first firm's exact legal name and spelling ("Maldanado and Benitez Law" as supplied).
