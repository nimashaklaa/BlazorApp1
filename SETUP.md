# BlazorApp1 Setup Guide

## Prerequisites
- .NET 8.0 SDK installed
- PostgreSQL installed and running
- Terminal/Command Line access

## Setup Instructions

### 1. Configure Environment Variables

Copy the example environment file and configure your settings:
```bash
cp .env.example .env
```

Then edit `.env` and update with your PostgreSQL credentials:
```env
ConnectionStrings__DefaultConnection=Host=localhost;Database=blazorapp1;Username=postgres;Password=your_actual_password
JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong
```

**Important:** Never commit the `.env` file to version control. It's already in `.gitignore`.

### 2. Create the Database

Open PostgreSQL and create the database:
```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE blazorapp1;

# Exit psql
\q
```

### 3. Create the Users Table

Run the SQL script to create the users table:
```bash
psql -U your_username -d blazorapp1 -f create_users_table.sql
```

Or manually execute the SQL:
```bash
psql -U your_username -d blazorapp1
```
Then paste the contents of `create_users_table.sql`.

### 4. Restore Dependencies

```bash
dotnet restore
```

### 5. Build the Project

```bash
dotnet build
```

### 6. Run the Application

```bash
dotnet run
```

The application will start and display URLs like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

## Testing the API Endpoints

### Test Signup
```bash
curl -X POST https://localhost:5001/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "password123"
  }'
```

### Test Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123"
  }'
```

### Test Database Connection
```bash
curl https://localhost:5001/api/database/health
```

## Project Structure

```
BlazorApp1/
├── Controllers/
│   ├── AuthController.cs       # Signup/Login endpoints
│   └── DatabaseController.cs   # Database health check
├── Models/
│   ├── SignupRequest.cs
│   ├── LoginRequest.cs
│   └── AuthResponse.cs
├── appsettings.json
├── appsettings.Development.json
└── create_users_table.sql      # Database schema
```

## API Endpoints

- `POST /api/auth/signup` - Create new user account
- `POST /api/auth/login` - Authenticate user
- `GET /api/database/health` - Check database connection

## Troubleshooting

### Database Connection Error
- Verify PostgreSQL is running: `pg_isready`
- Check connection string credentials
- Ensure database exists: `psql -l`

### Port Already in Use
- Change ports in `Properties/launchSettings.json`
- Or kill process using the port

### SSL Certificate Error
- Use `http://localhost:5000` instead of `https://localhost:5001`
- Or trust the development certificate: `dotnet dev-certs https --trust`
