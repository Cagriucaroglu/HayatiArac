using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.User.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string JwtId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }

    public ApplicationUser? User { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public static RefreshToken Create(Guid userId, string token, string jwtId, DateTime expiresAt, string createdByIp)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            JwtId = jwtId,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            IsRevoked = false
        };
    }

    public void Revoke(string revokedByIp)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
    }
}
