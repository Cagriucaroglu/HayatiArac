using HayatiArac.Modules.Favorite.Domain.Entities;

namespace HayatiArac.Modules.Favorite.Application.Interfaces;

public interface IAdvertSnapshotRepository
{
    Task<AdvertSnapshot?> GetByAdvertIdAsync(Guid advertId, CancellationToken ct = default);
    Task<List<AdvertSnapshot>> GetByAdvertIdsAsync(List<Guid> advertIds, CancellationToken ct = default);
    Task AddAsync(AdvertSnapshot snapshot, CancellationToken ct = default);
    Task UpdateAsync(AdvertSnapshot snapshot, CancellationToken ct = default);
}
