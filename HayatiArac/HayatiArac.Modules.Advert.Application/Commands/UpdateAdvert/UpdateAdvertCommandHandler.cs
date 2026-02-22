using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Domain.ValueObjects;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.UpdateAdvert;

public sealed class UpdateAdvertCommandHandler : IRequestHandler<UpdateAdvertCommand, Result>
{
    private readonly IAdvertRepository _advertRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAdvertCommandHandler(
        IAdvertRepository advertRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _advertRepository = advertRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateAdvertCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<Unit>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        var dto = request.Request;

        var advert = await _advertRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (advert is null)
            return Result.Failure<Unit>(Error.NotFound("Advert.NotFound", "İlan bulunamadı."));

        // Authorization: only owner can update
        if (advert.OwnerUserId != userId.Value)
            return Result.Failure<Unit>(Error.Forbidden("Advert.NotOwner", "Bu ilanı güncelleme yetkiniz yok."));

        var price = Money.Create(dto.Price, dto.Currency);
        var location = Location.Create(dto.City, dto.District);

        advert.Update(dto.Title, dto.Description, price, location);

        await _advertRepository.UpdateAsync(advert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
