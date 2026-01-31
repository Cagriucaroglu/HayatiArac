namespace HayatiArac.Modules.User.Application.DTOs;

public sealed record RegisterUserDto(
    string Email,
    string Password,
    string FirstName,
    string LastName
);
