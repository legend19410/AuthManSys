# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY AuthManSys.sln ./

# Copy project files
COPY AuthManSys.Api/AuthManSys.Api.csproj AuthManSys.Api/
COPY AuthManSys.Application/AuthManSys.Application.csproj AuthManSys.Application/
COPY AuthManSys.Domain/AuthManSys.Domain.csproj AuthManSys.Domain/
COPY AuthManSys.Infrastructure/AuthManSys.Infrastructure.csproj AuthManSys.Infrastructure/
COPY AuthManSys.Tests/AuthManSys.Tests.csproj AuthManSys.Tests/
COPY AuthManSys.Console/AuthManSys.Console.csproj AuthManSys.Console/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish AuthManSys.Api/AuthManSys.Api.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 9 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose port 80
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "AuthManSys.Api.dll"]