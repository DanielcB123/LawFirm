namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

public static class AuthPolicies
{
    public const string ContactsRead = nameof(ContactsRead);
    public const string ContactsManage = nameof(ContactsManage);
    public const string ConflictsSearch = nameof(ConflictsSearch);
    public const string ConflictsReview = nameof(ConflictsReview);
    public const string CalendarRead = nameof(CalendarRead);
    public const string CalendarManage = nameof(CalendarManage);
    public const string IntakeRead = nameof(IntakeRead);
    public const string IntakeManage = nameof(IntakeManage);
    public const string ClientPortal = nameof(ClientPortal);
    public const string StaffPortal = nameof(StaffPortal);
    public const string MatterReadOwn = nameof(MatterReadOwn);
    public const string MatterReadAssigned = nameof(MatterReadAssigned);
    public const string MatterReadAll = nameof(MatterReadAll);
    public const string MatterUpdateAssigned = nameof(MatterUpdateAssigned);
    public const string MatterUpdateAll = nameof(MatterUpdateAll);
    public const string DocumentUpload = nameof(DocumentUpload);
    public const string DocumentReview = nameof(DocumentReview);
    public const string BillingReadOwn = nameof(BillingReadOwn);
    public const string BillingManage = nameof(BillingManage);
    public const string UsersManage = nameof(UsersManage);
}
