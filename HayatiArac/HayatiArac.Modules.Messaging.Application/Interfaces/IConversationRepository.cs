using HayatiArac.Modules.Messaging.Domain.Entities;

namespace HayatiArac.Modules.Messaging.Application.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Conversation?> GetByParticipantsAndAdvertAsync(Guid buyerId, Guid sellerId, Guid advertId, CancellationToken ct = default);
    Task<List<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Conversation conversation, CancellationToken ct = default);
}
