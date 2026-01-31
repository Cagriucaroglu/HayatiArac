using FluentValidation;

namespace HayatiArac.Modules.User.Application.Commands.Login;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi gereklidir.")
            .EmailAddress().WithMessage("Gecerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Sifre gereklidir.");
    }
}
