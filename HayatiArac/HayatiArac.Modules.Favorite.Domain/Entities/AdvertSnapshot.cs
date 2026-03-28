using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Favorite.Domain.Entities;

// Advert modülünden integration event ile denormalize edilen ilan kopyası.
// Favorite listesi gösterilirken cross-module sorgu yapmaktan kaçınmak için tutulur.
public class AdvertSnapshot : BaseEntity
{
    public Guid AdvertId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public int Mileage { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public List<string> ImageUrls { get; private set; } = new();

    private AdvertSnapshot() { }

    public static AdvertSnapshot Create(
        Guid advertId,
        string title,
        string brand,
        string model,
        int year,
        int mileage,
        decimal price,
        string currency,
        string city,
        string status,
        List<string> imageUrls)
    {
        return new AdvertSnapshot
        {
            AdvertId = advertId,
            Title = title,
            Brand = brand,
            Model = model,
            Year = year,
            Mileage = mileage,
            Price = price,
            Currency = currency,
            City = city,
            Status = status,
            ImageUrls = imageUrls,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string title,
        string brand,
        string model,
        int year,
        int mileage,
        decimal price,
        string currency,
        string city)
    {
        Title = title;
        Brand = brand;
        Model = model;
        Year = year;
        Mileage = mileage;
        Price = price;
        Currency = currency;
        City = city;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
