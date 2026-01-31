using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Advert.Domain.Entities;

// Denormalized user info within Advert bounded context
// Populated/updated via Integration Events from User module
public class AdvertOwnerInfo : BaseEntity
{
    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? City { get; private set; }

    private AdvertOwnerInfo() { }

    public static AdvertOwnerInfo Create(Guid userId, string displayName, string email, string? city = null)
    {
        return new AdvertOwnerInfo
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DisplayName = displayName,
            Email = email,
            City = city,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateFromUser(string displayName, string? city)
    {
        DisplayName = displayName;
        City = city;
        UpdatedAt = DateTime.UtcNow;
    }
}
