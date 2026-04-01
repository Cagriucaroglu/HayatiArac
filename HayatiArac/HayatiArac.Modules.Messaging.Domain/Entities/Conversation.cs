using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Messaging.Domain.Entities;

/// <summary>
/// Bir alıcı ile satıcı arasındaki belirli bir ilana ait mesajlaşma oturumu.
/// </summary>
public class Conversation : BaseEntity
{
    public Guid AdvertId { get; private set; }
    public string AdvertTitle { get; private set; } = string.Empty;
    public Guid BuyerId { get; private set; }
    public string BuyerDisplayName { get; private set; } = string.Empty;
    public Guid SellerId { get; private set; }
    public string SellerDisplayName { get; private set; } = string.Empty;
    public DateTime LastMessageAt { get; private set; }

    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    private Conversation() { }

    public static Conversation Create(
        Guid advertId,
        string advertTitle,
        Guid buyerId,
        string buyerDisplayName,
        Guid sellerId,
        string sellerDisplayName)
    {
        return new Conversation
        {
            AdvertId = advertId,
            AdvertTitle = advertTitle,
            BuyerId = buyerId,
            BuyerDisplayName = buyerDisplayName,
            SellerId = sellerId,
            SellerDisplayName = sellerDisplayName,
            LastMessageAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateLastMessageAt()
    {
        LastMessageAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
