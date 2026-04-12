using HayatiArac.Modules.Notification.Domain.Enums;
using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Notification.Domain.Entities;

public class AppNotification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    private AppNotification() { }

    public static AppNotification Create(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedEntityId = null)
    {
        return new AppNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            RelatedEntityId = relatedEntityId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
