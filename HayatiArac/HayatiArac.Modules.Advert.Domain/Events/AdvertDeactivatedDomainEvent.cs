using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Advert.Domain.Events;

public sealed record AdvertDeactivatedDomainEvent(
    Guid AdvertId,
    Guid OwnerUserId
) : DomainEventBase;
