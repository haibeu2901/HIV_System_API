// Authentication Module
class AuthManager {
    constructor() {
        this.token = localStorage.getItem('token');
    }

    // Check authentication on page load
    checkAuth() {
        if (!this.token) {
            window.location.href = '/public-view/landingpage.html';
            return false;
        }
        return true;
    }

    // Get current token
    getToken() {
        return this.token || localStorage.getItem('token');
    }

    // Logout function
    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('accId');
        window.location.href = '/public-view/landingpage.html';
    }

    // Initialize authentication
    init() {
        // Add logout event listener
        const logoutBtn = document.getElementById('logout-btn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', () => this.logout());
        }
    }
}

// Export for use in other modules
window.AuthManager = AuthManager;
