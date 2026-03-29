using HayatiArac.Modules.Favorite.Application.Commands.AddFavorite;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Favorite.Infrastructure.Endpoints;

internal sealed class AddFavoriteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/favorites/{advertId:guid}", async (
            Guid advertId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new AddFavoriteCommand(advertId);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("AddFavorite")
        .WithSummary("İlanı favorilere ekle")
        .WithTags("Favorites")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
