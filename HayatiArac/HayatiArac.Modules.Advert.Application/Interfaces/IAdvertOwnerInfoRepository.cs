using HayatiArac.Modules.Advert.Domain.Entities;

namespace HayatiArac.Modules.Advert.Application.Interfaces;

public interface IAdvertOwnerInfoRepository
{
    Task<AdvertOwnerInfo?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdvertOwnerInfo> AddAsync(AdvertOwnerInfo entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AdvertOwnerInfo entity, CancellationToken cancellationToken = default);
}
