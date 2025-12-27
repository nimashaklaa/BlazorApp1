#!/bin/bash

# Personalized deployment script for Xshop.somee.com
# Usage: ./deploy-to-somee.sh

set -e

echo "🚀 Deploying BlazorApp1 to Xshop.somee.com"
echo ""

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Step 1: Publish
echo "📦 Step 1: Publishing application..."
if [ -d "./publish" ]; then
    echo "   Cleaning previous publish..."
    rm -rf ./publish
fi

dotnet publish BlazorApp1.csproj -c Release -o ./publish

if [ $? -ne 0 ]; then
    echo "❌ Publish failed!"
    exit 1
fi

echo "✅ Publish successful!"
echo ""

# Step 2: Show upload instructions
echo "📤 Step 2: Upload files to Somee.com"
echo ""
echo "Your FTP Details:"
echo "  Host: Xshop.somee.com (or 155.254.246.29)"
echo "  Username: amandi99"
echo "  Remote Path: /www.Xshop.somee.com"
echo ""
echo "Choose upload method:"
echo ""
echo "Option A - Using FileZilla (Recommended):"
echo "  1. Open FileZilla"
echo "  2. Connect to: Xshop.somee.com"
echo "  3. Username: amandi99"
echo "  4. Navigate to: /www.Xshop.somee.com"
echo "  5. Upload ALL files from: $SCRIPT_DIR/publish"
echo ""
echo "Option B - Using Terminal (SFTP):"
echo "  Run these commands:"
echo "  cd $SCRIPT_DIR/publish"
echo "  sftp amandi99@Xshop.somee.com"
echo "  cd /www.Xshop.somee.com"
echo "  put -r *"
echo "  exit"
echo ""
echo "Option C - Automated SFTP (requires password):"
read -p "Do you want to upload now via SFTP? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Connecting to SFTP..."
    cd ./publish
    echo "Commands to run in SFTP:"
    echo "  cd /www.Xshop.somee.com"
    echo "  put -r *"
    echo "  exit"
    echo ""
    echo "Starting SFTP session..."
    sftp amandi99@Xshop.somee.com <<EOF
cd /www.Xshop.somee.com
put -r *
exit
EOF
    echo ""
    echo "✅ Upload complete!"
else
    echo "⏭️  Skipping upload. You can upload manually later."
fi

echo ""
echo "🌐 Your website will be available at:"
echo "   http://www.Xshop.somee.com"
echo "   http://Xshop.somee.com"
echo ""
echo "⚠️  Don't forget to:"
echo "   1. Update appsettings.json on the server with production settings"
echo "   2. Configure your PostgreSQL database connection"
echo "   3. Set up JWT and Stripe keys"
echo ""

