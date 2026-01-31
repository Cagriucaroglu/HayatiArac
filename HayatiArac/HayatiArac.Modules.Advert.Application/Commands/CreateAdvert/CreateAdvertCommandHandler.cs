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
            return Result.Failure<Guid>("Kullanıcı doğrulanamadı.");

        var dto = request.Request;

        // Verify category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<Guid>("Kategori bulunamadı.");

        // Get owner info (should exist via integration event from User module)
        var ownerInfo = await _ownerInfoRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (ownerInfo is null)
            return Result.Failure<Guid>("Kullanıcı bilgisi bulunamadı. Lütfen profil bilgilerinizi tamamlayın.");

        // Create advert
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
            ownerInfo);

        // Add images if provided
        if (dto.ImageUrls is not null)
        {
            int order = 0;
            foreach (var imageUrl in dto.ImageUrls)
            {
                advert.AddImage(imageUrl, order++);
            }
        }

        await _advertRepository.AddAsync(advert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(advert.Id);
    }
}
