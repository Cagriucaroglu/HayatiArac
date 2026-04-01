using HayatiArac.Modules.Messaging.Application.DTOs;
using HayatiArac.Modules.Messaging.Application.Events;
using HayatiArac.Modules.Messaging.Application.Interfaces;
using HayatiArac.Modules.Messaging.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MassTransit;
using MediatR;

namespace HayatiArac.Modules.Messaging.Application.Commands.StartConversation;

public sealed class StartConversationCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IMessagingUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPublishEndpoint publishEndpoint) : IRequestHandler<StartConversationCommand , Result<Guid>>
{
    public async Task<Result<Guid>> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        Guid? buyerId = currentUserService.UserId;
        if(!buyerId.HasValue)
        {
            return Result.Failure<Guid>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı"));
        }
        StartConversationDto dto = request.Dto;
        if(buyerId == dto.SellerId)
        {
            return Result.Failure<Guid>(Error.Validation("Messaging.SelfMessage", "Kendi ilanınıza mesaj gönderemezsiniz"));
        }

        Conversation? conversation = await conversationRepository.GetByParticipantsAndAdvertAsync(buyerId.Value, dto.SellerId, dto.AdvertId, cancellationToken);

        Guid conversationId;
        if (conversation is not null)
            conversationId = conversation.Id;
        else
        {
            string buyerName = currentUserService.Email ?? "Kullanıcı";
            Conversation conversation1 = Conversation.Create(dto.AdvertId,dto.AdvertTitle,buyerId.Value, buyerName, dto.SellerId, dto.SellerDisplayName);
            await conversationRepository.AddAsync(conversation1, cancellationToken);
            conversationId = conversation1.Id;
        }
        Message message = Message.Create(conversationId, buyerId.Value, dto.InitialMessage);
        await messageRepository.AddAsync(message, cancellationToken);

        if (conversation is not null)
            conversation.UpdateLastMessageAt();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        publishEndpoint.Publish(new MessageSentEvent(conversationId, dto.SellerId, buyerId.Value, currentUserService.Email,
            dto.InitialMessage, message.CreatedAt), cancellationToken);

        return Result.Success(conversationId);
    }

}
