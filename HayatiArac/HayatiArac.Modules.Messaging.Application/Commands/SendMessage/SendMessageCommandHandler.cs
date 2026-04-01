using HayatiArac.Modules.Messaging.Application.Events;
using HayatiArac.Modules.Messaging.Application.Interfaces;
using HayatiArac.Modules.Messaging.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MassTransit;
using MediatR;

namespace HayatiArac.Modules.Messaging.Application.Commands.SendMessage;

public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<Guid>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMessagingUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IMessagingUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPublishEndpoint publishEndpoint)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<Guid>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        Guid? senderId = _currentUserService.UserId;
        if (!senderId.HasValue)
            return Result.Failure<Guid>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        Conversation? conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
            return Result.Failure<Guid>(Error.NotFound("Conversation.NotFound", "Konuşma bulunamadı."));

        if (conversation.BuyerId != senderId.Value && conversation.SellerId != senderId.Value)
            return Result.Failure<Guid>(Error.Forbidden("Messaging.NotParticipant", "Bu konuşmaya erişim yetkiniz yok."));

        Message message = Message.Create(request.ConversationId, senderId.Value, request.Content);
        await _messageRepository.AddAsync(message, cancellationToken);
        conversation.UpdateLastMessageAt();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Guid recipientId = conversation.BuyerId == senderId.Value
            ? conversation.SellerId
            : conversation.BuyerId;

        await _publishEndpoint.Publish(new MessageSentEvent(
            conversation.Id,
            recipientId,
            senderId.Value,
            _currentUserService.Email ?? "Kullanıcı",
            request.Content,
            message.CreatedAt), cancellationToken);

        return Result.Success(message.Id);
    }
}
