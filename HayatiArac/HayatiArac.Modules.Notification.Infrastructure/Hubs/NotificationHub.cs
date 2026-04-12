using HayatiArac.SharedKernel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HayatiArac.Modules.Notification.Infrastructure.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentUserService _currentUserService;

    public NotificationHub(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async Task OnConnectedAsync()
    {
        Guid? userId = _currentUserService.UserId;
        if (userId.HasValue)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId.Value}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Guid? userId = _currentUserService.UserId;
        if (userId.HasValue)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId.Value}");

        await base.OnDisconnectedAsync(exception);
    }
}
