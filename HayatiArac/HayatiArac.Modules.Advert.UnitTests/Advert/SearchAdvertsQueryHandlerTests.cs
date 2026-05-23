using FluentAssertions;
using HayatiArac.Modules.Advert.Application.DTOs;
using HayatiArac.Modules.Advert.Application.Interfaces;
using HayatiArac.Modules.Advert.Application.Queries.SearchAdverts;
using HayatiArac.Modules.Advert.Domain.Entities;
using HayatiArac.Modules.Advert.Domain.Enums;
using HayatiArac.Modules.Advert.Domain.ValueObjects;
using NSubstitute;

namespace HayatiArac.Modules.Advert.UnitTests.Advert;

public class SearchAdvertsQueryHandlerTests
{
    private readonly IAdvertRepository _advertRepository;
    private readonly SearchAdvertsQueryHandler _handler;

    public SearchAdvertsQueryHandlerTests()
    {
        _advertRepository = Substitute.For<IAdvertRepository>();
        _handler = new SearchAdvertsQueryHandler(_advertRepository);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WithPagedResults()
    {
        var advert = CreateTestAdvert("BMW X5");
        var pagedResult = new PagedResultDto<Domain.Entities.Advert>(
            [advert], TotalCount: 1, PageNumber: 1, PageSize: 20, TotalPages: 1);

        _advertRepository
            .SearchAdvertsPaginatedAsync(Arg.Any<SearchAdvertsRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new SearchAdvertsQuery(new SearchAdvertsRequestDto(null, null, null, null, null, null, null, null, null, null, null, null, null, null));
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items[0].Title.Should().Be("BMW X5");
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WithEmptyPage()
    {
        var pagedResult = new PagedResultDto<Domain.Entities.Advert>(
            [], TotalCount: 0, PageNumber: 1, PageSize: 20, TotalPages: 0);

        _advertRepository
            .SearchAdvertsPaginatedAsync(Arg.Any<SearchAdvertsRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new SearchAdvertsQuery(new SearchAdvertsRequestDto(null, null, null, null, null, null, null, null, null, null, null, null, null, null));
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ExposesPhoneNumber_WhenShowPhoneNumberTrue()
    {
        var advert = CreateTestAdvert(showPhoneNumber: true, phoneNumber: "05321234567");
        var pagedResult = new PagedResultDto<Domain.Entities.Advert>(
            [advert], TotalCount: 1, PageNumber: 1, PageSize: 20, TotalPages: 1);

        _advertRepository
            .SearchAdvertsPaginatedAsync(Arg.Any<SearchAdvertsRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new SearchAdvertsQuery(new SearchAdvertsRequestDto(null, null, null, null, null, null, null, null, null, null, null, null, null, null));
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Items[0].OwnerPhoneNumber.Should().Be("05321234567");
    }

    [Fact]
    public async Task Handle_HidesPhoneNumber_WhenShowPhoneNumberFalse()
    {
        var advert = CreateTestAdvert(showPhoneNumber: false, phoneNumber: "05321234567");
        var pagedResult = new PagedResultDto<Domain.Entities.Advert>(
            [advert], TotalCount: 1, PageNumber: 1, PageSize: 20, TotalPages: 1);

        _advertRepository
            .SearchAdvertsPaginatedAsync(Arg.Any<SearchAdvertsRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new SearchAdvertsQuery(new SearchAdvertsRequestDto(null, null, null, null, null, null, null, null, null, null, null, null, null, null));
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Items[0].OwnerPhoneNumber.Should().BeNull();
    }

    [Fact]
    public async Task Handle_PassesRequestDto_ToRepository_Unchanged()
    {
        var requestDto = new SearchAdvertsRequestDto("bmw", null, "İstanbul", AdvertStatus.Active, 100m, 500_000m, "BMW", null, 2015, 2023, 100_000, FuelType.Diesel, null, false, PageNumber: 2, PageSize: 10);
        var pagedResult = new PagedResultDto<Domain.Entities.Advert>(
            [], TotalCount: 0, PageNumber: 2, PageSize: 10, TotalPages: 0);

        _advertRepository
            .SearchAdvertsPaginatedAsync(Arg.Any<SearchAdvertsRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new SearchAdvertsQuery(requestDto);
        await _handler.Handle(query, CancellationToken.None);

        await _advertRepository
            .Received(1)
            .SearchAdvertsPaginatedAsync(requestDto, Arg.Any<CancellationToken>());
    }

    private static Domain.Entities.Advert CreateTestAdvert(
        string title = "Test İlan",
        bool showPhoneNumber = false,
        string? phoneNumber = null)
    {
        var ownerInfo = AdvertOwnerInfo.Create(Guid.NewGuid(), "Test Kullanıcı", "test@example.com");
        if (phoneNumber is not null)
            ownerInfo.UpdatePhone(phoneNumber);

        var advert = Domain.Entities.Advert.Create(
            title: title,
            description: "Test açıklama",
            price: Money.Create(100_000, CurrencyCode.TRY),
            location: Location.Create("İstanbul", "Kadıköy"),
            condition: AdvertCondition.Used,
            categoryId: Guid.NewGuid(),
            ownerUserId: Guid.NewGuid(),
            ownerInfo: ownerInfo,
            brand: "Toyota",
            model: "Corolla",
            year: 2020,
            mileage: 50_000,
            fuelType: FuelType.Gasoline,
            transmissionType: TransmissionType.Automatic,
            showPhoneNumber: showPhoneNumber);

        var category = Category.Create("Binek Araç", "binek-arac");
        typeof(Domain.Entities.Advert)
            .GetProperty(nameof(Domain.Entities.Advert.Category))!
            .SetValue(advert, category);

        return advert;
    }
}
