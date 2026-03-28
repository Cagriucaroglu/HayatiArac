using HayatiArac.Modules.Advert.Domain.Entities;

namespace HayatiArac.Modules.Advert.Application.Interfaces;

public interface IFavoriteRepository
{
    Task<Favorite?> GetAsync(Guid userId, Guid advertId, CancellationToken ct = default);
    Task<List<Favorite>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Favorite favorite, CancellationToken ct = default);
    Task RemoveAsync(Favorite favorite, CancellationToken ct = default);
}
