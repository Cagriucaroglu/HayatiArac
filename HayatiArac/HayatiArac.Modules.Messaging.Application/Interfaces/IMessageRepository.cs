using HayatiArac.Modules.Messaging.Domain.Entities;

namespace HayatiArac.Modules.Messaging.Application.Interfaces;

public interface IMessageRepository
{
    Task<List<Message>> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default);
    Task AddAsync(Message message, CancellationToken ct = default);
    Task MarkConversationAsReadAsync(Guid conversationId, Guid readerId, CancellationToken ct = default);
}
