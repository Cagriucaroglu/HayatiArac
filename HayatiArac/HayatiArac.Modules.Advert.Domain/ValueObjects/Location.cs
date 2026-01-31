using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Advert.Domain.ValueObjects;

public class Location : ValueObject
{
    public string City { get; private set; }
    public string District { get; private set; }

    private Location()
    {
        City = string.Empty;
        District = string.Empty;
    }

    private Location(string city, string district)
    {
        City = city;
        District = district;
    }

    public static Location Create(string city, string district)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Sehir bilgisi gereklidir.");

        return new Location(city, district ?? string.Empty);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return District;
    }
}
