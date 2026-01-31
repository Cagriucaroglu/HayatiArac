using HayatiArac.Modules.Advert.Domain.Entities;
using HayatiArac.Modules.Advert.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Advert.Infrastructure.Persistence;

public class AdvertDbContext : DbContext
{
    public const string Schema = "adverts";

    public DbSet<Domain.Entities.Advert> Adverts => Set<Domain.Entities.Advert>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AdvertOwnerInfo> AdvertOwnerInfos => Set<AdvertOwnerInfo>();
    public DbSet<AdvertImage> AdvertImages => Set<AdvertImage>();

    public AdvertDbContext(DbContextOptions<AdvertDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for this module
        modelBuilder.HasDefaultSchema(Schema);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdvertDbContext).Assembly);
    }
}
