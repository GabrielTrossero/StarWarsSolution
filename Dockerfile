# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY MoviesApp/*.csproj ./MoviesApp/
COPY MoviesApp.Tests/*.csproj ./MoviesApp.Tests/
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /app/MoviesApp
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "MoviesApp.dll"]
