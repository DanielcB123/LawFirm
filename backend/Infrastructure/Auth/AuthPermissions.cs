namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

public static class AuthPermissions
{
    public const string ContactsRead = "contacts.read";
    public const string ContactsManage = "contacts.manage";
    public const string ConflictsSearch = "conflicts.search";
    public const string ConflictsReview = "conflicts.review";
    public const string CalendarRead = "calendar.read";
    public const string CalendarManage = "calendar.manage";
    public const string IntakeRead = "intake.read";
    public const string IntakeManage = "intake.manage";
    public const string MattersReadOwn = "matters.read.own";
    public const string MattersReadAssigned = "matters.read.assigned";
    public const string MattersReadAll = "matters.read.all";
    public const string MattersUpdateAssigned = "matters.update.assigned";
    public const string MattersUpdateAll = "matters.update.all";
    public const string DocumentsUpload = "documents.upload";
    public const string DocumentsReview = "documents.review";
    public const string BillingReadOwn = "billing.read.own";
    public const string BillingManage = "billing.manage";
    public const string UsersManage = "users.manage";
}
