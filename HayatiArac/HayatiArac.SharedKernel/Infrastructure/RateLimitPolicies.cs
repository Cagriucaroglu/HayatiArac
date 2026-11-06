namespace HayatiArac.SharedKernel.Infrastructure;

public static class RateLimitPolicies
{
    // Login, register, refresh gibi kimlik doğrulama uçları.
    public const string Auth = "auth";

    // OTP doğrulama ve yeniden gönderim uçları.
    public const string Otp = "otp";
}
