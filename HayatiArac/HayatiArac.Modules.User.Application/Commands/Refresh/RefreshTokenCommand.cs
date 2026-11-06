using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.Refresh;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string IpAddress) : IRequest<Result<LoginResponse>>;
