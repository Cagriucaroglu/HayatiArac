using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Domain.Entities;
using HayatiArac.Modules.User.IntegrationEvents;
using HayatiArac.SharedKernel.Application;
using MassTransit;
using MediatR;

namespace HayatiArac.Modules.User.Application.Commands.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return Result.Failure<RegisterUserResponse>(
                Error.Conflict("Auth.EmailAlreadyExists", "A user with this email already exists"));
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = ApplicationUser.Create(request.Email, request.FirstName, request.LastName, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);

        // Publish Integration Event via MassTransit
        await _publishEndpoint.Publish(new UserRegisteredIntegrationEvent
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        }, cancellationToken);

        return Result.Success(new RegisterUserResponse(user.Id, user.Email));
    }
}
