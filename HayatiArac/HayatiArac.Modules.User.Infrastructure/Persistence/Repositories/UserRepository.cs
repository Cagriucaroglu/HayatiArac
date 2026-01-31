using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.User.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task CreateProfileAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        await _context.UserProfiles.AddAsync(profile, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateProfileAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        _context.UserProfiles.Update(profile);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
