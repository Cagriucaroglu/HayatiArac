using HayatiArac.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Messaging.Infrastructure.Persistence;

public class MessagingDbContext : DbContext
{
    public const string Schema = "messaging";

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    public MessagingDbContext(DbContextOptions<MessagingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MessagingDbContext).Assembly);
    }
}
