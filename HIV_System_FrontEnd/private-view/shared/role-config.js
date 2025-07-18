// Shared configuration for admin and manager roles
const ROLE_PERMISSIONS = {
    admin: {
        canManageAccounts: true,
        canCreateAccounts: true,
        canDeleteAccounts: true,
        canModifyRoles: true,
        canManageNotifications: true,
        canManageAppointments: true,
        canViewMedicalRecords: true,
        canManageMedicalRecords: true,
        canViewDashboard: true,
        canExportData: true,
        canManageSystem: true
    },
    manager: {
        canManageAccounts: false,
        canCreateAccounts: false,
        canDeleteAccounts: false,
        canModifyRoles: false,
        canManageNotifications: true,
        canManageAppointments: true,
        canViewMedicalRecords: true,
        canManageMedicalRecords: true,
        canViewDashboard: true,
        canExportData: true,
        canManageSystem: false
    }
};

// Role mappings
const ROLE_NAMES = {
    1: 'admin',
    2: 'doctor',
    3: 'patient',
    4: 'staff',
    5: 'manager'
};

// Function to check if user has permission for an action
function hasPermission(userRole, action) {
    const roleName = ROLE_NAMES[userRole] || 'patient';
    const permissions = ROLE_PERMISSIONS[roleName];

    if (!permissions) {
        return false;
    }

    return permissions[action] || false;
}

// Function to get user role from token or localStorage
function getUserRole() {
    const userRole = localStorage.getItem('userRole');
    return userRole ? parseInt(userRole) : null;
}

// Function to check if current user is admin
function isAdmin() {
    return getUserRole() === 1;
}

// Function to check if current user is manager
function isManager() {
    return getUserRole() === 5;
}

// Function to check if current user can access admin/manager features
function canAccessAdminFeatures() {
    const role = getUserRole();
    return role === 1 || role === 5; // admin or manager
}

// Function to get appropriate dashboard URL based on role
function getDashboardUrl(userRole) {
    const roleName = ROLE_NAMES[userRole];

    switch (roleName) {
        case 'admin':
        case 'manager':
            // Both admin and manager use the same admin dashboard
            return '/private-view/admin-view/admin-home/admin-home.html';
        case 'doctor':
            return '/private-view/doctor-view/doctor-dashboard/doctor-dashboard.html';
        case 'patient':
            return '/private-view/user-view/profile/profile.html';
        default:
            return '/public-view/landingpage.html';
    }
}

// Export functions for use in other modules
if (typeof window !== 'undefined') {
    window.roleUtils = {
        hasPermission,
        getUserRole,
        isAdmin,
        isManager,
        canAccessAdminFeatures,
        getDashboardUrl,
        ROLE_PERMISSIONS,
        ROLE_NAMES
    };
}

// For Node.js environments
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        hasPermission,
        getUserRole,
 isAdmin,
        isManager,
        canAccessAdminFeatures,
        getDashboardUrl,
        ROLE_PERMISSIONS,
        ROLE_NAMES
    };
}
