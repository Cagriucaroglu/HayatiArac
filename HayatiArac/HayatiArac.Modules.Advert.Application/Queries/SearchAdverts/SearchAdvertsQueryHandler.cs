using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Queries.SearchAdverts;

public sealed class SearchAdvertsQueryHandler
    : IRequestHandler<SearchAdvertsQuery, Result<PagedResultDto<AdvertDto>>>
{
    private readonly IAdvertRepository _advertRepository;

    public SearchAdvertsQueryHandler(IAdvertRepository advertRepository)
    {
        _advertRepository = advertRepository;
    }

    public async Task<Result<PagedResultDto<AdvertDto>>> Handle(
        SearchAdvertsQuery request,
        CancellationToken cancellationToken)
    {
        var dto = request.Request;

        // Single method call - returns paginated result with eager-loaded entities
        // Repository handles: validation, pagination, eager loading (Category, OwnerInfo, Images)
        var pagedResult = await _advertRepository.SearchAdvertsPaginatedAsync(
            dto.SearchTerm,
            dto.CategoryId,
            dto.City,
            dto.Status,
            dto.MinPrice,
            dto.MaxPrice,
            dto.PageNumber,
            dto.PageSize,
            cancellationToken);

        // Map domain entities to DTOs
        var advertDtos = pagedResult.Items.Select(a => new AdvertDto(
            a.Id,
            a.Title,
            a.Description,
            a.Price.Amount,
            a.Price.Currency.ToString(),
            a.Location.City,
            a.Location.District,
            a.Status.ToString(),
            a.Condition.ToString(),
            a.Category.Name,
            a.CategoryId,
            a.OwnerInfo.DisplayName,
            a.OwnerUserId,
            a.ShowPhoneNumber ? a.OwnerInfo.PhoneNumber : null,
            a.ShowPhoneNumber,
            a.Images.Select(img => img.Url).ToList(),
            a.CreatedAt,
            a.ExpiresAt,
            a.UpdatedAt
        )).ToList();

        // Return mapped result with same pagination info
        var result = new PagedResultDto<AdvertDto>(
            advertDtos,
            pagedResult.TotalCount,
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalPages);

        return Result.Success(result);
    }
}

