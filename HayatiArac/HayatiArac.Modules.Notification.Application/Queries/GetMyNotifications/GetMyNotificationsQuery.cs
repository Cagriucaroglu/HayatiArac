using HayatiArac.Modules.Notification.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Notification.Application.Queries.GetMyNotifications;

public sealed record GetMyNotificationsQuery : IRequest<Result<List<NotificationDto>>>;
