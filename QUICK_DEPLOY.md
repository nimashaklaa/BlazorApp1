# Quick Deploy Guide - Somee.com (Mac)

## 🚀 Quick Steps

### 1. Get FTP Credentials from Somee.com
- Log in to your Somee.com account
- Find FTP credentials in control panel:
  - FTP Host
  - FTP Username  
  - FTP Password

### 2. Publish Your App
```bash
./publish-for-somee.sh
```

### 3. Upload Files
**Using FileZilla (Recommended):**
1. Download FileZilla from [filezilla-project.org](https://filezilla-project.org/)
2. Connect using your FTP credentials
3. Navigate to website root (usually `/` or `/wwwroot`)
4. Upload ALL files from the `publish` folder

**Using Terminal:**
```bash
cd publish
sftp your-username@ftp.somee.com
put -r *
exit
```

### 4. Configure Settings
Update `appsettings.json` on Somee.com with:
- PostgreSQL connection string (use external service like ElephantSQL)
- JWT secret key
- Stripe keys (if using)

### 5. Test
Visit your Somee.com website URL

## ⚠️ Important Notes

1. **Database**: Somee.com free hosting doesn't include PostgreSQL. Use:
   - [ElephantSQL](https://www.elephantsql.com/) (Free tier available)
   - [Supabase](https://supabase.com/) (Free tier available)
   - [Neon](https://neon.tech/) (Free tier available)

2. **Connection String Format**:
   ```
   Host=your-host;Database=your-db;Username=your-user;Password=your-password
   ```

3. **File Upload**: Make sure to upload ALL files from the `publish` folder, including:
   - All `.dll` files
   - `web.config`
   - `appsettings.json`
   - `wwwroot` folder (if exists)

## 📚 Full Guide
See `DEPLOY_SOMEE.md` for detailed instructions.

