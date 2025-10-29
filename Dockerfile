# ======== BUILD STAGE ========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first
COPY AuthManSys.sln ./
COPY AuthManSys.Api/AuthManSys.Api.csproj AuthManSys.Api/
COPY AuthManSys.Application/AuthManSys.Application.csproj AuthManSys.Application/
COPY AuthManSys.Domain/AuthManSys.Domain.csproj AuthManSys.Domain/
COPY AuthManSys.Infrastructure/AuthManSys.Infrastructure.csproj AuthManSys.Infrastructure/
COPY AuthManSys.Tests/AuthManSys.Tests.csproj AuthManSys.Tests/

# Explicitly set NuGet source (prevents missing analyzers)
#RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# 🔧 Disable parallel restore (to avoid partial downloads)
RUN dotnet restore --disable-parallel --force

# Copy everything else
COPY . .

# ✅ Do NOT skip restore here (force clean restore/build link)
RUN dotnet build -c Release

# Publish the app
RUN dotnet publish AuthManSys.Api/AuthManSys.Api.csproj -c Release -o /app/publish

# ======== RUNTIME STAGE ========
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "AuthManSys.Api.dll"]
