using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HayatiArac.Modules.Advert.Infrastructure.Persistence.Configurations;

public class AdvertConfiguration : IEntityTypeConfiguration<Domain.Entities.Advert>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Advert> builder)
    {
        builder.ToTable("Adverts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.Condition)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.OwnerUserId)
            .IsRequired();

        // Owned Type: Money (Value Object)
        builder.OwnsOne(x => x.Price, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnName("Price")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .IsRequired()
                .HasColumnName("Currency")
                .HasConversion<string>();
        });

        // Owned Type: Location (Value Object)
        builder.OwnsOne(x => x.Location, location =>
        {
            location.Property(l => l.City)
                .IsRequired()
                .HasColumnName("City")
                .HasMaxLength(100);

            location.Property(l => l.District)
                .IsRequired()
                .HasColumnName("District")
                .HasMaxLength(100);
        });

        // Relationship: Advert -> Category (Many-to-One)
        builder.HasOne(x => x.Category)
            .WithMany(c => c.Adverts)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Advert -> AdvertOwnerInfo (One-to-One)
        builder.HasOne(x => x.OwnerInfo)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .HasPrincipalKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Advert -> AdvertImages (One-to-Many)
        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey("AdvertId")
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events collection
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.ShowPhoneNumber).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        // Index for performance
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.OwnerUserId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ExpiresAt);

        // Partial unique index: bir kullanıcının aynı anda yalnızca 1 aktif ilanı (race condition koruması)
        builder.HasIndex(x => x.OwnerUserId)
            .IsUnique()
            .HasFilter("[Status] = 'Active'")
            .HasDatabaseName("IX_Adverts_OwnerUserId_Active");
    }
}
