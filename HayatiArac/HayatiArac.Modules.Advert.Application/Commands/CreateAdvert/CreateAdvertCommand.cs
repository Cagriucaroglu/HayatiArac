using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.CreateAdvert;

public sealed record CreateAdvertCommand(CreateAdvertDto Request) : IRequest<Result<Guid>>;
