# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln . 
COPY MoviesApp.API/*.csproj ./MoviesApp.API/
COPY MoviesApp.Application/*.csproj ./MoviesApp.Application/
COPY MoviesApp.Domain/*.csproj ./MoviesApp.Domain/
COPY MoviesApp.Infrastructure/*.csproj ./MoviesApp.Infrastructure/
COPY MoviesApp.Tests/*.csproj ./MoviesApp.Tests/
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /app/MoviesApp.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "MoviesApp.API.dll"]

