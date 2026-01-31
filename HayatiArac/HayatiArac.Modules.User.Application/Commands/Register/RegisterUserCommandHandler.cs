using HayatiArac.Modules.User.Domain.Entities;
using HayatiArac.Modules.User.IntegrationEvents;
using HayatiArac.SharedKernel.Application;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HayatiArac.Modules.User.Application.Commands.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IPublishEndpoint publishEndpoint)
    {
        _userManager = userManager;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = ApplicationUser.Create(request.Email, request.FirstName, request.LastName);
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Result.Failure<RegisterUserResponse>(result.Errors.Select(e => e.Description));
        }

        // Publish Integration Event via MassTransit
        await _publishEndpoint.Publish(new UserRegisteredIntegrationEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        }, cancellationToken);

        return Result.Success(new RegisterUserResponse(user.Id, user.Email!));
    }
}
