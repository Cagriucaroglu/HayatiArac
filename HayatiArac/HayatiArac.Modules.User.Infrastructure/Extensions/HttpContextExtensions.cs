using Microsoft.AspNetCore.Http;

namespace HayatiArac.Modules.User.Infrastructure.Extensions;

internal static class HttpContextExtensions
{
    private const string ClientTypeHeader = "X-Client-Type";

    public static bool IsMobileClient(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ClientTypeHeader, out var clientType))
            return string.Equals(clientType.ToString(), "mobile", StringComparison.OrdinalIgnoreCase);

        return false;
    }
}
