using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HayatiArac.Modules.User.Application.Commands.Refresh;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private static readonly Error InvalidRefreshToken =
        Error.Unauthorized("Auth.InvalidRefreshToken", "Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.");

    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _jwtTokenService.FindRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (storedToken is null)
            return Result.Failure<LoginResponse>(InvalidRefreshToken);

        // Daha önce kullanılmış (rotate edilmiş) bir token tekrar geldiyse çalınmış olabilir:
        // kullanıcının tüm oturumları sonlandırılır.
        if (storedToken.IsRevoked)
        {
            _logger.LogWarning("Revoke edilmiş refresh token tekrar kullanıldı. UserId: {UserId}, IP: {Ip}",
                storedToken.UserId, request.IpAddress);
            await _refreshTokenRepository.RevokeAllUserTokensAsync(storedToken.UserId, cancellationToken);
            return Result.Failure<LoginResponse>(InvalidRefreshToken);
        }

        if (storedToken.IsExpired)
            return Result.Failure<LoginResponse>(InvalidRefreshToken);

        var user = storedToken.User;
        if (user is null || !user.IsActive || !user.IsFullyVerified || user.IsLockedOut())
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(storedToken.UserId, cancellationToken);
            return Result.Failure<LoginResponse>(InvalidRefreshToken);
        }

        // Rotation: her refresh'te eski token iptal edilir, yenisi üretilir.
        storedToken.Revoke(request.IpAddress);
        await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);

        var tokens = await _jwtTokenService.GenerateTokensAsync(user, request.IpAddress, cancellationToken);

        return Result.Success(new LoginResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.Role.ToString(),
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshTokenExpiresAt));
    }
}
