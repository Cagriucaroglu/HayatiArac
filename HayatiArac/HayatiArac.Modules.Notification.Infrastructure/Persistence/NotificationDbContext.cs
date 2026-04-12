using HayatiArac.Modules.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Notification.Infrastructure.Persistence;

public class NotificationDbContext : DbContext
{
    public const string Schema = "notifications";
    public DbSet<AppNotification> Notifications => Set<AppNotification>();

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
