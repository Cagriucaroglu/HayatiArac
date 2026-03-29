using HayatiArac.Modules.Favorite.Application.DTOs;
using HayatiArac.Modules.Favorite.Application.Queries.GetUserFavorites;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Favorite.Infrastructure.Endpoints;

internal sealed class GetUserFavoritesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/favorites", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserFavoritesQuery();
            var result = await sender.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetUserFavorites")
        .WithSummary("Kullanıcının favori ilanlarını getir")
        .WithTags("Favorites")
        .RequireAuthorization()
        .Produces<List<FavoriteDto>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
