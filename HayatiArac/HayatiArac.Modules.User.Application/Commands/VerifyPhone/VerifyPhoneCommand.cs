using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.VerifyPhone;

public sealed record VerifyPhoneCommand(Guid UserId, string OtpCode) : IRequest<Result>;
