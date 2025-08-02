// Token validation guard - Include this script in all private pages
(function() {
    'use strict';
    
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
            window.location.href = CONFIG.LOGIN_PAGE;
            return false;
        }
        return true;
    }
    
    // Validate token with API call
    async function validateToken() {
        const token = localStorage.getItem('token');
        
        if (!token) {
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
                clearAuthData();
                return false;
            }
            
            return response.ok;
        } catch (error) {
            console.error('Token validation failed:', error);
            // On network error, assume token is valid to avoid unnecessary redirects
            return true;
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
        // First check if token exists
        if (!checkTokenExistence()) {
            return;
        }
        
        // Then validate token with API
        const isValid = await validateToken();
        if (!isValid) {
            redirectToLogin();
        }
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
    
    // Export functions for external use
    window.TokenGuard = {
        validateToken: validateToken,
        redirectToLogin: redirectToLogin,
        clearAuthData: clearAuthData
    };
    
})();
