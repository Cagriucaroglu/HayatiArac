using FluentValidation;

namespace HayatiArac.Modules.Advert.Application.Commands.CreateAdvert;

public sealed class CreateAdvertCommandValidator : AbstractValidator<CreateAdvertCommand>
{
    public CreateAdvertCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Başlık gereklidir.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.Request.Description)
            .NotEmpty().WithMessage("Açıklama gereklidir.")
            .MaximumLength(5000).WithMessage("Açıklama en fazla 5000 karakter olabilir.");

        RuleFor(x => x.Request.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

        RuleFor(x => x.Request.City)
            .NotEmpty().WithMessage("Şehir bilgisi gereklidir.");

        RuleFor(x => x.Request.CategoryId)
            .NotEmpty().WithMessage("Kategori seçimi gereklidir.");
    }
}
