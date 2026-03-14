using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.ResendPhoneOtp;

public sealed record ResendPhoneOtpCommand(Guid UserId) : IRequest<Result>;
