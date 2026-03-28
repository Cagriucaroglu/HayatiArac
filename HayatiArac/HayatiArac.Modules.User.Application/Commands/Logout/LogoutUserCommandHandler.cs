using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.Logout;

public sealed class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ICurrentUserService _currentUserService;

    public LogoutUserCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ICurrentUserService currentUserService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        // In JWT auth, we revoke all refresh tokens for the user
        // The access token will expire naturally (client should delete it)
        if (_currentUserService.UserId.HasValue)
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(
                _currentUserService.UserId.Value,
                cancellationToken);
        }
    }
}
