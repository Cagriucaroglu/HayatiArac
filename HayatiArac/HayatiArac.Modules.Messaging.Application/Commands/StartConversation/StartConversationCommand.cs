using HayatiArac.Modules.Messaging.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Messaging.Application.Commands.StartConversation;

public sealed record StartConversationCommand(StartConversationDto Dto) : IRequest<Result<Guid>>;
