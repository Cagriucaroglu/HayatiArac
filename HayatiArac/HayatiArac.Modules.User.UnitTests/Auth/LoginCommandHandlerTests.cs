using FluentAssertions;
using HayatiArac.Modules.User.Application.Commands.Login;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Domain.Entities;
using NSubstitute;

namespace HayatiArac.Modules.User.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepository, _passwordHasher, _jwtTokenService);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("dummy-hash");
    }

    [Fact]
    public async Task Handle_ReturnsInvalidCredentials_WhenUserDoesNotExist()
    {
        var result = await _handler.Handle(Command("nobody@test.com", "x"), default);

        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_DoesNotRevealVerificationStatus_WhenPasswordWrong()
    {
        var user = ApplicationUser.Register("new@test.com", "Yeni", "Kullanici", "real-hash", "5551234567");
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("wrong", "real-hash").Returns(false);

        var result = await _handler.Handle(Command(user.Email, "wrong"), default);

        result.Error.Code.Should().Be("Auth.InvalidCredentials",
            "yanlış şifrede, hesabın doğrulanmamış olduğu bilgisi verilmemeli");
    }

    [Fact]
    public async Task Handle_ReturnsNotVerified_OnlyWhenPasswordCorrect()
    {
        var user = ApplicationUser.Register("new@test.com", "Yeni", "Kullanici", "real-hash", "5551234567");
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("correct", "real-hash").Returns(true);

        var result = await _handler.Handle(Command(user.Email, "correct"), default);

        result.Error.Code.Should().Be("Auth.AccountNotVerified");
    }

    private static LoginCommand Command(string email, string password) =>
        new(email, password, RememberMe: false, IpAddress: "127.0.0.1");
}
