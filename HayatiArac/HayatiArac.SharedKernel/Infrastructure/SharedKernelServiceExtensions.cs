using HayatiArac.SharedKernel.Application.Interfaces;
using HayatiArac.SharedKernel.Infrastructure.Middleware;
using HayatiArac.SharedKernel.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
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

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
