using HayatiArac.Modules.User.Application.Interfaces;

namespace HayatiArac.Modules.User.Infrastructure.Services;

public sealed class RedisOtpService : IOtpService
{
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);

    public RedisOtpService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<string> GenerateAndStoreAsync(string key, CancellationToken cancellationToken = default)
    {
        var otp = Random.Shared.Next(100000, 999999).ToString();
        await _cacheService.SetAsync(key, otp, OtpTtl, cancellationToken);
        return otp;
    }

    public async Task<bool> ValidateAsync(string key, string code, CancellationToken cancellationToken = default)
    {
        var stored = await _cacheService.GetAsync<string>(key, cancellationToken);

        if (stored == null || stored != code)
            return false;

        // Başarılı doğrulama — replay koruması için sil
        await _cacheService.RemoveAsync(key, cancellationToken);
        return true;
    }
}
