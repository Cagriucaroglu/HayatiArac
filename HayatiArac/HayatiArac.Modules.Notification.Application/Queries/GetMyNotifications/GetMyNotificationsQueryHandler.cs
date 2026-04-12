using HayatiArac.Modules.Notification.Application.DTOs;
using HayatiArac.Modules.Notification.Application.Interfaces;
using HayatiArac.Modules.Notification.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Notification.Application.Queries.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, Result<List<NotificationDto>>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        Guid? userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<List<NotificationDto>>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        List<AppNotification> notifications = await _notificationRepository.GetByUserIdAsync(userId.Value, cancellationToken);

        List<NotificationDto> result = notifications
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.Type.ToString(),
                n.IsRead,
                n.RelatedEntityId,
                n.CreatedAt))
            .ToList();

        return Result.Success(result);
    }
}
