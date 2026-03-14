using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace HayatiArac.Modules.User.Infrastructure.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = _settings.EnableSsl
        };

        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, "HayatiArac"),
            Subject = "E-posta Doğrulama Kodunuz",
            Body = $"Doğrulama kodunuz: {otp}\n\nBu kod 5 dakika geçerlidir.",
            IsBodyHtml = false
        };
        message.To.Add(email);

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("OTP e-postası gönderildi. E-posta: {Email}", email);
    }
}
