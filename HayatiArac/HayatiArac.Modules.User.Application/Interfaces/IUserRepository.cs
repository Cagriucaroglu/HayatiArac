using HayatiArac.Modules.User.Domain.Entities;

namespace HayatiArac.Modules.User.Application.Interfaces;

public interface IUserRepository
{
    Task<UserProfile?> GetProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CreateProfileAsync(UserProfile profile, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(UserProfile profile, CancellationToken cancellationToken = default);
}
