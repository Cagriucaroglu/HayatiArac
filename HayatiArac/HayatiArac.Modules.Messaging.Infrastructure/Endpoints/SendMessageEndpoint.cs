using HayatiArac.Modules.Messaging.Application.Commands.SendMessage;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Messaging.Infrastructure.Endpoints;

internal sealed class SendMessageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/conversations/{conversationId:guid}/messages", async (
            Guid conversationId,
            [FromBody] string content,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await sender.Send(new SendMessageCommand(conversationId, content), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .WithName("SendMessage")
        .WithSummary("Konuşmaya mesaj gönder")
        .WithTags("Messaging")
        .RequireAuthorization()
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }
}
