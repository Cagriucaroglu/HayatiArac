using Microsoft.AspNetCore.Http;

namespace HayatiArac.Modules.User.Infrastructure.Services.Authentication;

internal static class RefreshTokenCookie
{
    public const string Name = "refreshToken";

    // Cookie sadece refresh/logout endpoint'lerine gönderilsin; diğer API çağrılarına eklenmez.
    private const string CookiePath = "/api/users";

    public static void Append(HttpContext context, string token, DateTime expiresAtUtc) =>
        context.Response.Cookies.Append(Name, token, CreateOptions(expiresAtUtc));

    public static void Delete(HttpContext context) =>
        context.Response.Cookies.Delete(Name, CreateOptions(null));

    public static string? Read(HttpContext context) =>
        context.Request.Cookies.TryGetValue(Name, out var value) ? value : null;

    private static CookieOptions CreateOptions(DateTime? expiresAtUtc) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = CookiePath,
        Expires = expiresAtUtc,
        IsEssential = true
    };
}
