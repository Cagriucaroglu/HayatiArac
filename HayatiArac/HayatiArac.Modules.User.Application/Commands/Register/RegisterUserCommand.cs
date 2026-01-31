using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.Register;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<Result<RegisterUserResponse>>;

public sealed record RegisterUserResponse(
    Guid UserId,
    string Email
);
