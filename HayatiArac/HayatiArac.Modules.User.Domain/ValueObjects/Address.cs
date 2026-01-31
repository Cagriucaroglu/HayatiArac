using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.User.Domain.ValueObjects;

public class Address : ValueObject
{
    public string City { get; private set; }
    public string District { get; private set; }
    public string? Street { get; private set; }

    private Address(string city, string district, string? street)
    {
        City = city;
        District = district;
        Street = street;
    }

    public static Address Create(string city, string district, string? street = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Sehir bos olamaz.");
        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("Ilce bos olamaz.");

        return new Address(city, district, street);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return District;
        yield return Street;
    }
}
