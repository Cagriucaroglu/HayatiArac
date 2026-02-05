using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=HayatiAracDb;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsHistoryTable(
                "__EFMigrationsHistory", AdvertDbContext.Schema));

        return new AdvertDbContext(optionsBuilder.Options);
    }
}
