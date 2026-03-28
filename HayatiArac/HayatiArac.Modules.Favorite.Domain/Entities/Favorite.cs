using HayatiArac.SharedKernel.Domain;

namespace HayatiArac.Modules.Favorite.Domain.Entities;

public class Favorite : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid AdvertId { get; private set; }

    private Favorite() { }

    public static Favorite Create(Guid userId, Guid advertId)
    {
        return new Favorite
        {
            UserId = userId,
            AdvertId = advertId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
