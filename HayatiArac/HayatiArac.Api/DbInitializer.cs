using HayatiArac.Modules.Advert.Infrastructure.Persistence;
using HayatiArac.Modules.Favorite.Infrastructure.Persistence;
using HayatiArac.Modules.Messaging.Infrastructure.Persistence;
using HayatiArac.Modules.User.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Api;

public static class DbInitializer
{
    public static async Task MigrateAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        await MigrateContextAsync<UserDbContext>(sp, logger);
        await MigrateContextAsync<AdvertDbContext>(sp, logger);
        await MigrateContextAsync<FavoriteDbContext>(sp, logger);
        await MigrateContextAsync<MessagingDbContext>(sp, logger);
    }
    private static async Task MigrateContextAsync<TContext>(IServiceProvider sp, ILogger logger)
        where TContext : DbContext
    {
        var context = sp.GetRequiredService<TContext>();
        var contextName = typeof(TContext).Name;

        var pending = await context.Database.GetPendingMigrationsAsync();
        var pendingList = pending.ToList();

        if (pendingList.Count == 0)
        {
            logger.LogInformation("{Context}: güncel, migration gerekmedi.", contextName);
            return;
        }

        logger.LogInformation("{Context}: {Count} bekleyen migration uygulanıyor: {Migrations}",
            contextName, pendingList.Count, string.Join(", ", pendingList));

        await context.Database.MigrateAsync();

        logger.LogInformation("{Context}: migration tamamlandı.", contextName);
    }
}
