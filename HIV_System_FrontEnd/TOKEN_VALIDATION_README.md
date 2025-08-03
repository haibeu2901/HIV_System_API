# Enhanced Token Validation System

This system provides **zero-tolerance** token validation and immediate redirection for the HIV System Frontend application.

## 🛡️ Security Policy

**Absolute Rule:** No access to private pages without a valid, verified token. Any authentication failure results in immediate logout and redirect.

## 🔐 Enhanced Security Features

- **Immediate Security Check**: Script runs before any other code and blocks access if no token exists
- **Zero-Tolerance API Validation**: Any API failure (network error, invalid token, etc.) triggers immediate logout
- **Multi-layer Protection**: Multiple validation points throughout the user session
- **Continuous Monitoring**: Periodic validation and event-based checks
- **Secure-by-Default**: Network errors result in logout (no assumptions about token validity)

## Implementation

### For Private Pages

Add this script tag to the `<head>` section or before other scripts in any private page:

```html
<!-- Token validation guard -->
<script src="../../shared/token-guard.js"></script>
```

**Note**: Adjust the path based on your page's location relative to the `private-view/shared/` directory.

### For the Landing Page

The landing page automatically checks for valid tokens and redirects authenticated users to their appropriate dashboard.

### Files Overview

1. **`token-guard.js`** - Main validation script for private pages
2. **`auth-utils.js`** - Utility class with authentication helper functions
3. **`landingpage-script.js`** - Updated with token validation for the public landing page

## How It Works

### Enhanced Security Flow
1. **Immediate Barrier**: Before any page content loads, check if token exists
   - ❌ No token → Immediate redirect (no exceptions)
   - ✅ Token exists → Proceed to validation

2. **Strict API Validation**: Test token against backend with zero tolerance
   - ✅ Valid response → Access granted
   - ❌ Any failure (401, 403, network error, etc.) → Immediate logout

3. **Continuous Security**: Multiple validation triggers
   - Periodic checks (every 2 minutes)
   - Page focus events
   - Random interaction sampling (10% of clicks)

4. **Secure Cleanup**: On any failure
   - Clear all authentication data
   - Show appropriate message
   - Redirect to landing page

### Zero-Tolerance Policy
Unlike traditional "lenient" approaches, this system:
- **Never assumes** token validity on network errors
- **Always redirects** on any authentication uncertainty
- **Prioritizes security** over user convenience

## Role-based Redirection

Users are redirected to appropriate dashboards based on their role:

- **Role 1 (Admin)**: `/private-view/admin-view/admin-home/admin-home.html`
- **Role 2 (Doctor)**: `/private-view/doctor-view/doctor-dashboard/doctor-dashboard.html`
- **Role 3 (Patient)**: `/private-view/user-view/booking/appointment-booking.html`
- **Role 4 (Staff)**: `/private-view/staff-view/staff-dashboard/staff-dashboard.html`
- **Role 5 (Manager)**: `/private-view/manager-view/manager-home/manager-home.html`

## Error Handling

- **Network Errors**: Does not redirect on network failures to avoid unnecessary logouts
- **API Errors**: Only redirects on 401 (Unauthorized) or 403 (Forbidden) responses
- **Invalid Roles**: Clears token and prompts re-authentication

## Security Features

- **Token Expiry Handling**: Automatically detects and handles expired tokens
- **Data Cleanup**: Removes all authentication data when token is invalid
- **Multi-tab Support**: Validates tokens across browser tabs
- **Session Monitoring**: Continuous validation prevents stale sessions

## Usage Examples

### Adding to a New Private Page

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>My Private Page</title>
    <!-- Add token guard as first script -->
    <script src="../shared/token-guard.js"></script>
</head>
<body>
    <!-- Your page content -->
    
    <!-- Other scripts -->
    <script src="your-page-script.js"></script>
</body>
</html>
```

### Using Auth Utils in JavaScript

```javascript
// Check if user is authenticated
const isValid = await AuthUtils.validateToken();

// Redirect to login manually
AuthUtils.redirectToLogin('Please log in to continue');

// Clear authentication data
AuthUtils.clearAuthData();

// Get user information
const userRole = AuthUtils.getUserRole();
const userId = AuthUtils.getUserId();
const token = AuthUtils.getToken();
```

## Configuration

Update the configuration in `token-guard.js` if needed:

```javascript
const CONFIG = {
    API_BASE_URL: 'https://localhost:7009',
    LOGIN_PAGE: '/public-view/landingpage.html'
};
```

## Testing

To test the token validation:

1. **Valid Token Test**: Login normally and navigate to any private page
2. **Invalid Token Test**: Manually edit the token in localStorage to an invalid value
3. **Expired Token Test**: Wait for token to expire naturally
4. **No Token Test**: Clear localStorage and try to access a private page

The system should redirect to the landing page in all invalid token scenarios.

## Maintenance

- **Regular Updates**: Keep the API endpoint updated if it changes
- **Path Updates**: Update redirect paths if page structure changes
- **Role Updates**: Add new roles to the redirection logic as needed

## Troubleshooting

### Common Issues

1. **Incorrect Paths**: Ensure the script path is correct relative to your page
2. **CORS Errors**: Make sure the API allows cross-origin requests
3. **Network Issues**: Check that the API endpoint is accessible
4. **Role Conflicts**: Verify user roles match the expected values (1-5)

### Debug Mode

Add this to enable debug logging:

```javascript
// Add to any page's script section
localStorage.setItem('authDebug', 'true');
```

This will enable console logging for authentication events.
