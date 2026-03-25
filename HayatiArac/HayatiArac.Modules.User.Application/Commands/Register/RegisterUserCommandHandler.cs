using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Domain.Entities;
using HayatiArac.Modules.User.IntegrationEvents;
using HayatiArac.SharedKernel.Application;
using MassTransit;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IOtpService _otpService;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IPublishEndpoint publishEndpoint,
        IOtpService otpService,
        ISmsService smsService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _publishEndpoint = publishEndpoint;
        _otpService = otpService;
        _smsService = smsService;
        _emailService = emailService;
    }

    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // E-posta benzersizliği kontrolü
        var existingByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail != null)
            return Result.Failure<RegisterUserResponse>(
                Error.Conflict("Auth.EmailAlreadyExists", "Bu e-posta adresi zaten kullanılıyor."));

        // Telefon benzersizliği kontrolü
        var existingByPhone = await _userRepository.GetByPhoneAsync(request.PhoneNumber, cancellationToken);
        if (existingByPhone != null)
            return Result.Failure<RegisterUserResponse>(
                Error.Conflict("Auth.PhoneAlreadyExists", "Bu telefon numarası zaten kullanılıyor."));

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Hesap doğrulama tamamlanana kadar pasif oluşturulur
        var user = ApplicationUser.Register(
            request.Email,
            request.FirstName,
            request.LastName,
            passwordHash,
            request.PhoneNumber);

        await _userRepository.AddAsync(user, cancellationToken);

        // OTP üret ve gönder
        var emailOtp = await _otpService.GenerateAndStoreAsync($"otp:email:{user.Id}", cancellationToken);
        var phoneOtp = await _otpService.GenerateAndStoreAsync($"otp:phone:{user.Id}", cancellationToken);

        var fullPhoneNumber = request.PhoneCountryCode + user.PhoneNumber;
        await Task.WhenAll(
            _emailService.SendOtpAsync(user.Email, emailOtp, cancellationToken),
            _smsService.SendOtpAsync(fullPhoneNumber, phoneOtp, cancellationToken)
        );

        // Integration event yayınla (Advert modülüne AdvertOwnerInfo oluşturması için)
        await _publishEndpoint.Publish(new UserRegisteredIntegrationEvent
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        }, cancellationToken);

        return Result.Success(new RegisterUserResponse(user.Id, user.Email));
    }
}
