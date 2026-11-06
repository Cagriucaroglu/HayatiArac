using HayatiArac.Modules.User.Application.Commands.Refresh;
using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.Modules.User.Infrastructure.Extensions;
using HayatiArac.Modules.User.Infrastructure.Services.Authentication;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed record RefreshTokenRequest(string? RefreshToken);

internal sealed class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/refresh", async (
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            string? refreshToken;

            if (httpContext.IsMobileClient())
            {
                // Mobil istemci cookie kullanmaz; token body'de gelir (CSRF riski yok).
                var body = await httpContext.Request.ReadFromJsonAsync<RefreshTokenRequest>(cancellationToken);
                refreshToken = body?.RefreshToken;
            }
            else
            {
                if (!OriginValidator.IsAllowed(httpContext))
                    return Error.Forbidden("Auth.InvalidOrigin", "İstek kaynağı doğrulanamadı.").ToProblem();

                refreshToken = RefreshTokenCookie.Read(httpContext);
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Error.Unauthorized("Auth.MissingRefreshToken", "Oturum bulunamadı.").ToProblem();

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var result = await sender.Send(new RefreshTokenCommand(refreshToken, ipAddress), cancellationToken);

            if (result.IsFailure)
            {
                if (!httpContext.IsMobileClient())
                    RefreshTokenCookie.Delete(httpContext);

                return result.ToProblem();
            }

            return AuthTokenResponse.Create(httpContext, result.Value);
        })
        .WithName("RefreshToken")
        .WithSummary("Access token yenileme")
        .WithDescription("Web: HttpOnly refreshToken cookie'si ile çalışır, Origin doğrulanır. Mobil (X-Client-Type: mobile): body'de { refreshToken } beklenir. Her çağrıda refresh token rotate edilir.")
        .WithTags("Authentication")
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitPolicies.Auth)
        .Produces<LoginWebApiResponse>(StatusCodes.Status200OK)
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }
}
