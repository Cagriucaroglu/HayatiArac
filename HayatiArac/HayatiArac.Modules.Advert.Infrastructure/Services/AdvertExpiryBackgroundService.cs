using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.SharedKernel.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HayatiArac.Modules.Advert.Infrastructure.Services;

public sealed class AdvertExpiryBackgroundService : BackgroundService
{
    private const int BatchSize = 500;
    private const int DegreeOfParallelism = 4;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AdvertExpiryBackgroundService> _logger;

    public AdvertExpiryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AdvertExpiryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            _logger.LogInformation("Advert expiry servisi bir sonraki çalışma için {Delay} bekliyor.", delay);

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
                await ExpireAdvertsAsync(stoppingToken);
        }
    }

    private async Task ExpireAdvertsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Advert expiry işlemi başlatıldı.");

        var totalExpired = 0;
        var offset = 0;

        while (true)
        {
            List<Domain.Entities.Advert> batch;

            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IAdvertRepository>();
                batch = await repository.GetExpiredAdvertsBatchAsync(BatchSize, offset, ct);
            }

            if (batch.Count == 0)
                break;

            await Parallel.ForEachAsync(
                batch,
                new ParallelOptions { MaxDegreeOfParallelism = DegreeOfParallelism, CancellationToken = ct },
                async (advert, token) =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IAdvertRepository>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    advert.Expire();
                    await repository.UpdateAsync(advert, token);
                    await unitOfWork.SaveChangesAsync(token);
                });

            totalExpired += batch.Count;
            offset += batch.Count;

            if (batch.Count < BatchSize)
                break;
        }

        _logger.LogInformation("Advert expiry tamamlandı. Toplam sona erdirilen ilan: {Count}", totalExpired);
    }
}
