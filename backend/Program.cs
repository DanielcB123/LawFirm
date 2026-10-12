using EnterpriseKnowledgeAssistant.Api.Features.Auth;
using EnterpriseKnowledgeAssistant.Api.Features.WeatherForecast;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MatterForge API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Provide a valid JWT bearer token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});
builder.Services.AddLawFirmErrorHandling();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointModules();
builder.Services.AddLawFirmAuthorization();
builder.Services.AddLawFirmDemoAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddLawFirmPersistence();
builder.Services.AddLawFirmAccessControl();

builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

var app = builder.Build();
await app.Services.InitializeLawFirmPersistenceAsync();
if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDevelopmentDataAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseLawFirmErrorHandling();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpointModules();

app.Run();

public partial class Program;
