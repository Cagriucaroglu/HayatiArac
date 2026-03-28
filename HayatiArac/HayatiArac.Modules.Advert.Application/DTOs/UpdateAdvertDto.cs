using HayatiArac.Modules.Advert.Domain.Enums;

namespace HayatiArac.Modules.Advert.Application.DTOs;

public sealed record UpdateAdvertDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    CurrencyCode Currency,
    string City,
    string District,
    string Brand,
    string Model,
    int Year,
    int Mileage,
    FuelType FuelType,
    TransmissionType TransmissionType,
    bool HasHeavyDamageRecord
);
