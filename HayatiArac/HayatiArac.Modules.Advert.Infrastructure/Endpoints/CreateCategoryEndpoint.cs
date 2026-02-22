using HayatiArac.Modules.Advert.Application.Commands.CreateCategory;
using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Advert.Infrastructure.Endpoints;

internal sealed class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/adverts/categories", async (
            [FromBody] CreateCategoryDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCategoryCommand(
                request.Name,
                request.Slug,
                request.Description,
                request.ParentCategoryId,
                request.DisplayOrder);

            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"api/adverts/categories/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .WithName("CreateCategory")
        .WithSummary("Yeni kategori oluştur (Admin only)")
        .WithTags("Adverts")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
