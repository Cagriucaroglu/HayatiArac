using HayatiArac.SharedKernel.IntegrationEvents;

namespace HayatiArac.Modules.Advert.IntegrationEvents;

public sealed record AdvertUpdatedIntegrationEvent : IntegrationEvent
{
    public required Guid AdvertId { get; init; }
    public required string Title { get; init; }
    public required string Brand { get; init; }
    public required string Model { get; init; }
    public required int Year { get; init; }
    public required int Mileage { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required string City { get; init; }
}
