using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.ResendPhoneOtp;

public sealed class ResendPhoneOtpCommandHandler : IRequestHandler<ResendPhoneOtpCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ISmsService _smsService;
    private readonly ICacheService _cacheService;

    public ResendPhoneOtpCommandHandler(
        IUserRepository userRepository,
        IOtpService otpService,
        ISmsService smsService,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _smsService = smsService;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(ResendPhoneOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "Kullanici bulunamadi."));

        if (user.IsPhoneVerified)
            return Result.Failure(Error.Conflict("Auth.PhoneAlreadyVerified", "Telefon numarasi zaten dogrulanmis."));

        // Rate limit: 1 dakikada max 1 istek
        var rateLimitKey = $"otp:ratelimit:phone:{user.Id}";
        if (await _cacheService.ExistsAsync(rateLimitKey, cancellationToken))
            return Result.Failure(Error.Validation("Auth.TooManyRequests", "Cok fazla istek. Lutfen 1 dakika bekleyiniz."));

        await _cacheService.SetAsync(rateLimitKey, true, TimeSpan.FromMinutes(1), cancellationToken);

        var otp = await _otpService.GenerateAndStoreAsync($"otp:phone:{user.Id}", cancellationToken);
        await _smsService.SendOtpAsync(user.PhoneNumber!, otp, cancellationToken);

        return Result.Success();
    }
}
