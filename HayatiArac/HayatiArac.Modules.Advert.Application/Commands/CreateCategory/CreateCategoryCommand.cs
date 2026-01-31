using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string Slug,
    string? Description = null,
    Guid? ParentCategoryId = null,
    int DisplayOrder = 0) : IRequest<Result<Guid>>;
