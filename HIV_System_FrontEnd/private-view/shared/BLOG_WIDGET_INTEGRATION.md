# Blog Widget Integration Examples

This document shows how to add blog widgets to existing dashboard pages across different user roles.

## Quick Blog Widget Integration

### 1. Add to Any Existing Page

Add this HTML where you want the blog widget:

```html
<!-- Blog Widget Container -->
<div class="blog-widget">
    <div class="widget-header">
        <h3><i class="fas fa-comments"></i> Community Stories</h3>
        <a href="[role-specific-blog-page]" class="view-all-link">View All</a>
    </div>
    <div id="blog-widget-container"></div>
</div>

<!-- Include Blog Integration Script -->
<script src="../../shared/blog-integration.js"></script>
<script>
document.addEventListener('DOMContentLoaded', function() {
    const userToken = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    
    // Initialize compact blog widget
    initializeBlogIntegration('blog-widget-container', {
        compact: true,
        maxPosts: 3,
        showCreateButton: false,
        showFilters: false,
        showStats: false,
        userRole: 'your-role', // patient, doctor, staff, manager, admin
        authToken: userToken
    });
});
</script>

<!-- Widget Styles -->
<style>
.blog-widget {
    background: white;
    border-radius: 12px;
    padding: 20px;
    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    margin-bottom: 20px;
}

.widget-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 15px;
}

.widget-header h3 {
    margin: 0;
    color: #2c3e50;
    font-size: 16px;
    display: flex;
    align-items: center;
    gap: 8px;
}

.view-all-link {
    color: #3498db;
    text-decoration: none;
    font-size: 14px;
    font-weight: 500;
}

.view-all-link:hover {
    text-decoration: underline;
}
</style>
```

## Role-Specific Integration Examples

### Patient Dashboard Integration

Add to existing patient dashboard HTML:

```html
<!-- In patient dashboard main content -->
<div class="dashboard-grid">
    <!-- Existing dashboard cards -->
    <div class="dashboard-card">...</div>
    <div class="dashboard-card">...</div>
    
    <!-- Add blog widget -->
    <div class="dashboard-card blog-card">
        <div class="card-header">
            <h3><i class="fas fa-comments"></i> Community Stories</h3>
            <a href="../community/patient-community.html" class="btn-link">Share Your Story</a>
        </div>
        <div id="patient-blog-widget"></div>
    </div>
</div>

<script>
// Initialize patient blog widget
initializeBlogIntegration('patient-blog-widget', {
    compact: true,
    maxPosts: 3,
    showCreateButton: false,
    showFilters: false,
    showStats: false,
    userRole: 'patient',
    authToken: patientToken
});
</script>
```

### Doctor Dashboard Integration

Add to existing doctor dashboard:

```html
<!-- In doctor dashboard sidebar -->
<div class="sidebar-widgets">
    <!-- Existing widgets -->
    <div class="widget">...</div>
    
    <!-- Add blog widget -->
    <div class="widget community-widget">
        <div class="widget-title">
            <i class="fas fa-comments"></i>
            <span>Patient Insights</span>
            <a href="doctor-dashboard-with-blog.html" class="widget-link">View All</a>
        </div>
        <div id="doctor-blog-widget"></div>
    </div>
</div>

<script>
// Initialize doctor blog widget
initializeBlogIntegration('doctor-blog-widget', {
    compact: true,
    maxPosts: 2,
    showCreateButton: false,
    showFilters: false,
    showStats: false,
    userRole: 'doctor',
    authToken: doctorToken
});
</script>
```

### Staff Dashboard Integration

Add to existing staff dashboard:

```html
<!-- In staff dashboard main area -->
<div class="staff-overview">
    <!-- Existing overview sections -->
    <div class="overview-section">...</div>
    
    <!-- Add patient stories section -->
    <div class="overview-section patient-stories">
        <h3><i class="fas fa-heart"></i> Recent Patient Stories</h3>
        <p class="section-desc">Understanding patient experiences to provide better care</p>
        <div id="staff-blog-widget"></div>
        <div class="section-footer">
            <a href="../staff-community.html" class="btn-outline">View Community Stories</a>
        </div>
    </div>
</div>

<script>
// Initialize staff blog widget
initializeBlogIntegration('staff-blog-widget', {
    compact: true,
    maxPosts: 4,
    showCreateButton: false,
    showFilters: false,
    showStats: false,
    userRole: 'staff',
    authToken: staffToken
});
</script>
```

### Admin Dashboard Integration

Add to existing admin dashboard:

```html
<!-- In admin dashboard overview -->
<div class="admin-overview-grid">
    <!-- Existing admin cards -->
    <div class="admin-card">...</div>
    
    <!-- Add community management card -->
    <div class="admin-card community-management">
        <div class="card-header">
            <h3><i class="fas fa-shield-check"></i> Community Management</h3>
            <a href="../blog-management.html" class="btn-primary-sm">Manage</a>
        </div>
        <div class="card-stats">
            <div class="stat">
                <span class="stat-value" id="pendingReviews">0</span>
                <span class="stat-label">Pending Reviews</span>
            </div>
            <div class="stat">
                <span class="stat-value" id="publishedToday">0</span>
                <span class="stat-label">Published Today</span>
            </div>
        </div>
        <div id="admin-blog-widget"></div>
    </div>
</div>

<script>
// Initialize admin blog widget
initializeBlogIntegration('admin-blog-widget', {
    compact: true,
    maxPosts: 3,
    showCreateButton: false,
    showFilters: false,
    showStats: false,
    userRole: 'admin',
    authToken: adminToken
});

// Load admin stats
loadAdminBlogStats();
</script>
```

## Navigation Menu Integration

Add blog links to existing navigation menus:

### Patient Navigation
```html
<!-- Add to patient navigation menu -->
<nav class="patient-nav">
    <a href="../profile/profile.html"><i class="fas fa-user"></i> Profile</a>
    <a href="../medical-record/medical-record.html"><i class="fas fa-file-medical"></i> Medical Records</a>
    <a href="../community/patient-community.html"><i class="fas fa-comments"></i> Community</a>
    <a href="../appointment-view/appointment-view.html"><i class="fas fa-calendar"></i> Appointments</a>
</nav>
```

### Doctor Navigation
```html
<!-- Add to doctor navigation menu -->
<nav class="doctor-nav">
    <a href="doctor-dashboard.html"><i class="fas fa-tachometer-alt"></i> Dashboard</a>
    <a href="../patient-list/patient-list.html"><i class="fas fa-users"></i> Patients</a>
    <a href="doctor-dashboard-with-blog.html"><i class="fas fa-comments"></i> Community</a>
    <a href="../appointment-list/appointment-list.html"><i class="fas fa-calendar"></i> Appointments</a>
</nav>
```

### Staff Navigation
```html
<!-- Add to staff navigation menu -->
<nav class="staff-nav">
    <a href="staff-dashboard.html"><i class="fas fa-clipboard"></i> Dashboard</a>
    <a href="../patient-list/patient-list.html"><i class="fas fa-users"></i> Patients</a>
    <a href="staff-community.html"><i class="fas fa-comments"></i> Community</a>
    <a href="../appointment-list/appointment-list.html"><i class="fas fa-calendar"></i> Appointments</a>
</nav>
```

### Admin Navigation
```html
<!-- Add to admin navigation menu -->
<nav class="admin-nav">
    <a href="admin-dashboard.html"><i class="fas fa-tachometer-alt"></i> Dashboard</a>
    <a href="../user-management/user-management.html"><i class="fas fa-users-cog"></i> Users</a>
    <a href="blog-management.html"><i class="fas fa-comments"></i> Community</a>
    <a href="../reports/reports.html"><i class="fas fa-chart-bar"></i> Reports</a>
</nav>
```

## File Structure Summary

```
HIV_System_FrontEnd/
├── public-view/
│   └── blog/
│       └── blog-public.html                    # Public blog access
├── private-view/
│   ├── shared/
│   │   ├── blog-integration.js                 # Main integration script
│   │   └── BLOG_INTEGRATION_GUIDE.md          # This guide
│   ├── user-view/
│   │   └── community/
│   │       └── patient-community.html          # Patient community page
│   ├── doctor-view/
│   │   └── doctor-dashboard/
│   │       └── doctor-dashboard-with-blog.html # Doctor blog integration
│   ├── staff-view/
│   │   └── staff-community.html                # Staff community page
│   ├── manager-view/
│   │   └── manager-blog-overview.html          # Manager overview
│   └── admin-view/
│       └── blog-management.html                # Admin management
```

## Implementation Checklist

### For Each User Role:
- [ ] Add navigation menu link to blog page
- [ ] Add blog widget to main dashboard
- [ ] Configure appropriate permissions (read/write/moderate)
- [ ] Test authentication integration
- [ ] Customize styling to match role theme
- [ ] Add role-specific features (moderation, analytics, etc.)

### Testing:
- [ ] Test blog loading in each role
- [ ] Test authentication flows
- [ ] Test create/edit functionality where applicable
- [ ] Test responsive design on mobile
- [ ] Test error handling (network issues, API failures)

### Deployment:
- [ ] Update navigation across all existing pages
- [ ] Deploy blog integration script to shared location
- [ ] Configure API endpoints and authentication
- [ ] Train staff on new community features
- [ ] Create user documentation

This comprehensive integration makes the community blog accessible to all user roles with appropriate features and permissions for each role type.
