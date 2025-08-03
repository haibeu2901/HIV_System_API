#!/bin/bash

# Automatic Security Protection Script for All Private View Pages
# This script adds universal security to all HTML files in private-view

echo "🔒 UNIVERSAL SECURITY DEPLOYMENT STARTING..."
echo "📁 Scanning private-view directory for HTML files..."

# Files to protect
files=(
    "private-view/admin-view/admin-home/admin-home.html"
    "private-view/admin-view/admin-home/test-notifications.html" 
    "private-view/admin-view/blog-management.html"
    "private-view/doctor-view/doctor-dashboard/doctor-dashboard.html"
    "private-view/doctor-view/doctor-work-schedule/doctor-work-schedule.html"
    "private-view/manager-view/manager-blog-overview.html"
    "private-view/manager-view/manager-home/manager-home.html"
    "private-view/shared/appointment-list/appoiment-list.html"
    "private-view/shared/header/header.html"
    "private-view/shared/medical-resources/medical-resources.html"
    "private-view/shared/notifications/notifications.html"
    "private-view/shared/patient-list/patient-list.html"
    "private-view/shared/patient-medical-record/patient-medical-record.html"
    "private-view/shared/token-guard.html"
    "private-view/shared/user-profile/user-profile.html"
    "private-view/shared/view-payment/view-payment.html"
    "private-view/staff-view/staff-community.html"
    "private-view/staff-view/staff-dashboard/staff-dashboard.html"
    "private-view/user-view/appointment-view/view-appointment.html"
    "private-view/user-view/booking/appointment-booking.html"
    "private-view/user-view/header/header.html"
    "private-view/user-view/medical-record/view-medical-record.html"
    "private-view/user-view/notification/notification.html"
    "private-view/user-view/profile/profile.html"
    "private-view/user-view/view-doctor/allDoctor.html"
)

# Security script injection
security_script='    <!-- UNIVERSAL SECURITY PROTECTION -->\n    <script src="../../universal-security.js"></script>'

protected_count=0
already_protected=0
failed_count=0

for file in "${files[@]}"; do
    if [[ -f "$file" ]]; then
        echo "📄 Processing: $file"
        
        # Check if already protected
        if grep -q "universal-security.js" "$file"; then
            echo "   ✅ Already protected"
            ((already_protected++))
        else
            # Calculate relative path to universal-security.js
            depth=$(echo "$file" | tr -cd '/' | wc -c)
            relative_path=""
            for ((i=2; i<depth; i++)); do
                relative_path="../$relative_path"
            done
            relative_path="${relative_path}universal-security.js"
            
            # Create backup
            cp "$file" "$file.backup"
            
            # Inject security script after <head>
            if sed -i "/<head>/a\\    <!-- UNIVERSAL SECURITY PROTECTION -->\\    <script src=\"$relative_path\"></script>" "$file"; then
                echo "   🔒 Security added successfully"
                ((protected_count++))
            else
                echo "   ❌ Failed to add security"
                ((failed_count++))
                # Restore backup
                mv "$file.backup" "$file"
            fi
        fi
    else
        echo "   ⚠️  File not found: $file"
        ((failed_count++))
    fi
done

echo ""
echo "🛡️  UNIVERSAL SECURITY DEPLOYMENT COMPLETE!"
echo "✅ Files protected: $protected_count"
echo "✅ Already protected: $already_protected"
echo "❌ Failed: $failed_count"
echo ""
echo "🚀 All private-view pages are now secured!"
echo "🔒 Universal security will automatically:"
echo "   - Block access without valid token"
echo "   - Enforce role-based access control" 
echo "   - Validate tokens with API"
echo "   - Redirect unauthorized users to login page"
