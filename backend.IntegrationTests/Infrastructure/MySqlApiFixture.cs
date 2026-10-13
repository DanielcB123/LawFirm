using System.Net.Http.Headers;
using System.Security.Cryptography;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MySql;

namespace EnterpriseKnowledgeAssistant.IntegrationTests.Infrastructure;

/// <summary>
/// Starts a disposable MySQL 8.0.36 container and the API against it. The schema is created only by
/// applying the EF Core migrations at startup, the same path a new deployment uses.
/// </summary>
public sealed class MySqlApiFixture : IAsyncLifetime
{
    public const string MySqlImage = "mysql:8.0.36";

    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage(MySqlImage)
        .WithDatabase("lawfirm_test")
        // root so schema-state tests can create extra throwaway databases inside this container only.
        .WithUsername("root")
        .WithPassword(RandomNumberGenerator.GetHexString(24))
        .Build();

    private WebApplicationFactory<Program>? _factory;

    public string ConnectionString => _container.GetConnectionString();

    public WebApplicationFactory<Program> Factory =>
        _factory ?? throw new InvalidOperationException("Fixture has not been initialized.");

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var signingKey = RandomNumberGenerator.GetHexString(64);
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:LawFirm", ConnectionString);
            builder.UseSetting("Jwt:Issuer", "LearnCSharpDotNet-tests");
            builder.UseSetting("Jwt:Audience", "LearnCSharpDotNet-tests");
            builder.UseSetting("Jwt:SigningKey", signingKey);
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
            builder.UseSetting(AuthConstants.DemoUsersEnabledKey, "false");
        });

        // Forces host startup, which applies migrations.
        _ = _factory.Services;
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    public HttpClient CreateClient(TestActor? actor = null)
    {
        var client = Factory.CreateClient();
        if (actor is not null)
        {
            var tokenService = Factory.Services.GetRequiredService<ITokenService>();
            var token = tokenService.CreateAccessToken(new DemoUser(
                actor.Id, $"{actor.Id}@example.test", actor.Id, actor.Role, string.Empty, actor.Permissions));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }

        return client;
    }

    public async Task WithDbAsync(Func<LawFirmDbContext, Task> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LawFirmDbContext>();
        await action(dbContext);
    }

    public async Task<T> WithDbAsync<T>(Func<LawFirmDbContext, Task<T>> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LawFirmDbContext>();
        return await action(dbContext);
    }
}

[CollectionDefinition(Name)]
public sealed class MySqlCollection : ICollectionFixture<MySqlApiFixture>
{
    public const string Name = "MySQL";
}
