#!/bin/bash

# Script to publish BlazorApp1 for Somee.com deployment
# Usage: ./publish-for-somee.sh

set -e  # Exit on error

echo "🚀 Publishing BlazorApp1 for Somee.com deployment..."
echo ""

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Clean previous publish
if [ -d "./publish" ]; then
    echo "🧹 Cleaning previous publish folder..."
    rm -rf ./publish
fi

# Publish the application
echo "📦 Publishing application (Release configuration)..."
dotnet publish -c Release -o ./publish

# Check if publish was successful
if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Publish successful!"
    echo ""
    echo "📁 Published files are in: $SCRIPT_DIR/publish"
    echo ""
    echo "📋 Next steps:"
    echo "   1. Review the files in the 'publish' folder"
    echo "   2. Update appsettings.json with production settings if needed"
    echo "   3. Upload all files from 'publish' folder to Somee.com via FTP"
    echo "   4. Configure environment variables on Somee.com"
    echo ""
    echo "📖 See DEPLOY_SOMEE.md for detailed instructions"
    echo ""
    
    # List the publish folder contents
    echo "📂 Published files:"
    ls -lh ./publish | head -20
    echo ""
    echo "Total files: $(find ./publish -type f | wc -l)"
else
    echo ""
    echo "❌ Publish failed! Check the error messages above."
    exit 1
fi


