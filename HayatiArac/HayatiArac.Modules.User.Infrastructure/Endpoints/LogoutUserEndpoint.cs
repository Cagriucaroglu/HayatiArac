using HayatiArac.Modules.User.Application.Commands.Logout;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.IdentityModel.Tokens.Jwt;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed class LogoutUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/logout", async (
            HttpContext httpContext,
            ISender sender,
            ITokenBlacklistService tokenBlacklistService,
            CancellationToken cancellationToken) =>
        {
            // Access token'ı blacklist'e ekle
            var jti = httpContext.User.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            var expClaim = httpContext.User.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (!string.IsNullOrEmpty(jti) && !string.IsNullOrEmpty(expClaim)
                && long.TryParse(expClaim, out var expUnix))
            {
                var expiry = DateTimeOffset.FromUnixTimeSeconds(expUnix);
                var remainingLifetime = expiry - DateTimeOffset.UtcNow;

                if (remainingLifetime > TimeSpan.Zero)
                    await tokenBlacklistService.RevokeAccessTokenAsync(jti, remainingLifetime, cancellationToken);
            }

            // Web istemcileri için refreshToken cookie'sini sil
            if (httpContext.Request.Cookies.ContainsKey("refreshToken"))
            {
                httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                });
            }

            // DB'den refresh token'ları revoke et
            await sender.Send(new LogoutUserCommand(), cancellationToken);

            return Results.Ok(new { message = "Cikis basarili." });
        })
        .WithName("LogoutUser")
        .WithSummary("Kullanici cikisi")
        .WithTags("Authentication")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK);
    }
}
