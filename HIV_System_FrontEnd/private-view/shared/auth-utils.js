// Authentication utilities for token validation and redirection
class AuthUtils {
    static async validateToken() {
        const token = localStorage.getItem('token');
        
        if (!token) {
            return false;
        }

        try {
            // Test token validity by making a simple API call
            const response = await fetch('https://localhost:7009/api/Account/View-profile', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.status === 401 || response.status === 403) {
                // Token is invalid or expired
                this.clearAuthData();
                return false;
            }

            return response.ok;
        } catch (error) {
            console.error('Token validation failed:', error);
            // If network error, assume token is still valid to avoid unnecessary logouts
            return true;
        }
    }

    static clearAuthData() {
        localStorage.removeItem('token');
        localStorage.removeItem('userRole');
        localStorage.removeItem('accId');
        localStorage.removeItem('userId');
        localStorage.removeItem('fullName');
    }

    static redirectToLogin(message = 'Your session has expired. Please log in again.') {
        this.clearAuthData();
        console.log('Redirecting to landing page:', message);
        const encodedMessage = encodeURIComponent(message);
        window.location.href = `/public-view/landingpage.html?reason=${encodedMessage}`;
    }

    static async checkAuthAndRedirect() {
        const isValid = await this.validateToken();
        if (!isValid) {
            this.redirectToLogin();
        }
        return isValid;
    }

    static getUserRole() {
        return localStorage.getItem('userRole');
    }

    static getToken() {
        return localStorage.getItem('token');
    }

    static getUserId() {
        return localStorage.getItem('accId');
    }

    // Check if user should be redirected based on current page and auth status
    static async handlePageAccess() {
        const currentPath = window.location.pathname;
        const token = localStorage.getItem('token');
        
        // If on a private page
        if (currentPath.includes('/private-view/')) {
            if (!token) {
                // No token, redirect to landing page
                window.location.href = '/public-view/landingpage.html';
                return false;
            }
            
            // Validate token
            const isValid = await this.validateToken();
            if (!isValid) {
                this.redirectToLogin();
                return false;
            }
            
            // Check if user is on the correct page for their role
            const userRole = this.getUserRole();
            const isCorrectPage = this.isUserOnCorrectPage(currentPath, userRole);
            if (!isCorrectPage) {
                this.redirectToLogin('You do not have permission to access this page.');
                return false;
            }
        }
        
        // If on landing page and has valid token, redirect to appropriate dashboard
        if (currentPath.includes('landingpage.html') && token) {
            const isValid = await this.validateToken();
            if (isValid) {
                const userRole = this.getUserRole();
                this.redirectToDashboard(userRole);
                return false;
            }
        }
        
        return true;
    }

    static redirectToDashboard(role) {
        const roleInt = parseInt(role);
        
        switch (roleInt) {
            case 1: // Admin
                window.location.href = '/private-view/admin-view/admin-home/admin-home.html';
                break;
            case 2: // Doctor
                window.location.href = '/private-view/doctor-view/doctor-dashboard/doctor-dashboard.html';
                break;
            case 3: // Patient
                window.location.href = '/private-view/user-view/booking/appointment-booking.html';
                break;
            case 4: // Staff
                window.location.href = '/private-view/staff-view/staff-dashboard/staff-dashboard.html';
                break;
            case 5: // Manager
                window.location.href = '/private-view/manager-view/manager-home/manager-home.html';
                break;
            default:
                console.warn('Unknown role:', role);
                this.redirectToLogin('Invalid user role. Please log in again.');
        }
    }

    // Check if user is on the correct page for their role
    static isUserOnCorrectPage(currentPath, userRole) {
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
            console.warn('Unknown role:', userRole);
            return false;
        }

        // Check if current path matches any of the allowed paths
        return allowedPaths.some(allowedPath => currentPath.includes(allowedPath));
    }

    // Force role verification for sensitive actions
    static async verifyRoleAccess(requiredRoles = []) {
        const userRole = parseInt(this.getUserRole());
        
        if (!requiredRoles.includes(userRole)) {
            this.redirectToLogin('You do not have permission to perform this action.');
            return false;
        }
        
        // Also validate token to ensure session is still valid
        const isValidToken = await this.validateToken();
        if (!isValidToken) {
            this.redirectToLogin('Your session has expired. Please log in again.');
            return false;
        }
        
        return true;
    }

    // Additional method to handle direct URL access attempts
    static handleUnauthorizedAccess() {
        console.log('Unauthorized access attempt detected');
        this.clearAuthData();
        window.location.href = '/public-view/landingpage.html';
    }
}

// Auto-initialize on page load
document.addEventListener('DOMContentLoaded', async () => {
    await AuthUtils.handlePageAccess();
});

// Export for use in other modules
window.AuthUtils = AuthUtils;
