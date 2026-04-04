using HayatiArac.Modules.Messaging.Application.Interfaces;
using HayatiArac.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Messaging.Infrastructure.Persistence.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly MessagingDbContext _context;

    public ConversationRepository(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Conversations
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Conversation?> GetByParticipantsAndAdvertAsync(Guid buyerId, Guid sellerId, Guid advertId, CancellationToken ct = default)
    {
        return await _context.Conversations
            .FirstOrDefaultAsync(x => x.BuyerId == buyerId && x.SellerId == sellerId && x.AdvertId == advertId, ct);
    }

    public async Task<List<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Conversations
            .Where(x => x.BuyerId == userId || x.SellerId == userId)
            .OrderByDescending(x => x.LastMessageAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Conversation conversation, CancellationToken ct = default)
    {
        await _context.Conversations.AddAsync(conversation, ct);
    }
}
