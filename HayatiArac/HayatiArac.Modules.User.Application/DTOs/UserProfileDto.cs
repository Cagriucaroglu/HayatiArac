namespace HayatiArac.Modules.User.Application.DTOs;

public sealed record UserProfileDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? City,
    string? District,
    string? AvatarUrl,
    string? Bio,
    DateTime CreatedAt
);
