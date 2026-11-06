using FluentAssertions;
using HayatiArac.Modules.User.Application.Commands.Refresh;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HayatiArac.Modules.User.UnitTests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private const string RawToken = "raw-refresh-token";
    private const string Ip = "127.0.0.1";

    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            _jwtTokenService, _refreshTokenRepository, NullLogger<RefreshTokenCommandHandler>.Instance);

        _jwtTokenService
            .GenerateTokensAsync(Arg.Any<ApplicationUser>(), Ip, Arg.Any<CancellationToken>())
            .Returns(("new-access", "new-refresh", DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public async Task Handle_ReturnsUnauthorized_WhenTokenUnknown()
    {
        _jwtTokenService.FindRefreshTokenAsync(RawToken, Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        var result = await _handler.Handle(new RefreshTokenCommand(RawToken, Ip), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_RevokesAllUserTokens_WhenRevokedTokenIsReused()
    {
        var user = ActiveUser();
        var token = StoredToken(user);
        token.Revoke(Ip);
        _jwtTokenService.FindRefreshTokenAsync(RawToken, Arg.Any<CancellationToken>()).Returns(token);

        var result = await _handler.Handle(new RefreshTokenCommand(RawToken, Ip), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
        await _refreshTokenRepository.Received(1).RevokeAllUserTokensAsync(user.Id, Arg.Any<CancellationToken>());
        await _jwtTokenService.DidNotReceive()
            .GenerateTokensAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsUnauthorized_WhenTokenExpired()
    {
        var token = StoredToken(ActiveUser(), expiresAt: DateTime.UtcNow.AddMinutes(-1));
        _jwtTokenService.FindRefreshTokenAsync(RawToken, Arg.Any<CancellationToken>()).Returns(token);

        var result = await _handler.Handle(new RefreshTokenCommand(RawToken, Ip), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_RevokesAllTokens_WhenUserIsInactive()
    {
        var user = ActiveUser();
        user.IsActive = false;
        _jwtTokenService.FindRefreshTokenAsync(RawToken, Arg.Any<CancellationToken>()).Returns(StoredToken(user));

        var result = await _handler.Handle(new RefreshTokenCommand(RawToken, Ip), default);

        result.IsFailure.Should().BeTrue();
        await _refreshTokenRepository.Received(1).RevokeAllUserTokensAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RotatesToken_WhenValid()
    {
        var user = ActiveUser();
        var token = StoredToken(user);
        _jwtTokenService.FindRefreshTokenAsync(RawToken, Arg.Any<CancellationToken>()).Returns(token);

        var result = await _handler.Handle(new RefreshTokenCommand(RawToken, Ip), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access");
        result.Value.RefreshToken.Should().Be("new-refresh");
        token.IsRevoked.Should().BeTrue("eski token rotation sonrası tekrar kullanılamamalı");
        await _refreshTokenRepository.Received(1).UpdateAsync(token, Arg.Any<CancellationToken>());
    }

    private static ApplicationUser ActiveUser() =>
        ApplicationUser.Create("user@test.com", "Test", "User", "hash");

    private static RefreshToken StoredToken(ApplicationUser user, DateTime? expiresAt = null)
    {
        var token = RefreshToken.Create(user.Id, "hashed", Guid.NewGuid().ToString(), expiresAt ?? DateTime.UtcNow.AddDays(7), Ip);
        token.User = user;
        return token;
    }
}
