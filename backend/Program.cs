using EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.OpenApi;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLawFirmOpenApi();
builder.Services.AddLawFirmErrorHandling();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointModules();
builder.Services.AddLawFirmAuthorization();
builder.Services.AddLawFirmDemoAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddLawFirmPersistence();
builder.Services.AddLawFirmAccessControl();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITokenService, TokenService>();

var app = builder.Build();
await app.Services.InitializeLawFirmPersistenceAsync();
if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDevelopmentDataAsync();
}

app.UseLawFirmOpenApi();
app.UseLawFirmErrorHandling();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpointModules();

app.Run();

public partial class Program;
