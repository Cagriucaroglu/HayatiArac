using Microsoft.AspNetCore.Identity;

namespace HayatiArac.Modules.User.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; private set; }
    public UserProfile? Profile { get; private set; }

    private ApplicationUser() { }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void SetName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static ApplicationUser Create(string email, string firstName, string lastName)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow
        };
    }
}
