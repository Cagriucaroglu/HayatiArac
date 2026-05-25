using FluentAssertions;
using HayatiArac.IntegrationTests.Infrastructure;
using HayatiArac.Modules.Advert.Application.Commands.CreateAdvert;
using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Domain.Entities;
using HayatiArac.Modules.Advert.Domain.Enums;
using HayatiArac.Modules.Advert.Infrastructure.Persistence;
using HayatiArac.Modules.Advert.Infrastructure.Persistence.Repositories;
using HayatiArac.Modules.Advert.IntegrationEvents;
using HayatiArac.Modules.Favorite.Infrastructure.IntegrationEventHandlers;
using HayatiArac.Modules.Favorite.Infrastructure.Persistence;
using HayatiArac.Modules.Favorite.Infrastructure.Persistence.Repositories;
using HayatiArac.SharedKernel.Application.Interfaces;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using MassTransit;
using MassTransit.Testing;
using HayatiArac.Modules.Favorite.Application.Interfaces;

namespace HayatiArac.IntegrationTests.Events;

/// <summary>
/// CreateAdvert komutu çalıştırıldığında, AdvertCreatedIntegrationEvent üzerinden
/// Favorite modülünde AdvertSnapshot oluşturulduğunu doğrular.
/// Hem gerçek DB (TestContainers) hem MassTransit Test Harness kullanır.
/// </summary>
[Collection("Database")]
public class AdvertCreatedFlowTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;
    private Guid _userId;
    private Category _category = null!;

    public AdvertCreatedFlowTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Her test öncesi her iki DB'yi de temizle
        _fixture.AdvertContext.ChangeTracker.Clear();
        await _fixture.AdvertContext.Database.ExecuteSqlRawAsync(
            $"DELETE FROM [{AdvertDbContext.Schema}].[AdvertImages]; " +
            $"DELETE FROM [{AdvertDbContext.Schema}].[Adverts]; " +
            $"DELETE FROM [{AdvertDbContext.Schema}].[AdvertOwnerInfos]; " +
            $"DELETE FROM [{AdvertDbContext.Schema}].[Categories];");

        _fixture.FavoriteContext.ChangeTracker.Clear();
        await _fixture.FavoriteContext.Database.ExecuteSqlRawAsync(
            $"DELETE FROM [{FavoriteDbContext.Schema}].[AdvertSnapshots];");

        // Seed: category ve ownerInfo
        _userId = Guid.NewGuid();
        _category = Category.Create("Binek Araç", "binek-arac");
        var ownerInfo = AdvertOwnerInfo.Create(_userId, "Test Kullanıcı", "test@test.com");

        _fixture.AdvertContext.Categories.Add(_category);
        _fixture.AdvertContext.AdvertOwnerInfos.Add(ownerInfo);
        await _fixture.AdvertContext.SaveChangesAsync();
        _fixture.AdvertContext.ChangeTracker.Clear();

        // DI container: sadece Favorite tarafı ve MassTransit için
        // Favorite için fixture'dan bağımsız yeni bir FavoriteDbContext oluşturuyoruz
        // (fixture context'ini DI scope dispose etmesin diye)
        _provider = new ServiceCollection()
            .AddDbContext<FavoriteDbContext>(opt =>
                opt.UseSqlServer(_fixture.ConnectionString, sql =>
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", FavoriteDbContext.Schema)))
            .AddScoped<IAdvertSnapshotRepository, AdvertSnapshotRepository>()
            .AddScoped<IFavoriteUnitOfWork, FavoriteUnitOfWork>()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<AdvertCreatedIntegrationEventHandler>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task CreateAdvert_PublishesEvent_AndCreatesAdvertSnapshot()
    {
        // Advert tarafı: fixture'ın context'i ile manual olarak oluşturuyoruz
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(_userId);

        var handler = new CreateAdvertCommandHandler(
            advertRepository: new AdvertRepository(_fixture.AdvertContext),
            categoryRepository: new CategoryRepository(_fixture.AdvertContext),
            ownerInfoRepository: new AdvertOwnerInfoRepository(_fixture.AdvertContext),
            currentUserService: currentUserService,
            unitOfWork: new UnitOfWork(_fixture.AdvertContext),
            publishEndpoint: _harness.Bus);  // Test Harness'in Bus'ı IPublishEndpoint olarak

        var command = new CreateAdvertCommand(new CreateAdvertDto(
            Title: "Temiz BMW X5",
            Description: "Hasar kayıtsız, ilk elden",
            Price: 1_500_000m,
            Currency: CurrencyCode.TRY,
            City: "İstanbul",
            District: "Kadıköy",
            Condition: AdvertCondition.Used,
            CategoryId: _category.Id,
            Brand: "BMW",
            Model: "X5",
            Year: 2022,
            Mileage: 30_000,
            FuelType: FuelType.Diesel,
            TransmissionType: TransmissionType.Automatic,
            HasHeavyDamageRecord: false,
            ShowPhoneNumber: true,
            ImageUrls: null));

        // ── Act ─────────────────────────────────────────────────────────────
        var result = await handler.Handle(command, CancellationToken.None);

        // ── Assert ──────────────────────────────────────────────────────────

        // 1. Command başarılı sonuçlandı mı?
        result.IsSuccess.Should().BeTrue();

        // ── Diagnosis ───────────────────────────────────────────────────────
        // Event gerçekten publish edildi mi?
        (await _harness.Published.Any<AdvertCreatedIntegrationEvent>()).Should().BeTrue();

        // Consumer exception fırlattı mı?
        (await _harness.Published.Any<Fault<AdvertCreatedIntegrationEvent>>()).Should().BeFalse();

        // 2. MassTransit event'i tüketti mi?
        (await _harness.Consumed.Any<AdvertCreatedIntegrationEvent>()).Should().BeTrue();

        // 3. Favorite modülünün DB'sinde snapshot oluştu mu?
        _fixture.FavoriteContext.ChangeTracker.Clear();
        var snapshot = await _fixture.FavoriteContext.AdvertSnapshots
            .FirstOrDefaultAsync(s => s.AdvertId == result.Value);

        snapshot.Should().NotBeNull();
        snapshot!.Title.Should().Be("Temiz BMW X5");
        snapshot.Brand.Should().Be("BMW");
        snapshot.City.Should().Be("İstanbul");
        snapshot.Status.Should().Be("Active");
    }
}
