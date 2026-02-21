using HayatiArac.Modules.User.Application.Commands.EnsureAdminExists;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HayatiArac.Modules.User.Infrastructure.Services;

/// <summary>
/// Background service that initializes database with required data on application startup.
/// Follows CQRS pattern by delegating to application commands.
/// </summary>
public class DatabaseInitializerService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializerService> _logger;

    public DatabaseInitializerService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseInitializerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 Database initialization started...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            // Ensure admin user exists
            var result = await mediator.Send(new EnsureAdminExistsCommand(), cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Database initialization completed successfully");
            }
            else
            {
                _logger.LogError("❌ Database initialization failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ An error occurred during database initialization");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database initializer service stopped");
        return Task.CompletedTask;
    }
}
