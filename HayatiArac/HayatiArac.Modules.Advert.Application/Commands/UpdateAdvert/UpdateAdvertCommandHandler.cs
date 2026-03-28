using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Domain.ValueObjects;
using HayatiArac.Modules.Advert.IntegrationEvents;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MassTransit;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.UpdateAdvert;

public sealed class UpdateAdvertCommandHandler : IRequestHandler<UpdateAdvertCommand, Result>
{
    private readonly IAdvertRepository _advertRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateAdvertCommandHandler(
        IAdvertRepository advertRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _advertRepository = advertRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
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

        advert.Update(
            dto.Title,
            dto.Description,
            price,
            location,
            dto.Brand,
            dto.Model,
            dto.Year,
            dto.Mileage,
            dto.FuelType,
            dto.TransmissionType,
            dto.HasHeavyDamageRecord);

        await _advertRepository.UpdateAsync(advert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new AdvertUpdatedIntegrationEvent
        {
            AdvertId = advert.Id,
            Title = advert.Title,
            Brand = advert.Brand,
            Model = advert.Model,
            Year = advert.Year,
            Mileage = advert.Mileage,
            Price = advert.Price.Amount,
            Currency = advert.Price.Currency.ToString(),
            City = advert.Location.City
        }, cancellationToken);

        return Result.Success();
    }
}
