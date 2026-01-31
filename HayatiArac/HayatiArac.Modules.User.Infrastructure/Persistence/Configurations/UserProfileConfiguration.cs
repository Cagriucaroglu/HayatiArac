using HayatiArac.Modules.User.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HayatiArac.Modules.User.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(p => p.Id);

        builder.OwnsOne(p => p.PhoneNumber, phone =>
        {
            phone.Property(p => p.CountryCode).HasMaxLength(5).HasColumnName("PhoneCountryCode");
            phone.Property(p => p.Number).HasMaxLength(15).HasColumnName("PhoneNumber");
        });

        builder.OwnsOne(p => p.Address, address =>
        {
            address.Property(a => a.City).HasMaxLength(100).HasColumnName("City");
            address.Property(a => a.District).HasMaxLength(100).HasColumnName("District");
            address.Property(a => a.Street).HasMaxLength(200).HasColumnName("Street");
        });

        builder.Property(p => p.AvatarUrl).HasMaxLength(500);
        builder.Property(p => p.Bio).HasMaxLength(1000);
    }
}
