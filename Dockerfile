# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj y restaurar como capas separadas
COPY *.sln .
COPY MoviesApp.API/*.csproj ./MoviesApp.API/
COPY MoviesApp.Application/*.csproj ./MoviesApp.Application/
COPY MoviesApp.Domain/*.csproj ./MoviesApp.Domain/
COPY MoviesApp.Infrastructure/*.csproj ./MoviesApp.Infrastructure/
COPY MoviesApp.Tests/*.csproj ./MoviesApp.Tests/
RUN dotnet restore

# Copiar el resto y compilar
COPY . .
WORKDIR /src/MoviesApp.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render define $PORT, pero EXPOSE es solo documentación
EXPOSE 8080

ENTRYPOINT ["dotnet", "MoviesApp.API.dll"]


