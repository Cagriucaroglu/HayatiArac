using HayatiArac.Modules.User.Domain.Entities;

namespace HayatiArac.Modules.User.Application.Interfaces;

public interface IJwtTokenService
{
    Task<(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt)>
        GenerateTokensAsync(ApplicationUser user, string ipAddress, CancellationToken cancellationToken = default);

    Task<string> GenerateAccessTokenAsync(ApplicationUser user);

    // Revoke/expire durumundan bağımsız döner; reuse detection için handler durumu kendisi değerlendirir.
    Task<RefreshToken?> FindRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
