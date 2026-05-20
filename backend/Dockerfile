# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (better layer caching for restore)
COPY ServisBinjaket.sln ./
COPY src/ServisBinjaket.Api/ServisBinjaket.Api.csproj                        src/ServisBinjaket.Api/
COPY src/ServisBinjaket.Application/ServisBinjaket.Application.csproj        src/ServisBinjaket.Application/
COPY src/ServisBinjaket.Domain/ServisBinjaket.Domain.csproj                  src/ServisBinjaket.Domain/
COPY src/ServisBinjaket.Infrastructure/ServisBinjaket.Infrastructure.csproj  src/ServisBinjaket.Infrastructure/
COPY tests/ServisBinjaket.Tests/ServisBinjaket.Tests.csproj                  tests/ServisBinjaket.Tests/

RUN dotnet restore

# Copy the rest and publish
COPY . .
RUN dotnet publish src/ServisBinjaket.Api/ServisBinjaket.Api.csproj \
    -c Release -o /app/publish

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/uploads

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

ENTRYPOINT ["dotnet", "ServisBinjaket.Api.dll"]
