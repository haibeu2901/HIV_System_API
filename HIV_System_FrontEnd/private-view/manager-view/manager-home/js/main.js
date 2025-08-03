// Manager Dashboard Main - API Integration
document.addEventListener('DOMContentLoaded', async function() {
    // Initialize manager authentication
    if (!initializeManagerAuth()) {
        return;
    }
    
    // Initialize all required managers
    await initializeManagerModules();
    
    // Set up logout and modal functionality
    setupManagerLogout();
    setupManagerModals();
    
    // Load initial dashboard data
    if (window.managerDashboard) {
        await window.managerDashboard.loadDashboardData();
    }
});

// Initialize manager modules
async function initializeManagerModules() {
    try {
        // Initialize core managers (same as admin)
        const authManager = new AuthManager();
        const utilsManager = new UtilsManager();
        const modalManager = new ModalManager();
        const navigationManager = new ManagerNavigationManager();
        
        // Initialize feature managers
        const appointmentManager = new AppointmentManager(authManager);
        const notificationManager = new NotificationManager(authManager);
        const medicalRecordManager = new MedicalRecordManager(authManager);
        
        // Initialize manager dashboard
        const dashboardManager = new ManagerDashboardManager();
        
        // Make managers globally available
        window.authManager = authManager;
        window.utilsManager = utilsManager;
        window.utils = utilsManager; // Also expose as utils for compatibility
        window.modalManager = modalManager;
        window.navigationManager = navigationManager;
        window.appointmentManager = appointmentManager;
        window.notificationManager = notificationManager;
        window.medicalRecordManager = medicalRecordManager;
        window.managerDashboard = dashboardManager;
        
        // Initialize all managers
        if (typeof authManager.init === 'function') await authManager.init();
        if (typeof utilsManager.init === 'function') await utilsManager.init();
        if (typeof modalManager.init === 'function') await modalManager.init();
        if (typeof navigationManager.init === 'function') await navigationManager.init();
        if (typeof appointmentManager.init === 'function') await appointmentManager.init();
        if (typeof notificationManager.init === 'function') await notificationManager.init();
        if (typeof medicalRecordManager.init === 'function') await medicalRecordManager.init();
        
    } catch (error) {
        console.error('Failed to initialize manager modules:', error);
    }
}

// Initialize manager authentication
function initializeManagerAuth() {
    const token = localStorage.getItem('token');
    const userRole = localStorage.getItem('userRole');
    const userId = localStorage.getItem('accId'); // Use accId as userId
    
    if (!token || !userId) {
        window.location.href = '../../../public-view/landingpage.html';
        return false;
    }
    
    const role = parseInt(userRole);
    
    // Allow both manager (5) and admin (1) roles
    if (role !== 5 && role !== 1) {
        alert('Access denied. You do not have permission to access this page.');
        window.location.href = '../../../public-view/landingpage.html';
        return false;
    }
    
    return true;
}

// Setup manager logout
function setupManagerLogout() {
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function() {
            if (confirm('Are you sure you want to logout?')) {
                localStorage.clear();
                window.location.href = '../../../public-view/landingpage.html';
            }
        });
    }
}

// Setup manager modals and event listeners
function setupManagerModals() {
    // Modal close buttons
    document.querySelectorAll('.close-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const modalId = this.getAttribute('data-modal');
            closeModal(modalId);
        });
    });
    
    // Modal backdrop clicks
    document.querySelectorAll('.modal').forEach(modal => {
        modal.addEventListener('click', function(e) {
            if (e.target === this) {
                closeModal(this.id);
            }
        });
    });
    
    // Create notification button
    const createNotificationBtn = document.getElementById('create-notification-btn');
    if (createNotificationBtn) {
        createNotificationBtn.addEventListener('click', function() {
            showModal('createNotificationModal');
        });
    }
    
    // Notification filter
    const notificationFilter = document.getElementById('notification-filter');
    if (notificationFilter) {
        notificationFilter.addEventListener('change', function() {
            if (window.notificationManager) {
                window.notificationManager.filterNotifications();
            }
        });
    }
    
    // Create notification form
    const createNotificationForm = document.getElementById('createNotificationForm');
    if (createNotificationForm) {
        createNotificationForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const title = document.getElementById('notification-title').value;
            const message = document.getElementById('notification-message').value;
            const type = document.getElementById('notification-type').value;
            
            if (window.notificationManager) {
                await window.notificationManager.createNotification(title, message, type);
                closeModal('createNotificationModal');
            }
        });
    }
    
    // Edit notification form
    const editNotificationForm = document.getElementById('editNotificationForm');
    if (editNotificationForm) {
        editNotificationForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const id = document.getElementById('edit-notification-id').value;
            const type = document.getElementById('edit-notification-type').value;
            const message = document.getElementById('edit-notification-message').value;
            
            if (window.notificationManager) {
                await window.notificationManager.updateNotification(id, type, message);
                closeModal('editNotificationModal');
            }
        });
    }
}

// Show modal
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'flex';
    }
}

// Close modal
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
    }
}

// Manager Dashboard Manager Class
class ManagerDashboardManager {
    constructor() {
        this.baseUrl = 'https://localhost:7009/api';
        this.userId = localStorage.getItem('accId'); // Use accId as userId
        this.token = localStorage.getItem('token');
    }

    // Load dashboard data from API
    async loadDashboardData() {
        try {
            const response = await fetch(`${this.baseUrl}/Dashboard/manager?userId=${this.userId}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${this.token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            
            this.updateDashboardUI(data);
            this.updateCharts(data);
            
        } catch (error) {
            console.error('Error loading manager dashboard:', error);
            this.showErrorMessage('Failed to load dashboard data. Please try again.');
            this.showFallbackData();
        }
    }

    // Update dashboard UI with API data
    updateDashboardUI(data) {
        // Update statistics cards
        this.updateStatElement('total-users', data.totalPatients + data.totalDoctors + data.totalStaff);
        this.updateStatElement('total-patients', data.totalPatients);
        this.updateStatElement('total-doctors', data.totalDoctors);
        this.updateStatElement('total-staff', data.totalStaff);
        this.updateStatElement('total-appointments', data.monthlyAppointments);
        this.updateStatElement('pending-appointments', data.todayAppointments);
        this.updateStatElement('total-services', data.serviceUtilization ? data.serviceUtilization.length : 0);
        this.updateStatElement('total-revenue', this.formatCurrency(data.totalRevenue));
        this.updateStatElement('monthly-revenue', this.formatCurrency(data.monthlyRevenue));
        
        // Update recent activity
        this.updateRecentActivity(data);
    }

    // Update statistics element
    updateStatElement(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value;
        }
    }

    // Update charts with API data
    updateCharts(data) {
        // This would integrate with Chart.js or similar library
        // Charts implementation can be added here
    }

    // Update recent activity section
    updateRecentActivity(data) {
        const activityList = document.getElementById('recent-activity-list');
        if (!activityList) return;

        let activityHTML = '';
        
        // Add doctor performance activities
        if (data.doctorPerformance && data.doctorPerformance.length > 0) {
            data.doctorPerformance.forEach(doctor => {
                activityHTML += `
                    <div class="activity-item">
                        <i class="fas fa-user-md"></i>
                        <div class="activity-content">
                            <p><strong>${doctor.doctorName}</strong> handled ${doctor.appointmentCount} appointments</p>
                            <small>${doctor.patientCount} patients served</small>
                        </div>
                    </div>
                `;
            });
        }

        // Add staff performance activities
        if (data.staffPerformance && data.staffPerformance.length > 0) {
            data.staffPerformance.forEach(staff => {
                if (staff.testResultCount > 0 || staff.serviceCount > 0) {
                    activityHTML += `
                        <div class="activity-item">
                            <i class="fas fa-user-cog"></i>
                            <div class="activity-content">
                                <p><strong>${staff.staffName}</strong> processed ${staff.testResultCount} test results</p>
                                <small>${staff.serviceCount} services completed</small>
                            </div>
                        </div>
                    `;
                }
            });
        }

        // Add service utilization
        if (data.serviceUtilization && data.serviceUtilization.length > 0) {
            data.serviceUtilization.forEach(service => {
                if (service.utilizationCount > 0) {
                    activityHTML += `
                        <div class="activity-item">
                            <i class="fas fa-concierge-bell"></i>
                            <div class="activity-content">
                                <p><strong>${service.serviceName}</strong> used ${service.utilizationCount} times</p>
                                <small>Revenue: ${this.formatCurrency(service.revenue)}</small>
                            </div>
                        </div>
                    `;
                }
            });
        }

        activityList.innerHTML = activityHTML || '<div class="no-data">No recent activity found</div>';
    }

    // Show fallback data on error
    showFallbackData() {
        const fallbackData = {
            'total-users': 16,
            'total-patients': 11,
            'total-doctors': 2,
            'total-staff': 3,
            'total-appointments': 7,
            'pending-appointments': 0,
            'total-services': 5,
            'total-revenue': this.formatCurrency(1280000),
            'monthly-revenue': this.formatCurrency(1280000)
        };

        // Update with fallback data
        Object.keys(fallbackData).forEach(key => {
            this.updateStatElement(key, fallbackData[key]);
        });

        // Show fallback activity
        const activityList = document.getElementById('recent-activity-list');
        if (activityList) {
            activityList.innerHTML = `
                <div class="activity-item">
                    <i class="fas fa-info-circle"></i>
                    <div class="activity-content">
                        <p>Dashboard running in offline mode</p>
                        <small>Please check your connection</small>
                    </div>
                </div>
            `;
        }
    }

    // Format currency
    formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }

    // Show error message
    showErrorMessage(message) {
        const notification = document.createElement('div');
        notification.className = 'error-notification';
        notification.innerHTML = `
            <i class="fas fa-exclamation-triangle"></i>
            <span>${message}</span>
        `;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            notification.remove();
        }, 5000);
    }
}

// Export for use in other modules
window.ManagerDashboardManager = ManagerDashboardManager;