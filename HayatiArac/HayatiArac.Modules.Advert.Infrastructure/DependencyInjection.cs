using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Infrastructure.Persistence;
using HayatiArac.Modules.Advert.Infrastructure.Persistence.Repositories;
using HayatiArac.SharedKernel.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.Advert.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAdvertInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AdvertDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IAdvertRepository, AdvertRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAdvertOwnerInfoRepository, AdvertOwnerInfoRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
