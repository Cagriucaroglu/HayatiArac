using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Queries.GetAdvertById;

public sealed class GetAdvertByIdQueryHandler : IRequestHandler<GetAdvertByIdQuery, Result<AdvertDto>>
{
    private readonly IAdvertRepository _advertRepository;

    public GetAdvertByIdQueryHandler(IAdvertRepository advertRepository)
    {
        _advertRepository = advertRepository;
    }

    public async Task<Result<AdvertDto>> Handle(GetAdvertByIdQuery request, CancellationToken cancellationToken)
    {
        var advert = await _advertRepository.GetByIdWithDetailsAsync(request.AdvertId, cancellationToken);

        if (advert is null)
            return Result.Failure<AdvertDto>("İlan bulunamadı.");

        var dto = new AdvertDto(
            advert.Id,
            advert.Title,
            advert.Description,
            advert.Price.Amount,
            advert.Price.Currency.ToString(),
            advert.Location.City,
            advert.Location.District,
            advert.Status.ToString(),
            advert.Condition.ToString(),
            advert.Category.Name,
            advert.CategoryId,
            advert.OwnerInfo.DisplayName,
            advert.OwnerUserId,
            advert.Images.Select(img => img.Url).ToList(),
            advert.CreatedAt,
            advert.UpdatedAt);

        return Result.Success(dto);
    }
}
