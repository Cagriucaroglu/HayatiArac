using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Notification.Application.Commands.MarkAsRead;

public sealed record MarkAsReadCommand(Guid NotificationId) : IRequest<Result>;
