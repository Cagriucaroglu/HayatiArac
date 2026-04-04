using HayatiArac.Modules.Messaging.Application.DTOs;
using HayatiArac.Modules.Messaging.Application.Interfaces;
using HayatiArac.Modules.Messaging.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Messaging.Application.Queries.GetMyConversations;

public sealed class GetMyConversationsQueryHandler : IRequestHandler<GetMyConversationsQuery, Result<List<ConversationDto>>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyConversationsQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ConversationDto>>> Handle(GetMyConversationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<List<ConversationDto>>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        List<Conversation> conversations = await _conversationRepository.GetByUserIdAsync(userId.Value, cancellationToken);

        List<ConversationDto> result = [];
        foreach (var conv in conversations)
        { 
            var messages = await _messageRepository.GetByConversationIdAsync(conv.Id, cancellationToken);
            var lastMessage = messages.MaxBy(m => m.CreatedAt);
            var unreadCount = messages.Count(m => m.SenderId != userId.Value && !m.IsRead);

            var isbuyer = conv.BuyerId == userId.Value;
            result.Add(new ConversationDto(
                conv.Id,
                conv.AdvertId,
                conv.AdvertTitle,
                isbuyer ? conv.SellerId : conv.BuyerId,
                isbuyer ? conv.SellerDisplayName : conv.BuyerDisplayName,
                lastMessage?.Content,
                conv.LastMessageAt,
                unreadCount));
        }

        return Result.Success(result);
    }
}
