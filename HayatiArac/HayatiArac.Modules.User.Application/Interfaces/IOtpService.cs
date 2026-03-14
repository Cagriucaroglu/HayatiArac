namespace HayatiArac.Modules.User.Application.Interfaces;

public interface IOtpService
{
    /// <summary>Redis'e TTL=5dk ile 6 haneli OTP üretip depolar, kodu döner.</summary>
    Task<string> GenerateAndStoreAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Kodu doğrular; başarılıysa Redis'ten siler (replay koruması).</summary>
    Task<bool> ValidateAsync(string key, string code, CancellationToken cancellationToken = default);
}
