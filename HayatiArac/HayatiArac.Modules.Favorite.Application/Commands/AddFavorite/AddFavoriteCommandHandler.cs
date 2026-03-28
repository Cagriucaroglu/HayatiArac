using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.Modules.Favorite.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Favorite.Application.Commands.AddFavorite;

public sealed class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, Result>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IAdvertSnapshotRepository _snapshotRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddFavoriteCommandHandler(
        IFavoriteRepository favoriteRepository,
        IAdvertSnapshotRepository snapshotRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _favoriteRepository = favoriteRepository;
        _snapshotRepository = snapshotRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<Unit>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        var snapshot = await _snapshotRepository.GetByAdvertIdAsync(request.AdvertId, cancellationToken);
        if (snapshot is null)
            return Result.Failure<Unit>(Error.NotFound("Advert.NotFound", "İlan bulunamadı."));

        var existing = await _favoriteRepository.GetAsync(userId.Value, request.AdvertId, cancellationToken);
        if (existing is not null)
            return Result.Failure<Unit>(Error.Conflict("Favorite.AlreadyExists", "Bu ilan zaten favorilerinizde."));

        var favorite = Favorite.Create(userId.Value, request.AdvertId);
        await _favoriteRepository.AddAsync(favorite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
