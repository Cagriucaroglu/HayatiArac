using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.DeactivateAdvert;

public sealed class DeactivateAdvertCommandHandler : IRequestHandler<DeactivateAdvertCommand, Result>
{
    private readonly IAdvertRepository _advertRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAdvertCommandHandler(
        IAdvertRepository advertRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _advertRepository = advertRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateAdvertCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure("Kullanıcı doğrulanamadı.");

        var advert = await _advertRepository.GetByIdAsync(request.AdvertId, cancellationToken);
        if (advert is null)
            return Result.Failure("İlan bulunamadı.");

        // Authorization: only owner can deactivate
        if (advert.OwnerUserId != userId.Value)
            return Result.Failure("Bu ilanı pasifleştirme yetkiniz yok.");

        advert.Deactivate();

        await _advertRepository.UpdateAsync(advert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
