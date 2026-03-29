using HayatiArac.Modules.Favorite.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Favorite.Application.Queries.GetUserFavorites;

public sealed record GetUserFavoritesQuery : IRequest<Result<List<FavoriteDto>>>;
