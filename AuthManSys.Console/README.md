# AuthManSys Console Testing Tool

A command-line interface for testing the AuthManSys API endpoints.

## Usage

1. Start the API server:
   ```bash
   cd AuthManSys.Api
   dotnet run --urls="http://localhost:5000"
   ```

2. In a new terminal, run the console app:
   ```bash
   cd AuthManSys.Console
   dotnet run
   ```

## Available Commands

- `login <username> <password>` - Login with credentials
- `validate` - Validate current token
- `userinfo` - Get user information (requires auth)
- `test` - Test protected endpoint (requires auth)
- `weather` - Test weather endpoint (no auth required)
- `set-url <url>` - Set base URL (default: http://localhost:5000)
- `token` - Show current token
- `clear` - Clear current token
- `help` - Show command help
- `exit` - Exit application

## Quick Test Sequence

```
> login admin Admin123!
> validate
> userinfo
> test
> weather
```

## Features

- Interactive command-line interface
- Automatic token management
- Pretty-printed responses
- Error handling and status reporting
- Base URL configuration for different environments