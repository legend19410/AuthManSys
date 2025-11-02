# =========================================
# ======== BUILD STAGE ====================
# =========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (for caching)
COPY AuthManSys.sln ./
COPY AuthManSys.Api/AuthManSys.Api.csproj AuthManSys.Api/
COPY AuthManSys.Application/AuthManSys.Application.csproj AuthManSys.Application/
COPY AuthManSys.Domain/AuthManSys.Domain.csproj AuthManSys.Domain/
COPY AuthManSys.Infrastructure/AuthManSys.Infrastructure.csproj AuthManSys.Infrastructure/
COPY AuthManSys.Tests/AuthManSys.Tests.csproj AuthManSys.Tests/

# Install dependencies & VS Debugger
RUN apt-get update && apt-get install -y unzip curl procps \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /root/.vs-debugger

# Restore NuGet packages
RUN dotnet restore --disable-parallel --force

# Copy everything else
COPY . .

# Build and publish
RUN dotnet build -c Release
RUN dotnet publish AuthManSys.Api/AuthManSys.Api.csproj -c Release -o /app/publish

# =========================================
# ======== RUNTIME STAGE ==================
# =========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "AuthManSys.Api.dll"]

# =========================================
# ======== DEBUG STAGE ====================
# =========================================
FROM build AS debug
WORKDIR /src

# Copy VS debugger to debug stage
COPY --from=build /root/.vs-debugger /root/.vs-debugger

# Add debugger to PATH
ENV PATH="/root/.vs-debugger:${PATH}"

# Environment for debugging
ENV ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=1 \
    ASPNETCORE_URLS=http://0.0.0.0:8080 \
    VSDBG_ATTACH_TIMEOUT=60

# Expose port for debugger and hot reload
EXPOSE 8080

# Mount source for hot reload
VOLUME ["/src"]

# Start app with watch for hot reload
ENTRYPOINT ["dotnet", "watch", "run", "--project", "AuthManSys.Api/AuthManSys.Api.csproj", "--urls=http://0.0.0.0:8080"]
