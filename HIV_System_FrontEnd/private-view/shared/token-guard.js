// Token validation guard - Include this script in all private pages
(function() {
    'use strict';
    
    // IMMEDIATE SECURITY CHECK - Run before anything else
    const immediateToken = localStorage.getItem('token');
    if (!immediateToken) {
        console.log('SECURITY: No token found, immediate redirect to landing page');
        clearAuthData();
        window.location.href = '/public-view/landingpage.html';
        return; // Stop execution
    }
    
    // IMMEDIATE ROLE CHECK - Run before anything else
    const immediateRole = localStorage.getItem('userRole');
    const currentPath = window.location.pathname;
    if (immediateRole) {
        const roleInt = parseInt(immediateRole);
        const rolePaths = {
            1: ['/private-view/admin-view/', '/private-view/shared/'],
            2: ['/private-view/doctor-view/', '/private-view/shared/'],
            3: ['/private-view/user-view/', '/private-view/shared/'],
            4: ['/private-view/staff-view/', '/private-view/shared/'],
            5: ['/private-view/manager-view/', '/private-view/shared/']
        };
        
        const allowedPaths = rolePaths[roleInt];
        if (allowedPaths && !allowedPaths.some(path => currentPath.includes(path))) {
            console.log('SECURITY: Role access denied, immediate redirect to landing page');
            clearAuthData();
            window.location.href = '/public-view/landingpage.html';
            return; // Stop execution
        }
    } else {
        console.log('SECURITY: No role found, immediate redirect to landing page');
        clearAuthData();
        window.location.href = '/public-view/landingpage.html';
        return; // Stop execution
    }

    // Clear authentication data helper function
    function clearAuthData() {
        localStorage.removeItem('token');
        localStorage.removeItem('userRole');
        localStorage.removeItem('accId');
        localStorage.removeItem('userId');
        localStorage.removeItem('fullName');
    }
    
    // Configuration
    const CONFIG = {
        API_BASE_URL: 'https://localhost:7009',
        LOGIN_PAGE: '/public-view/landingpage.html'
    };
    
    // Immediate token check function
    function checkTokenExistence() {
        const token = localStorage.getItem('token');
        if (!token) {
            console.log('No token found, redirecting to landing page...');
            redirectToLandingPage('Authentication required.');
            return false;
        }
        return true;
    }
    
    // Validate token with API call
    async function validateToken() {
        const token = localStorage.getItem('token');
        
        if (!token) {
            console.log('No token found during validation, redirecting to landing page...');
            redirectToLandingPage('Authentication required.');
            return false;
        }
        
        try {
            const response = await fetch(`${CONFIG.API_BASE_URL}/api/Account/View-profile`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.status === 401 || response.status === 403) {
                console.log('Token invalid or expired, clearing auth data...');
                redirectToLandingPage('Session expired. Please log in again.');
                return false;
            }
            
            if (!response.ok) {
                console.log('Token validation failed with status:', response.status);
                redirectToLandingPage('Authentication failed. Please log in again.');
                return false;
            }
            
            return true;
        } catch (error) {
            console.error('Token validation failed:', error);
            // On network error, redirect to be safe - no access without valid token
            redirectToLandingPage('Unable to verify session. Please log in again.');
            return false;
        }
    }
    
    // Clear authentication data
    function clearAuthData() {
        localStorage.removeItem('token');
        localStorage.removeItem('userRole');
        localStorage.removeItem('accId');
        localStorage.removeItem('userId');
        localStorage.removeItem('fullName');
    }
    
    // Redirect to landing page with message
    function redirectToLandingPage(message = 'Access denied. Please log in.') {
        clearAuthData();
        console.log('Redirecting to landing page:', message);
        const encodedMessage = encodeURIComponent(message);
        window.location.href = `${CONFIG.LOGIN_PAGE}?reason=${encodedMessage}`;
    }

    // Legacy function for backward compatibility
    function redirectToLogin(message = 'Access denied. Please log in.') {
        redirectToLandingPage(message);
    }
    
    // Main validation function
    async function performTokenValidation() {
        // First check if token exists - if not, immediate redirect
        if (!checkTokenExistence()) {
            return; // Already redirected
        }
        
        // If token exists, validate it with API
        const isValid = await validateToken();
        if (!isValid) {
            return; // Already redirected by validateToken
        }
        
        // Check role-based access
        const hasRoleAccess = checkRoleAccess();
        if (!hasRoleAccess) {
            return; // Already redirected by checkRoleAccess
        }
    }
    
    // Check if user has correct role for the current page
    function checkRoleAccess() {
        const currentPath = window.location.pathname;
        const userRole = localStorage.getItem('userRole');
        
        if (!userRole) {
            console.log('No user role found, redirecting to landing page...');
            redirectToLandingPage('Invalid session. Please log in again.');
            return false;
        }
        
        const roleInt = parseInt(userRole);
        
        // Define allowed paths for each role
        const rolePaths = {
            1: [ // Admin
                '/private-view/admin-view/',
                '/private-view/shared/' // Shared components accessible to all roles
            ],
            2: [ // Doctor
                '/private-view/doctor-view/',
                '/private-view/shared/'
            ],
            3: [ // Patient/User
                '/private-view/user-view/',
                '/private-view/shared/'
            ],
            4: [ // Staff
                '/private-view/staff-view/',
                '/private-view/shared/'
            ],
            5: [ // Manager
                '/private-view/manager-view/',
                '/private-view/shared/'
            ]
        };

        // Get allowed paths for the user's role
        const allowedPaths = rolePaths[roleInt];
        
        if (!allowedPaths) {
            console.log('Unknown role:', userRole);
            redirectToLandingPage('Invalid user role. Please log in again.');
            return false;
        }

        // Check if current path matches any of the allowed paths
        const hasAccess = allowedPaths.some(allowedPath => currentPath.includes(allowedPath));
        
        if (!hasAccess) {
            console.log('Role access denied. User role:', userRole, 'Path:', currentPath);
            redirectToLandingPage('You do not have permission to access this page.');
            return false;
        }
        
        return true;
    }
    
    // Run validation immediately if DOM is already loaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', performTokenValidation);
    } else {
        performTokenValidation();
    }
    
    // Also run validation when page becomes visible (handles tab switching)
    document.addEventListener('visibilitychange', function() {
        if (!document.hidden) {
            performTokenValidation();
        }
    });
    
    // Periodic token validation (every 2 minutes)
    setInterval(function() {
        performTokenValidation();
    }, 2 * 60 * 1000); // 2 minutes
    
    // Also check when user interacts with the page
    document.addEventListener('click', function() {
        // Only check occasionally to avoid too many API calls
        if (Math.random() < 0.1) { // 10% chance on each click
            performTokenValidation();
        }
    });
    
    // Export functions for external use
    window.TokenGuard = {
        validateToken: validateToken,
        redirectToLogin: redirectToLogin,
        clearAuthData: clearAuthData,
        performTokenValidation: performTokenValidation,
        checkRoleAccess: checkRoleAccess
    };
    
})();
