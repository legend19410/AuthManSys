# AuthManSys - Authentication Management System

A comprehensive .NET 9 Web API project with JWT authentication, user management, and role-based authorization.

## Database Setup

This project supports both SQLite and SQL Server databases. You can switch between them using configuration.

### Option 1: SQLite (Default)
- **No installation required** - SQLite is embedded
- Database file is created automatically in the API project directory
- Set `"DatabaseProvider": "SQLite"` in appsettings.json

### Option 2: SQL Server with Docker
- Requires Docker to be installed on your system
- Run SQL Server in a Docker container
- Set `"DatabaseProvider": "SqlServer"` in appsettings.json

## Getting Started with SQL Server

1. **Start SQL Server Container:**
   ```bash
   # From the project root directory
   docker-compose up -d
   ```

2. **Change Database Provider:**
   Update `appsettings.json` or `appsettings.Development.json`:
   ```json
   {
     "DatabaseProvider": "SqlServer"
   }
   ```

3. **Create SQL Server Migration:**
   ```bash
   # From the API project directory
   cd AuthManSys.Api
   dotnet ef migrations add InitialCreate_SqlServer --project ../AuthManSys.Infrastructure
   ```

4. **Update Database:**
   ```bash
   dotnet ef database update --project ../AuthManSys.Infrastructure
   ```

## 🔐 Security Configuration

**IMPORTANT**: This project uses sensitive configuration that should NOT be committed to git.

### Initial Setup

1. **Copy Configuration Templates:**
   ```bash
   # Copy template files to create your local configuration
   cp AuthManSys.Api/appsettings.template.json AuthManSys.Api/appsettings.json
   cp AuthManSys.Api/appsettings.Development.template.json AuthManSys.Api/appsettings.Development.json
   ```

2. **Update Sensitive Values:**
   Edit the copied files and replace placeholders:
   - `YOUR_JWT_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG` - Generate a secure JWT secret
   - `YOUR_ADMIN_PASSWORD` - Set a strong admin password
   - `YOUR_SQL_SERVER_PASSWORD` - Update SQL Server password if using

3. **Environment Variables (Optional):**
   You can also use environment variables:
   ```bash
   export JwtSettings__SecretKey="YourSecureSecretKey"
   export DefaultCredentials__Password="YourAdminPassword"
   ```

### ⚠️ Security Best Practices

- **Never commit appsettings.json files** - They are in .gitignore
- Use strong, unique passwords (minimum 12 characters)
- Generate cryptographically secure JWT secret keys (minimum 32 characters)
- Rotate JWT secrets regularly in production
- Use environment variables or Azure Key Vault in production

## Configuration

### Connection Strings
- **SQLite**: `"Data Source=AuthManSysDb.sqlite"`
- **SQL Server**: `"Server=localhost,1433;Database=AuthManSysDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true"`

### Database Provider Setting
Set in appsettings.json:
```json
{
  "DatabaseProvider": "SQLite"  // or "SqlServer"
}
```

## Docker Compose Services

The `docker-compose.yml` file includes:
- **SQL Server 2022 Express** container
- **Port**: 1433 (mapped to host)
- **SA Password**: `YourStrong!Passw0rd`
- **Persistent Volume**: `sqlserver_data`

## Commands

### Start SQL Server Container
```bash
docker-compose up -d
```

### Stop SQL Server Container
```bash
docker-compose down
```

### View Container Logs
```bash
docker-compose logs sqlserver
```

### Remove Container and Data
```bash
docker-compose down -v
```

## Development

- The project automatically switches database providers based on the `DatabaseProvider` setting
- Both SQLite and SQL Server use the same Entity Framework models and migrations
- SQL-specific syntax is handled automatically (e.g., `GETUTCDATE()` vs `datetime('now')`)