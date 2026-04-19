using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.IntegrationEvents;
using HayatiArac.SharedKernel.Application.Interfaces;
using MassTransit;
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
            DateTime now = DateTime.UtcNow;
            DateTime nextMidnight = now.Date.AddDays(1);
            TimeSpan delay = nextMidnight - now;

            _logger.LogInformation("Advert expiry servisi bir sonraki çalışma için {Delay} bekliyor.", delay);

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
                await ExpireAdvertsAsync(stoppingToken);
        }
    }

    private async Task ExpireAdvertsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Advert expiry işlemi başlatıldı.");

        int totalExpired = 0;
        int offset = 0;

        while (true)
        {
            List<Domain.Entities.Advert> batch;

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                IAdvertRepository repository = scope.ServiceProvider.GetRequiredService<IAdvertRepository>();
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
                    IAdvertRepository repository = scope.ServiceProvider.GetRequiredService<IAdvertRepository>();
                    IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    IPublishEndpoint publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                    advert.Expire();
                    await repository.UpdateAsync(advert, token);
                    await unitOfWork.SaveChangesAsync(token);

                    await publishEndpoint.Publish(new AdvertExpiredIntegrationEvent
                    {
                        AdvertId = advert.Id,
                        Title = advert.Title,
                        OwnerUserId = advert.OwnerUserId
                    }, token);
                });

            totalExpired += batch.Count;
            offset += batch.Count;

            if (batch.Count < BatchSize)
                break;
        }

        _logger.LogInformation("Advert expiry tamamlandı. Toplam sona erdirilen ilan: {Count}", totalExpired);
    }
}
