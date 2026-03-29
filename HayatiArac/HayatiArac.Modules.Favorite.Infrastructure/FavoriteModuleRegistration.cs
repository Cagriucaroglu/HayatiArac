using HayatiArac.Modules.Favorite.Application;
using HayatiArac.SharedKernel.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.Favorite.Infrastructure;

public class FavoriteModuleRegistration : IModuleRegistration
{
    public void ConfigureEndpoints(WebApplication app)
    {
    }

    public void ConfigureMiddleware(WebApplication app)
    {

    }

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFavoriteApplication();
        services.AddFavoriteInfrastructure(configuration);
        services.AddEndpoints(typeof(FavoriteModuleRegistration).Assembly);
    }
}
