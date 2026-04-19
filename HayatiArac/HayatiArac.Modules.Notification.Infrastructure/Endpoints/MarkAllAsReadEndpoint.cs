using HayatiArac.Modules.Notification.Application.Commands.MarkAllAsRead;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Notification.Infrastructure.Endpoints;

internal sealed class MarkAllAsReadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/notifications/read-all", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result result = await sender.Send(new MarkAllAsReadCommand(), cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("MarkAllNotificationsAsRead")
        .WithSummary("Tüm bildirimleri okundu olarak işaretle")
        .WithTags("Notifications")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent);
    }
}
