using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.SharedKernel.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HayatiArac.Modules.Advert.Infrastructure.Services;

public sealed class PurgeExpiredAdvertsJob(
    IAdvertRepository advertRepository,
    IUnitOfWork unitOfWork,
    ILogger<PurgeExpiredAdvertsJob> logger)
{
    public async Task ExecuteAsync()
    {
        DateTime cutoff = DateTime.UtcNow.AddDays(-30);
        int deleted = await advertRepository.DeleteOldExpiredAdvertsAsync(cutoff);
        logger.LogInformation("Purged {Count} expired adverts older than {Cutoff:yyyy-MM- dd}", deleted, cutoff);
    }
}
