using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private static readonly Error InvalidCredentials =
        Error.Validation("Auth.InvalidCredentials", "E-posta/kullanıcı adı veya şifre hatalı.");

    // Var olmayan kullanıcıda karşılaştırılacak, rastgele bir şifrenin hash'i (ilk ihtiyaçta bir kez üretilir).
    private static string? _dummyPasswordHash;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Try to find user by email first, then by username
        var user = await _userRepository.GetByEmailAsync(request.EmailOrUsername, cancellationToken);

        if (user == null)
        {
            user = await _userRepository.GetByUsernameAsync(request.EmailOrUsername, cancellationToken);
        }

        if (user == null)
        {
            // Zamanlama farkından hesap varlığı anlaşılmasın diye yine de bir hash doğrulaması yapılır.
            _dummyPasswordHash ??= _passwordHasher.HashPassword(Guid.NewGuid().ToString());
            _passwordHasher.VerifyPassword(request.Password, _dummyPasswordHash);
            return Result.Failure<LoginResponse>(InvalidCredentials);
        }

        // Kilitliyken şifre denenmez (brute-force'u durdurur).
        if (user.IsLockedOut())
            return Result.Failure<LoginResponse>(Error.Validation("Auth.AccountLocked", "Hesabiniz kilitlendi. Lutfen daha sonra tekrar deneyin."));

        // Verify password
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            // Increment failed login attempts
            user.IncrementFailedLogin();

            // Lock account after 5 failed attempts for 15 minutes
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockAccount(15);
                await _userRepository.UpdateAsync(user, cancellationToken);
                return Result.Failure<LoginResponse>(Error.Validation("Auth.AccountLockedDueToFailedAttempts", "Account locked due to too many failed login attempts"));
            }

            await _userRepository.UpdateAsync(user, cancellationToken);
            return Result.Failure<LoginResponse>(InvalidCredentials);
        }

        // Hesap durumu bilgisi sadece şifre doğruysa verilir; aksi halde hesabın varlığı sızardı.
        if (!user.IsFullyVerified)
            return Result.Failure<LoginResponse>(Error.Validation("Auth.AccountNotVerified", "Hesabiniz dogrulanmamis. Lutfen e-posta ve telefon dogrulamasini tamamlayiniz."));

        if (!user.IsActive)
            return Result.Failure<LoginResponse>(Error.Validation("Auth.AccountInactive", "Hesabiniz aktif degil."));

        // Successful login - reset failed attempts and update last login
        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Generate JWT tokens
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
