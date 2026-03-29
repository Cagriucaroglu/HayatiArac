using HayatiArac.Modules.Advert.IntegrationEvents;
using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.Modules.Favorite.Domain.Entities;
using MassTransit;

namespace HayatiArac.Modules.Favorite.Infrastructure.IntegrationEventHandlers;

/// <summary>
/// Handles AdvertCreatedIntegrationEvent from Advert module.
/// Creates an AdvertSnapshot (denormalized copy) so Favorite listings
/// don't need cross-module DB queries.
/// </summary>
public sealed class AdvertCreatedIntegrationEventHandler : IConsumer<AdvertCreatedIntegrationEvent>
{
    private readonly IAdvertSnapshotRepository _snapshotRepository;
    private readonly IFavoriteUnitOfWork _unitOfWork;

    public AdvertCreatedIntegrationEventHandler(
        IAdvertSnapshotRepository snapshotRepository,
        IFavoriteUnitOfWork unitOfWork)
    {
        _snapshotRepository = snapshotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<AdvertCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        // Idempotency: skip if snapshot already exists
        var existing = await _snapshotRepository.GetByAdvertIdAsync(@event.AdvertId, context.CancellationToken);
        if (existing is not null)
            return;

        var snapshot = AdvertSnapshot.Create(
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

        await _snapshotRepository.AddAsync(snapshot, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
