using HayatiArac.Modules.Advert.Application;
using HayatiArac.Modules.Advert.Infrastructure.Persistence;
using HayatiArac.SharedKernel.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.Advert.Infrastructure;

public class AdvertModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Application layer (MediatR, FluentValidation)
        services.AddAdvertApplication();

        // Infrastructure layer (DbContext, Repositories, UnitOfWork)
        services.AddAdvertInfrastructure(configuration);

        // Register endpoints
        services.AddEndpoints(typeof(AdvertModuleRegistration).Assembly);
    }

    public void ConfigureMiddleware(WebApplication app)
    {
        // No specific middleware for Advert module yet
    }

    public void ConfigureEndpoints(WebApplication app)
    {
        // Endpoints are mapped via MapEndpoints() in Program.cs
    }
}
