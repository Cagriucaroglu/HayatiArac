using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.User.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string CountryCode { get; private set; }
    public string Number { get; private set; }

    private PhoneNumber(string countryCode, string number)
    {
        CountryCode = countryCode;
        Number = number;
    }

    public static PhoneNumber Create(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Telefon numarasi bos olamaz.");

        return new PhoneNumber(countryCode, number);
    }

    public override string ToString() => $"+{CountryCode}{Number}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }
}
