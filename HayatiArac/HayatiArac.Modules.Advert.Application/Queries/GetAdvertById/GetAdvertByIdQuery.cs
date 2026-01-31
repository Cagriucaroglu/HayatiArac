using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Queries.GetAdvertById;

public sealed record GetAdvertByIdQuery(Guid AdvertId) : IRequest<Result<AdvertDto>>;
