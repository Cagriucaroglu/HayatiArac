using HayatiArac.Modules.User.Application.Commands.Register;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed class RegisterUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/register", async (
            [FromBody] RegisterUserCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Errors = result.Errors });
        })
        .WithName("RegisterUser")
        .WithSummary("Yeni kullanici kaydi")
        .WithTags("Users")
        .AllowAnonymous()
        .Produces<RegisterUserResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
