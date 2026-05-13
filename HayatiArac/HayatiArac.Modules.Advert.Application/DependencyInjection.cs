using FluentValidation;
using HayatiArac.SharedKernel.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HayatiArac.Modules.Advert.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAdvertApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
