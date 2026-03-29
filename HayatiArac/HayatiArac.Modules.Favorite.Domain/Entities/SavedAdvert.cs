using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Favorite.Domain.Entities;

public class SavedAdvert : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid AdvertId { get; private set; }

    private SavedAdvert() { }

    public static SavedAdvert Create(Guid userId, Guid advertId)
    {
        return new SavedAdvert
        {
            UserId = userId,
            AdvertId = advertId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
