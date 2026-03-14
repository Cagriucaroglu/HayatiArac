namespace HayatiArac.Modules.User.Application.Interfaces;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string otp, CancellationToken cancellationToken = default);
}
