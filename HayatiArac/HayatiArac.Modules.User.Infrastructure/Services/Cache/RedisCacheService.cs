using System.Text.Json;
using HayatiArac.Modules.User.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace HayatiArac.Modules.User.Infrastructure.Services.Cache;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var data = await _distributedCache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(data))
            return default;

        return JsonSerializer.Deserialize<T>(data, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15)
        };

        var serialized = JsonSerializer.Serialize(value, _jsonOptions);
        await _distributedCache.SetStringAsync(key, serialized, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var data = await _distributedCache.GetStringAsync(key, cancellationToken);
        return !string.IsNullOrEmpty(data);
    }
}
