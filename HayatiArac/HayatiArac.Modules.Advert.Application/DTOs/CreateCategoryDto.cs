namespace HayatiArac.Modules.Advert.Application.DTOs;

public sealed record CreateCategoryDto(
    string Name,
    string Slug,
    string? Description = null,
    Guid? ParentCategoryId = null,
    int DisplayOrder = 0
);
