using HayatiArac.Modules.User.Application;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Application.Settings;
using HayatiArac.Modules.User.Infrastructure.Persistence;
using HayatiArac.Modules.User.Infrastructure.Persistence.Repositories;
using HayatiArac.Modules.User.Infrastructure.Services;
using HayatiArac.Modules.User.Infrastructure.Services.Authentication;
using HayatiArac.SharedKernel.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.User.Infrastructure;

public class UserModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // EF Core DbContext with SQL Server + schema
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlServer => sqlServer.MigrationsHistoryTable(
                    "__EFMigrationsHistory", UserDbContext.SchemaName)));

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Hosted Services (Database initialization)
        services.AddHostedService<DatabaseInitializerService>();

        // Application layer registrations
        services.AddUserApplication();

        // Register endpoints
        services.AddEndpoints(typeof(UserModuleRegistration).Assembly);
    }

    public void ConfigureMiddleware(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public void ConfigureEndpoints(WebApplication app)
    {
        // Endpoints are mapped via MapEndpoints() in Program.cs
    }
}
