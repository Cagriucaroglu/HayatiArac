namespace HayatiArac.Modules.User.Application.DTOs;

public sealed record LoginRequest(
    string EmailOrUsername,
    string Password,
    bool RememberMe = false);
