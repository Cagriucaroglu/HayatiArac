using HayatiArac.Modules.Advert.Domain.Enums;
using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Advert.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public CurrencyCode Currency { get; private set; }

    private Money() { }

    private Money(decimal amount, CurrencyCode currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, CurrencyCode currency = CurrencyCode.TRY)
    {
        if (amount < 0)
            throw new ArgumentException("Fiyat negatif olamaz.");

        return new Money(amount, currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
