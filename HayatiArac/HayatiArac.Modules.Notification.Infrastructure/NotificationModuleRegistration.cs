using HayatiArac.Modules.Notification.Application;
using HayatiArac.Modules.Notification.Infrastructure.Hubs;
using HayatiArac.SharedKernel.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.Notification.Infrastructure;

public class NotificationModuleRegistration : IModuleRegistration
{
    public void ConfigureEndpoints(WebApplication app)
    {
    }

    public void ConfigureMiddleware(WebApplication app)
    {
        app.MapHub<NotificationHub>("/hubs/notifications");
    }

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNotificationApplication();
        services.AddNotificationInfrastructure(configuration);
        services.AddEndpoints(typeof(NotificationModuleRegistration).Assembly);
    }
}
