FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src
COPY ["Shop.Users/Shop.Users.csproj", "Shop.Users/"]
COPY ["Shop.Shared/Shop.Shared.csproj", "Shop.Shared/"]
RUN dotnet restore "Shop.Users/Shop.Users.csproj"
COPY . .
WORKDIR "/src/Shop.Users"
RUN dotnet build "./Shop.Users.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Development
RUN dotnet publish "./Shop.Users.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shop.Users.dll"]
