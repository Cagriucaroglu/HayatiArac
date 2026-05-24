using FluentAssertions;
using HayatiArac.IntegrationTests.Infrastructure;
using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Domain.Entities;
using HayatiArac.Modules.Advert.Domain.Enums;
using HayatiArac.Modules.Advert.Domain.ValueObjects;
using HayatiArac.Modules.Advert.Infrastructure.Persistence;
using HayatiArac.Modules.Advert.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using AdvertEntity = HayatiArac.Modules.Advert.Domain.Entities.Advert;

namespace HayatiArac.IntegrationTests.Advert;

/// <summary>
/// IClassFixture sayesinde tüm testler için tek bir container başlar,
/// her test sınıfı kendi verisini temizler.
/// </summary>
public class AdvertRepositoryTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly AdvertDbContext _context;
    private readonly AdvertRepository _repository;
    private Category _category = null!;
    private AdvertOwnerInfo _ownerInfo = null!;

    public AdvertRepositoryTests(DatabaseFixture fixture)
    {
        _context = fixture.AdvertContext;
        _repository = new AdvertRepository(_context);
    }

    public async Task InitializeAsync()
    {
        // Her test öncesi tabloları temizle
        _context.ChangeTracker.Clear();
        await _context.Database.ExecuteSqlRawAsync(
            $"DELETE FROM [{AdvertDbContext.Schema}].[AdvertImages]; " +
            $"DELETE FROM [{AdvertDbContext.Schema}].[Adverts]; " +
            $"DELETE FROM [{AdvertDbContext.Schema}].[AdvertOwnerInfos]; " +
            $"DELETE FROM [{AdvertDbContext.Schema}].[Categories];");

        // Paylaşılan seed verisi
        _category = Category.Create("Binek Araç", "binek-arac");
        _ownerInfo = AdvertOwnerInfo.Create(Guid.NewGuid(), "Test Kullanıcı", "test@test.com", "İstanbul");

        await _context.Categories.AddAsync(_category);
        await _context.AdvertOwnerInfos.AddAsync(_ownerInfo);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Search_ReturnsOnlyActiveAdverts_WhenNoStatusFilter()
    {
        await SeedAsync(CreateAdvert("Aktif İlan", status: AdvertStatus.Active));
        await SeedAsync(CreateAdvert("Pasif İlan", status: AdvertStatus.Inactive));

        var result = await _repository.SearchAdvertsPaginatedAsync(
            new SearchAdvertsRequestDto(null, null, null, Status: null, null, null, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Aktif İlan");
    }

    [Fact]
    public async Task Search_FiltersByCity()
    {
        await SeedAsync(CreateAdvert("İstanbul İlan", city: "İstanbul"));
        await SeedAsync(CreateAdvert("Ankara İlan", city: "Ankara"));

        var result = await _repository.SearchAdvertsPaginatedAsync(
            new SearchAdvertsRequestDto(null, null, City: "Ankara", null, null, null, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Ankara İlan");
    }

    [Fact]
    public async Task Search_FiltersByBrand()
    {
        await SeedAsync(CreateAdvert("BMW İlan", brand: "BMW"));
        await SeedAsync(CreateAdvert("Toyota İlan", brand: "Toyota"));

        var result = await _repository.SearchAdvertsPaginatedAsync(
            new SearchAdvertsRequestDto(null, null, null, null, null, null, Brand: "BMW", null, null, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Brand.Should().Be("BMW");
    }

    [Fact]
    public async Task Search_FiltersByPriceRange()
    {
        await SeedAsync(CreateAdvert("Ucuz İlan", price: 50_000m));
        await SeedAsync(CreateAdvert("Orta İlan", price: 200_000m));
        await SeedAsync(CreateAdvert("Pahalı İlan", price: 800_000m));

        var result = await _repository.SearchAdvertsPaginatedAsync(
            new SearchAdvertsRequestDto(null, null, null, null, MinPrice: 100_000m, MaxPrice: 500_000m, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Orta İlan");
    }

    [Fact]
    public async Task Search_FiltersByYearRange()
    {
        await SeedAsync(CreateAdvert("Eski Araç", year: 2010));
        await SeedAsync(CreateAdvert("Orta Araç", year: 2018));
        await SeedAsync(CreateAdvert("Yeni Araç", year: 2023));

        var result = await _repository.SearchAdvertsPaginatedAsync(
            new SearchAdvertsRequestDto(null, null, null, null, null, null, null, null, MinYear: 2015, MaxYear: 2020, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Orta Araç");
    }

    [Fact]
    public async Task Search_FiltersByFuelType()
    {
        await SeedAsync(CreateAdvert("Dizel", fuelType: FuelType.Diesel));
        await SeedAsync(CreateAdvert("Elektrik", fuelType: FuelType.Electric));

        var result = await _repository.SearchAdvertsPaginatedAsync(
            new SearchAdvertsRequestDto(null, null, null, null, null, null, null, null, null, null, null, FuelType: FuelType.Electric, null, null),
            CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].FuelType.Should().Be(FuelType.Electric);
    }

    [Fact]
    public async Task Search_ReturnsPaginatedResults()
    {
        for (var i = 1; i <= 5; i++)
            await SeedAsync(CreateAdvert($"İlan {i}"));

        var result = await _repository.SearchAdvertsPaginatedAsync(
            new SearchAdvertsRequestDto(null, null, null, null, null, null, null, null, null, null, null, null, null, null, PageNumber: 2, PageSize: 2),
            CancellationToken.None);

        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.PageNumber.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Search_FiltersBySearchTerm_InTitle()
    {
        await SeedAsync(CreateAdvert("Temiz BMW X5"));
        await SeedAsync(CreateAdvert("Toyota Corolla satılık"));

        var result = await _repository.SearchAdvertsPaginatedAsync(
            new SearchAdvertsRequestDto(SearchTerm: "BMW", null, null, null, null, null, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Contain("BMW");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private AdvertEntity CreateAdvert(
        string title,
        string city = "İstanbul",
        string brand = "Toyota",
        decimal price = 150_000m,
        int year = 2020,
        FuelType fuelType = FuelType.Gasoline,
        AdvertStatus status = AdvertStatus.Active)
    {
        var advert = AdvertEntity.Create(
            title: title,
            description: "Test açıklama",
            price: Money.Create(price, CurrencyCode.TRY),
            location: Location.Create(city, "Merkez"),
            condition: AdvertCondition.Used,
            categoryId: _category.Id,
            ownerUserId: _ownerInfo.UserId,
            ownerInfo: _ownerInfo,
            brand: brand,
            model: "Corolla",
            year: year,
            mileage: 50_000,
            fuelType: fuelType,
            transmissionType: TransmissionType.Automatic);

        if (status == AdvertStatus.Inactive)
            advert.Deactivate();

        return advert;
    }

    private async Task SeedAsync(AdvertEntity advert)
    {
        await _context.Adverts.AddAsync(advert);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }
}
