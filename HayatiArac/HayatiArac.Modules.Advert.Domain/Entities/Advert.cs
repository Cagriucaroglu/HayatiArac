using HayatiArac.Modules.Advert.Domain.Enums;
using HayatiArac.Modules.Advert.Domain.Events;
using HayatiArac.Modules.Advert.Domain.ValueObjects;
using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Advert.Domain.Entities;

public class Advert : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Price { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    public AdvertStatus Status { get; private set; }
    public AdvertCondition Condition { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public Guid OwnerUserId { get; private set; }
    public AdvertOwnerInfo OwnerInfo { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool ShowPhoneNumber { get; private set; }

    private readonly List<AdvertImage> _images = new();
    public IReadOnlyCollection<AdvertImage> Images => _images.AsReadOnly();

    private Advert() { }

    public static Advert Create(
        string title,
        string description,
        Money price,
        Location location,
        AdvertCondition condition,
        Guid categoryId,
        Guid ownerUserId,
        AdvertOwnerInfo ownerInfo,
        bool showPhoneNumber = false)
    {
        var now = DateTime.UtcNow;
        var advert = new Advert
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Price = price,
            Location = location,
            Status = AdvertStatus.Active,
            Condition = condition,
            CategoryId = categoryId,
            OwnerUserId = ownerUserId,
            OwnerInfo = ownerInfo,
            ShowPhoneNumber = showPhoneNumber,
            ExpiresAt = now.AddDays(30),
            CreatedAt = now
        };

        advert.AddDomainEvent(new AdvertCreatedDomainEvent(advert.Id, title, ownerUserId));
        return advert;
    }

    public void Update(string title, string description, Money price, Location location)
    {
        Title = title;
        Description = description;
        Price = price;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == AdvertStatus.Active)
        {
            Status = AdvertStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new AdvertDeactivatedDomainEvent(Id, OwnerUserId));
        }
    }

    public void Activate()
    {
        Status = AdvertStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddImage(string url, int displayOrder)
    {
        _images.Add(new AdvertImage(Guid.NewGuid(), Id, url, displayOrder));
    }

    public void MarkAsSold()
    {
        Status = AdvertStatus.Sold;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status == AdvertStatus.Active)
        {
            Status = AdvertStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new AdvertExpiredDomainEvent(Id, OwnerUserId));
        }
    }
}
