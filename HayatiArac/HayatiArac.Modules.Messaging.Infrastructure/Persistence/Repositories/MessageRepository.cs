using HayatiArac.Modules.Messaging.Application.Interfaces;
using HayatiArac.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Messaging.Infrastructure.Persistence.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly MessagingDbContext _context;

    public MessageRepository(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Message>> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default)
    {
        return await _context.Messages
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Message message, CancellationToken ct = default)
    {
        await _context.Messages.AddAsync(message, ct);
    }

    public async Task MarkConversationAsReadAsync(Guid conversationId, Guid readerId, CancellationToken ct = default)
    {
        await _context.Messages
            .Where(x => x.ConversationId == conversationId && x.SenderId != readerId && !x.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true), ct);
    }
}
