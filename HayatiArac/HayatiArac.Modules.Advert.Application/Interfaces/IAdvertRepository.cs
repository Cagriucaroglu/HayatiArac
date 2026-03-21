using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Domain.Entities;
using HayatiArac.Modules.Advert.Domain.Enums;
using HayatiArac.SharedKernel.Application.Interfaces;

namespace HayatiArac.Modules.Advert.Application.Interfaces;

public interface IAdvertRepository : IRepository<Domain.Entities.Advert>
{
    Task<Domain.Entities.Advert?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search adverts with pagination and eager loading (Category, OwnerInfo, Images)
    /// Returns PagedResultDto with items, total count, and pagination info
    /// </summary>
    Task<PagedResultDto<Domain.Entities.Advert>> SearchAdvertsPaginatedAsync(
        string? searchTerm,
        Guid? categoryId,
        string? city,
        AdvertStatus? status,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<List<Domain.Entities.Advert>> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default);

    /// <summary>Kullanıcının aktif ilanı varsa döner (tek ilan kuralı).</summary>
    Task<Domain.Entities.Advert?> GetActiveAdvertByUserAsync(Guid ownerUserId, CancellationToken cancellationToken = default);

    /// <summary>Süresi dolmuş ve henüz Expired statüsüne geçirilmemiş ilanları batch olarak döner.</summary>
    Task<List<Domain.Entities.Advert>> GetExpiredAdvertsBatchAsync(int batchSize, int offset, CancellationToken cancellationToken = default);
}
