namespace HayatiArac.Modules.Messaging.Application.DTOs;

public sealed record StartConversationDto(
    Guid AdvertId,
    string AdvertTitle,
    Guid SellerId,
    string SellerDisplayName,
    string InitialMessage
);
