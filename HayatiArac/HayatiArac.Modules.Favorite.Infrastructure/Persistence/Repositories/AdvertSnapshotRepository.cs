using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.Modules.Favorite.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Favorite.Infrastructure.Persistence.Repositories;

public class AdvertSnapshotRepository : IAdvertSnapshotRepository
{
    private readonly FavoriteDbContext _context;

    public AdvertSnapshotRepository(FavoriteDbContext context)
    {
        _context = context;
    }

    public async Task<AdvertSnapshot?> GetByAdvertIdAsync(Guid advertId, CancellationToken ct = default)
    {
        return await _context.AdvertSnapshots
            .FirstOrDefaultAsync(x => x.AdvertId == advertId, ct);
    }

    public async Task<List<AdvertSnapshot>> GetByAdvertIdsAsync(List<Guid> advertIds, CancellationToken ct = default)
    {
        return await _context.AdvertSnapshots
            .Where(x => advertIds.Contains(x.AdvertId))
            .ToListAsync(ct);
    }

    public async Task AddAsync(AdvertSnapshot snapshot, CancellationToken ct = default)
    {
        await _context.AdvertSnapshots.AddAsync(snapshot, ct);
    }

    public Task UpdateAsync(AdvertSnapshot snapshot, CancellationToken ct = default)
    {
        _context.AdvertSnapshots.Update(snapshot);
        return Task.CompletedTask;
    }
}
