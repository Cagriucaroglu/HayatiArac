using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Domain.ValueObjects;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using MediatR;

namespace HayatiArac.Modules.Advert.Application.Commands.CreateAdvert;

public sealed class CreateAdvertCommandHandler : IRequestHandler<CreateAdvertCommand, Result<Guid>>
{
    private readonly IAdvertRepository _advertRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAdvertOwnerInfoRepository _ownerInfoRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdvertCommandHandler(
        IAdvertRepository advertRepository,
        ICategoryRepository categoryRepository,
        IAdvertOwnerInfoRepository ownerInfoRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _advertRepository = advertRepository;
        _categoryRepository = categoryRepository;
        _ownerInfoRepository = ownerInfoRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateAdvertCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<Guid>(Error.Unauthorized("User.Unauthorized", "Kullanıcı doğrulanamadı."));

        var dto = request.Request;

        // Kategori kontrolü
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<Guid>(Error.NotFound("Category.NotFound", "Kategori bulunamadı."));

        // Kullanıcı bilgisi kontrolü
        var ownerInfo = await _ownerInfoRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (ownerInfo is null)
            return Result.Failure<Guid>(Error.NotFound("OwnerInfo.NotFound", "Kullanıcı profili bulunamadı."));

        // İş Kuralı 1: Zaten aktif bir ilan var mı?
        var activeAdvert = await _advertRepository.GetActiveAdvertByUserAsync(userId.Value, cancellationToken);
        if (activeAdvert is not null)
            return Result.Failure<Guid>(Error.Conflict("Advert.AlreadyActive", "Zaten aktif bir ilanınız bulunmaktadır. Yeni ilan vermek için önce mevcut ilanınızı kaldırın."));

        // İş Kuralı 2: 30 günlük bekleme süresi kontrolü
        if (ownerInfo.LastAdvertPublishedAt.HasValue)
        {
            var cooldownEnd = ownerInfo.LastAdvertPublishedAt.Value.AddDays(30);
            if (DateTime.UtcNow < cooldownEnd)
            {
                var daysLeft = (int)Math.Ceiling((cooldownEnd - DateTime.UtcNow).TotalDays);
                return Result.Failure<Guid>(Error.Validation("Advert.CooldownActive",
                    $"Son ilanınızdan itibaren 30 gün dolmadı. Yeni ilan için {daysLeft} gün beklemeniz gerekmektedir."));
            }
        }

        // İlan oluştur (ExpiresAt = şimdi + 30 gün)
        var price = Money.Create(dto.Price, dto.Currency);
        var location = Location.Create(dto.City, dto.District);

        var advert = Domain.Entities.Advert.Create(
            dto.Title,
            dto.Description,
            price,
            location,
            dto.Condition,
            dto.CategoryId,
            userId.Value,
            ownerInfo,
            dto.ShowPhoneNumber);

        if (dto.ImageUrls is not null)
        {
            int order = 0;
            foreach (var imageUrl in dto.ImageUrls)
                advert.AddImage(imageUrl, order++);
        }

        await _advertRepository.AddAsync(advert, cancellationToken);

        // İş Kuralı 3: Son yayın tarihini güncelle
        ownerInfo.UpdateLastPublished();
        await _ownerInfoRepository.UpdateAsync(ownerInfo, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(advert.Id);
    }
}
