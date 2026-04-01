using HayatiArac.Modules.Messaging.Application.DTOs;
using HayatiArac.Modules.Messaging.Application.Interfaces;
using HayatiArac.Modules.Messaging.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Messaging.Application.Queries.GetConversationMessages;

public sealed class GetConversationMessagesQueryHandler : IRequestHandler<GetConversationMessagesQuery, Result<List<MessageDto>>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMessagingUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetConversationMessagesQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IMessagingUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MessageDto>>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        Guid? userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<List<MessageDto>>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        Conversation? conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
            return Result.Failure<List<MessageDto>>(Error.NotFound("Conversation.NotFound", "Konuşma bulunamadı."));

        if (conversation.BuyerId != userId.Value && conversation.SellerId != userId.Value)
            return Result.Failure<List<MessageDto>>(Error.Forbidden("Messaging.NotParticipant", "Bu konuşmaya erişim yetkiniz yok."));

        List<Message> messages = await _messageRepository.GetByConversationIdAsync(request.ConversationId, cancellationToken);

        // Karşı taraftan gelen okunmamış mesajları okundu olarak işaretle
        await _messageRepository.MarkConversationAsReadAsync(request.ConversationId, userId.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        List<MessageDto> result = messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(m.Id, m.SenderId, m.Content, m.IsRead, m.CreatedAt))
            .ToList();

        return Result.Success(result);
    }
}
