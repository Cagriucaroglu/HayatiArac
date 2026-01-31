using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.UpdateAdvert;

public sealed record UpdateAdvertCommand(UpdateAdvertDto Request) : IRequest<Result>;
