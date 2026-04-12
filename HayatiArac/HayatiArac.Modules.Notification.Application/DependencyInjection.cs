using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HayatiArac.Modules.Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
