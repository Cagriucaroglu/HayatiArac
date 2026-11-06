using HayatiArac.Modules.User.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace HayatiArac.Modules.User.Infrastructure.Services;

public sealed class RedisOtpService : IOtpService
{
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);

    // 6 haneli kod 5 dakikada denenerek bulunamasın: bu kadar yanlış denemeden sonra kod geçersiz olur.
    private const int MaxFailedAttempts = 5;

    public RedisOtpService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<string> GenerateAndStoreAsync(string key, CancellationToken cancellationToken = default)
    {
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        await _cacheService.SetAsync(key, otp, OtpTtl, cancellationToken);
        await _cacheService.RemoveAsync(AttemptsKey(key), cancellationToken);
        return otp;
    }

    public async Task<bool> ValidateAsync(string key, string code, CancellationToken cancellationToken = default)
    {
        var stored = await _cacheService.GetAsync<string>(key, cancellationToken);
        if (stored == null)
            return false;

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(stored), Encoding.UTF8.GetBytes(code ?? string.Empty)))
        {
            var attempts = await _cacheService.GetAsync<int>(AttemptsKey(key), cancellationToken) + 1;

            if (attempts >= MaxFailedAttempts)
            {
                await _cacheService.RemoveAsync(key, cancellationToken);
                await _cacheService.RemoveAsync(AttemptsKey(key), cancellationToken);
            }
            else
            {
                await _cacheService.SetAsync(AttemptsKey(key), attempts, OtpTtl, cancellationToken);
            }

            return false;
        }

        // Başarılı doğrulama — replay koruması için sil
        await _cacheService.RemoveAsync(key, cancellationToken);
        await _cacheService.RemoveAsync(AttemptsKey(key), cancellationToken);
        return true;
    }

    private static string AttemptsKey(string key) => $"{key}:attempts";
}
