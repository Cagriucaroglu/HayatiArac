using HayatiArac.Modules.Notification.Application.Interfaces;
using HayatiArac.Modules.Notification.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Notification.Application.Commands.MarkAsRead;

public sealed class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public MarkAsReadCommandHandler(
        INotificationRepository notificationRepository,
        INotificationUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        Guid? userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        AppNotification? notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification is null)
            return Result.Failure(Error.NotFound("Notification.NotFound", "Bildirim bulunamadı."));

        if (notification.UserId != userId.Value)
            return Result.Failure(Error.Forbidden("Notification.Forbidden", "Bu bildirime erişim yetkiniz yok."));

        notification.MarkAsRead();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
