namespace HayatiArac.Modules.User.Application.DTOs;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber);
