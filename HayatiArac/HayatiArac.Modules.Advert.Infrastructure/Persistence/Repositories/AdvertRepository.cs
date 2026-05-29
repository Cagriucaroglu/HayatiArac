using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Advert.Infrastructure.Persistence.Repositories;

public class AdvertRepository : IAdvertRepository
{
    private readonly AdvertDbContext _context;

    public AdvertRepository(AdvertDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Advert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Adverts.FindAsync([id], cancellationToken);
    }

    public async Task<Domain.Entities.Advert?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Adverts
            .Include(a => a.Category)
            .Include(a => a.OwnerInfo)
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Advert>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Adverts.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Search adverts with pagination and eager loading (similar to ProductRepository pattern)
    public async Task<PagedResultDto<Domain.Entities.Advert>> SearchAdvertsPaginatedAsync(
        SearchAdvertsRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = dto.PageNumber < 1 ? 1 : dto.PageNumber;
        var pageSize = dto.PageSize < 1 ? 20 : dto.PageSize > 100 ? 100 : dto.PageSize;

        var query = _context.Adverts
            .Include(a => a.Category)
            .Include(a => a.OwnerInfo)
            .Include(a => a.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.SearchTerm))
            query = query.Where(a =>
                a.Title.Contains(dto.SearchTerm) ||
                a.Description.Contains(dto.SearchTerm));

        if (dto.CategoryId.HasValue)
            query = query.Where(a => a.CategoryId == dto.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(dto.City))
            query = query.Where(a => a.Location.City.Contains(dto.City));

        if (dto.Status.HasValue)
            query = query.Where(a => a.Status == dto.Status.Value);
        else
            query = query.Where(a => a.Status == AdvertStatus.Active);

        if (dto.MinPrice.HasValue)
            query = query.Where(a => a.Price.Amount >= dto.MinPrice.Value);

        if (dto.MaxPrice.HasValue)
            query = query.Where(a => a.Price.Amount <= dto.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(dto.Brand))
            query = query.Where(a => a.Brand.Contains(dto.Brand));

        if (!string.IsNullOrWhiteSpace(dto.Model))
            query = query.Where(a => a.Model.Contains(dto.Model));

        if (dto.MinYear.HasValue)
            query = query.Where(a => a.Year >= dto.MinYear.Value);

        if (dto.MaxYear.HasValue)
            query = query.Where(a => a.Year <= dto.MaxYear.Value);

        if (dto.MaxMileage.HasValue)
            query = query.Where(a => a.Mileage <= dto.MaxMileage.Value);

        if (dto.FuelType.HasValue)
            query = query.Where(a => a.FuelType == dto.FuelType.Value);

        if (dto.TransmissionType.HasValue)
            query = query.Where(a => a.TransmissionType == dto.TransmissionType.Value);

        if (dto.HasHeavyDamageRecord.HasValue)
            query = query.Where(a => a.HasHeavyDamageRecord == dto.HasHeavyDamageRecord.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (pageNumber - 1) * pageSize;
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        // Get paginated results with ordering (newest first)
        var adverts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<Domain.Entities.Advert>(
            adverts,
            totalCount,
            pageNumber,
            pageSize,
            totalPages);
    }

    public async Task<List<Domain.Entities.Advert>> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Adverts
            .Include(a => a.Category)
            .Include(a => a.OwnerInfo)
            .Include(a => a.Images)
            .Where(a => a.OwnerUserId == ownerUserId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Advert?> GetActiveAdvertByUserAsync(Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Adverts
            .FirstOrDefaultAsync(a =>
                a.OwnerUserId == ownerUserId &&
                a.Status == AdvertStatus.Active,
                cancellationToken);
    }

    public async Task<List<Domain.Entities.Advert>> GetExpiredAdvertsBatchAsync(
        int batchSize, int offset, CancellationToken cancellationToken = default)
    {
        return await _context.Adverts
            .Where(a => a.Status == AdvertStatus.Active && a.ExpiresAt <= DateTime.UtcNow)
            .OrderBy(a => a.ExpiresAt)
            .Skip(offset)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Advert> AddAsync(Domain.Entities.Advert entity, CancellationToken cancellationToken = default)
    {
        await _context.Adverts.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Domain.Entities.Advert entity, CancellationToken cancellationToken = default)
    {
        _context.Adverts.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.Advert entity, CancellationToken cancellationToken = default)
    {
        _context.Adverts.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> DeleteOldExpiredAdvertsAsync(DateTime cutOffDate, CancellationToken cancellationToken = default)
    {
        return await _context.Adverts.Where(a => a.Status == AdvertStatus.Expired && a.ExpiresAt <= cutOffDate)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
