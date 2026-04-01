using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Messaging.Application.Commands.SendMessage;

public sealed record SendMessageCommand(Guid ConversationId, string Content) : IRequest<Result<Guid>>;
