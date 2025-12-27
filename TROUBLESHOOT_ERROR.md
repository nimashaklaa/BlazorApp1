# Troubleshooting the Error

The error is still occurring. Let's diagnose step by step.

## Step 1: Verify appsettings.json was uploaded correctly

**Check via FileZilla:**
1. Connect to your server
2. Navigate to `/www.Xshop.somee.com`
3. Right-click `appsettings.json` → View/Edit
4. Verify it contains:
   - Your Neon connection string
   - JWT SecretKey (should be `JtjjNzRT8ucJQzBrjKmBTBxOsLP45D0ybHA2pjzwclQ=`)
   - Issuer and Audience set to `http://xshop.somee.com`

**If it's not correct:**
- Copy the correct content from your local `appsettings.json`
- Paste and save in FileZilla

## Step 2: Create Database Tables in Neon

The database tables need to exist. Create them in Neon:

1. **Go to Neon Console:**
   - Visit [https://console.neon.tech/](https://console.neon.tech/)
   - Open your project

2. **Open SQL Editor:**
   - Click on "SQL Editor" in the left menu
   - Or go to your database → SQL Editor

3. **Run this SQL:**
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

4. **Click "Run"** to execute the SQL

## Step 3: Test Database Connection

After creating tables, test if the connection works:

**Option A: Use the setup endpoint (if app starts):**
- Try: `http://xshop.somee.com/api/database/setup` (POST request)
- Use a tool like Postman or curl

**Option B: Test connection directly:**
- The `/api/database/health` endpoint should work if connection is good

## Step 4: Check Somee.com Error Logs

1. Log in to your Somee.com control panel
2. Look for "Error Logs" or "Application Logs"
3. Check for specific error messages
4. Common errors:
   - "Connection string not found" → appsettings.json not uploaded
   - "Connection refused" → Database connection issue
   - "Table does not exist" → Need to create tables
   - "SSL connection required" → SSL mode issue

## Step 5: Verify Connection String Format

Your connection string should be:
```
Host=ep-dry-dew-a1fsspq3-pooler.ap-southeast-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_O6V7XisANdGE;Port=5432;SslMode=Require
```

**Common issues:**
- Missing `SslMode=Require` (Neon requires SSL)
- Wrong password (check for special characters)
- Wrong host (make sure it's the pooler endpoint)

## Step 6: Restart the Application

After uploading `appsettings.json`, Somee.com might need to restart the app:
1. Check Somee.com control panel for "Restart" or "Restart Application" option
2. Or wait a few minutes for auto-restart

## Quick Test Commands

Once the app is working, test these endpoints:

```bash
# Test database connection
curl http://xshop.somee.com/api/database/health

# Setup database tables (if needed)
curl -X POST http://xshop.somee.com/api/database/setup
```

## Still Not Working?

If the error persists:
1. Check Somee.com error logs for the exact error message
2. Verify the connection string works from your local machine
3. Make sure Neon database allows connections from Somee.com IPs (usually enabled by default)

