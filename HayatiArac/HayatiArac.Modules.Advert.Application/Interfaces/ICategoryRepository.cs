using HayatiArac.Modules.Advert.Domain.Entities;
using HayatiArac.SharedKernel.Application.Interfaces;

namespace HayatiArac.Modules.Advert.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetAllWithSubCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
