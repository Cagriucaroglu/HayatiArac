using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.AddFavorite;

public sealed class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, Result>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IAdvertRepository _advertRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddFavoriteCommandHandler(
        IFavoriteRepository favoriteRepository,
        IAdvertRepository advertRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _favoriteRepository = favoriteRepository;
        _advertRepository = advertRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<Unit>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        var advert = await _advertRepository.GetByIdAsync(request.AdvertId, cancellationToken);
        if (advert is null)
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
