using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Application.Queries.SearchAdverts;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Advert.Infrastructure.Endpoints;

internal sealed class SearchAdvertsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/adverts/search", async (
            [FromBody] SearchAdvertsRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            // Herkese açık aramada sadece aktif ilanlar listelenir; istemcinin gönderdiği status yok sayılır
            // (aksi halde silinmiş/pasif ilanlar anonim olarak görülebilirdi).
            var query = new SearchAdvertsQuery(request with { Status = null });
            var result = await sender.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("SearchAdverts")
        .WithSummary("İlanları arama ve filtreleme")
        .WithTags("Adverts")
        .AllowAnonymous()
        .Produces<PagedResultDto<AdvertDto>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
