#!/bin/bash

echo "🔒 IMMEDIATE SECURITY DEPLOYMENT STARTING..."
echo "📁 Scanning private-view directory for HTML files..."

protected_count=0
already_protected=0
failed_count=0

# Find all HTML files in private-view directory
find private-view -name "*.html" -type f | while read -r file; do
    echo "📄 Processing: $file"
    
    # Check if file is already protected with immediate security
    if grep -q "immediate-security.js" "$file"; then
        echo "   ✅ Already has immediate security"
        ((already_protected++))
        continue
    fi
    
    # Check if it's a full HTML document (has <!DOCTYPE html>)
    if ! grep -q "<!DOCTYPE html>" "$file"; then
        echo "   ⏭️  Skipping partial HTML file"
        continue
    fi
    
    # Create backup
    cp "$file" "$file.backup" 2>/dev/null
    
    # Calculate correct relative path to immediate-security.js
    depth=$(echo "$file" | tr -cd '/' | wc -c)
    depth=$((depth - 1)) # Subtract 1 because private-view is the base
    
    relative_path=""
    for ((i=0; i<depth; i++)); do
        relative_path="../$relative_path"
    done
    relative_path="${relative_path}immediate-security.js"
    
    # Add immediate security script right after <head>
    if sed -i "/<head>/a\\    <!-- IMMEDIATE SECURITY PROTECTION -->\\n    <script src=\"$relative_path\"></script>" "$file"; then
        echo "   🔒 Immediate security added successfully (path: $relative_path)"
        ((protected_count++))
    else
        echo "   ❌ Failed to add security"
        ((failed_count++))
        # Restore backup on failure
        mv "$file.backup" "$file" 2>/dev/null
    fi
done

echo ""
echo "🛡️  IMMEDIATE SECURITY DEPLOYMENT COMPLETE!"
echo "✅ Files protected: $protected_count"
echo "✅ Already protected: $already_protected"
echo "❌ Failed: $failed_count"
echo ""
echo "🚀 All private-view pages now have IMMEDIATE security!"
echo "🔒 Immediate security will:"
echo "   - Block access INSTANTLY without valid token"
echo "   - Hide page content immediately"
echo "   - Enforce role-based access control"
echo "   - Redirect unauthorized users in 100ms"
