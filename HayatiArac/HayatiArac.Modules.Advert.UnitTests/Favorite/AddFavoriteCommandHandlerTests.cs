using FluentAssertions;
using HayatiArac.Modules.Favorite.Application.Commands.AddFavorite;
using HayatiArac.Modules.Favorite.Application.Interfaces;
using HayatiArac.Modules.Favorite.Domain.Entities;
using HayatiArac.SharedKernel.Application;
using HayatiArac.SharedKernel.Application.Interfaces;
using NSubstitute;

namespace HayatiArac.Modules.Advert.UnitTests.Favorite;

public class AddFavoriteCommandHandlerTests
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IAdvertSnapshotRepository _snapshotRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFavoriteUnitOfWork _unitOfWork;
    private readonly AddFavoriteCommandHandler _handler;

    public AddFavoriteCommandHandlerTests()
    {
        _favoriteRepository = Substitute.For<IFavoriteRepository>();
        _snapshotRepository = Substitute.For<IAdvertSnapshotRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IFavoriteUnitOfWork>();

        _handler = new AddFavoriteCommandHandler(
            _favoriteRepository,
            _snapshotRepository,
            _currentUserService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsUnauthorized_WhenNoCurrentUser()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        var result = await _handler.Handle(new AddFavoriteCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenAdvertSnapshotMissing()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _snapshotRepository
            .GetByAdvertIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AdvertSnapshot?)null);

        var result = await _handler.Handle(new AddFavoriteCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenFavoriteAlreadyExists()
    {
        var userId = Guid.NewGuid();
        var advertId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _snapshotRepository
            .GetByAdvertIdAsync(advertId, Arg.Any<CancellationToken>())
            .Returns(CreateSnapshot(advertId));
        _favoriteRepository
            .GetAsync(userId, advertId, Arg.Any<CancellationToken>())
            .Returns(SavedAdvert.Create(userId, advertId));

        var result = await _handler.Handle(new AddFavoriteCommand(advertId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenValidRequest()
    {
        var userId = Guid.NewGuid();
        var advertId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _snapshotRepository
            .GetByAdvertIdAsync(advertId, Arg.Any<CancellationToken>())
            .Returns(CreateSnapshot(advertId));
        _favoriteRepository
            .GetAsync(userId, advertId, Arg.Any<CancellationToken>())
            .Returns((SavedAdvert?)null);

        var result = await _handler.Handle(new AddFavoriteCommand(advertId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SavesAndCommits_WhenFavoriteCreated()
    {
        var userId = Guid.NewGuid();
        var advertId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _snapshotRepository
            .GetByAdvertIdAsync(advertId, Arg.Any<CancellationToken>())
            .Returns(CreateSnapshot(advertId));
        _favoriteRepository
            .GetAsync(userId, advertId, Arg.Any<CancellationToken>())
            .Returns((SavedAdvert?)null);

        await _handler.Handle(new AddFavoriteCommand(advertId), CancellationToken.None);

        await _favoriteRepository.Received(1).AddAsync(Arg.Any<SavedAdvert>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static AdvertSnapshot CreateSnapshot(Guid advertId) =>
        AdvertSnapshot.Create(advertId, "Test İlan", "Toyota", "Corolla", 2020, 50_000, 150_000m, "TRY", "İstanbul", "Active", []);
}
