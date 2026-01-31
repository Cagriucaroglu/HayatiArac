using HayatiArac.SharedKernel.Application.Interfaces;

namespace HayatiArac.Modules.Advert.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AdvertDbContext _context;

    public UnitOfWork(AdvertDbContext context)
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
