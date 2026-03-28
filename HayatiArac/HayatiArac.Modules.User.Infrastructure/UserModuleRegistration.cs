using HayatiArac.Modules.User.Application;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Application.Settings;
using HayatiArac.Modules.User.Infrastructure.Persistence;
using HayatiArac.Modules.User.Infrastructure.Persistence.Repositories;
using HayatiArac.Modules.User.Infrastructure.Services;
using HayatiArac.Modules.User.Infrastructure.Services.Authentication;
using HayatiArac.Modules.User.Infrastructure.Services.Cache;
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

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "HayatiArac:";
        });

        // Services
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
        services.AddScoped<IOtpService, RedisOtpService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ISmsService, NetgsmSmsService>();

        // HttpClient for Netgsm
        services.AddHttpClient<NetgsmSmsService>();

        // Settings
        services.Configure<NetgsmSettings>(configuration.GetSection(NetgsmSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));

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
