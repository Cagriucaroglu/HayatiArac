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
        optionsBuilder.UseSqlite(
            "Data Source=hayatiarac.db",
            sqlite => sqlite.MigrationsHistoryTable(
                "__EFMigrationsHistory", AdvertDbContext.Schema));

        return new AdvertDbContext(optionsBuilder.Options);
    }
}
