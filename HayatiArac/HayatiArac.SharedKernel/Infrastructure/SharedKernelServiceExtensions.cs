using HayatiArac.SharedKernel.Application.Interfaces;
using HayatiArac.SharedKernel.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.SharedKernel.Infrastructure;

public static class SharedKernelServiceExtensions
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}
