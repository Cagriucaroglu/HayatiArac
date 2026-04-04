using HayatiArac.Modules.Messaging.Application.Interfaces;

namespace HayatiArac.Modules.Messaging.Infrastructure.Persistence;

public class MessagingUnitOfWork : IMessagingUnitOfWork
{
    private readonly MessagingDbContext _context;

    public MessagingUnitOfWork(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
