using HayatiArac.Modules.Advert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HayatiArac.Modules.Advert.Infrastructure.Persistence.Configurations;

public class AdvertImageConfiguration : IEntityTypeConfiguration<AdvertImage>
{
    public void Configure(EntityTypeBuilder<AdvertImage> builder)
    {
        builder.ToTable("AdvertImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        // AdvertId is added as shadow property via relationship in AdvertConfiguration
        builder.Property<Guid>("AdvertId").IsRequired();

        builder.HasIndex("AdvertId");
        builder.HasIndex(x => x.DisplayOrder);
    }
}
