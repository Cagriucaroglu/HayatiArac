using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Application.Queries.GetAdvertById;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Advert.Infrastructure.Endpoints;

internal sealed class GetAdvertByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/adverts/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAdvertByIdQuery(id);
            var result = await sender.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetAdvertById")
        .WithSummary("İlan detayını getir")
        .WithTags("Adverts")
        .AllowAnonymous()
        .Produces<AdvertDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
