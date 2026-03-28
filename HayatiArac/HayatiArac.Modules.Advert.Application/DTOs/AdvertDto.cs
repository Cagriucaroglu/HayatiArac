namespace HayatiArac.Modules.Advert.Application.DTOs;

public sealed record AdvertDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    string Currency,
    string City,
    string District,
    string Status,
    string Condition,
    string CategoryName,
    Guid CategoryId,
    string OwnerDisplayName,
    Guid OwnerUserId,
    string? OwnerPhoneNumber,   // Yalnızca ShowPhoneNumber=true ise dolu gelir
    bool ShowPhoneNumber,
    List<string> ImageUrls,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? UpdatedAt,
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string FuelType,
    string TransmissionType,
    bool HasHeavyDamageRecord
);
