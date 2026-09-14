# Etapa 1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/EmergencyCart.API/EmergencyCart.API.csproj", "EmergencyCart.API/"]
COPY ["src/EmergencyCart.Application/EmergencyCart.Application.csproj", "EmergencyCart.Application/"]
COPY ["src/EmergencyCart.Domain/EmergencyCart.Domain.csproj", "EmergencyCart.Domain/"]
COPY ["src/EmergencyCart.Infrastructure/EmergencyCart.Infrastructure.csproj", "EmergencyCart.Infrastructure/"]
RUN dotnet restore "EmergencyCart.API/EmergencyCart.API.csproj"

COPY src/ .
WORKDIR /src/EmergencyCart.API
RUN dotnet publish -c Release -o /app/publish

# Etapa 2
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EmergencyCart.API.dll"]