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
                ipAddress);er

            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        })
        .WithName("Login")
        .WithSummary("User login with JWT")
        .WithDescription("Login with email/username and password. Returns JWT access and refresh tokens.")
        .WithTags("Authentication")
        .AllowAnonymous()
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
