using HayatiArac.Modules.User.Application.Commands.Login;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed class LoginUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/login", async (
            [FromBody] LoginUserCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { Message = "Giris basarili." })
                : Results.Json(new { Error = result.Error }, statusCode: StatusCodes.Status401Unauthorized);
        })
        .WithName("LoginUser")
        .WithSummary("Kullanici girisi")
        .WithTags("Users")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
