namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

public interface IDemoUserStore
{
    DemoUser? FindByEmail(string email);
    bool VerifyPassword(DemoUser user, string password);
}
