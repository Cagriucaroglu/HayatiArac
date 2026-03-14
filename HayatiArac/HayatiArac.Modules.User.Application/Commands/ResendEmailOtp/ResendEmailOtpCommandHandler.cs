using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.ResendEmailOtp;

public sealed class ResendEmailOtpCommandHandler : IRequestHandler<ResendEmailOtpCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;

    public ResendEmailOtpCommandHandler(
        IUserRepository userRepository,
        IOtpService otpService,
        IEmailService emailService,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _emailService = emailService;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(ResendEmailOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "Kullanici bulunamadi."));

        if (user.IsEmailVerified)
            return Result.Failure(Error.Conflict("Auth.EmailAlreadyVerified", "E-posta adresi zaten dogrulanmis."));

        // Rate limit: 1 dakikada max 1 istek
        var rateLimitKey = $"otp:ratelimit:email:{user.Id}";
        if (await _cacheService.ExistsAsync(rateLimitKey, cancellationToken))
            return Result.Failure(Error.Validation("Auth.TooManyRequests", "Cok fazla istek. Lutfen 1 dakika bekleyiniz."));

        await _cacheService.SetAsync(rateLimitKey, true, TimeSpan.FromMinutes(1), cancellationToken);

        var otp = await _otpService.GenerateAndStoreAsync($"otp:email:{user.Id}", cancellationToken);
        await _emailService.SendOtpAsync(user.Email, otp, cancellationToken);

        return Result.Success();
    }
}
