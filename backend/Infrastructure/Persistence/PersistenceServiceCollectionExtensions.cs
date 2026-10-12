using System.Data;
using System.Data.Common;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public const string ConnectionStringName = "LawFirm";
    public const string ApplyMigrationsOnStartupKey = "Database:ApplyMigrationsOnStartup";
    public const string BaselineRunbookPath = "Markdown/database/migration-baseline.md";

    public static readonly MySqlServerVersion ServerVersion = new(new Version(8, 0, 36));

    private static readonly string[] KnownApplicationTables =
    [
        "parties", "party_aliases", "party_relationships", "conflict_checks", "conflict_decisions",
        "audit_events", "calendar_entries", "intake_records", "intake_stage_history", "intake_notes",
        "matters", "matter_stage_history", "matter_tasks", "matter_documents", "matter_timeline_actions",
        "consultation_requests"
    ];

    public static IServiceCollection AddLawFirmPersistence(this IServiceCollection services)
    {
        services.AddDbContext<LawFirmDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' is not configured. Set it with user-secrets or the " +
                    $"ConnectionStrings__{ConnectionStringName} environment variable.");
            }

            options.UseMySql(connectionString, ServerVersion);
        });
        services.AddScoped<IAuditTrailService, AuditTrailService>();

        return services;
    }

    public static async Task InitializeLawFirmPersistenceAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LawFirmDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("LawFirmPersistence");
        var applyMigrations = configuration.GetValue<bool>(ApplyMigrationsOnStartupKey);

        var state = await DetectSchemaStateAsync(dbContext);
        if (state == SchemaState.LegacyUnmanaged)
        {
            throw new InvalidOperationException(
                "The configured database contains application tables but no EF Core migration history. It was " +
                "probably created by the previous EnsureCreated startup path. It will not be modified automatically. " +
                $"Follow {BaselineRunbookPath} to baseline it, or point the connection string at a new empty database.");
        }

        var pending = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
        if (pending.Length == 0)
        {
            return;
        }

        if (!applyMigrations)
        {
            throw new InvalidOperationException(
                $"The database has {pending.Length} pending migration(s) ({string.Join(", ", pending)}) and " +
                $"{ApplyMigrationsOnStartupKey} is false. Apply migrations with a migration-privileged account.");
        }

        logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", pending.Length, string.Join(", ", pending));
        await dbContext.Database.MigrateAsync();
    }

    private enum SchemaState
    {
        Empty,
        Managed,
        LegacyUnmanaged
    }

    private static async Task<SchemaState> DetectSchemaStateAsync(LawFirmDbContext dbContext)
    {
        var connection = dbContext.Database.GetDbConnection();
        try
        {
            await connection.OpenAsync();
        }
        catch (MySqlException exception) when (exception.ErrorCode == MySqlErrorCode.UnknownDatabase)
        {
            return SchemaState.Empty;
        }

        try
        {
            var tableNames = await ReadTableNamesAsync(connection);
            if (tableNames.Contains("__EFMigrationsHistory"))
            {
                return SchemaState.Managed;
            }

            return KnownApplicationTables.Any(tableNames.Contains) ? SchemaState.LegacyUnmanaged : SchemaState.Empty;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<HashSet<string>> ReadTableNamesAsync(DbConnection connection)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE()";
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }
}
