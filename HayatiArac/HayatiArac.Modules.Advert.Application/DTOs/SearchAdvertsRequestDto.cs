using HayatiArac.Modules.Advert.Domain.Enums;

namespace HayatiArac.Modules.Advert.Application.DTOs;

public sealed record SearchAdvertsRequestDto(
    string? SearchTerm,
    Guid? CategoryId,
    string? City,
    AdvertStatus? Status,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Brand,
    string? Model,
    int? MinYear,
    int? MaxYear,
    int? MaxMileage,
    FuelType? FuelType,
    TransmissionType? TransmissionType,
    bool? HasHeavyDamageRecord,
    int PageNumber = 1,
    int PageSize = 20
);
