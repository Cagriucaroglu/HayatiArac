using HayatiArac.Modules.Advert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HayatiArac.Modules.Advert.Infrastructure.Persistence.Configurations;

public class AdvertOwnerInfoConfiguration : IEntityTypeConfiguration<AdvertOwnerInfo>
{
    public void Configure(EntityTypeBuilder<AdvertOwnerInfo> builder)
    {
        builder.ToTable("AdvertOwnerInfos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        // Unique index on UserId (one owner info per user)
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
