using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.Modules.User.Infrastructure.Extensions;
using HayatiArac.Modules.User.Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Http;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed record LoginWebApiResponse(
    Guid UserId,
    string UserName,
    string Email,
    string Role,
    string AccessToken,
    DateTime AccessTokenExpiresAt);

internal static class AuthTokenResponse
{
    // Mobil istemciler token'ları body'de alır (Secure Storage kullanırlar);
    // web istemcilerinde refresh token HttpOnly cookie'ye taşınır, JS'e hiç verilmez.
    public static IResult Create(HttpContext httpContext, LoginResponse response)
    {
        if (httpContext.IsMobileClient())
            return Results.Ok(response);

        RefreshTokenCookie.Append(httpContext, response.RefreshToken, response.RefreshTokenExpiresAt);

        return Results.Ok(new LoginWebApiResponse(
            response.UserId,
            response.UserName,
            response.Email,
            response.Role,
            response.AccessToken,
            response.AccessTokenExpiresAt));
    }
}
