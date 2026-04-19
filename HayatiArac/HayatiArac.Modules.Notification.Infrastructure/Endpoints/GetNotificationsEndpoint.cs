using HayatiArac.Modules.Notification.Application.DTOs;
using HayatiArac.Modules.Notification.Application.Queries.GetMyNotifications;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.Notification.Infrastructure.Endpoints;

internal sealed class GetNotificationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/notifications", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<List<NotificationDto>> result = await sender.Send(new GetMyNotificationsQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetMyNotifications")
        .WithSummary("Kullanıcının bildirimlerini getir")
        .WithTags("Notifications")
        .RequireAuthorization()
        .Produces<List<NotificationDto>>(StatusCodes.Status200OK);
    }
}
