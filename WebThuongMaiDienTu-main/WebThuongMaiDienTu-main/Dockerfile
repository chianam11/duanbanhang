FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ShopMVC/ShopMVC.csproj", "ShopMVC/"]
RUN dotnet restore "ShopMVC/ShopMVC.csproj"

COPY . .
WORKDIR /src/ShopMVC
RUN dotnet publish "ShopMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShopMVC.dll"]
