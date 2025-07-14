// Navigation Module
class NavigationManager {
    constructor() {
        this.currentSection = 'dashboard';
    }

    // Section navigation
    showSection(sectionId) {
        // Hide all sections
        document.querySelectorAll('.admin-section').forEach(section => {
            section.classList.remove('active');
        });
        
        // Remove active class from all nav links
        document.querySelectorAll('.nav-link').forEach(link => {
            link.classList.remove('active');
        });
        
        // Show selected section
        const targetSection = document.getElementById(sectionId);
        if (targetSection) {
            targetSection.classList.add('active');
        }
        
        // Add active class to clicked nav link using data attribute
        const activeLink = document.querySelector(`[data-section="${sectionId}"]`);
        if (activeLink) {
            activeLink.classList.add('active');
        }
        
        // Update current section
        this.currentSection = sectionId;
        
        // Load section-specific data
        this.loadSectionData(sectionId);
    }

    // Load section-specific data
    loadSectionData(sectionId) {
        switch(sectionId) {
            case 'dashboard':
                window.dashboardManager.loadDashboardData();
                break;
            case 'notifications':
                window.notificationManager.loadNotifications();
                break;
            case 'appointments':
                window.appointmentManager.loadAppointments();
                break;
            case 'medical-records':
                window.medicalRecordManager.loadMedicalRecords();
                break;
            case 'accounts':
                window.accountManager.loadAccounts();
                break;
        }
    }

    // Initialize navigation
    init() {
        // Navigation links
        const navLinks = document.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const section = link.getAttribute('data-section');
                this.showSection(section);
            });
        });
    }
}

// Export for use in other modules
window.NavigationManager = NavigationManager;
