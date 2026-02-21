using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HayatiArac.Modules.Advert.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext factory for EF Core migrations
/// </summary>
public class AdvertDbContextFactory : IDesignTimeDbContextFactory<AdvertDbContext>
{
    public AdvertDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AdvertDbContext>();

        // Use a default connection string for design-time
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=HayatiAracDb;User Id=sa;Password=Test12345;TrustServerCertificate=True;MultipleActiveResultSets=true",
            sqlServer => sqlServer.MigrationsHistoryTable(
                "__EFMigrationsHistory", AdvertDbContext.Schema));

        return new AdvertDbContext(optionsBuilder.Options);
    }
}
