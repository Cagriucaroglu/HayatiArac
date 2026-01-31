using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Domain.Entities;
using HayatiArac.Modules.User.IntegrationEvents;
using HayatiArac.SharedKernel.Application.Interfaces;
using MassTransit;

namespace HayatiArac.Modules.Advert.Application.IntegrationEventHandlers;

/// <summary>
/// Handles UserRegisteredIntegrationEvent from User module
/// Creates AdvertOwnerInfo (denormalized user data) for Advert bounded context
/// </summary>
public sealed class UserRegisteredIntegrationEventHandler : IConsumer<UserRegisteredIntegrationEvent>
{
    private readonly IAdvertOwnerInfoRepository _ownerInfoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserRegisteredIntegrationEventHandler(
        IAdvertOwnerInfoRepository ownerInfoRepository,
        IUnitOfWork unitOfWork)
    {
        _ownerInfoRepository = ownerInfoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var @event = context.Message;

        // Check if already exists (idempotency)
        var existing = await _ownerInfoRepository.GetByUserIdAsync(@event.UserId, context.CancellationToken);
        if (existing is not null)
            return; // Already processed

        // Create denormalized owner info
        var displayName = $"{@event.FirstName} {@event.LastName}".Trim();
        var ownerInfo = AdvertOwnerInfo.Create(
            @event.UserId,
            displayName,
            @event.Email);

        await _ownerInfoRepository.AddAsync(ownerInfo, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
