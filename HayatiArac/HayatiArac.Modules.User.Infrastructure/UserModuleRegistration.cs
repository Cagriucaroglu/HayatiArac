using HayatiArac.Modules.User.Application;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Domain.Entities;
using HayatiArac.Modules.User.Infrastructure.Persistence;
using HayatiArac.Modules.User.Infrastructure.Persistence.Repositories;
using HayatiArac.Modules.User.Infrastructure.Services;
using HayatiArac.SharedKernel.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.User.Infrastructure;

public class UserModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // EF Core DbContext with SQLite + schema
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                sqlite => sqlite.MigrationsHistoryTable(
                    "__EFMigrationsHistory", UserDbContext.SchemaName)));

        // ASP.NET Identity
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

        // Cookie Authentication
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = "HayatiArac.Auth";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.LoginPath = "/api/users/login";
            options.AccessDeniedPath = "/api/users/access-denied";
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            };
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();

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
