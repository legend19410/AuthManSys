# AuthManSys - Authentication Management System

A comprehensive .NET 9 Web API project with JWT authentication, user management, and role-based authorization.

## Database Setup

This project supports both SQLite and SQL Server databases. You can switch between them using configuration.

### Option 1: SQLite (Default)
- **No installation required** - SQLite is embedded
- Database file is created automatically in the API project directory
- Set `"DatabaseProvider": "SQLite"` in appsettings.json

### Option 2: MySQL with Docker
- Requires Docker to be installed on your system
- Run MySQL in a Docker container
- Set `"DatabaseProvider": "MySQL"` in appsettings.json

## Getting Started with MySQL

1. **Start MySQL Container:**
   ```bash
   # From the project root directory
   docker compose up --build -d
   ```

2. **Change Database Provider:**
   Update `appsettings.json` or `appsettings.Development.json`:
   ```json
   {
     "DatabaseProvider": "MySQL"
   }
   ```

3. **Create MySQL Migration:**
   ```bash
   # From the API project directory
   cd AuthManSys.Api
   dotnet ef migrations add InitialCreate_MySQL --project ../AuthManSys.Infrastructure
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
- **MySQL**: `"Server=localhost;Port=3307;Database=AuthManSysDb;User=authuser;Password=P@ssw0rd123!;"`

### Database Provider Setting
Set in appsettings.json:
```json
{
  "DatabaseProvider": "SQLite"  // or "MySQL"
}
```

## Docker Compose Services

The `docker-compose.yml` file includes:
- **MySQL 8.0** container
- **Port**: 3307 (mapped to host port 3307, container port 3306)
- **Database**: AuthManSysDb
- **User**: authuser
- **Password**: P@ssw0rd123!
- **Persistent Volume**: `mysql_data`
- **AuthManSys API** container
- **Port**: 8081 (mapped to host port 8081, container port 8080)

## Commands

### Start All Containers
```bash
docker compose up --build -d
```

### Stop All Containers
```bash
docker compose down
```

### View Container Logs
```bash
# View API logs
docker compose logs authman-api

# View MySQL logs
docker compose logs mysql
```

### Connect to MySQL Database from Terminal

You can connect to the MySQL database running inside the Docker container in several ways:

#### Option 1: Using Docker Exec (Recommended)
```bash
# Connect to MySQL container and open MySQL shell
docker exec -it authmansys_mysql mysql -u authuser -p

# When prompted, enter password: P@ssw0rd123!
# Then you can run SQL commands like:
# USE AuthManSysDb;
# SHOW TABLES;
# SELECT * FROM Users;
```

#### Option 2: Using MySQL Client from Host
If you have MySQL client installed on your host machine:
```bash
mysql -h localhost -P 3307 -u authuser -p
# Enter password when prompted: P@ssw0rd123!
```

#### Option 3: Using Docker Run (One-time connection)
```bash
docker run -it --rm --network authmansys_default mysql:8.0 mysql -h mysql -u authuser -p
# Enter password when prompted: P@ssw0rd123!
```

### Database Connection Details
- **Host**: localhost (when connecting from host machine)
- **Port**: 3307 (host machine port)
- **Database**: AuthManSysDb
- **Username**: authuser
- **Password**: P@ssw0rd123!

### Remove Container and Data
```bash
docker compose down -v
```

## Database Management with Console App

You can manage the database in the Docker container using the console app from your terminal:

### Database Seeding Commands

#### Check Database Status
```bash
# From project root directory
cd AuthManSys.Console
dotnet run -- db status
```

#### Run Database Migrations
```bash
# Apply any pending migrations to the database
cd AuthManSys.Console
dotnet run -- db migrate
```

#### Seed Database with Initial Data
```bash
# Add default roles, admin user, and sample data
cd AuthManSys.Console
dotnet run -- db seed
```

#### Reset and Reseed Database
```bash
# ⚠️ WARNING: This deletes ALL data and reseeds
cd AuthManSys.Console
dotnet run -- db reset
```

### Running Console Commands from Docker Container

You can also run the console app commands directly in the Docker environment:

#### Option 1: Using Docker Exec (Recommended)
```bash
# Execute console commands inside the running API container
docker exec -it authmansys_api dotnet run --project AuthManSys.Console -- db status
docker exec -it authmansys_api dotnet run --project AuthManSys.Console -- db seed
docker exec -it authmansys_api dotnet run --project AuthManSys.Console -- db reset
```

#### Option 2: Temporary Container with Same Network
```bash
# Run console commands using temporary container with same network
docker run --rm -it \
  --network authmansys_default \
  -v $(pwd):/src \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  dotnet run --project AuthManSys.Console -- db status
```

### Environment Variables for Docker Commands

If running console commands in Docker, you may need to set environment variables:
```bash
# Set MySQL connection for Docker environment
docker exec -it authmansys_api \
  env ConnectionStrings__MySqlConnection="Server=mysql;Port=3306;Database=AuthManSysDb;User=authuser;Password=P@ssw0rd123!;" \
  dotnet run --project AuthManSys.Console -- db seed
```

### Available Console Commands

| Command | Description |
|---------|-------------|
| `db status` | Check database connection and migration status |
| `db migrate` | Apply pending database migrations |
| `db seed` | Seed database with initial data (roles, admin user) |
| `db reset` | **⚠️ Delete all data and reseed database** |
| `user list` | List all users in the system |
| `auth login` | Test authentication functionality |
| `menu` | Start interactive console menu |

### Quick Reference - Common Commands

```bash
# Check if containers are running
docker compose ps

# Start containers
docker compose up --build -d

# Check database status
dotnet run -- db status

# Seed database with initial data
dotnet run -- db seed

# Reset database (delete all data and reseed)
dotnet run -- db reset

# Connect to MySQL shell
docker exec -it authmansys_mysql mysql -u authuser -p
# Password: P@ssw0rd123!

# View API logs
docker compose logs authman-api

# Stop containers
docker compose down
```

## Development

- The project automatically switches database providers based on the `DatabaseProvider` setting
- Both SQLite and MySQL use the same Entity Framework models and migrations
- Database-specific syntax is handled automatically (e.g., `GETUTCDATE()` vs `datetime('now')`)
- **UserIds**: The `UserId` column in `AspNetUsers` table is configured for auto-increment starting from 1