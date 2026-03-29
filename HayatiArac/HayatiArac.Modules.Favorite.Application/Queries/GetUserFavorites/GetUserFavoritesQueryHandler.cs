using HayatiArac.Modules.Favorite.Application.DTOs;
using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Favorite.Application.Queries.GetUserFavorites;

public sealed class GetUserFavoritesQueryHandler : IRequestHandler<GetUserFavoritesQuery, Result<List<FavoriteDto>>>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IAdvertSnapshotRepository _snapshotRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserFavoritesQueryHandler(
        IFavoriteRepository favoriteRepository,
        IAdvertSnapshotRepository snapshotRepository,
        ICurrentUserService currentUserService)
    {
        _favoriteRepository = favoriteRepository;
        _snapshotRepository = snapshotRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<FavoriteDto>>> Handle(GetUserFavoritesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<List<FavoriteDto>>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        var savedAdverts = await _favoriteRepository.GetByUserAsync(userId.Value, cancellationToken);
        if (savedAdverts.Count == 0)
            return Result.Success(new List<FavoriteDto>());

        var advertIds = savedAdverts.Select(x => x.AdvertId).ToList();
        var snapshots = await _snapshotRepository.GetByAdvertIdsAsync(advertIds, cancellationToken);

        var snapshotMap = snapshots.ToDictionary(s => s.AdvertId);

        var result = savedAdverts
            .Where(sa => snapshotMap.ContainsKey(sa.AdvertId))
            .Select(sa =>
            {
                var snap = snapshotMap[sa.AdvertId];
                return new FavoriteDto(
                    snap.AdvertId,
                    snap.Title,
                    snap.Brand,
                    snap.Model,
                    snap.Year,
                    snap.Mileage,
                    snap.Price,
                    snap.Currency,
                    snap.City,
                    snap.Status,
                    snap.ImageUrls,
                    sa.CreatedAt
                );
            })
            .ToList();

        return Result.Success(result);
    }
}
