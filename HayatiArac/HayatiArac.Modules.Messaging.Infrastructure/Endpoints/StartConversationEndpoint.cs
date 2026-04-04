using HayatiArac.Modules.Messaging.Application.Commands.StartConversation;
using HayatiArac.Modules.Messaging.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace HayatiArac.Modules.Messaging.Infrastructure.Endpoints;

internal sealed class StartConversationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/conversations", async(
            [FromBody] StartConversationDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await sender.Send(new StartConversationCommand(request), cancellationToken);
            return result.IsSuccess
                   ? Results.Created($"api/conversations/{result.Value}", new { id = result.Value })
                   : Results.BadRequest(result.Error);
        })
        .WithName("StartConversation")
        .WithSummary("Yeni konuşma başlat veya mevcut konuşmaya mesaj ekle")
        .WithTags("Messaging")
        .RequireAuthorization()
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
