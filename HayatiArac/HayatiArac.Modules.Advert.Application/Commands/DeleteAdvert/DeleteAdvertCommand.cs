using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.DeleteAdvert;

public sealed record DeleteAdvertCommand(Guid AdvertId) : IRequest<Result>;
