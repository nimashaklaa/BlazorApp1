# Deploying to Somee.com from Mac

This guide will help you deploy your Blazor application to somee.com using a Mac.

## Prerequisites

1. **Somee.com Account**: Sign up at [somee.com](https://somee.com) and get your FTP credentials
2. **FTP Client for Mac**: You can use:
   - **FileZilla** (Free, recommended): Download from [filezilla-project.org](https://filezilla-project.org/)
   - **Cyberduck** (Free): Download from [cyberduck.io](https://cyberduck.io/)
   - **Terminal FTP** (Built-in): Use `sftp` or `ftp` command
3. **.NET 8.0 SDK**: Already installed (you're using it)

## Step 1: Get Your Somee.com FTP Credentials

1. Log in to your somee.com account
2. Go to your hosting control panel
3. Find your **FTP credentials**:
   - FTP Host/Server (usually something like `ftp.somee.com` or your domain)
   - FTP Username
   - FTP Password
   - FTP Port (usually 21)

## Step 2: Prepare Your Application for Production

### 2.1 Update appsettings.json for Production

Your `appsettings.json` should have production-ready settings. Somee.com will use this file.

### 2.2 Create Production appsettings.json

You'll need to configure:
- Database connection string (Somee.com may provide PostgreSQL or you might need external hosting)
- JWT settings
- Stripe settings (if using)

**Note**: Somee.com free hosting may not include PostgreSQL. You might need:
- Use an external PostgreSQL service (like ElephantSQL, Supabase, or Neon)
- Or use SQL Server if Somee.com provides it (would require code changes)

## Step 3: Publish Your Application

Run the publish script from the project root:

```bash
./publish-for-somee.sh
```

Or manually:

```bash
# Navigate to project directory
cd "/Users/amandinimasha/self/demos for .net/BlazorApp1"

# Publish the application
dotnet publish -c Release -o ./publish
```

This creates a `publish` folder with all files needed for deployment.

## Step 4: Upload Files to Somee.com

### Option A: Using FileZilla (Recommended)

1. **Open FileZilla**
2. **Connect to FTP**:
   - Host: Your FTP host from Somee.com
   - Username: Your FTP username
   - Password: Your FTP password
   - Port: 21 (or as provided)
   - Click "Quickconnect"

3. **Navigate to your website root**:
   - Usually `/` or `/wwwroot` or `/httpdocs` (check Somee.com docs)
   - Somee.com might have a specific folder like `/www` or `/public_html`

4. **Upload files**:
   - On the left (Local site): Navigate to your `publish` folder
   - On the right (Remote site): Navigate to your website root
   - Select all files from `publish` folder
   - Drag and drop or right-click → Upload

### Option B: Using Terminal (SFTP)

```bash
# Navigate to publish folder
cd "/Users/amandinimasha/self/demos for .net/BlazorApp1/publish"

# Connect via SFTP
sftp your-username@ftp.somee.com

# Once connected:
cd /wwwroot  # or whatever your root directory is
put -r *     # Upload all files recursively
exit
```

### Option C: Using Cyberduck

1. Open Cyberduck
2. Click "Open Connection"
3. Select "FTP (File Transfer Protocol)"
4. Enter your FTP credentials
5. Connect
6. Navigate to your website root
7. Drag and drop files from your `publish` folder

## Step 5: Configure Environment Variables on Somee.com

Somee.com may allow you to set environment variables through their control panel. You'll need to set:

- `ConnectionStrings__DefaultConnection` - Your PostgreSQL connection string
- `JwtSettings__SecretKey` - Your JWT secret key
- `JwtSettings__Issuer` - Usually your domain
- `JwtSettings__Audience` - Usually your domain
- `Stripe__PublishableKey` - Your Stripe publishable key
- `Stripe__SecretKey` - Your Stripe secret key

**Alternative**: If Somee.com doesn't support environment variables, you can:
1. Create a `.env` file and upload it (if dotenv.net works)
2. Or modify `appsettings.json` directly (less secure, but works)

## Step 6: Verify Deployment

1. Visit your Somee.com website URL
2. Check if the application loads
3. Test your endpoints

## Important Notes for Somee.com

1. **Database**: Somee.com free hosting typically doesn't include PostgreSQL. You'll likely need:
   - External PostgreSQL hosting (ElephantSQL, Supabase, Neon, etc.)
   - Update your connection string to point to the external database

2. **.NET Runtime**: Somee.com should have .NET 8.0 runtime installed, but verify this in their documentation

3. **File Structure**: Make sure you upload:
   - All DLL files
   - `appsettings.json`
   - `wwwroot` folder (if it exists)
   - `BlazorApp1.dll` (your main application)
   - All dependency DLLs

4. **Startup**: Somee.com may require a specific startup configuration. Check their documentation for:
   - Web.config requirements
   - Application startup settings
   - Any specific folder structure

## Troubleshooting

### Application Not Starting
- Check Somee.com error logs in control panel
- Verify all DLL files are uploaded
- Check if .NET 8.0 runtime is available

### Database Connection Errors
- Verify your external PostgreSQL database is accessible
- Check connection string format
- Ensure database firewall allows Somee.com IPs

### 500 Internal Server Error
- Check Somee.com error logs
- Verify environment variables are set correctly
- Check file permissions on Somee.com

### Files Not Uploading
- Verify FTP credentials
- Check if you're in the correct directory
- Ensure you have write permissions

## Alternative: Use Somee.com Control Panel Upload

Somee.com might have a file manager in their control panel:
1. Log in to Somee.com control panel
2. Find "File Manager" or "Website Files"
3. Upload a ZIP file of your `publish` folder
4. Extract it in the website root

## Next Steps

After deployment:
1. Test all functionality
2. Set up your external PostgreSQL database
3. Configure domain (if using custom domain)
4. Set up SSL certificate (if needed)


