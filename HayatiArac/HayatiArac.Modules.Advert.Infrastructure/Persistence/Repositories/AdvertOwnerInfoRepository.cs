using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Advert.Infrastructure.Persistence.Repositories;

public class AdvertOwnerInfoRepository : IAdvertOwnerInfoRepository
{
    private readonly AdvertDbContext _context;

    public AdvertOwnerInfoRepository(AdvertDbContext context)
    {
        _context = context;
    }

    public async Task<AdvertOwnerInfo?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AdvertOwnerInfos
            .FirstOrDefaultAsync(o => o.UserId == userId, cancellationToken);
    }

    public async Task<AdvertOwnerInfo> AddAsync(AdvertOwnerInfo entity, CancellationToken cancellationToken = default)
    {
        await _context.AdvertOwnerInfos.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(AdvertOwnerInfo entity, CancellationToken cancellationToken = default)
    {
        _context.AdvertOwnerInfos.Update(entity);
        return Task.CompletedTask;
    }
}
