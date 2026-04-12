using HayatiArac.Modules.Notification.Application.Interfaces;
using HayatiArac.Modules.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Notification.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AppNotification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<AppNotification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task AddAsync(AppNotification notification, CancellationToken ct = default)
    {
        await _context.Notifications.AddAsync(notification, ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.UpdatedAt, DateTime.UtcNow),
                ct);
    }
}
