using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Domain.Entities;
using HayatiArac.Modules.User.Domain.Enums;
using HayatiArac.SharedKernel.Application;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HayatiArac.Modules.User.Application.Commands.EnsureAdminExists;

public sealed class EnsureAdminExistsCommandHandler : IRequestHandler<EnsureAdminExistsCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<EnsureAdminExistsCommandHandler> _logger;

    public EnsureAdminExistsCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<EnsureAdminExistsCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(EnsureAdminExistsCommand request, CancellationToken cancellationToken)
    {
        const string adminEmail = "admin@hayatiarac.com";
        const string adminPassword = "Admin123!";

        // Check if admin already exists
        var existingAdmin = await _userRepository.GetByEmailAsync(adminEmail, cancellationToken);
        if (existingAdmin != null)
        {
            _logger.LogInformation("Admin user already exists: {Email}", adminEmail);
            return Result<Guid>.Success(existingAdmin.Id);
        }

        // Create admin user
        var passwordHash = _passwordHasher.HashPassword(adminPassword);
        
        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            PhoneNumber = "5551234567",
            PasswordHash = passwordHash,
            Role = UserRole.Admin,
            IsEmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(admin, cancellationToken);

        _logger.LogInformation("✅ Admin user created successfully");
        _logger.LogInformation("📧 Email: {Email}", adminEmail);
        _logger.LogInformation("🔑 Password: {Password}", adminPassword);

        return Result<Guid>.Success(admin.Id);
    }
}
