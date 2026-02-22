using HayatiArac.Modules.User.Domain.Entities;
using HayatiArac.SharedKernel.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.User.Infrastructure.Persistence;

public class UserDbContext : DbContext, IUnitOfWork
{
    public const string SchemaName = "users";

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema for all tables
        modelBuilder.HasDefaultSchema(SchemaName);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}
