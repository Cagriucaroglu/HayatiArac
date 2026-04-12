using HayatiArac.Modules.Notification.Domain.Entities;

namespace HayatiArac.Modules.Notification.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<AppNotification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<AppNotification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AppNotification notification, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
