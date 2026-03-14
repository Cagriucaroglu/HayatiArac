using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(Guid UserId, string OtpCode) : IRequest<Result>;
