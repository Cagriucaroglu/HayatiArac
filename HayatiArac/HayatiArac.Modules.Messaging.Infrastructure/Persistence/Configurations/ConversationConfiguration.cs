using HayatiArac.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HayatiArac.Modules.Messaging.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdvertId).IsRequired();

        builder.Property(x => x.AdvertTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.BuyerId).IsRequired();

        builder.Property(x => x.BuyerDisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.SellerId).IsRequired();

        builder.Property(x => x.SellerDisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LastMessageAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        // Aynı ilan için aynı alıcı-satıcı çifti tek konuşma
        builder.HasIndex(x => new { x.BuyerId, x.SellerId, x.AdvertId }).IsUnique();
        builder.HasIndex(x => x.BuyerId);
        builder.HasIndex(x => x.SellerId);

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
