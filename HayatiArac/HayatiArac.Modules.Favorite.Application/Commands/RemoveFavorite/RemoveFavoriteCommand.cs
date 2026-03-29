using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Favorite.Application.Commands.RemoveFavorite;

public sealed record RemoveFavoriteCommand(Guid AdvertId) : IRequest<Result>;
