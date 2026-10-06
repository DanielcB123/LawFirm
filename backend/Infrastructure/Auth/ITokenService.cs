namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

public interface ITokenService
{
    TokenResult CreateAccessToken(DemoUser user);
}
