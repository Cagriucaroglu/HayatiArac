using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.DeleteAdvert;

public sealed class DeleteAdvertCommandHandler : IRequestHandler<DeleteAdvertCommand, Result>
{
    private readonly IAdvertRepository _advertRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAdvertCommandHandler(
        IAdvertRepository advertRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _advertRepository = advertRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteAdvertCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure("Kullanıcı doğrulanamadı.");

        var advert = await _advertRepository.GetByIdAsync(request.AdvertId, cancellationToken);
        if (advert is null)
            return Result.Failure("İlan bulunamadı.");

        // Authorization: only owner can delete
        if (advert.OwnerUserId != userId.Value)
            return Result.Failure("Bu ilanı silme yetkiniz yok.");

        await _advertRepository.DeleteAsync(advert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
