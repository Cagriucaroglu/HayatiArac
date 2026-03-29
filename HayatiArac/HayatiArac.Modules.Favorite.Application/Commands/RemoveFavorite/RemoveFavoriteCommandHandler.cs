using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Favorite.Application.Commands.RemoveFavorite;

public sealed class RemoveFavoriteCommandHandler : IRequestHandler<RemoveFavoriteCommand, Result>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFavoriteUnitOfWork _unitOfWork;

    public RemoveFavoriteCommandHandler(
        IFavoriteRepository favoriteRepository,
        ICurrentUserService currentUserService,
        IFavoriteUnitOfWork unitOfWork)
    {
        _favoriteRepository = favoriteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<Unit>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        var savedAdvert = await _favoriteRepository.GetAsync(userId.Value, request.AdvertId, cancellationToken);
        if (savedAdvert is null)
            return Result.Failure<Unit>(Error.NotFound("Favorite.NotFound", "Bu ilan favorilerinizde bulunamadı."));

        await _favoriteRepository.RemoveAsync(savedAdvert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
