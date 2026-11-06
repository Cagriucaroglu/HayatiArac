using HayatiArac.Api;
using HayatiArac.Modules.Advert.Infrastructure;
using HayatiArac.Modules.Advert.Infrastructure.Services;
using HayatiArac.Modules.Favorite.Infrastructure;
using HayatiArac.Modules.Messaging.Infrastructure;
using HayatiArac.Modules.Notification.Infrastructure;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Infrastructure;
using HayatiArac.SharedKernel.Infrastructure;
using MassTransit;
using Serilog;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();   

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HayatiArac API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    { 
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token'ınızı girin. Örnek: eyJhbGci..."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});


// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"];

if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT Secret yapılandırılmamış veya 32 karakterden kısa. Geliştirmede: dotnet user-secrets set \"Jwt:Secret\" \"<değer>\"; " +
        "sunucuda: EnvironmentFile içinde Jwt__Secret=<değer>.");
}

// İstek/yanıt gövdelerinde enum'lar okunabilir string olarak ("Gasoline"); sayı da kabul edilmeye devam eder.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Nginx arkasında gerçek istemci IP'si ve şeması (rate limiting, loglama, Origin kontrolü için).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownProxies.Clear();
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            status = StatusCodes.Status429TooManyRequests,
            title = "Çok Fazla İstek",
            detail = "Çok fazla deneme yaptınız. Lütfen biraz sonra tekrar deneyin."
        }, cancellationToken);
    };

    static string ClientIp(HttpContext ctx) => ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    options.AddPolicy(RateLimitPolicies.Auth, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ClientIp(ctx), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    options.AddPolicy(RateLimitPolicies.Otp, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ClientIp(ctx), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

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

builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();

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

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });
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

app.UseForwardedHeaders();

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.XContentTypeOptions = "nosniff";
    headers.XFrameOptions = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseExceptionHandling();

app.UseSerilogRequestLogging();

app.UseHangfireDashboard("/hangfire");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

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

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<PurgeExpiredAdvertsJob>(
    "purge-expired-adverts",
    job => job.ExecuteAsync(),
    Cron.Weekly(DayOfWeek.Sunday, 2));

app.Run();
