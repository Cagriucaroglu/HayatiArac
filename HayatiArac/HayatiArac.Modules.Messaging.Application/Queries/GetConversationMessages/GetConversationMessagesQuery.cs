using HayatiArac.Modules.Messaging.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Messaging.Application.Queries.GetConversationMessages;

public sealed record GetConversationMessagesQuery(Guid ConversationId) : IRequest<Result<List<MessageDto>>>;
