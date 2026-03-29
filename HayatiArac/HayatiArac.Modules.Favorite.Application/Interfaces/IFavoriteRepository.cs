using HayatiArac.Modules.Favorite.Domain.Entities;

namespace HayatiArac.Modules.Favorite.Application.Interfaces;

public interface IFavoriteRepository
{
    Task<SavedAdvert?> GetAsync(Guid userId, Guid advertId, CancellationToken ct = default);
    Task<List<SavedAdvert>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(SavedAdvert savedAdvert, CancellationToken ct = default);
    Task RemoveAsync(SavedAdvert savedAdvert, CancellationToken ct = default);
}
