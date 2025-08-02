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
        alert(message);
        window.location.href = '/public-view/landingpage.html';
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
}

// Auto-initialize on page load
document.addEventListener('DOMContentLoaded', async () => {
    await AuthUtils.handlePageAccess();
});

// Export for use in other modules
window.AuthUtils = AuthUtils;
