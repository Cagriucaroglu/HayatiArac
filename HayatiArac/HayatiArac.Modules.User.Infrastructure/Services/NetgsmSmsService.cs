using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace HayatiArac.Modules.User.Infrastructure.Services;

public sealed class NetgsmSmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly NetgsmSettings _settings;
    private readonly ILogger<NetgsmSmsService> _logger;

    private const string NetgsmApiUrl = "https://api.netgsm.com.tr/sms/send/otp";

    public NetgsmSmsService(
        HttpClient httpClient,
        IOptions<NetgsmSettings> settings,
        ILogger<NetgsmSmsService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendOtpAsync(string phoneNumber, string otp, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            apikey = _settings.ApiKey,
            otp,
            tel = phoneNumber,
            msgbaslik = _settings.Sender
        };

        var response = await _httpClient.PostAsJsonAsync(NetgsmApiUrl, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Netgsm SMS gönderilemedi. Telefon: {Phone}, Status: {Status}",
                phoneNumber, response.StatusCode);
        }
        else
        {
            _logger.LogInformation("OTP SMS gönderildi. Telefon: {Phone}", phoneNumber);
        }
    }
}
