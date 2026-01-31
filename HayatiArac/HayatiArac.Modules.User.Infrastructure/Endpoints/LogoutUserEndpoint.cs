using HayatiArac.Modules.User.Application.Commands.Logout;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed class LogoutUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/logout", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            await sender.Send(new LogoutUserCommand(), cancellationToken);
            return Results.Ok(new { Message = "Cikis basarili." });
        })
        .WithName("LogoutUser")
        .WithSummary("Kullanici cikisi")
        .WithTags("Users")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK);
    }
}
