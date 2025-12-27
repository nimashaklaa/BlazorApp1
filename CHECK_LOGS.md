# Check Application Logs

The Event Viewer shows the app started, but we need to see runtime errors.

## Step 1: Check IIS Log Viewer

1. In Somee.com control panel, click **"IIS log viewer"** tab
2. Look for recent errors or failed requests
3. Check for any error messages

## Step 2: Check if Tables Exist in Neon

**This is critical!** The error is likely because tables don't exist.

1. Go to [Neon Console](https://console.neon.tech/)
2. Open your project → SQL Editor
3. Run this to check if tables exist:

```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public';
```

**If you don't see `users` in the results, create it:**

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

## Step 3: Test Database Connection from Neon

In Neon SQL Editor, run:
```sql
SELECT 1;
```

If this works, your database is accessible.

## Step 4: Check stdout Logs

Somee.com might have stdout logs. Check if there's a "Logs" section or look for files like:
- `logs/stdout`
- `logs/stderr`
- Any `.log` files in the File Manager

## Most Likely Issue

Since the app starts but fails on requests, the issue is almost certainly:
1. **Tables don't exist** - Create them in Neon (90% likely)
2. **Connection failing** - Check Neon connection settings

The `/api/database/health` endpoint just runs `SELECT 1`, so if it's failing, it's a connection issue or the connection string is wrong.

