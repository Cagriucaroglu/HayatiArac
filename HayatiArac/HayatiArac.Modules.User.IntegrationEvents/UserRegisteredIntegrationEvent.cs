using HayatiArac.SharedKernel.IntegrationEvents;

namespace HayatiArac.Modules.User.IntegrationEvents;

public sealed record UserRegisteredIntegrationEvent : IntegrationEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? PhoneNumber { get; init; }
}
