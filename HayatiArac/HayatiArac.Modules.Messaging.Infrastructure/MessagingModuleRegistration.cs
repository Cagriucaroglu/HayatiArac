using HayatiArac.Modules.Messaging.Application;
using HayatiArac.Modules.Messaging.Infrastructure.Hubs;
using HayatiArac.SharedKernel.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.Messaging.Infrastructure;

public class MessagingModuleRegistration: IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMessagingApplication();
        services.AddMessagingInfrastructure(configuration);
        services.AddEndpoints(typeof(MessagingModuleRegistration).Assembly);
    }

    public void ConfigureMiddleware(WebApplication app)
    {
        app.MapHub<MessagingHub>("/hubs/messaging");
    }

    public void ConfigureEndpoints(WebApplication app)
    {
    }
}
