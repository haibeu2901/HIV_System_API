// CRITICAL SECURITY GUARD - Insert this at the very beginning of <head> in ALL private pages
(function() {
    'use strict';
    
    // IMMEDIATE blocking - before any other content can load
    const currentUrl = window.location.href;
    const currentPath = window.location.pathname;
    
    // Check if we're in a private directory
    const isPrivate = currentUrl.includes('/private-view/') || 
                     currentUrl.includes('\\private-view\\') ||
                     currentPath.includes('/private-view/');
    
    if (isPrivate) {
        // Immediate token check
        const token = localStorage.getItem('token');
        const userRole = localStorage.getItem('userRole');
        
        if (!token) {
            // BLOCK IMMEDIATELY
            document.documentElement.style.display = 'none';
            document.body && (document.body.style.display = 'none');
            
            // Clear any auth data
            localStorage.removeItem('token');
            localStorage.removeItem('userRole');
            localStorage.removeItem('accId');
            localStorage.removeItem('userId');
            localStorage.removeItem('fullName');
            
            // Redirect to login
            setTimeout(() => {
                window.location.href = '/public-view/landingpage.html?reason=' + encodeURIComponent('Authentication required');
            }, 100);
            return;
        }
        
        if (!userRole) {
            // BLOCK IMMEDIATELY 
            document.documentElement.style.display = 'none';
            document.body && (document.body.style.display = 'none');
            
            // Clear auth data
            localStorage.removeItem('token');
            localStorage.removeItem('userRole');
            localStorage.removeItem('accId');
            localStorage.removeItem('userId');
            localStorage.removeItem('fullName');
            
            setTimeout(() => {
                window.location.href = '/public-view/landingpage.html?reason=' + encodeURIComponent('Invalid user role');
            }, 100);
            return;
        }
        
        // Role-based access control
        const role = parseInt(userRole);
        const accessRules = {
            1: ['/private-view/admin-view/', '/private-view/shared/'], // Admin
            2: ['/private-view/doctor-view/', '/private-view/shared/'], // Doctor
            3: ['/private-view/user-view/', '/private-view/shared/'], // Patient
            4: ['/private-view/staff-view/', '/private-view/shared/'], // Staff
            5: ['/private-view/manager-view/', '/private-view/shared/'] // Manager
        };
        
        const allowedPaths = accessRules[role];
        if (!allowedPaths) {
            document.documentElement.style.display = 'none';
            document.body && (document.body.style.display = 'none');
            localStorage.removeItem('token');
            localStorage.removeItem('userRole');
            setTimeout(() => {
                window.location.href = '/public-view/landingpage.html?reason=' + encodeURIComponent('Invalid user role');
            }, 100);
            return;
        }
        
        const hasAccess = allowedPaths.some(allowedPath => 
            currentPath.includes(allowedPath) || currentUrl.includes(allowedPath)
        );
        
        if (!hasAccess) {
            document.documentElement.style.display = 'none';
            document.body && (document.body.style.display = 'none');
            setTimeout(() => {
                window.location.href = '/public-view/landingpage.html?reason=' + encodeURIComponent('Access denied for your role');
            }, 100);
            return;
        }
        
        console.log('✅ Security check passed for role:', role, 'at path:', currentPath);
    }
})();
