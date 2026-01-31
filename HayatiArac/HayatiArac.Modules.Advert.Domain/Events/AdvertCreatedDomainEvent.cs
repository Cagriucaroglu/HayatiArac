using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Advert.Domain.Events;

public sealed record AdvertCreatedDomainEvent(
    Guid AdvertId,
    string Title,
    Guid OwnerUserId
) : DomainEventBase;
