using FluentAssertions;
using HayatiArac.Modules.Advert.Application.Commands.CreateAdvert;
using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Domain.Enums;

namespace HayatiArac.Modules.Advert.UnitTests.Advert;

public class CreateAdvertCommandValidatorTests
{
    private readonly CreateAdvertCommandValidator _validator = new();

    [Fact]
    public async Task Validate_Passes_WithValidCommand()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_Fails_WhenTitleEmpty()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { Title = "" }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Başlık gereklidir.");
    }

    [Fact]
    public async Task Validate_Fails_WhenTitleTooLong()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { Title = new string('A', 201) }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Başlık en fazla 200 karakter olabilir.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_Fails_WhenPriceNotPositive(decimal price)
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { Price = price }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Fiyat 0'dan büyük olmalıdır.");
    }

    [Fact]
    public async Task Validate_Fails_WhenCityEmpty()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { City = "" }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Şehir bilgisi gereklidir.");
    }

    [Fact]
    public async Task Validate_Fails_WhenCategoryIdEmpty()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { CategoryId = Guid.Empty }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Kategori seçimi gereklidir.");
    }

    [Fact]
    public async Task Validate_Fails_WhenBrandEmpty()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { Brand = "" }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Marka gereklidir.");
    }

    [Fact]
    public async Task Validate_Fails_WhenModelEmpty()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { Model = "" }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Model gereklidir.");
    }

    [Fact]
    public async Task Validate_Fails_WhenYearTooOld()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { Year = 1899 }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Year"));
    }

    [Fact]
    public async Task Validate_Fails_WhenYearInFuture()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { Year = DateTime.UtcNow.Year + 1 }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Year"));
    }

    [Fact]
    public async Task Validate_Fails_WhenMileageNegative()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { Mileage = -1 }
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Kilometre 0 veya daha büyük olmalıdır.");
    }

    [Fact]
    public async Task Validate_Passes_WhenMileageIsZero()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with
        {
            Request = ValidDto() with { Mileage = 0 }
        });

        result.IsValid.Should().BeTrue();
    }

    private static CreateAdvertDto ValidDto() => new(
        Title: "Test İlan",
        Description: "Test açıklama metni",
        Price: 150_000m,
        Currency: CurrencyCode.TRY,
        City: "İstanbul",
        District: "Kadıköy",
        Condition: AdvertCondition.Used,
        CategoryId: Guid.NewGuid(),
        Brand: "Toyota",
        Model: "Corolla",
        Year: 2020,
        Mileage: 50_000,
        FuelType: FuelType.Gasoline,
        TransmissionType: TransmissionType.Automatic,
        HasHeavyDamageRecord: false,
        ShowPhoneNumber: false,
        ImageUrls: null);

    private static CreateAdvertCommand ValidCommand() => new(ValidDto());
}
