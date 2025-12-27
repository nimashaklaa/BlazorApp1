# Final Fix - Database Connection Error

The app is running but failing when trying to connect to the database. Here's how to fix it:

## Most Likely Issue: Connection String on Server

The `appsettings.json` on your server might still have the old configuration. 

## Step 1: Verify appsettings.json on Server

**In FileZilla:**
1. Connect to server
2. Navigate to `/www.Xshop.somee.com`
3. Right-click `appsettings.json` → View/Edit
4. **It MUST contain:**

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
    "DefaultConnection": "Host=ep-dry-dew-a1fsspq3-pooler.ap-southeast-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_O6V7XisANdGE;Port=5432;SslMode=Require"
  },
  "JwtSettings": {
    "SecretKey": "JtjjNzRT8ucJQzBrjKmBTBxOsLP45D0ybHA2pjzwclQ=",
    "Issuer": "http://xshop.somee.com",
    "Audience": "http://xshop.somee.com"
  },
  "Stripe": {
    "PublishableKey": "YOUR_STRIPE_PUBLISHABLE_KEY",
    "SecretKey": "YOUR_STRIPE_SECRET_KEY"
  }
}
```

**If it doesn't match, replace it completely!**

## Step 2: Check Somee.com Error Logs

1. Log in to Somee.com control panel
2. Find "Error Logs" or "Application Logs"
3. Look for errors like:
   - "Connection refused"
   - "SSL connection required"
   - "Connection string not found"
   - "Table does not exist"

## Step 3: Test Database Connection from Neon

1. Go to [Neon Console](https://console.neon.tech/)
2. Open SQL Editor
3. Run: `SELECT 1;`
4. If this works, your database is accessible

## Step 4: Alternative Connection String Format

If the connection still fails, try this format (without pooler):

```
Host=ep-dry-dew-a1fsspq3.ap-southeast-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_O6V7XisANdGE;Port=5432;SslMode=Require
```

(Remove `-pooler` from the hostname)

## Step 5: Create Tables in Neon

Even if connection works, you need tables:

1. Go to Neon Console → SQL Editor
2. Run:

```sql
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    hearts INTEGER NOT NULL DEFAULT 10,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
```

## Step 6: Restart Application

After updating appsettings.json:
1. Check Somee.com control panel for "Restart" option
2. Or wait 2-3 minutes for auto-restart

## Still Not Working?

The Somee.com error logs will show the exact error. That's the key to fixing this!

