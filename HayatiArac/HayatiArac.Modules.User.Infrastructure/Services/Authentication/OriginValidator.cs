using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.User.Infrastructure.Services.Authentication;

// Cookie ile çalışan endpoint'lerde (refresh) SameSite=Strict'e ek CSRF savunması:
// istek, aynı origin'den ya da Security:AllowedOrigins listesindeki bir origin'den gelmeli.
internal static class OriginValidator
{
    public static bool IsAllowed(HttpContext context)
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (string.IsNullOrEmpty(origin))
            return false;

        var sameOrigin = $"{context.Request.Scheme}://{context.Request.Host}";
        if (string.Equals(origin, sameOrigin, StringComparison.OrdinalIgnoreCase))
            return true;

        var allowedOrigins = context.RequestServices
            .GetRequiredService<IConfiguration>()
            .GetSection("Security:AllowedOrigins")
            .Get<string[]>() ?? [];

        return allowedOrigins.Any(o => string.Equals(o.TrimEnd('/'), origin, StringComparison.OrdinalIgnoreCase));
    }
}
