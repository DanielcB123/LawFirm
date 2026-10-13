using EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.OpenApi;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLawFirmOpenApi();
builder.Services.AddLawFirmErrorHandling();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointModules();
builder.Services.AddLawFirmAuthorization();
builder.Services.AddLawFirmDemoAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddLawFirmPersistence();
builder.Services.AddLawFirmAccessControl();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("PublicConsultationCreate", context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

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
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpointModules();

app.Run();

public partial class Program;
