using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.User.IntegrationEvents;
using HayatiArac.SharedKernel.Application.Interfaces;
using MassTransit;

namespace HayatiArac.Modules.Advert.Application.IntegrationEventHandlers;

/// <summary>
/// Handles UserProfileUpdatedIntegrationEvent from User module
/// Updates AdvertOwnerInfo (denormalized user data) when user profile changes
/// </summary>
public sealed class UserProfileUpdatedIntegrationEventHandler : IConsumer<UserProfileUpdatedIntegrationEvent>
{
    private readonly IAdvertOwnerInfoRepository _ownerInfoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserProfileUpdatedIntegrationEventHandler(
        IAdvertOwnerInfoRepository ownerInfoRepository,
        IUnitOfWork unitOfWork)
    {
        _ownerInfoRepository = ownerInfoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<UserProfileUpdatedIntegrationEvent> context)
    {
        var @event = context.Message;

        // Get existing owner info
        var ownerInfo = await _ownerInfoRepository.GetByUserIdAsync(@event.UserId, context.CancellationToken);
        if (ownerInfo is null)
            return; // User hasn't created any adverts yet, no need to update

        // Update denormalized data
        var displayName = $"{@event.FirstName} {@event.LastName}".Trim();
        ownerInfo.UpdateFromUser(displayName, @event.City);

        await _ownerInfoRepository.UpdateAsync(ownerInfo, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
