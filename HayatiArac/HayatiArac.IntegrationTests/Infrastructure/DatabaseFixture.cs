using HayatiArac.Modules.Advert.Infrastructure.Persistence;
using HayatiArac.Modules.Favorite.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HayatiArac.IntegrationTests.Infrastructure;

/// <summary>
/// Tüm integration testleri için tek bir SQL Server container başlatır.
/// xUnit IClassFixture ile test sınıfları arasında paylaşılır.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString { get; private set; } = null!;
    public AdvertDbContext AdvertContext { get; private set; } = null!;
    public FavoriteDbContext FavoriteContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();
        ConnectionString = connectionString;

        AdvertContext = new AdvertDbContext(
            new DbContextOptionsBuilder<AdvertDbContext>()
                .UseSqlServer(connectionString, sql =>
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", AdvertDbContext.Schema))
                .Options);
        FavoriteContext = new FavoriteDbContext(
            new DbContextOptionsBuilder<FavoriteDbContext>()
                .UseSqlServer(connectionString, sql =>
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", FavoriteDbContext.Schema))
                .Options);

        await AdvertContext.Database.MigrateAsync();
        await FavoriteContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await AdvertContext.DisposeAsync();
        await FavoriteContext.DisposeAsync();
        await _container.DisposeAsync();
    }
}
