using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HayatiArac.Modules.Favorite.Infrastructure.Persistence;

public class FavoriteDbContextFactory : IDesignTimeDbContextFactory<FavoriteDbContext>
{
    public FavoriteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FavoriteDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=HayatiAracDb;User Id=sa;Password=Test12345;TrustServerCertificate=True;MultipleActiveResultSets=true",
            sqlServer => sqlServer.MigrationsHistoryTable(
                "__EFMigrationsHistory", FavoriteDbContext.Schema));

        return new FavoriteDbContext(optionsBuilder.Options);
    }
}
