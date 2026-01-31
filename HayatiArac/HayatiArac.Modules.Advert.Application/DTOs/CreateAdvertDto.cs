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
    List<string>? ImageUrls
);
