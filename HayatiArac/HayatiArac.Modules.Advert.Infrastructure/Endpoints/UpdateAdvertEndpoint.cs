using HayatiArac.Modules.Advert.Application.Commands.UpdateAdvert;
using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Advert.Infrastructure.Endpoints;

internal sealed class UpdateAdvertEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/adverts/{id:guid}", async (
            Guid id,
            [FromBody] UpdateAdvertDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (id != request.Id)
                return Results.BadRequest("URL'deki ID ile body'deki ID uyuşmuyor.");

            var command = new UpdateAdvertCommand(request);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("UpdateAdvert")
        .WithSummary("İlanı güncelle")
        .WithTags("Adverts")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }
}
