# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

AuthManSys is a .NET 9 authentication management system built using Clean Architecture principles with the following layers:

- **Domain** (`AuthManSys.Domain`): Core business entities and domain logic
- **Application** (`AuthManSys.Application`): Business logic, uses MediatR for CQRS pattern, organized in feature modules
- **Infrastructure** (`AuthManSys.Infrastructure`): Data access, external services, database context with Entity Framework Core
- **API** (`AuthManSys.Api`): Web API controllers, middleware, dependency injection setup
- **Console** (`AuthManSys.Console`): CLI management tool with System.CommandLine for database operations and Google Docs integration

## Key Technologies & Patterns

- **Authentication**: ASP.NET Identity with JWT tokens and custom permission-based authorization
- **Database**: Dual support for MySQL (default) and SQL Server via Entity Framework Core
- **CQRS**: MediatR for command/query separation in Application layer
- **Permission System**: Custom role-based permission model with caching using MemoryCache
- **Console CLI**: System.CommandLine for interactive database management and Google API operations
- **Google Integration**: Full Google Docs and Drive API integration via service account authentication

## Project Structure

```
src/
├── AuthManSys.Api/          # Web API layer
├── AuthManSys.Application/  # Business logic with MediatR modules
│   ├── Common/              # Shared interfaces and models
│   └── Modules/             # Feature modules (direct repository access)
├── AuthManSys.Console/      # CLI management application
│   ├── Commands/            # CLI command implementations
│   ├── Services/            # Console-specific services
│   └── Storage/             # Sensitive files (gitignored)
├── AuthManSys.Domain/       # Core entities and domain models
└── AuthManSys.Infrastructure/ # Data access and external services
    ├── Database/            # EF Core context, migrations, seeders
    │   ├── Migrations/      # Database migration files
    │   └── Seeder/          # Modular database seeding system
    └── GoogleApi/           # Google Docs/Drive API services

tests/
├── AuthManSys.Api.Tests/
├── AuthManSys.Application.Tests/
├── AuthManSys.Domain.Tests/
└── AuthManSys.Integration.Tests/
```

## Essential Commands

### Database Operations

```bash
# Check database status and migrations
dotnet run --project src/AuthManSys.Console -- db status

# Apply database migrations
dotnet run --project src/AuthManSys.Console -- db migrate

# Seed database with initial data
dotnet run --project src/AuthManSys.Console -- db seed

# Reset database (WARNING: deletes all data)
dotnet run --project src/AuthManSys.Console -- db reset

# List all users
dotnet run --project src/AuthManSys.Console -- user list
```

### Google Docs Operations

```bash
# Create a new Google Document
dotnet run --project src/AuthManSys.Console -- google create "Document Title"

# Write content to existing document
dotnet run --project src/AuthManSys.Console -- google write "document-id" "Content"

# List Google Documents
dotnet run --project src/AuthManSys.Console -- google list

# Export document as text or PDF
dotnet run --project src/AuthManSys.Console -- google export "document-id" --format pdf
```

### Development & Testing

```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/AuthManSys.Api.Tests

# Run API project (requires database)
dotnet run --project src/AuthManSys.Api

# Start containerized environment
docker compose up --build -d

# View container logs
docker compose logs authman-api
```

## Configuration Management

### Database Configuration

The system supports dual database providers via `appsettings.json`:

```json
{
  "DatabaseProvider": "MySQL",  // or "SqlServer"
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3307;Database=AuthManSysDb;User=authuser;Password=P@ssw0rd123!;",
    "SqlServerConnection": "Server=localhost,1433;Database=AuthManSys;..."
  }
}
```

### Sensitive File Management

- **Storage folders**: `src/AuthManSys.Console/Storage/` and `src/AuthManSys.Api/Storage/` contain sensitive files (automatically gitignored)
- **Google API keys**: Service account JSON files go in Storage folders
- **Configuration files**: Never commit `appsettings.json` files with real credentials

### Google API Setup

1. Service account key files must be placed in `src/AuthManSys.Console/Storage/`
2. Update `GoogleApi.ServiceAccountKeyPath` in `appsettings.json` to point to Storage folder
3. Documents must be shared with service account email for write access
4. See `docs/GOOGLE_DOCS_INTEGRATION.md` for complete setup guide

## Permission System Architecture

The system implements a sophisticated permission-based authorization model:

- **Custom Entities**: `Permission`, `RolePermission` entities extend ASP.NET Identity
- **Permission Categories**: User Management, Role Management, System Administration
- **Caching Layer**: `PermissionCacheManager` with MemoryCache for performance
- **Authorization**: Custom `PermissionAuthorizationHandler` and `PermissionPolicyProvider`
- **Seeding**: Modular seeder system with `MasterSeeder` orchestrating all seeders

## Database Seeding System

The application uses a modular seeding architecture:

- **MasterSeeder**: Orchestrates all seeding operations in correct dependency order
- **Individual Seeders**: `UserSeeder`, `RoleSeeder`, `PermissionSeeder`, `RolePermissionSeeder`, `UserRoleSeeder`
- **Verified Emails**: All seeded users have `EmailConfirmed = true` for immediate login capability
- **Manual UserIds**: Uses `ValueGeneratedNever()` to avoid MySQL auto-increment conflicts

## Application Layer Refactoring

Recent architectural changes implemented direct repository access pattern:

- **Removed**: Thin wrapper services that only called repositories
- **Direct Access**: Application modules now access repositories directly via dependency injection
- **MediatR Integration**: Commands and queries use MediatR for cross-cutting concerns
- **Repository Pattern**: Maintained at Infrastructure layer with interfaces in Application.Common

## Development Notes

- **Working Directory**: Console commands run from solution root, configuration paths reflect this
- **Docker Environment**: Full containerized development setup with hot reload support
- **Migration Management**: Single consolidated migration replacing multiple legacy migrations
- **Test Coverage**: Four test projects covering all layers with integration tests
- **Documentation**: Comprehensive docs in `docs/` folder for permission system and Google integration

## Container Development

The `docker-compose.yml` provides a complete development environment:

- **MySQL 8.0**: Port 3307, persistent volume, health checks
- **API Container**: Hot reload enabled, development environment configured
- **Network**: Internal communication between containers
- **Console Operations**: Can be executed inside containers or from host

Use Docker for consistent development environment, especially for database-dependent operations and testing integration scenarios.