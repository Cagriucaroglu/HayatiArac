using HayatiArac.Modules.User.Application.Commands.Register;
using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/register", async (
            [FromBody] RegisterRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName);

            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"api/users/{result.Value.UserId}", result.Value)
                : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        })
        .WithName("Register")
        .WithSummary("User registration")
        .WithDescription("Register a new user account")
        .WithTags("Authentication")
        .AllowAnonymous()
        .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
