using HayatiArac.Modules.User.Application.DTOs;
using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HayatiArac.Modules.User.Application.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, Result<UserProfileDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<Result<UserProfileDto>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == null)
            return Result.Failure<UserProfileDto>("Kullanici giris yapmamis.");

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.Value.ToString());
        if (user == null)
            return Result.Failure<UserProfileDto>("Kullanici bulunamadi.");

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
