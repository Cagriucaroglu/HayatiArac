using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HayatiArac.Modules.User.Application.Commands.Login;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result>
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginUserCommandHandler(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<Result> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure("Gecersiz e-posta veya sifre.");

        var result = await _signInManager.PasswordSignInAsync(
            user, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (!result.Succeeded)
            return Result.Failure("Gecersiz e-posta veya sifre.");

        user.UpdateLastLogin();
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}
