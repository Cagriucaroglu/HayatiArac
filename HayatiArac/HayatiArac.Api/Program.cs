using HayatiArac.Api;
using HayatiArac.Modules.Advert.Infrastructure;
using HayatiArac.Modules.Favorite.Infrastructure;
using HayatiArac.Modules.Messaging.Infrastructure;
using HayatiArac.Modules.Notification.Infrastructure;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Infrastructure;
using HayatiArac.SharedKernel.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HayatiArac API", Version = "v1" });
});

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret is not configured. Please set it in appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.Claims
                    .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrEmpty(jti))
                {
                    context.Fail("Missing JTI claim");
                    return;
                }

                var blacklistService = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenBlacklistService>();

                if (await blacklistService.IsTokenRevokedAsync(jti))
                    context.Fail("Token has been revoked");
            },
            OnMessageReceived = context =>
            {
                string? accessToken = context.Request.Query["access_token"];
                PathString path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/messaging") ||
                     path.StartsWithSegments("/hubs/notifications")))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }   
        };
    });

builder.Services.AddAuthorization();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
string? redisConnectionString = builder.Configuration.GetConnectionString("Redis");

builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString!, name: "sql-server", tags: ["ready"])
    .AddRedis(redisConnectionString!, name: "redis", tags: ["ready"]);

// SharedKernel — ICurrentUserService ve cross-cutting servisler
builder.Services.AddSharedKernel();

// Module registrations
var modules = new IModuleRegistration[]
{
    new UserModuleRegistration(),
    new AdvertModuleRegistration(),
    new FavoriteModuleRegistration(),
    new MessagingModuleRegistration(),
    new NotificationModuleRegistration()
};

foreach (var module in modules)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

// MassTransit for integration events
builder.Services.AddMassTransit(x =>
{
    // Register consumers from User module
    x.AddConsumers(typeof(UserModuleRegistration).Assembly);

    // Register consumers from Advert module
    x.AddConsumers(typeof(AdvertModuleRegistration).Assembly);

    x.AddConsumers(typeof(FavoriteModuleRegistration).Assembly);

    // Register consumers from Messaging module
    x.AddConsumers(typeof(MessagingModuleRegistration).Assembly);

    // Register consumers from Notification module
    x.AddConsumers(typeof(NotificationModuleRegistration).Assembly);

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Otomatik migration
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
await DbInitializer.MigrateAsync(app.Services, startupLogger);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HayatiArac API v1"));
}

app.UseExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Module middleware
foreach (var module in modules)
{
    module.ConfigureMiddleware(app);
}

// Map endpoints
app.MapEndpoints();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.Run();
