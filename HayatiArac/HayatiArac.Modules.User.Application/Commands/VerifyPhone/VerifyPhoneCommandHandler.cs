using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.VerifyPhone;

public sealed class VerifyPhoneCommandHandler : IRequestHandler<VerifyPhoneCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;

    public VerifyPhoneCommandHandler(IUserRepository userRepository, IOtpService otpService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
    }

    public async Task<Result> Handle(VerifyPhoneCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "Kullanici bulunamadi."));

        if (user.IsPhoneVerified)
            return Result.Failure(Error.Conflict("Auth.PhoneAlreadyVerified", "Telefon numarasi zaten dogrulanmis."));

        var isValid = await _otpService.ValidateAsync($"otp:phone:{user.Id}", request.OtpCode, cancellationToken);
        if (!isValid)
            return Result.Failure(Error.Validation("Auth.InvalidOtp", "Gecersiz veya suresi dolmus dogrulama kodu."));

        user.SetPhoneVerified();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
