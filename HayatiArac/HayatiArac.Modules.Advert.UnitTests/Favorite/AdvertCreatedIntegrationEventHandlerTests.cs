using HayatiArac.Modules.Advert.IntegrationEvents;
using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.Modules.Favorite.Domain.Entities;
using HayatiArac.Modules.Favorite.Infrastructure.IntegrationEventHandlers;
using MassTransit;
using NSubstitute;

namespace HayatiArac.Modules.Advert.UnitTests.Favorite;

public class AdvertCreatedIntegrationEventHandlerTests
{
    private readonly IAdvertSnapshotRepository repository;
    private readonly IFavoriteUnitOfWork unitOfWork;
    private readonly AdvertCreatedIntegrationEventHandler handler;

    public AdvertCreatedIntegrationEventHandlerTests()
    {
        repository = Substitute.For<IAdvertSnapshotRepository>();
        unitOfWork = Substitute.For<IFavoriteUnitOfWork>();
        handler = new AdvertCreatedIntegrationEventHandler(repository, unitOfWork);
    }

    [Fact]
    public async Task Consume_CreatesSnapshot_WhenAdvertCreated()
    {
        AdvertCreatedIntegrationEvent @event = CreateEvent();
        ConsumeContext<AdvertCreatedIntegrationEvent> context = CreateContext(@event);
        
        repository.GetByAdvertIdAsync(@event.AdvertId , Arg.Any<CancellationToken>())
            .Returns((AdvertSnapshot?)null);
        await handler.Consume(context);

        await repository.Received(1).AddAsync(
            Arg.Is<AdvertSnapshot>(s => 
                    s.AdvertId == @event.AdvertId &&
                    s.Title == @event.Title &&
                    s.Brand == @event.Brand
                ),
            Arg.Any<CancellationToken>()
        );
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_SkipsCreation_WhenSnapshotAlreadyExists()
    {
        AdvertCreatedIntegrationEvent @event = CreateEvent();
        ConsumeContext<AdvertCreatedIntegrationEvent> context = CreateContext(@event);

        var existingSnapshot = AdvertSnapshot.Create(
            @event.AdvertId,
            @event.Title,
            @event.Brand,
            @event.Model,
            @event.Year,
            @event.Mileage,
            @event.Price,
            @event.Currency,
            @event.City,
            @event.Status,
            @event.ImageUrls);

        repository.GetByAdvertIdAsync(@event.AdvertId, Arg.Any<CancellationToken>()).Returns(existingSnapshot);

        await handler.Consume(context);

        await repository.DidNotReceive().AddAsync(Arg.Any<AdvertSnapshot>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

    }

    // Helpers
    private static AdvertCreatedIntegrationEvent CreateEvent() => new()
    {
        AdvertId = Guid.NewGuid(),
        Title = "Test İlan",
        OwnerUserId = Guid.NewGuid(),
        Brand = "Toyota",
        Model = "Corolla",
        Year = 2020,
        Mileage = 50_000,
        Price = 150_000m,
        Currency = "TRY",
        City = "İstanbul",
        Status = "Active",
        ImageUrls = []
    };
    
    private static ConsumeContext<AdvertCreatedIntegrationEvent> CreateContext(
        AdvertCreatedIntegrationEvent @event)
    {
        ConsumeContext<AdvertCreatedIntegrationEvent> context = Substitute.For<ConsumeContext<AdvertCreatedIntegrationEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

}
