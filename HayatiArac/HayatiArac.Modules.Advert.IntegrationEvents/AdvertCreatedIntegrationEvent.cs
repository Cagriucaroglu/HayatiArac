using HayatiArac.SharedKernel.IntegrationEvents;

namespace HayatiArac.Modules.Advert.IntegrationEvents;

public sealed record AdvertCreatedIntegrationEvent : IntegrationEvent
{
    public required Guid AdvertId { get; init; }
    public required string Title { get; init; }
    public required Guid OwnerUserId { get; init; }
}
