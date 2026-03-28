using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.AddFavorite;

public sealed record AddFavoriteCommand(Guid AdvertId) : IRequest<Result>;
