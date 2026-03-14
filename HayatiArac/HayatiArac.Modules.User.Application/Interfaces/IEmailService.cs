namespace HayatiArac.Modules.User.Application.Interfaces;

public interface IEmailService
{
    Task SendOtpAsync(string email, string otp, CancellationToken cancellationToken = default);
}
