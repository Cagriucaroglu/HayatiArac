using HayatiArac.Modules.Favorite.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HayatiArac.Modules.Favorite.Infrastructure.Persistence;

public class FavoriteDbContext : DbContext
{
    public const string Schema = "favorites";

    public DbSet<SavedAdvert> SavedAdverts => Set<SavedAdvert>();
    public DbSet<AdvertSnapshot> AdvertSnapshots => Set<AdvertSnapshot>();

    public FavoriteDbContext(DbContextOptions<FavoriteDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FavoriteDbContext).Assembly);
    }
}
