using HayatiArac.Modules.User.Application.Commands.Login;
using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.Modules.User.Infrastructure.Extensions;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed record LoginWebApiResponse(
    Guid UserId,
    string UserName,
    string Email,
    string Role,
    string AccessToken,
    DateTime AccessTokenExpiresAt);

internal sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/login", async (
            [FromBody] LoginRequest request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var command = new LoginCommand(
                request.EmailOrUsername,
                request.Password,
                request.RememberMe,
                ipAddress);

            var result = await sender.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });

            // Mobil istemciler: token'ları JSON body'de alır (Secure Storage kullanırlar)
            if (httpContext.IsMobileClient())
                return Results.Ok(result.Value);

            // Web istemcileri: RefreshToken HttpOnly cookie'ye taşınır (XSS koruması)
            httpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Value.RefreshTokenExpiresAt,
                Path = "/",
                IsEssential = true
            });

            return Results.Ok(new LoginWebApiResponse(
                result.Value.UserId,
                result.Value.UserName,
                result.Value.Email,
                result.Value.Role,
                result.Value.AccessToken,
                result.Value.AccessTokenExpiresAt));
        })
        .WithName("Login")
        .WithSummary("User login with JWT")
        .WithDescription("Login with email/username and password. Mobile clients (X-Client-Type: mobile) receive both tokens in the response body; web clients receive only the access token — the refresh token is set as an HttpOnly cookie.")
        .WithTags("Authentication")
        .AllowAnonymous()
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces<LoginWebApiResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
