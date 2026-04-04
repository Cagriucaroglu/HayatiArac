using HayatiArac.Modules.Messaging.Application.DTOs;
using HayatiArac.Modules.Messaging.Application.Queries.GetMyConversations;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Messaging.Infrastructure.Endpoints;

internal sealed class GetMyConversationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/conversations", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<List<ConversationDto>> result = await sender.Send(new GetMyConversationsQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetMyConversations")
        .WithSummary("Kullanıcının konuşmalarını getir")
        .WithTags("Messaging")
        .RequireAuthorization()
        .Produces<List<ConversationDto>>(StatusCodes.Status200OK);
    }
}
