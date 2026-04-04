using HayatiArac.Modules.Messaging.Application.Interfaces;
using HayatiArac.Modules.Messaging.Infrastructure.Persistence;
using HayatiArac.Modules.Messaging.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.Modules.Messaging.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMessagingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<MessagingDbContext>(options => options.UseSqlServer(connectionString,
            sqlServer => sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", MessagingDbContext.Schema)));
        services.AddScoped<IConversationRepository , ConversationRepository>();
        services.AddScoped<IMessageRepository , MessageRepository>();
        services.AddScoped<IMessagingUnitOfWork, MessagingUnitOfWork>();

        services.AddSignalR();
        return services;
    }
}
