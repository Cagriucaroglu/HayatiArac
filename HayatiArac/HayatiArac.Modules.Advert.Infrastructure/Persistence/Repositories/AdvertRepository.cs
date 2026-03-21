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
    /// Returns PagedResultDto with items, total count, and pagination info
    /// </summary>
    public async Task<PagedResultDto<Domain.Entities.Advert>> SearchAdvertsPaginatedAsync(
        string? searchTerm,
        Guid? categoryId,
        string? city,
        AdvertStatus? status,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Input validation
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Max 100 items per page

        // Build query with eager loading
        var query = _context.Adverts
            .Include(a => a.Category)
            .Include(a => a.OwnerInfo)
            .Include(a => a.Images)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a =>
                a.Title.Contains(searchTerm) ||
                a.Description.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(a => a.Location.City.Contains(city));
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }
        else
        {
            // Default: show only active adverts if no status filter
            query = query.Where(a => a.Status == AdvertStatus.Active);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(a => a.Price.Amount >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(a => a.Price.Amount <= maxPrice.Value);
        }

        // Get total count (before pagination)
        var totalCount = await query.CountAsync(cancellationToken);

        // Calculate pagination
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
}
