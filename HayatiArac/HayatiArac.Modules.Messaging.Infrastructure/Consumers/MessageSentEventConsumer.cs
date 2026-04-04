using HayatiArac.Modules.Messaging.Application.Events;
using HayatiArac.Modules.Messaging.Infrastructure.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace HayatiArac.Modules.Messaging.Infrastructure.Consumers;

/// <summary>
/// MessageSentEvent'i alır ve SignalR üzerinden alıcıya anlık bildirim gönderir.
/// </summary>
public sealed class MessageSentEventConsumer : IConsumer<MessageSentEvent>
{
    private readonly IHubContext<MessagingHub> _hubContext;

    public MessageSentEventConsumer(IHubContext<MessagingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<MessageSentEvent> context)
    {
        MessageSentEvent @event = context.Message;

        await _hubContext.Clients
            .Group($"user-{@event.RecipientId}")
            .SendAsync("ReceiveMessage", new
            {
                conversationId = @event.ConversationId,
                senderId = @event.SenderId,
                senderName = @event.SenderName,
                content = @event.Content,
                sentAt = @event.SentAt
            }, context.CancellationToken);
    }
}
