using HayatiArac.Modules.User.Domain.Entities;

namespace HayatiArac.Modules.User.Application.Interfaces;

public interface IJwtTokenService
{
    Task<(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt)> 
        GenerateTokensAsync(ApplicationUser user, string ipAddress, CancellationToken cancellationToken = default);
    
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    
    Task<RefreshToken?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
