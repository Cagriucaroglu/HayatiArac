using HayatiArac.Modules.Advert.Domain.Enums;

namespace HayatiArac.Modules.Advert.Application.DTOs;

public sealed record CreateAdvertDto(
    string Title,
    string Description,
    decimal Price,
    CurrencyCode Currency,
    string City,
    string District,
    AdvertCondition Condition,
    Guid CategoryId,
    string Brand,
    string Model,
    int Year,
    int Mileage,
    FuelType FuelType,
    TransmissionType TransmissionType,
    bool HasHeavyDamageRecord,
    bool ShowPhoneNumber,
    List<string>? ImageUrls
);
