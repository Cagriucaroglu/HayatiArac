namespace HayatiArac.Modules.Advert.Application.DTOs;

public sealed record FavoriteDto(
    Guid AdvertId,
    string Title,
    string Brand,
    string Model,
    int Year,
    int Mileage,
    decimal Price,
    string Currency,
    string City,
    string Status,
    List<string> ImageUrls,
    DateTime FavoritedAt
);
