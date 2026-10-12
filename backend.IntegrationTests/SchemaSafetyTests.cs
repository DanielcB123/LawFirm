using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using EnterpriseKnowledgeAssistant.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace EnterpriseKnowledgeAssistant.IntegrationTests;

/// <summary>
/// Startup must never silently alter a database it does not manage. Each test uses its own throwaway database
/// inside the disposable test container.
/// </summary>
[Collection(MySqlCollection.Name)]
public sealed class SchemaSafetyTests(MySqlApiFixture fixture)
{
    [Fact]
    public async Task Fixture_schema_was_created_by_migrations()
    {
        await fixture.WithDbAsync(async db =>
        {
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
            Assert.Contains(await db.Database.GetAppliedMigrationsAsync(), id => id.EndsWith("_Baseline", StringComparison.Ordinal));
        });
    }

    [Fact]
    public async Task Legacy_database_without_migration_history_is_refused_and_left_untouched()
    {
        var database = await CreateDatabaseAsync("CREATE TABLE matters (Id char(36) NOT NULL PRIMARY KEY)");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => InitializeAsync(database, applyMigrations: true));

        Assert.Contains("migration-baseline.md", error.Message);
        Assert.Equal(["matters"], await TableNamesAsync(database));
    }

    [Fact]
    public async Task Pending_migrations_are_not_applied_unless_enabled()
    {
        var database = await CreateDatabaseAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => InitializeAsync(database, applyMigrations: false));

        Assert.Empty(await TableNamesAsync(database));
    }

    [Fact]
    public async Task Development_seed_runs_on_a_migrated_database_and_grants_the_seeded_client()
    {
        var database = await CreateDatabaseAsync();
        await using var provider = BuildProvider(database, applyMigrations: true);

        await provider.InitializeLawFirmPersistenceAsync();
        await provider.SeedDevelopmentDataAsync();

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LawFirmDbContext>();
        var grant = await db.MatterAccessGrants.Include(item => item.MatterRecord).SingleAsync();
        Assert.Equal("u-client-001", grant.ActorId);
        Assert.Equal(MatterAccessGrantTypes.Client, grant.GrantType);
        Assert.Equal("MAT-2026-0001", grant.MatterRecord.MatterNumber);
    }

    private async Task<string> CreateDatabaseAsync(string? setupSql = null)
    {
        var name = "schema_" + Guid.NewGuid().ToString("N")[..12];
        await using var connection = new MySqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using (var create = new MySqlCommand($"CREATE DATABASE `{name}`", connection))
        {
            await create.ExecuteNonQueryAsync();
        }

        if (setupSql is not null)
        {
            await connection.ChangeDatabaseAsync(name);
            await using var setup = new MySqlCommand(setupSql, connection);
            await setup.ExecuteNonQueryAsync();
        }

        return name;
    }

    private async Task InitializeAsync(string database, bool applyMigrations)
    {
        await using var provider = BuildProvider(database, applyMigrations);
        await provider.InitializeLawFirmPersistenceAsync();
    }

    private ServiceProvider BuildProvider(string database, bool applyMigrations)
    {
        var connectionString = new MySqlConnectionStringBuilder(fixture.ConnectionString) { Database = database }.ConnectionString;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:LawFirm"] = connectionString,
                [PersistenceServiceCollectionExtensions.ApplyMigrationsOnStartupKey] = applyMigrations.ToString()
            })
            .Build();
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging()
            .AddHttpContextAccessor();
        services.AddLawFirmPersistence();
        return services.BuildServiceProvider();
    }

    private async Task<string[]> TableNamesAsync(string database)
    {
        await using var connection = new MySqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = @schema ORDER BY TABLE_NAME", connection);
        command.Parameters.AddWithValue("@schema", database);
        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names.ToArray();
    }
}
