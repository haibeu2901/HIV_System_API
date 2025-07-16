# Admin and Manager Dashboard Documentation

## Overview
This system provides two similar but distinct dashboards for different user roles:
- **Admin Dashboard**: Full access to all features including account management
- **Manager Dashboard**: Limited access without account management capabilities

## File Structure

```
private-view/
├── admin-view/
│   └── admin-home/
│       ├── admin-home.html
│       ├── admin-home.css
│       └── js/
│           ├── utils.js
│           ├── auth.js
│           ├── navigation.js
│           ├── dashboard.js
│           ├── appointments.js
│           ├── notifications.js
│           ├── medical-records.js
│           ├── accounts.js
│           └── main.js
├── manager-view/
│   └── manager-home/
│       ├── manager-home.html
│       ├── manager-home.css
│       └── js/
│           ├── navigation.js
│           └── main.js
└── shared/
    └── role-config.js
```

## Shared Features

Both admin and manager dashboards share the following features:

### 1. Dashboard Overview
- Total users, patients, doctors, staff statistics
- Appointment metrics (total, pending)
- Revenue tracking
- Interactive charts and graphs
- Recent activity feed

### 2. Notifications Management
- View all notifications
- Create new notifications
- Edit existing notifications
- Filter by type (system, appointment, reminder)
- Support for multiple notification types:
  - Appt Confirm
  - Appointment Update
  - Appointment Request
  - Appointment Reminder
  - System Alert
  - ARV Consultation
  - Test Result
  - Blog Approval

### 3. Appointments Management
- View all appointments
- Filter by date and status
- Status options: Pending, Confirmed, Completed, Cancelled
- Appointment details and management

### 4. Medical Records
- View patient medical records
- Search by patient name or ID
- Medical record management

## Admin-Only Features

The admin dashboard includes additional features that managers cannot access:

### 1. Account Management
- Create new accounts (Doctor, Patient)
- View all user accounts
- Filter by role
- Account search functionality
- User role management

### 2. System Administration
- Full system control
- Advanced configuration options
- User role modifications

## Role-Based Access Control

### Role Permissions

The system uses a role-based permission system defined in `shared/role-config.js`:

```javascript
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
```

### Role Mapping

```javascript
const ROLE_NAMES = {
    1: 'admin',
    2: 'doctor',
    3: 'patient',
    4: 'staff',
    5: 'manager'
};
```

## Implementation Details

### 1. Shared JavaScript Files

Both dashboards use the same JavaScript files for common functionality:
- `utils.js` - Utility functions
- `auth.js` - Authentication handling
- `dashboard.js` - Dashboard statistics and charts
- `appointments.js` - Appointment management
- `notifications.js` - Notification system
- `medical-records.js` - Medical records handling

### 2. Role-Specific Files

Manager dashboard has its own:
- `navigation.js` - Navigation without accounts section
- `main.js` - Manager-specific initialization

### 3. Styling

Manager dashboard extends admin styles with:
- Custom color scheme (blue theme)
- Hidden account management elements
- Role-specific visual indicators

### 4. Navigation Differences

**Admin Navigation:**
- Dashboard
- Notifications
- Appointments
- Medical Records
- **Accounts** (Admin only)

**Manager Navigation:**
- Dashboard
- Notifications
- Appointments
- Medical Records

## Usage Instructions

### For Administrators:
1. Access: `/private-view/admin-view/admin-home/admin-home.html`
2. Full access to all features
3. Can manage user accounts and system settings

### For Managers:
1. Access: `/private-view/manager-view/manager-home/manager-home.html`
2. Limited access without account management
3. Can manage notifications, appointments, and medical records

### Authentication:
Both dashboards require valid JWT tokens stored in localStorage:
- `token` - JWT authentication token
- `userRole` - User role (1 for admin, 5 for manager)
- `userId` - User identifier

## Development Notes

### Adding New Features:
1. Add feature to shared JavaScript files if both roles need it
2. Use role-config.js to check permissions
3. Update both HTML files if UI changes are needed

### Permission Checking:
```javascript
// Check if user has permission for an action
if (roleUtils.hasPermission(userRole, 'canManageAccounts')) {
    // Show account management features
}

// Check if user is admin
if (roleUtils.isAdmin()) {
    // Admin-only code
}

// Check if user is manager
if (roleUtils.isManager()) {
    // Manager-only code
}
```

### Styling Customization:
- Admin styles are in `admin-home.css`
- Manager styles extend admin styles via `manager-home.css`
- Use role-specific CSS classes for differentiation

## Security Considerations

1. **Client-side Role Checking**: Remember that client-side role checking is for UI purposes only
2. **Server-side Validation**: Always validate permissions on the server side
3. **Token Validation**: Ensure JWT tokens are properly validated
4. **Access Control**: Implement proper access control at the API level

## Future Enhancements

1. **Dynamic Role Management**: Allow runtime role permission changes
2. **Audit Logging**: Track admin and manager actions
3. **Advanced Analytics**: Role-specific dashboard analytics
4. **Multi-tenant Support**: Support for multiple organizations
5. **Mobile Optimization**: Responsive design improvements
