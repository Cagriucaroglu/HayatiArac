using HayatiArac.Modules.User.Domain.ValueObjects;
using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.User.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public ApplicationUser User { get; private set; } = null!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public Address? Address { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }

    private UserProfile() { }

    public static UserProfile Create(Guid userId)
    {
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePhone(PhoneNumber phone)
    {
        PhoneNumber = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(Address address)
    {
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBio(string bio)
    {
        Bio = bio;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
