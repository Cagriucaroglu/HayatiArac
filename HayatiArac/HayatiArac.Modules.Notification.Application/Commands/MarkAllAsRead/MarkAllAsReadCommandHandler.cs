using HayatiArac.Modules.Notification.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Notification.Application.Commands.MarkAllAsRead;

public sealed class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public MarkAllAsReadCommandHandler(
        INotificationRepository notificationRepository,
        INotificationUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        Guid? userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        await _notificationRepository.MarkAllAsReadAsync(userId.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
