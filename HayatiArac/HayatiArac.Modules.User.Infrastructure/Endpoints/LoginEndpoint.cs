using HayatiArac.Modules.User.Application.Commands.Login;
using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

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

            return result.IsSuccess
                ? AuthTokenResponse.Create(httpContext, result.Value)
                : result.ToProblem();
        })
        .WithName("Login")
        .WithSummary("User login with JWT")
        .WithDescription("Login with email/username and password. Mobile clients (X-Client-Type: mobile) receive both tokens in the response body; web clients receive only the access token — the refresh token is set as an HttpOnly cookie.")
        .WithTags("Authentication")
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitPolicies.Auth)
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces<LoginWebApiResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
