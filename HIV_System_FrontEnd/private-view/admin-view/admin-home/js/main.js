// Main Application Controller
class AdminApp {
    constructor() {
        this.managers = {};
        this.isInitialized = false;
    }

    // Initialize the application
    async init() {
        if (this.isInitialized) return;

        try {
            // Initialize core managers
            this.managers.auth = new AuthManager();
            this.managers.utils = new UtilsManager();
            this.managers.modal = new ModalManager();
            this.managers.navigation = new NavigationManager();
            
            // Check authentication first
            if (!this.managers.auth.checkAuth()) {
                return;
            }

            // Set up role-based access
            this.setupRoleBasedAccess();

            // Initialize feature managers
            this.managers.dashboard = new DashboardManager(this.managers.auth);
            this.managers.appointment = new AppointmentManager(this.managers.auth);
            this.managers.notification = new NotificationManager(this.managers.auth);
            this.managers.medicalRecord = new MedicalRecordManager(this.managers.auth);
            this.managers.account = new AccountManager(this.managers.auth);

            // Make managers globally available
            window.authManager = this.managers.auth;
            window.utilsManager = this.managers.utils;
            window.utils = this.managers.utils; // Also expose as utils for compatibility
            window.modalManager = this.managers.modal;
            window.navigationManager = this.managers.navigation;
            window.dashboardManager = this.managers.dashboard;
            window.appointmentManager = this.managers.appointment;
            window.notificationManager = this.managers.notification;
            window.medicalRecordManager = this.managers.medicalRecord;
            window.accountManager = this.managers.account;

            // Initialize all managers
            await this.initializeManagers();

            // Load initial data
            await this.loadInitialData();

            this.isInitialized = true;
            console.log('Admin application initialized successfully');

        } catch (error) {
            console.error('Failed to initialize admin application:', error);
            this.showError('Failed to initialize application. Please refresh the page.');
        }
    }

    // Setup role-based access
    setupRoleBasedAccess() {
        const userRole = parseInt(localStorage.getItem('userRole'));
        const dashboardTitle = document.getElementById('dashboard-title');
        const pageTitle = document.getElementById('page-title');
        
        // Update dashboard title and page title based on role
        if (userRole === 5) {
            if (dashboardTitle) {
                dashboardTitle.textContent = 'Manager Dashboard';
            }
            if (pageTitle) {
                pageTitle.textContent = 'Manager Dashboard - CareFirst HIV Clinic';
            }
        } else {
            if (dashboardTitle) {
                dashboardTitle.textContent = 'Admin Dashboard';
            }
            if (pageTitle) {
                pageTitle.textContent = 'Admin Dashboard - CareFirst HIV Clinic';
            }
        }
        
        // Hide accounts section for managers
        if (userRole === 5) {
            const adminOnlyElements = document.querySelectorAll('[data-role="admin-only"]');
            adminOnlyElements.forEach(element => {
                element.style.display = 'none';
            });
            console.log('🔒 Manager access: Accounts section hidden');
        }
        
        console.log('🔑 Role-based access set up for role:', userRole);
    }

    // Initialize all managers
    async initializeManagers() {
        const initPromises = Object.values(this.managers).map(manager => {
            if (typeof manager.init === 'function') {
                return manager.init();
            }
            return Promise.resolve();
        });

        await Promise.all(initPromises);
    }

    // Load initial data
    async loadInitialData() {
        try {
            // Ensure all modals are hidden on page load
            this.ensureModalsAreClosed();
            
            // Initialize charts and load dashboard data
            await this.managers.dashboard.initializeCharts();
            
            // Show dashboard section
            this.managers.navigation.showSection('dashboard');
            
        } catch (error) {
            console.error('Failed to load initial data:', error);
        }
    }

    // Ensure all modals are properly closed
    ensureModalsAreClosed() {
        const modalIds = [
            'createAccountModal',
            'createNotificationModal',
            'editAccountModal',
            'patientProfileModal',
            'doctorProfileModal',
            'generalProfileModal'
        ];
        
        modalIds.forEach(modalId => {
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.style.display = 'none';
            }
        });
    }

    // Show error message
    showError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'error-banner';
        errorDiv.innerHTML = `
            <div class="error-content">
                <i class="fas fa-exclamation-triangle"></i>
                <span>${message}</span>
                <button onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;
        document.body.insertBefore(errorDiv, document.body.firstChild);
    }

    // Get manager by name
    getManager(name) {
        return this.managers[name];
    }

    // Check if app is initialized
    isReady() {
        return this.isInitialized;
    }
}

// Initialize the application when DOM is loaded
document.addEventListener('DOMContentLoaded', async () => {
    // Create global app instance
    window.adminApp = new AdminApp();
    
    // Initialize the application
    await window.adminApp.init();
});

// Export for use in other modules
window.AdminApp = AdminApp;
