# Fix the Error - Configure Production Settings

The error occurs because your `appsettings.json` on the server is missing required configuration.

## Quick Fix Steps

### Step 1: Set Up Free PostgreSQL Database

You need an external PostgreSQL database. Here are free options:

#### Option A: ElephantSQL (Recommended - Easiest)
1. Go to [https://www.elephantsql.com/](https://www.elephantsql.com/)
2. Click "Get a managed PostgreSQL database"
3. Sign up (free tier available)
4. Create a new instance
5. Go to your instance details
6. Copy the **Connection String** (it looks like: `postgres://user:pass@host:port/dbname`)

**Convert ElephantSQL format to Npgsql format:**
- ElephantSQL gives: `postgres://user:pass@host:port/dbname`
- Convert to: `Host=host;Database=dbname;Username=user;Password=pass;Port=port`

#### Option B: Supabase
1. Go to [https://supabase.com/](https://supabase.com/)
2. Sign up and create a project
3. Go to Settings → Database
4. Copy the connection string

#### Option C: Neon
1. Go to [https://neon.tech/](https://neon.tech/)
2. Sign up and create a project
3. Copy the connection string

### Step 2: Generate JWT Secret Key

You need a secret key (at least 32 characters). Generate one:

**Option A: Use this online tool:**
- Go to [https://generate-secret.vercel.app/32](https://generate-secret.vercel.app/32)
- Copy the generated key

**Option B: Generate locally:**
```bash
openssl rand -base64 32
```

### Step 3: Update appsettings.json

Edit the `appsettings.json` file with your values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_HOST;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;Port=5432"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_GENERATED_SECRET_KEY_AT_LEAST_32_CHARACTERS",
    "Issuer": "http://xshop.somee.com",
    "Audience": "http://xshop.somee.com"
  },
  "Stripe": {
    "PublishableKey": "pk_test_YOUR_KEY",
    "SecretKey": "sk_test_YOUR_KEY"
  }
}
```

**Replace:**
- `YOUR_HOST` - Your database host
- `YOUR_DB` - Your database name
- `YOUR_USER` - Your database username
- `YOUR_PASSWORD` - Your database password
- `YOUR_GENERATED_SECRET_KEY_AT_LEAST_32_CHARACTERS` - Your JWT secret key
- Stripe keys (if you're using Stripe, otherwise you can leave placeholders)

### Step 4: Upload Updated appsettings.json

**Using FileZilla:**
1. Open FileZilla and connect to your server
2. Navigate to `/www.Xshop.somee.com`
3. Find `appsettings.json`
4. Right-click → View/Edit
5. Paste your updated configuration
6. Save and close
7. FileZilla will ask to upload - click Yes

**Or upload the file:**
1. Edit `appsettings.json` locally with your values
2. Upload it via FileZilla (replace the existing file)

### Step 5: Create Database Tables

After setting up the database, you need to create the users table:

1. Connect to your PostgreSQL database (using a tool like pgAdmin, DBeaver, or command line)
2. Run the SQL from `create_users_table.sql` in your project

Or use the database setup endpoint (if it works):
- Visit: `http://xshop.somee.com/api/database/setup` (POST request)

### Step 6: Test

1. Visit: `http://xshop.somee.com/api/database/health`
   - Should return: `{"status":"ok","result":1}`

2. Visit: `http://xshop.somee.com`
   - Should load without errors

## Example Complete appsettings.json

Here's an example with placeholder values you need to replace:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=hattie.db.elephantsql.com;Database=abc123;Username=abc123;Password=xyz789;Port=5432"
  },
  "JwtSettings": {
    "SecretKey": "MySuperSecretJWTKeyForXshopApp2024MustBe32Chars",
    "Issuer": "http://xshop.somee.com",
    "Audience": "http://xshop.somee.com"
  },
  "Stripe": {
    "PublishableKey": "pk_test_51...",
    "SecretKey": "sk_test_51..."
  }
}
```

## Troubleshooting

### Still getting errors?
1. Check Somee.com error logs in control panel
2. Verify connection string format is correct
3. Test database connection from your local machine first
4. Make sure database allows connections from Somee.com IPs

### Database connection issues?
- Some databases require you to allow specific IPs
- Check your database provider's firewall/network settings
- Somee.com's IP might need to be whitelisted

