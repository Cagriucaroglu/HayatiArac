using HayatiArac.Modules.Advert.Application.Commands.CreateAdvert;
using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Advert.Infrastructure.Endpoints;

internal sealed class CreateAdvertEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/adverts", async (
            [FromBody] CreateAdvertDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateAdvertCommand(request);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"api/adverts/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .WithName("CreateAdvert")
        .WithSummary("Yeni ilan oluştur")
        .WithTags("Adverts")
        .RequireAuthorization()
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
