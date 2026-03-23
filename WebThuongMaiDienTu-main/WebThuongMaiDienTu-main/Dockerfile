# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ShopMVC/ShopMVC.csproj", "ShopMVC/"]
RUN dotnet restore "ShopMVC/ShopMVC.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ShopMVC"
RUN dotnet build "ShopMVC.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ShopMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopMVC.dll"]