using HayatiArac.Modules.User.Domain.Enums;
using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.User.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Authentication security fields
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastPasswordChangedAt { get; set; }

    public UserProfile? Profile { get; private set; }

    public bool IsPhoneVerified { get; private set; } = false;
    public DateTime? PhoneVerifiedAt { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}";
    public bool IsFullyVerified => IsEmailVerified && IsPhoneVerified;

    // Helper methods for authentication
    public bool IsLockedOut() => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    public void IncrementFailedLogin() => FailedLoginAttempts++;
    public void ResetFailedLogin() => FailedLoginAttempts = 0;
    public void LockAccount(int minutes) => LockoutEnd = DateTime.UtcNow.AddMinutes(minutes);

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        ResetFailedLogin();
    }

    public void SetName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void SetEmailVerified()
    {
        IsEmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        if (IsFullyVerified)
            IsActive = true;
    }

    public void SetPhoneVerified()
    {
        IsPhoneVerified = true;
        PhoneVerifiedAt = DateTime.UtcNow;
        if (IsFullyVerified)
            IsActive = true;
    }

    // Kayıt akışı için — doğrulama tamamlanana kadar pasif
    public static ApplicationUser Register(
        string email, string firstName, string lastName, string passwordHash, string? phoneNumber)
    {
        return new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber,
            Role = UserRole.User,
            IsActive = false,
            IsEmailVerified = false,
            IsPhoneVerified = false
        };
    }

    // Admin/sistem oluşturma için — doğrulama atlanır
    public static ApplicationUser Create(string email, string firstName, string lastName, string passwordHash)
    {
        return new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            Role = UserRole.User,
            IsActive = true,
            IsEmailVerified = true,
            IsPhoneVerified = true
        };
    }
}
