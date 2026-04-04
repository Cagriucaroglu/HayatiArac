using HayatiArac.Modules.Messaging.Application.DTOs;
using HayatiArac.Modules.Messaging.Application.Queries.GetConversationMessages;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Messaging.Infrastructure.Endpoints;

internal sealed class GetConversationMessagesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/conversations/{conversationId:guid}/messages", async (
            Guid conversationId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<List<MessageDto>> result = await sender.Send(new GetConversationMessagesQuery(conversationId), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetConversationMessages")
        .WithSummary("Konuşma mesajlarını getir")
        .WithTags("Messaging")
        .RequireAuthorization()
        .Produces<List<MessageDto>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
