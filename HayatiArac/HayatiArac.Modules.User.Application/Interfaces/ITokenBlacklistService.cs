namespace HayatiArac.Modules.User.Application.Interfaces;

public interface ITokenBlacklistService
{
    Task RevokeAccessTokenAsync(string jti, TimeSpan remainingLifetime, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default);
}
