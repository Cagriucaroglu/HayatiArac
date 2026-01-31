using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.User.Domain.Events;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName
) : DomainEventBase;
