using HayatiArac.SharedKernel.IntegrationEvents;

namespace HayatiArac.Modules.User.IntegrationEvents;

public sealed record UserProfileUpdatedIntegrationEvent : IntegrationEvent
{
    public required Guid UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? City { get; init; }
}
