using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.Modules.User.Application.Queries.GetCurrentUser;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/me", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCurrentUserQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Unauthorized();
        })
        .WithName("GetCurrentUser")
        .WithSummary("Mevcut kullanici bilgilerini getir")
        .WithTags("Users")
        .RequireAuthorization()
        .Produces<UserProfileDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
