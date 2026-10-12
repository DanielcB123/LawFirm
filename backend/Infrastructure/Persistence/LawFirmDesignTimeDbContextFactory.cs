using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

/// <summary>
/// Used only by <c>dotnet ef</c> to scaffold migrations. The connection string is never opened
/// because the server version is explicit; it must not point at a real database.
/// </summary>
public sealed class LawFirmDesignTimeDbContextFactory : IDesignTimeDbContextFactory<LawFirmDbContext>
{
    public LawFirmDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LawFirmDbContext>()
            .UseMySql("Server=design-time-only;Database=design_time_only", PersistenceServiceCollectionExtensions.ServerVersion)
            .Options;

        return new LawFirmDbContext(options);
    }
}
