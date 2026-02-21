namespace HayatiArac.Modules.User.Application.DTOs;

public sealed record LoginResponse(
    Guid UserId,
    string UserName,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);
