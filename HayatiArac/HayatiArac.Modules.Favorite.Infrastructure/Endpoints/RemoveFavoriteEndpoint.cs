using HayatiArac.Modules.Favorite.Application.Commands.RemoveFavorite;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Favorite.Infrastructure.Endpoints;

internal sealed class RemoveFavoriteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/favorites/{advertId:guid}", async (
            Guid advertId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveFavoriteCommand(advertId);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("RemoveFavorite")
        .WithSummary("İlanı favorilerden kaldır")
        .WithTags("Favorites")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
