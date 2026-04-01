using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Messaging.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }

    private Message() { }

    public static Message Create(Guid conversationId, Guid senderId, string content)
    {
        return new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
