using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<UserProfileDto>>;
