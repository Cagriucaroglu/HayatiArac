using HayatiArac.Modules.Advert.IntegrationEvents;
using HayatiArac.Modules.Favorite.Application.Interfaces;
using MassTransit;

namespace HayatiArac.Modules.Favorite.Infrastructure.IntegrationEventHandlers;

/// <summary>
/// Handles AdvertUpdatedIntegrationEvent from Advert module.
/// Keeps the AdvertSnapshot in sync when an advert's details change.
/// </summary>
public sealed class AdvertUpdatedIntegrationEventHandler : IConsumer<AdvertUpdatedIntegrationEvent>
{
    private readonly IAdvertSnapshotRepository _snapshotRepository;
    private readonly IFavoriteUnitOfWork _unitOfWork;

    public AdvertUpdatedIntegrationEventHandler(
        IAdvertSnapshotRepository snapshotRepository,
        IFavoriteUnitOfWork unitOfWork)
    {
        _snapshotRepository = snapshotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<AdvertUpdatedIntegrationEvent> context)
    {
        var @event = context.Message;

        var snapshot = await _snapshotRepository.GetByAdvertIdAsync(@event.AdvertId, context.CancellationToken);
        if (snapshot is null)
            return; // Snapshot not yet created; AdvertCreated event may arrive later

        snapshot.Update(
            @event.Title,
            @event.Brand,
            @event.Model,
            @event.Year,
            @event.Mileage,
            @event.Price,
            @event.Currency,
            @event.City);

        await _snapshotRepository.UpdateAsync(snapshot, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
