// Token validation guard - Include this script in all private pages
(function() {
    'use strict';
    
    // IMMEDIATE SECURITY CHECK - Run before anything else
    const immediateToken = localStorage.getItem('token');
    if (!immediateToken) {
        console.log('SECURITY: No token found, immediate redirect to login');
        alert('You need to be logged in to access this page.');
        window.location.href = '/public-view/landingpage.html';
        return; // Stop execution
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
            console.log('No token found, redirecting to login...');
            redirectToLogin('You need to be logged in to access this page.');
            return false;
        }
        return true;
    }
    
    // Validate token with API call
    async function validateToken() {
        const token = localStorage.getItem('token');
        
        if (!token) {
            console.log('No token found during validation, redirecting to login...');
            redirectToLogin('You need to be logged in to access this page.');
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
                redirectToLogin('Your session has expired. Please log in again.');
                return false;
            }
            
            if (!response.ok) {
                console.log('Token validation failed with status:', response.status);
                redirectToLogin('Authentication failed. Please log in again.');
                return false;
            }
            
            return true;
        } catch (error) {
            console.error('Token validation failed:', error);
            // On network error, redirect to be safe - no access without valid token
            redirectToLogin('Unable to verify your session. Please log in again.');
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
    
    // Redirect to login with message
    function redirectToLogin(message = 'Your session has expired. Please log in again.') {
        clearAuthData();
        alert(message);
        window.location.href = CONFIG.LOGIN_PAGE;
    }
    
    // Main validation function
    async function performTokenValidation() {
        // First check if token exists - if not, immediate redirect
        if (!checkTokenExistence()) {
            return; // Already redirected
        }
        
        // If token exists, validate it with API
        const isValid = await validateToken();
        // validateToken now handles its own redirects, so we don't need additional logic here
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
        performTokenValidation: performTokenValidation
    };
    
})();
