using HayatiArac.Modules.Messaging.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Messaging.Application.Queries.GetMyConversations;

public sealed record GetMyConversationsQuery : IRequest<Result<List<ConversationDto>>>;
