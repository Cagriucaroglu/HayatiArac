using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.ResendEmailOtp;

public sealed record ResendEmailOtpCommand(Guid UserId) : IRequest<Result>;
