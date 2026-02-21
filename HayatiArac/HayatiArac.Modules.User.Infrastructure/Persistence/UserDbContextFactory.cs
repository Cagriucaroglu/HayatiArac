using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HayatiArac.Modules.User.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext factory for EF Core migrations
/// </summary>
public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();

        // Use a default connection string for design-time
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=HayatiAracDb;User Id=sa;Password=Test12345;TrustServerCertificate=True;MultipleActiveResultSets=true",
            sqlServer => sqlServer.MigrationsHistoryTable(
                "__EFMigrationsHistory", UserDbContext.SchemaName));
        
        return new UserDbContext(optionsBuilder.Options);
    }
}
