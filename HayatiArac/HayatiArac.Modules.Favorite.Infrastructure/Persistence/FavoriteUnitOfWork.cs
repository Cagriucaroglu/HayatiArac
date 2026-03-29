using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.SharedKernel.Application.Interfaces;

namespace HayatiArac.Modules.Favorite.Infrastructure.Persistence;

public class FavoriteUnitOfWork : IFavoriteUnitOfWork
{
    private readonly FavoriteDbContext _context;

    public FavoriteUnitOfWork(FavoriteDbContext context)
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
