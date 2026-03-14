using FluentValidation;

namespace HayatiArac.Modules.User.Application.Commands.Register;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi gereklidir.")
            .EmailAddress().WithMessage("Gecerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Sifre gereklidir.")
            .MinimumLength(6).WithMessage("Sifre en az 6 karakter olmalidir.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad gereklidir.")
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad gereklidir.")
            .MaximumLength(50);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarasi gereklidir.")
            .Matches(@"^\d{10}$").WithMessage("Telefon numarasi 10 haneli olmali ve sadece rakam icermeli (baskode olmadan, ornek: 5551234567).");
    }
}
