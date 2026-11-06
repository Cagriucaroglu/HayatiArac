using FluentAssertions;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Infrastructure.Services;
using System.Text.Json;

namespace HayatiArac.Modules.User.UnitTests.Auth;

public class RedisOtpServiceTests
{
    private const string Key = "otp:email:test-user";

    private readonly InMemoryCacheService _cache = new();
    private readonly RedisOtpService _service;

    public RedisOtpServiceTests()
    {
        _service = new RedisOtpService(_cache);
    }

    [Fact]
    public async Task Validate_ReturnsTrue_AndConsumesCode_WhenCorrect()
    {
        var code = await _service.GenerateAndStoreAsync(Key);

        (await _service.ValidateAsync(Key, code)).Should().BeTrue();
        (await _service.ValidateAsync(Key, code)).Should().BeFalse("kod tek kullanımlık olmalı");
    }

    [Fact]
    public async Task Validate_InvalidatesCode_AfterTooManyWrongAttempts()
    {
        var code = await _service.GenerateAndStoreAsync(Key);
        var wrong = code == "111111" ? "222222" : "111111";

        for (var i = 0; i < 5; i++)
            (await _service.ValidateAsync(Key, wrong)).Should().BeFalse();

        (await _service.ValidateAsync(Key, code)).Should().BeFalse("5 yanlış denemeden sonra doğru kod da reddedilmeli");
    }

    [Fact]
    public async Task Validate_StillAcceptsCode_BeforeAttemptLimit()
    {
        var code = await _service.GenerateAndStoreAsync(Key);
        var wrong = code == "111111" ? "222222" : "111111";

        for (var i = 0; i < 4; i++)
            await _service.ValidateAsync(Key, wrong);

        (await _service.ValidateAsync(Key, code)).Should().BeTrue();
    }

    [Fact]
    public async Task Generate_ProducesSixDigitCode()
    {
        var code = await _service.GenerateAndStoreAsync(Key);

        code.Should().MatchRegex(@"^\d{6}$");
    }

    private sealed class InMemoryCacheService : ICacheService
    {
        private readonly Dictionary<string, string> _store = new();

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(_store.TryGetValue(key, out var json) ? JsonSerializer.Deserialize<T>(json) : default);

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            _store[key] = JsonSerializer.Serialize(value);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _store.Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(_store.ContainsKey(key));
    }
}
