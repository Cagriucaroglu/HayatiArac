using HayatiArac.Modules.Advert.Domain.Enums;

namespace HayatiArac.Modules.Advert.Application.DTOs;

public sealed record SearchAdvertsRequestDto(
    string? SearchTerm,
    Guid? CategoryId,
    string? City,
    AdvertStatus? Status,
    decimal? MinPrice,
    decimal? MaxPrice,
    int PageNumber = 1,
    int PageSize = 20
);
