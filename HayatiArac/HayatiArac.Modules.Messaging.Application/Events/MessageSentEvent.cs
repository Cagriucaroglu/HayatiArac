namespace HayatiArac.Modules.Messaging.Application.Events;

/// <summary>
/// Mesaj gönderildiğinde publish edilen internal event.
/// Consumer (Infrastructure) bu eventi alarak SignalR ile alıcıya push eder.
/// </summary>
public sealed record MessageSentEvent(
    Guid ConversationId,
    Guid RecipientId,
    Guid SenderId,
    string SenderName,
    string Content,
    DateTime SentAt
);
