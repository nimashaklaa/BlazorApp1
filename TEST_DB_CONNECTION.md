# Test Database Connection

Your appsettings.json is correct! Now let's test the database connection.

## Step 1: Test Database in Neon Console

1. Go to [Neon Console](https://console.neon.tech/)
2. Open your project → SQL Editor
3. Run this to test connection:
   ```sql
   SELECT 1;
   ```
4. If this works, your database is accessible

## Step 2: Check if Tables Exist

Run this in Neon SQL Editor:
```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public';
```

If you don't see `users` in the results, you need to create it!

## Step 3: Create Tables (If Missing)

Run this SQL in Neon Console:

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

## Step 4: Test Connection String Format

Your connection string in appsettings.json is:
```
Host=ep-dry-dew-a1fsspq3-pooler.ap-southeast-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_O6V7XisANdGE;Port=5432;SslMode=Require
```

This looks correct! But if it still fails, try without the pooler:

```
Host=ep-dry-dew-a1fsspq3.ap-southeast-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_O6V7XisANdGE;Port=5432;SslMode=Require
```

(Remove `-pooler` from hostname)

## Step 5: Check Somee.com Error Logs

The error logs will show the exact error:
- "Table 'users' does not exist" → Create tables
- "Connection refused" → Connection issue
- "SSL connection required" → SSL mode issue
- "Password authentication failed" → Wrong password

## Most Likely Issue

Since appsettings.json is correct, the issue is probably:
1. **Tables don't exist** - Create them in Neon (90% likely)
2. **Connection timeout** - Somee.com might be blocking connections
3. **SSL issue** - Try the non-pooler endpoint

## Quick Test

After creating tables, test:
```bash
curl http://xshop.somee.com/api/database/health
```

Should return: `{"status":"ok","result":1}`


