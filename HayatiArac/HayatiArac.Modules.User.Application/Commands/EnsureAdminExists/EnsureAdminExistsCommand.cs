using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.EnsureAdminExists;

public sealed record EnsureAdminExistsCommand : IRequest<Result<Guid>>;
