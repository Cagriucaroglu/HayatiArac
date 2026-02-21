using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.Login;

public sealed record LoginCommand(
    string EmailOrUsername,
    string Password,
    bool RememberMe,
    string IpAddress) : IRequest<Result<LoginResponse>>;
