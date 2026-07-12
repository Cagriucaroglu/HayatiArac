FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for layer caching — restore only runs when csproj files change
COPY HayatiArac.sln .
COPY HayatiArac.Api/HayatiArac.Api.csproj HayatiArac.Api/
COPY HayatiArac.SharedKernel/HayatiArac.SharedKernel.csproj HayatiArac.SharedKernel/
COPY HayatiArac.Modules.Advert.Application/HayatiArac.Modules.Advert.Application.csproj HayatiArac.Modules.Advert.Application/
COPY HayatiArac.Modules.Advert.Domain/HayatiArac.Modules.Advert.Domain.csproj HayatiArac.Modules.Advert.Domain/
COPY HayatiArac.Modules.Advert.Infrastructure/HayatiArac.Modules.Advert.Infrastructure.csproj HayatiArac.Modules.Advert.Infrastructure/
COPY HayatiArac.Modules.Advert.IntegrationEvents/HayatiArac.Modules.Advert.IntegrationEvents.csproj HayatiArac.Modules.Advert.IntegrationEvents/
COPY HayatiArac.Modules.Favorite.Application/HayatiArac.Modules.Favorite.Application.csproj HayatiArac.Modules.Favorite.Application/
COPY HayatiArac.Modules.Favorite.Domain/HayatiArac.Modules.Favorite.Domain.csproj HayatiArac.Modules.Favorite.Domain/
COPY HayatiArac.Modules.Favorite.Infrastructure/HayatiArac.Modules.Favorite.Infrastructure.csproj HayatiArac.Modules.Favorite.Infrastructure/
COPY HayatiArac.Modules.Messaging.Application/HayatiArac.Modules.Messaging.Application.csproj HayatiArac.Modules.Messaging.Application/
COPY HayatiArac.Modules.Messaging.Domain/HayatiArac.Modules.Messaging.Domain.csproj HayatiArac.Modules.Messaging.Domain/
COPY HayatiArac.Modules.Messaging.Infrastructure/HayatiArac.Modules.Messaging.Infrastructure.csproj HayatiArac.Modules.Messaging.Infrastructure/
COPY HayatiArac.Modules.Notification.Application/HayatiArac.Modules.Notification.Application.csproj HayatiArac.Modules.Notification.Application/
COPY HayatiArac.Modules.Notification.Domain/HayatiArac.Modules.Notification.Domain.csproj HayatiArac.Modules.Notification.Domain/
COPY HayatiArac.Modules.Notification.Infrastructure/HayatiArac.Modules.Notification.Infrastructure.csproj HayatiArac.Modules.Notification.Infrastructure/
COPY HayatiArac.Modules.User.Application/HayatiArac.Modules.User.Application.csproj HayatiArac.Modules.User.Application/
COPY HayatiArac.Modules.User.Domain/HayatiArac.Modules.User.Domain.csproj HayatiArac.Modules.User.Domain/
COPY HayatiArac.Modules.User.Infrastructure/HayatiArac.Modules.User.Infrastructure.csproj HayatiArac.Modules.User.Infrastructure/
COPY HayatiArac.Modules.User.IntegrationEvents/HayatiArac.Modules.User.IntegrationEvents.csproj HayatiArac.Modules.User.IntegrationEvents/

RUN dotnet restore HayatiArac.Api/HayatiArac.Api.csproj

COPY . .

RUN dotnet publish HayatiArac.Api/HayatiArac.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN mkdir -p logs

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "HayatiArac.Api.dll"]
