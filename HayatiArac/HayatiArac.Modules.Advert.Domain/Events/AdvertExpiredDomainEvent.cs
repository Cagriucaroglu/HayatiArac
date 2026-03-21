using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Advert.Domain.Events;

public sealed record AdvertExpiredDomainEvent(Guid AdvertId, Guid OwnerUserId) : DomainEventBase;
