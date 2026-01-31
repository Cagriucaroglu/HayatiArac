namespace HayatiArac.Modules.Advert.Application.DTOs;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    int DisplayOrder,
    List<CategoryDto> SubCategories
);
