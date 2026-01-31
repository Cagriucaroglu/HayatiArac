using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Queries.SearchAdverts;

public sealed record SearchAdvertsQuery(SearchAdvertsRequestDto Request)
    : IRequest<Result<PagedResultDto<AdvertDto>>>;
