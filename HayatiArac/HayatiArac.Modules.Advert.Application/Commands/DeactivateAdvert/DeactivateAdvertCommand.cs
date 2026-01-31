using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.DeactivateAdvert;

public sealed record DeactivateAdvertCommand(Guid AdvertId) : IRequest<Result>;
