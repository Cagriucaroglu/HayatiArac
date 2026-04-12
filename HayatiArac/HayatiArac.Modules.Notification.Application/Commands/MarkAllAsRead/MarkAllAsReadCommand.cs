using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Notification.Application.Commands.MarkAllAsRead;

public sealed record MarkAllAsReadCommand : IRequest<Result>;
