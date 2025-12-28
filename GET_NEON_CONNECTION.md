# How to Get Your Neon Database Connection String

## Steps to Get Neon Connection String:

1. **Log in to Neon**
   - Go to [https://console.neon.tech/](https://console.neon.tech/)
   - Sign in to your account

2. **Select Your Project**
   - Click on your project (or create one if you don't have it)

3. **Get Connection String**
   - In your project dashboard, look for "Connection Details" or "Connection String"
   - You'll see something like:
     ```
     postgres://username:password@ep-xxx-xxx.us-east-2.aws.neon.tech/dbname?sslmode=require
     ```

4. **Convert to Npgsql Format**
   Neon gives you a connection string like:
   ```
   postgres://user:password@host/dbname?sslmode=require
   ```
   
   Convert it to:
   ```
   Host=host;Database=dbname;Username=user;Password=password;Port=5432;SslMode=Require
   ```

## Example:
If Neon gives you:
```
postgres://myuser:mypass@ep-cool-name-123456.us-east-2.aws.neon.tech/neondb?sslmode=require
```

Convert to:
```
Host=ep-cool-name-123456.us-east-2.aws.neon.tech;Database=neondb;Username=myuser;Password=mypass;Port=5432;SslMode=Require
```

## Quick Copy:
Once you have your Neon connection string, share it with me and I'll update the appsettings.json file for you!


