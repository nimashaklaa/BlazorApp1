# Your Somee.com Deployment Information

## Your Website Details
- **Website URL**: http://www.Xshop.somee.com or http://Xshop.somee.com
- **FTP Host**: Xshop.somee.com (or IP: 155.254.246.29)
- **FTP Username**: amandi99
- **FTP Path**: /www.Xshop.somee.com
- **FTP Port**: 21 (default)

## Quick Deployment Steps

### 1. Publish Your Application
```bash
./publish-for-somee.sh
```

### 2. Upload Using FileZilla

**Connection Settings:**
- **Host**: `Xshop.somee.com` (or `155.254.246.29`)
- **Username**: `amandi99`
- **Password**: [Your FTP password from Somee.com]
- **Port**: `21`
- **Protocol**: FTP

**Upload Path:**
- Navigate to: `/www.Xshop.somee.com` on the remote server
- Upload ALL files from your local `publish` folder

### 3. Upload Using Terminal (SFTP)

```bash
cd publish
sftp amandi99@Xshop.somee.com
# Enter your FTP password when prompted
cd /www.Xshop.somee.com
put -r *
exit
```

### 4. Test Your Website
Visit: http://www.Xshop.somee.com

## Important Configuration

After uploading, you'll need to configure:

1. **Database Connection**: Update `appsettings.json` on the server with your PostgreSQL connection string
2. **JWT Settings**: Add your JWT secret key and issuer/audience
3. **Stripe Keys**: Add your Stripe API keys if using payments

## Troubleshooting

If the site doesn't work:
1. Check Somee.com error logs in control panel
2. Verify all files were uploaded
3. Check file permissions
4. Verify .NET 8.0 runtime is available on Somee.com

