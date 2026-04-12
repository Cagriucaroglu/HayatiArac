using HayatiArac.Modules.Notification.Application.Interfaces;

namespace HayatiArac.Modules.Notification.Infrastructure.Persistence;

public class NotificationUnitOfWork(NotificationDbContext context) : INotificationUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        context.Dispose();
    }
}
