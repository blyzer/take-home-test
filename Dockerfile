# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["backend/src/Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj", "backend/src/Fundo.Applications.WebApi/"]
RUN dotnet restore "backend/src/Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj"
COPY . .
WORKDIR "/src/backend/src/Fundo.Applications.WebApi"
RUN dotnet build "Fundo.Applications.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Fundo.Applications.WebApi.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fundo.Applications.WebApi.dll"]