using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HayatiArac.Modules.Messaging.Infrastructure.Persistence;

public class MessagingDbContextFactory : IDesignTimeDbContextFactory<MessagingDbContext>
{
    public MessagingDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<MessagingDbContext> optionsBuilder = new DbContextOptionsBuilder<MessagingDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=HayatiAracDb;User Id=sa;Password=Test12345;TrustServerCertificate=True;MultipleActiveResultSets=true",
            sqlServer => sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", MessagingDbContext.Schema));

        return new MessagingDbContext(optionsBuilder.Options);
    }
}
