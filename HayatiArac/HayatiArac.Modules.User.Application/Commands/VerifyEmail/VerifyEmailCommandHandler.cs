using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;

    public VerifyEmailCommandHandler(IUserRepository userRepository, IOtpService otpService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "Kullanici bulunamadi."));

        if (user.IsEmailVerified)
            return Result.Failure(Error.Conflict("Auth.EmailAlreadyVerified", "E-posta adresi zaten dogrulanmis."));

        var isValid = await _otpService.ValidateAsync($"otp:email:{user.Id}", request.OtpCode, cancellationToken);
        if (!isValid)
            return Result.Failure(Error.Validation("Auth.InvalidOtp", "Gecersiz veya suresi dolmus dogrulama kodu."));

        user.SetEmailVerified();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
