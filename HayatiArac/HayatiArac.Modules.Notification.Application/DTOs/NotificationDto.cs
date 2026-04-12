namespace HayatiArac.Modules.Notification.Application.DTOs;

public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    Guid? RelatedEntityId,
    DateTime CreatedAt
);
