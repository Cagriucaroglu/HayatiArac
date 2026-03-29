using HayatiArac.Modules.Favorite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HayatiArac.Modules.Favorite.Infrastructure.Persistence.Configurations;

public class SavedAdvertConfiguration : IEntityTypeConfiguration<SavedAdvert>
{
    public void Configure(EntityTypeBuilder<SavedAdvert> builder)
    {
        builder.ToTable("SavedAdverts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.AdvertId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        // One user can save each advert only once
        builder.HasIndex(x => new { x.UserId, x.AdvertId }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
