using HayatiArac.Modules.Notification.Application.Interfaces;
using HayatiArac.Modules.Notification.Infrastructure.Persistence;
using HayatiArac.Modules.Notification.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<NotificationDbContext>(options => options.UseSqlServer(connectionString, sqlServer => 
            sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", NotificationDbContext.Schema)));
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationUnitOfWork, NotificationUnitOfWork>();

        return services;
    }
}
