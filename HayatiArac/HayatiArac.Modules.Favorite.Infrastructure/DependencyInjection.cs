using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.Modules.Favorite.Infrastructure.Persistence;
using HayatiArac.Modules.Favorite.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.Favorite.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFavoriteInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<FavoriteDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServer => sqlServer.MigrationsHistoryTable(
                    "__EFMigrationsHistory", FavoriteDbContext.Schema)));

        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IAdvertSnapshotRepository, AdvertSnapshotRepository>();
        services.AddScoped<IFavoriteUnitOfWork, FavoriteUnitOfWork>();

        return services;
    }
}
