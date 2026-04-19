using HayatiArac.Modules.Notification.Application.Commands.MarkAsRead;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Notification.Infrastructure.Endpoints;

internal sealed class MarkAsReadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/notifications/{id:guid}/read", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result result = await sender.Send(new MarkAsReadCommand(id), cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("MarkNotificationAsRead")
        .WithSummary("Bildirimi okundu olarak işaretle")
        .WithTags("Notifications")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent);
    }
}
