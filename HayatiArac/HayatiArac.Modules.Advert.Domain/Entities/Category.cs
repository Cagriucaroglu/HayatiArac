using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Advert.Domain.Entities;

public class Category : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }
    public int DisplayOrder { get; private set; }

    private readonly List<Category> _subCategories = new();
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    private readonly List<Advert> _adverts = new();
    public IReadOnlyCollection<Advert> Adverts => _adverts.AsReadOnly();

    private Category() { }

    public static Category Create(string name, string slug, string? description = null,
        Guid? parentCategoryId = null, int displayOrder = 0)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = description,
            ParentCategoryId = parentCategoryId,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string slug, string? description, int displayOrder)
    {
        Name = name;
        Slug = slug;
        Description = description;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}
