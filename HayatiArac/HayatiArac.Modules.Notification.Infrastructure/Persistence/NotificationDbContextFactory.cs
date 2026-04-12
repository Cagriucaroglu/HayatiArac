using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HayatiArac.Modules.Notification.Infrastructure.Persistence;

public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<NotificationDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=HayatiAracDb;User Id=sa;Password=Test12345;TrustServerCertificate = True; MultipleActiveResultSets = true",
                sqlServer => sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", NotificationDbContext.Schema)
            );

        return new NotificationDbContext(optionsBuilder.Options);

    }
}
