using HayatiArac.Modules.User.Application.Interfaces;

namespace HayatiArac.Modules.User.Infrastructure.Services.Authentication;

public sealed class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ICacheService _cacheService;
    private const string BlacklistKeyPrefix = "revoked:jti:";

    public TokenBlacklistService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task RevokeAccessTokenAsync(string jti, TimeSpan remainingLifetime, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException("JTI cannot be null or empty", nameof(jti));

        var key = $"{BlacklistKeyPrefix}{jti}";
        await _cacheService.SetAsync(key, true, remainingLifetime, cancellationToken);
    }

    public async Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return false;

        var key = $"{BlacklistKeyPrefix}{jti}";
        return await _cacheService.ExistsAsync(key, cancellationToken);
    }
}
