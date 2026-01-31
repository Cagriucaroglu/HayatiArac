using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password,
    bool RememberMe = false
) : IRequest<Result>;
