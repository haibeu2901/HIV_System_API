# Blog Integration Guide for All User Roles

This guide shows how to integrate the community blog system into different user views across the HIV System. **The blog is now accessible from every view with visible navigation options!**

## 🎯 BLOG NAVIGATION - ALL ROLES NOW HAVE VISIBLE ACCESS!

### ✅ **PATIENT/USER NAVIGATION OPTIONS**
1. **Header Dropdown**: "Blog & Community" link in user header navigation
2. **Appointment View**: Dedicated "Access Blog & Community" button on appointments page  
3. **Booking Page**: "Visit Blog & Community" button on appointment booking page
4. **Direct Access**: All links lead to `patient-community.html`

### ✅ **DOCTOR NAVIGATION OPTIONS**  
1. **Header Dropdown**: "Blog & Community" link with automatic role routing
2. **Dashboard Quick Actions**: Prominent "Blog & Cộng đồng" button in doctor dashboard
3. **Smart Routing**: Header automatically routes to `doctor-dashboard-with-blog.html`

### ✅ **STAFF NAVIGATION OPTIONS**
1. **Header Dropdown**: "Blog & Community" link with automatic role routing  
2. **Dashboard Card**: Interactive "Blog & Cộng đồng" card in staff dashboard (clickable)
3. **Smart Routing**: Header automatically routes to `staff-community.html`

### ✅ **ADMIN NAVIGATION OPTIONS**
1. **Header Dropdown**: "Blog & Community" link with automatic role routing
2. **Admin Navigation Menu**: "Blog Management" link in main admin navigation
3. **Smart Routing**: Header automatically routes to `blog-management.html`

### ✅ **MANAGER NAVIGATION OPTIONS**  
1. **Header Dropdown**: "Blog & Community" link with automatic role routing
2. **Manager Navigation Menu**: "Tổng quan Blog" link in main manager navigation
3. **Smart Routing**: Header automatically routes to `manager-blog-overview.html`

## 🌟 Blog Access Points by Role

### 1. **Public Users** (No Login Required)
- **Main Access**: `public-view/blog/blog-public.html`
- **Permissions**: Read published stories only
- **Features**: Browse, search, filter stories
- **Access Points**:
  - Landing page testimonials section
  - Public blog page
  - Community stories showcase

### 2. **Patients/Users** (Authenticated)
- **Main Access**: `private-view/user-view/community/patient-community.html`
- **Permissions**: Read, create, share stories
- **Features**: Full community experience
- **Access Points**:
  - User dashboard community widget
  - Dedicated community page
  - Profile page story section
  - Medical record page encouragement stories

### 3. **Doctors** (Authenticated)
- **Main Access**: `private-view/doctor-view/doctor-dashboard/doctor-dashboard-with-blog.html`
- **Permissions**: Read, create professional insights
- **Features**: Patient stories overview, professional announcements
- **Access Points**:
  - Doctor dashboard sidebar (compact view)
  - Doctor dashboard community tab (full view)
  - Patient management page insights
  - Medical consultation support stories

### 4. **Staff/Nurses** (Authenticated)
- **Main Access**: `private-view/staff-view/staff-community.html`
- **Permissions**: Read, moderate, create announcements
- **Features**: Patient support stories, staff announcements
- **Access Points**:
  - Staff dashboard community section
  - Patient care support stories
  - Appointment management encouragement
  - Staff collaboration announcements

### 5. **Managers** (Authenticated)
- **Main Access**: `private-view/manager-view/manager-blog-overview.html`
- **Permissions**: Read, create announcements, basic moderation
- **Features**: Overview statistics, announcements
- **Access Points**:
  - Manager dashboard overview
  - Community engagement metrics
  - Announcement creation
  - Patient satisfaction stories

### 6. **Administrators** (Full Access)
- **Main Access**: `private-view/admin-view/blog-management.html`
- **Permissions**: Full moderation, management, analytics
- **Features**: Complete blog management system
- **Access Points**:
  - Admin dashboard main blog management
  - Moderation panel
  - Analytics and reporting
  - User management integration

## Overview

The blog system can be integrated into any page with different configurations based on user roles:

- **Public View**: Read-only access to published stories
- **Patient/User View**: Full access - read, create, and share stories
- **Doctor View**: Read access + professional insights and creation
- **Staff View**: Read access + moderation capabilities
- **Admin/Manager View**: Full moderation and management

## 📍 Complete Blog Access Map

### **PUBLIC ACCESS**
- **File**: `public-view/blog/blog-public.html`
- **URL**: `/blog/blog-public.html`
- **Features**: Browse published stories, search, filter
- **Authentication**: None required (guest access)

### **PATIENT/USER ACCESS**
- **Main Page**: `private-view/user-view/community/patient-community.html`
- **Dashboard Widget**: Add to any patient dashboard page
- **Navigation**: Patient menu → Community
- **Features**: Read, create, share stories, full community experience
- **Authentication**: Patient login required

### **DOCTOR ACCESS**
- **Main Page**: `private-view/doctor-view/doctor-dashboard/doctor-dashboard-with-blog.html`
- **Sidebar Widget**: Compact view in doctor dashboard
- **Community Tab**: Full view in doctor dashboard tabs
- **Features**: Read patient stories, create professional insights
- **Authentication**: Doctor login required

### **STAFF/NURSE ACCESS**
- **Main Page**: `private-view/staff-view/staff-community.html`
- **Dashboard Widget**: Add to staff dashboard
- **Features**: Read patient stories, create announcements, patient support resources
- **Authentication**: Staff login required

### **MANAGER ACCESS**
- **Main Page**: `private-view/manager-view/manager-blog-overview.html`
- **Dashboard**: Manager dashboard with analytics
- **Features**: Community overview, engagement metrics, create announcements
- **Authentication**: Manager login required

### **ADMIN ACCESS**
- **Main Page**: `private-view/admin-view/blog-management.html`
- **Management Panel**: Full moderation interface
- **Features**: Complete blog management, moderation, analytics, user management
- **Authentication**: Admin login required

## 🚀 Quick Access URLs by Role

| Role | Primary URL | Dashboard Widget | Navigation Path |
|------|-------------|------------------|----------------|
| **Public** | `/blog/blog-public.html` | N/A | Landing Page → Community Stories |
| **Patient** | `/user-view/community/patient-community.html` | Dashboard → Community Widget | Menu → Community |
| **Doctor** | `/doctor-view/doctor-dashboard/doctor-dashboard-with-blog.html` | Sidebar + Community Tab | Dashboard → Community Tab |
| **Staff** | `/staff-view/staff-community.html` | Dashboard Widget | Menu → Community |
| **Manager** | `/manager-view/manager-blog-overview.html` | Analytics Dashboard | Menu → Community Overview |
| **Admin** | `/admin-view/blog-management.html` | Management Panel | Admin Panel → Blog Management |

## 🔧 Integration Examples

### Add to Existing Dashboard (Any Role)
```html
<!-- Add this to any existing dashboard -->
<div class="dashboard-section">
    <h3><i class="fas fa-comments"></i> Community Stories</h3>
    <div id="blog-widget"></div>
</div>

<script src="../../shared/blog-integration.js"></script>
<script>
initializeBlogIntegration('blog-widget', {
    compact: true,
    maxPosts: 3,
    userRole: 'your-role', // patient, doctor, staff, manager, admin
    authToken: userToken
});
</script>
```

### Add to Navigation Menu
```html
<!-- Add to role-specific navigation -->
<nav class="role-nav">
    <!-- Existing menu items -->
    <a href="[role-blog-page]"><i class="fas fa-comments"></i> Community</a>
</nav>
```

### 1. Include the Blog Integration Script

```html
<script src="../../shared/blog-integration.js"></script>
```

### 2. Add a Container Element

```html
<div id="your-blog-container"></div>
```

### 3. Initialize the Blog Integration

```javascript
document.addEventListener('DOMContentLoaded', function() {
    // Get user's auth token
    const userToken = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    
    // Initialize blog integration
    initializeBlogIntegration('your-blog-container', {
        compact: false,              // true for sidebar/compact view
        maxPosts: null,             // limit number of posts (null = no limit)
        showCreateButton: true,     // allow story creation
        showFilters: true,          // show filter tabs
        showStats: true,           // show community stats
        userRole: 'patient',       // user role for customization
        authToken: userToken       // authentication token
    });
});
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `compact` | boolean | false | Use compact view for sidebars |
| `maxPosts` | number/null | null | Limit number of posts displayed |
| `showCreateButton` | boolean | true | Show "Share Story" button |
| `showFilters` | boolean | true | Show filter tabs (Hot, New, etc.) |
| `showStats` | boolean | true | Show community statistics |
| `userRole` | string | 'public' | User role for customization |
| `authToken` | string | null | Authentication token for API calls |

## Role-Specific Examples

### 1. Patient/User Views
```javascript
initializeBlogIntegration('community-stories', {
    compact: false,
    showCreateButton: true,
    showFilters: true,
    showStats: true,
    userRole: 'patient',
    authToken: patientToken
});
```

### 2. Doctor Dashboard Sidebar
```javascript
initializeBlogIntegration('doctor-blog-sidebar', {
    compact: true,
    maxPosts: 3,
    showCreateButton: false,
    showFilters: false,
    showStats: false,
    userRole: 'doctor',
    authToken: doctorToken
});
```

### 3. Admin Moderation Panel
```javascript
initializeBlogIntegration('admin-blog-moderation', {
    compact: false,
    showCreateButton: true,
    showFilters: true,
    showStats: true,
    userRole: 'admin',
    authToken: adminToken
});
```

### 4. Public Landing Page
```javascript
initializeBlogIntegration('public-testimonials', {
    compact: true,
    maxPosts: 5,
    showCreateButton: false,
    showFilters: false,
    showStats: false,
    userRole: 'public',
    authToken: null
});
```

## File Structure

```
private-view/
├── shared/
│   └── blog-integration.js          # Main blog integration script
├── doctor-view/
│   └── doctor-dashboard/
│       └── doctor-dashboard-with-blog.html  # Example doctor integration
├── user-view/
│   └── community/
│       └── patient-community.html   # Example patient integration
├── admin-view/
│   └── blog-moderation.html        # Admin moderation interface
└── staff-view/
    └── staff-blog-view.html        # Staff blog interface
```

## API Integration

The blog system automatically connects to:
- `GET /api/SocialBlog/GetAllBlog` - Load all blog posts
- `POST /api/SocialBlog/CreateBlog` - Create new blog post

Request body for creating blogs:
```json
{
    "title": "string",
    "content": "string",
    "isPublic": true
}
```

## Authentication

The system uses Bearer token authentication. Make sure to:

1. Store user tokens in localStorage or sessionStorage
2. Pass the token when initializing the blog integration
3. Handle authentication errors gracefully

## Styling

The blog integration includes comprehensive CSS styling that adapts to:
- Light/dark themes
- Responsive breakpoints
- Compact vs full views
- Role-specific color schemes

## Examples in This Project

1. **Doctor Dashboard**: `doctor-dashboard-with-blog.html`
   - Shows compact view in sidebar
   - Full view in dedicated community tab
   - Doctor-specific color scheme

2. **Patient Community**: `patient-community.html`
   - Full community experience
   - Support guidelines
   - Resource links

3. **Public Blog**: `blog-public.html`
   - Public access to stories
   - Authentication for creation
   - Full-featured blog system

## Next Steps

1. Choose the appropriate configuration for each user role
2. Integrate the blog system into existing dashboards
3. Customize styling to match your design system
4. Test authentication flows for different user types
5. Add role-specific features (moderation, reporting, etc.)

## Support

For questions or issues with the blog integration:
- Check browser console for errors
- Verify API endpoints are accessible
- Ensure authentication tokens are valid
- Review network requests in browser dev tools
