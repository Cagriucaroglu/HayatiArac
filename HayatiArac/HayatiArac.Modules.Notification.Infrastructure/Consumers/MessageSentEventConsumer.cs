using HayatiArac.Modules.Messaging.Application.Events;
using HayatiArac.Modules.Notification.Application.Interfaces;
using HayatiArac.Modules.Notification.Domain.Entities;
using HayatiArac.Modules.Notification.Domain.Enums;
using HayatiArac.Modules.Notification.Infrastructure.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace HayatiArac.Modules.Notification.Infrastructure.Consumers;

public sealed class MessageSentEventConsumer : IConsumer<MessageSentEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;

    public MessageSentEventConsumer(
        INotificationRepository notificationRepository,
        INotificationUnitOfWork unitOfWork,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<MessageSentEvent> context)
    {
        MessageSentEvent @event = context.Message;

        AppNotification notification = AppNotification.Create(
            userId: @event.RecipientId,
            title: "Yeni Mesaj",
            message: $"{@event.SenderName} size bir mesaj gönderdi.",
            type: NotificationType.NewMessage,
            relatedEntityId: @event.ConversationId);

        await _notificationRepository.AddAsync(notification, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        await _hubContext.Clients
            .Group($"user-{@event.RecipientId}")
            .SendAsync("ReceiveNotification", new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type.ToString(),
                isRead = notification.IsRead,
                relatedEntityId = notification.RelatedEntityId,
                createdAt = notification.CreatedAt
            }, context.CancellationToken);
    }
}
