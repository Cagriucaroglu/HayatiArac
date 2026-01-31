using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Advert.Domain.Entities;

public class AdvertImage : BaseEntity
{
    public Guid AdvertId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    private AdvertImage() { }

    public AdvertImage(Guid id, Guid advertId, string url, int displayOrder)
    {
        Id = id;
        AdvertId = advertId;
        Url = url;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }
}
