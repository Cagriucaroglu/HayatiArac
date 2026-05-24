using FluentAssertions;
using HayatiArac.IntegrationTests.Infrastructure;
using HayatiArac.Modules.Favorite.Domain.Entities;
using HayatiArac.Modules.Favorite.Infrastructure.Persistence;
using HayatiArac.Modules.Favorite.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace HayatiArac.IntegrationTests.Favorite;

public class FavoriteRepositoryTests : IClassFixture<DatabaseFixture> , IAsyncLifetime
{
    private readonly FavoriteDbContext  favoriteDbContext;
    private readonly FavoriteRepository repository;

    public FavoriteRepositoryTests(DatabaseFixture fixture)
    {
        favoriteDbContext = fixture.FavoriteContext;
        repository = new FavoriteRepository(favoriteDbContext);
    }

    public async Task InitializeAsync()
    {
        favoriteDbContext.ChangeTracker.Clear();
        await favoriteDbContext.Database.ExecuteSqlRawAsync(
            $"DELETE FROM [{FavoriteDbContext.Schema}].[SavedAdverts];");

    }

    public Task DisposeAsync() => Task.CompletedTask;
    
    [Fact]
    public async Task GetAsync_ReturnsNull_WhenNotFavorited()
    {
        SavedAdvert? result = await repository.GetAsync(Guid.NewGuid(), Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ThenGetAsync_ReturnsSavedAdvert()
    {
        Guid userId = Guid.NewGuid();
        Guid advertId = Guid.NewGuid();

        await repository.AddAsync(SavedAdvert.Create(userId, advertId));
        
        await favoriteDbContext.SaveChangesAsync();

        favoriteDbContext.ChangeTracker.Clear();

        SavedAdvert? result = await repository.GetAsync(userId, advertId);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result!.AdvertId.Should().Be(advertId);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsAllFavoritesForUser()
    {
        Guid userId = Guid.NewGuid();

        await repository.AddAsync(SavedAdvert.Create(userId, Guid.NewGuid()));
        await repository.AddAsync(SavedAdvert.Create(userId, Guid.NewGuid()));
        await repository.AddAsync(SavedAdvert.Create(Guid.NewGuid(), Guid.NewGuid()));
        
        await favoriteDbContext.SaveChangesAsync();
        favoriteDbContext.ChangeTracker.Clear();

        List<SavedAdvert> result = await repository.GetByUserAsync(userId);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(x => x.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task RemoveAsync_RemovesFavorite()
    {
        Guid userId = Guid.NewGuid();
        Guid advertId = Guid.NewGuid();

        await repository.AddAsync(SavedAdvert.Create(userId, advertId));
        
        await favoriteDbContext.SaveChangesAsync();
        favoriteDbContext.ChangeTracker.Clear();

        SavedAdvert? toRemove = await repository.GetAsync(userId, advertId);
        await repository.RemoveAsync(toRemove);
        await favoriteDbContext.SaveChangesAsync();
        favoriteDbContext.ChangeTracker.Clear();

        var result = await repository.GetAsync(userId, advertId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_SameAdvertTwice_ThrowsDuplicateKey()
    {
        Guid userId = Guid.NewGuid();
        Guid advertId = Guid.NewGuid();

        await repository.AddAsync(SavedAdvert.Create(userId , advertId));
        await favoriteDbContext.SaveChangesAsync();
        favoriteDbContext.ChangeTracker.Clear(); // Neden ChangeTracker.Clear() gerekiyor? Aynı Id'ye sahip entity'yi DB'den tekrar çekmeye çalıştığında EF Core önce ChangeTracker'a bakar. Orada varsa DB'ye hiç gitmez, cache'den döndürür. Bu testlerde yanlış sonuç verir

                await repository.AddAsync(SavedAdvert.Create(userId, advertId));
        var act = async () => await favoriteDbContext.SaveChangesAsync();

        await act.Should().ThrowAsync<Exception>();
    }

}
