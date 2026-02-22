using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.SharedKernel.Application;
using MediatR;

namespace HayatiArac.Modules.User.Application.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, Result<UserProfileDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<Result<UserProfileDto>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == null)
            return Result.Failure<UserProfileDto>(Error.Unauthorized("User.Unauthenticated", "Kullanici giris yapmamis."));

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user == null)
            return Result.Failure<UserProfileDto>(Error.NotFound("User.NotFound", "Kullanici bulunamadi."));

        var dto = new UserProfileDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Profile?.PhoneNumber?.ToString(),
            user.Profile?.Address?.City,
            user.Profile?.Address?.District,
            user.Profile?.AvatarUrl,
            user.Profile?.Bio,
            user.CreatedAt);
        return Result.Success(dto);
    }
}
