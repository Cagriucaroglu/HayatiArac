using System.Security.Claims;
using HayatiArac.Modules.User.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HayatiArac.Modules.User.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? Guid.Parse(userId) : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User
        .FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User
        .Identity?.IsAuthenticated ?? false;
}
