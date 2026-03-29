using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.Modules.Favorite.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Favorite.Infrastructure.Persistence.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly FavoriteDbContext _context;

    public FavoriteRepository(FavoriteDbContext context)
    {
        _context = context;
    }

    public async Task<SavedAdvert?> GetAsync(Guid userId, Guid advertId, CancellationToken ct = default)
    {
        return await _context.SavedAdverts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.AdvertId == advertId, ct);
    }

    public async Task<List<SavedAdvert>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.SavedAdverts
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(SavedAdvert savedAdvert, CancellationToken ct = default)
    {
        await _context.SavedAdverts.AddAsync(savedAdvert, ct);
    }

    public Task RemoveAsync(SavedAdvert savedAdvert, CancellationToken ct = default)
    {
        _context.SavedAdverts.Remove(savedAdvert);
        return Task.CompletedTask;
    }
}
