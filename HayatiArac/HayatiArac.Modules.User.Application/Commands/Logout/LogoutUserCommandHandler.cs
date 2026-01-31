using HayatiArac.Modules.User.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HayatiArac.Modules.User.Application.Commands.Logout;

public sealed class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutUserCommandHandler(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await _signInManager.SignOutAsync();
    }
}
