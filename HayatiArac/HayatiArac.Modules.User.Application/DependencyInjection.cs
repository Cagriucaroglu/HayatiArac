using FluentValidation;
using HayatiArac.SharedKernel.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.User.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
