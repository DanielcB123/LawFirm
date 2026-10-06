using Microsoft.AspNetCore.Identity;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

public sealed class DemoUserStore : IDemoUserStore
{
    private readonly PasswordHasher<string> _passwordHasher = new();
    private readonly IReadOnlyDictionary<string, DemoUser> _usersByEmail;

    public DemoUserStore()
    {
        var users = new[]
        {
            CreateUser(
                id: "u-client-001",
                email: "client@parkerreedlaw.com",
                displayName: "Casey Client",
                role: AuthRoles.Client,
                plainPassword: "ClientPass!123",
                permissions: [AuthPermissions.MattersReadOwn, AuthPermissions.DocumentsUpload, AuthPermissions.BillingReadOwn]),
            CreateUser(
                id: "u-client-002",
                email: "maria.client@parkerreedlaw.com",
                displayName: "Maria Client",
                role: AuthRoles.Client,
                plainPassword: "ClientPass!456",
                permissions: [AuthPermissions.MattersReadOwn, AuthPermissions.DocumentsUpload, AuthPermissions.BillingReadOwn]),
            CreateUser(
                id: "u-paralegal-001",
                email: "paralegal@parkerreedlaw.com",
                displayName: "Priya Paralegal",
                role: AuthRoles.Paralegal,
                plainPassword: "ParalegalPass!123",
                permissions:
                [
                    AuthPermissions.ContactsRead,
                    AuthPermissions.ContactsManage,
                    AuthPermissions.ConflictsSearch,
                    AuthPermissions.IntakeRead,
                    AuthPermissions.IntakeManage,
                    AuthPermissions.CalendarRead,
                    AuthPermissions.CalendarManage,
                    AuthPermissions.MattersReadAssigned,
                    AuthPermissions.MattersUpdateAssigned,
                    AuthPermissions.DocumentsUpload,
                    AuthPermissions.DocumentsReview
                ]),
            CreateUser(
                id: "u-paralegal-002",
                email: "dylan.paralegal@parkerreedlaw.com",
                displayName: "Dylan Paralegal",
                role: AuthRoles.Paralegal,
                plainPassword: "ParalegalPass!456",
                permissions:
                [
                    AuthPermissions.ContactsRead,
                    AuthPermissions.ContactsManage,
                    AuthPermissions.ConflictsSearch,
                    AuthPermissions.IntakeRead,
                    AuthPermissions.IntakeManage,
                    AuthPermissions.CalendarRead,
                    AuthPermissions.CalendarManage,
                    AuthPermissions.MattersReadAssigned,
                    AuthPermissions.MattersUpdateAssigned,
                    AuthPermissions.DocumentsUpload,
                    AuthPermissions.DocumentsReview
                ]),
            CreateUser(
                id: "u-attorney-001",
                email: "attorney@parkerreedlaw.com",
                displayName: "Avery Attorney",
                role: AuthRoles.Attorney,
                plainPassword: "AttorneyPass!123",
                permissions:
                [
                    AuthPermissions.ContactsRead,
                    AuthPermissions.ContactsManage,
                    AuthPermissions.ConflictsSearch,
                    AuthPermissions.ConflictsReview,
                    AuthPermissions.IntakeRead,
                    AuthPermissions.IntakeManage,
                    AuthPermissions.CalendarRead,
                    AuthPermissions.CalendarManage,
                    AuthPermissions.MattersReadAssigned,
                    AuthPermissions.MattersReadAll,
                    AuthPermissions.MattersUpdateAll,
                    AuthPermissions.DocumentsUpload,
                    AuthPermissions.DocumentsReview,
                    AuthPermissions.BillingManage
                ]),
            CreateUser(
                id: "u-attorney-002",
                email: "jordan.attorney@parkerreedlaw.com",
                displayName: "Jordan Attorney",
                role: AuthRoles.Attorney,
                plainPassword: "AttorneyPass!456",
                permissions:
                [
                    AuthPermissions.ContactsRead,
                    AuthPermissions.ContactsManage,
                    AuthPermissions.ConflictsSearch,
                    AuthPermissions.ConflictsReview,
                    AuthPermissions.IntakeRead,
                    AuthPermissions.IntakeManage,
                    AuthPermissions.CalendarRead,
                    AuthPermissions.CalendarManage,
                    AuthPermissions.MattersReadAssigned,
                    AuthPermissions.MattersReadAll,
                    AuthPermissions.MattersUpdateAll,
                    AuthPermissions.DocumentsUpload,
                    AuthPermissions.DocumentsReview,
                    AuthPermissions.BillingManage
                ]),
            CreateUser(
                id: "u-admin-001",
                email: "admin@parkerreedlaw.com",
                displayName: "Alex Admin",
                role: AuthRoles.Admin,
                plainPassword: "AdminPass!123",
                permissions:
                [
                    AuthPermissions.ContactsRead,
                    AuthPermissions.ContactsManage,
                    AuthPermissions.ConflictsSearch,
                    AuthPermissions.ConflictsReview,
                    AuthPermissions.IntakeRead,
                    AuthPermissions.IntakeManage,
                    AuthPermissions.CalendarRead,
                    AuthPermissions.CalendarManage,
                    AuthPermissions.MattersReadAll,
                    AuthPermissions.MattersUpdateAll,
                    AuthPermissions.DocumentsReview,
                    AuthPermissions.BillingManage,
                    AuthPermissions.UsersManage
                ]),
            CreateUser(
                id: "u-admin-002",
                email: "sam.admin@parkerreedlaw.com",
                displayName: "Sam Admin",
                role: AuthRoles.Admin,
                plainPassword: "AdminPass!456",
                permissions:
                [
                    AuthPermissions.ContactsRead,
                    AuthPermissions.ContactsManage,
                    AuthPermissions.ConflictsSearch,
                    AuthPermissions.ConflictsReview,
                    AuthPermissions.IntakeRead,
                    AuthPermissions.IntakeManage,
                    AuthPermissions.CalendarRead,
                    AuthPermissions.CalendarManage,
                    AuthPermissions.MattersReadAll,
                    AuthPermissions.MattersUpdateAll,
                    AuthPermissions.DocumentsReview,
                    AuthPermissions.BillingManage,
                    AuthPermissions.UsersManage
                ])
        };

        _usersByEmail = users.ToDictionary(user => user.Email, StringComparer.OrdinalIgnoreCase);
    }

    public DemoUser? FindByEmail(string email)
    {
        return _usersByEmail.GetValueOrDefault(email);
    }

    public bool VerifyPassword(DemoUser user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user.Email, user.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    private DemoUser CreateUser(
        string id,
        string email,
        string displayName,
        string role,
        string plainPassword,
        IReadOnlyCollection<string> permissions)
    {
        var hash = _passwordHasher.HashPassword(email, plainPassword);
        return new DemoUser(id, email, displayName, role, hash, permissions);
    }
}
