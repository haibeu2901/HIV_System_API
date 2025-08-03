// Universal Security Guard for ALL private-view pages
// This script automatically protects any page in the private-view directory
(function() {
    'use strict';
    
    console.log('🔒 UNIVERSAL SECURITY GUARD ACTIVATED');
    console.log('📍 Current URL:', window.location.href);
    console.log('📂 Current Path:', window.location.pathname);
    
    // IMMEDIATE SECURITY CHECK - Block access before any content renders
    const currentPath = window.location.pathname;
    const currentUrl = window.location.href;
    
    // Check if we're in a private-view directory (handle both file:// and http:// protocols)
    const isPrivateView = currentPath.includes('/private-view/') || 
                         currentUrl.includes('/private-view/') ||
                         currentUrl.includes('\\private-view\\');
    
    if (isPrivateView) {
        console.log('🔍 Detected private-view access attempt');
        
        // Immediate token check
        const token = localStorage.getItem('token');
        const userRole = localStorage.getItem('userRole');
        
        console.log('🎫 Token present:', !!token);
        console.log('👤 Role present:', !!userRole);
        
        if (!token) {
            console.log('❌ BLOCKING: No authentication token');
            blockAccess('Authentication required. Please log in to continue.');
            return;
        }
        
        if (!userRole) {
            console.log('❌ BLOCKING: No user role found');
            blockAccess('Invalid session. Please log in again.');
            return;
        }
        
        // Role-based access control
        const roleInt = parseInt(userRole);
        if (!isValidRole(roleInt)) {
            console.log('❌ BLOCKING: Invalid role -', roleInt);
            blockAccess('Invalid user role. Please log in again.');
            return;
        }
        
        // Check if user has access to this specific path
        if (!hasRoleAccess(currentPath, roleInt)) {
            console.log('❌ BLOCKING: Role', roleInt, 'cannot access', currentPath);
            blockAccess('You do not have permission to access this page.');
            return;
        }
        
        console.log('✅ SECURITY CHECK PASSED for role', roleInt);
        
        // Additional async token validation
        validateTokenAsync();
    }
    
    function isValidRole(role) {
        return [1, 2, 3, 4, 5].includes(role);
    }
    
    function hasRoleAccess(path, role) {
        // Define role-based access patterns
        const accessRules = {
            1: ['/private-view/admin-view/', '/private-view/shared/'], // Admin
            2: ['/private-view/doctor-view/', '/private-view/shared/'], // Doctor
            3: ['/private-view/user-view/', '/private-view/shared/'], // Patient
            4: ['/private-view/staff-view/', '/private-view/shared/'], // Staff
            5: ['/private-view/manager-view/', '/private-view/shared/'] // Manager
        };
        
        const allowedPaths = accessRules[role];
        if (!allowedPaths) {
            return false;
        }
        
        return allowedPaths.some(allowedPath => path.includes(allowedPath));
    }
    
    function blockAccess(reason) {
        console.log('🚫 ACCESS BLOCKED:', reason);
        
        // Clear all auth data
        localStorage.removeItem('token');
        localStorage.removeItem('userRole');
        localStorage.removeItem('accId');
        localStorage.removeItem('userId');
        localStorage.removeItem('fullName');
        
        // Prevent page rendering
        document.documentElement.style.display = 'none';
        
        // Show loading overlay while redirecting
        showRedirectOverlay(reason);
        
        // Redirect after short delay
        setTimeout(() => {
            const encodedReason = encodeURIComponent(reason);
            window.location.href = `/public-view/landingpage.html?reason=${encodedReason}`;
        }, 1000);
    }
    
    function showRedirectOverlay(reason) {
        // Remove any existing overlay
        const existingOverlay = document.getElementById('security-overlay');
        if (existingOverlay) {
            existingOverlay.remove();
        }
        
        // Create security overlay
        const overlay = document.createElement('div');
        overlay.id = 'security-overlay';
        overlay.innerHTML = `
            <div style="
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                display: flex;
                justify-content: center;
                align-items: center;
                z-index: 999999;
                font-family: Arial, sans-serif;
            ">
                <div style="
                    background: white;
                    padding: 40px;
                    border-radius: 10px;
                    text-align: center;
                    box-shadow: 0 10px 30px rgba(0,0,0,0.3);
                    max-width: 400px;
                    margin: 20px;
                ">
                    <div style="
                        width: 60px;
                        height: 60px;
                        background: #ff6b6b;
                        border-radius: 50%;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        margin: 0 auto 20px;
                        color: white;
                        font-size: 24px;
                    ">🔒</div>
                    <h2 style="margin: 0 0 15px 0; color: #333;">Access Denied</h2>
                    <p style="margin: 0 0 20px 0; color: #666; line-height: 1.5;">${reason}</p>
                    <div style="
                        display: inline-block;
                        padding: 10px 20px;
                        background: #667eea;
                        color: white;
                        border-radius: 5px;
                        font-size: 14px;
                    ">
                        Redirecting to login page...
                    </div>
                </div>
            </div>
        `;
        
        // Insert overlay immediately
        if (document.body) {
            document.body.appendChild(overlay);
        } else {
            // If body doesn't exist yet, wait for it
            document.addEventListener('DOMContentLoaded', () => {
                document.body.appendChild(overlay);
            });
        }
    }
    
    async function validateTokenAsync() {
        const token = localStorage.getItem('token');
        if (!token) return;
        
        try {
            console.log('🔄 Performing async token validation...');
            
            const response = await fetch('https://localhost:7009/api/Account/View-profile', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.status === 401 || response.status === 403) {
                console.log('❌ Token validation failed - token expired or invalid');
                blockAccess('Your session has expired. Please log in again.');
                return;
            }
            
            if (!response.ok) {
                console.log('❌ Token validation failed - server error');
                blockAccess('Authentication failed. Please log in again.');
                return;
            }
            
            console.log('✅ Token validation successful');
            
        } catch (error) {
            console.error('❌ Token validation network error:', error);
            blockAccess('Unable to verify session. Please log in again.');
        }
    }
    
    // Auto-check on page visibility change
    document.addEventListener('visibilitychange', function() {
        if (!document.hidden && window.location.pathname.includes('/private-view/')) {
            validateTokenAsync();
        }
    });
    
    // Periodic validation every 2 minutes
    setInterval(function() {
        if (window.location.pathname.includes('/private-view/')) {
            validateTokenAsync();
        }
    }, 2 * 60 * 1000);
    
    console.log('🛡️ Universal Security Guard setup complete');
    
})();
