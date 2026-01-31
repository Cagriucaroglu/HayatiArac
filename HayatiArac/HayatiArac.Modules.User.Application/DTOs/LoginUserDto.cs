namespace HayatiArac.Modules.User.Application.DTOs;

public sealed record LoginUserDto(
    string Email,
    string Password,
    bool RememberMe = false
);
