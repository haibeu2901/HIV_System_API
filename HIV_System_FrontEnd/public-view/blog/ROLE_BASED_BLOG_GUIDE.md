# Role-Based HIV Support Blog System

## Overview
This blog system implements role-based access control where different user types have different permissions for creating, viewing, and managing blog posts.

## User Roles and Permissions

### 🔓 Public (Guest Users)
- **Permissions**: View published blog posts only
- **Actions**: Read stories, share content
- **Restrictions**: Cannot create, comment, or vote on posts

### 👤 Patient
- **Permissions**: Full blog interaction
- **Actions**: 
  - Create blog posts (requires approval)
  - Like/dislike posts
  - Comment on posts
  - Edit/delete own posts
- **Notes**: New posts go to "Pending" status for review

### 👩‍⚕️ Doctor
- **Permissions**: Patient permissions + verification abilities
- **Actions**:
  - All patient actions
  - Approve/reject pending posts
  - Verify blog content for medical accuracy
  - Moderate comments
- **Special**: Can see pending posts for review

### 👥 Staff
- **Permissions**: Same as Doctor
- **Actions**:
  - All doctor actions
  - Support blog management
  - Help with content moderation

### 🛡️ Admin
- **Permissions**: Full system control
- **Actions**:
  - All previous permissions
  - CRUD operations on any blog post
  - View analytics and statistics
  - Manage all users' content
  - Direct publish (bypasses approval)

### 👔 Manager
- **Permissions**: Same as Admin
- **Actions**:
  - Full administrative control
  - Strategic oversight of blog content
  - Analytics and reporting

## How to Use the System

### 1. Authentication
1. Open the blog page
2. In the authentication section at the top:
   - Enter your User ID (number)
   - Select your role from the dropdown
   - Click "Sign In"

### 2. Role-Specific Features

#### For Patients:
- Click "Create Story" to write a new blog post
- Your posts will show as "Pending Review" until approved
- You can like and comment on published posts
- Edit or delete your own posts

#### For Doctors/Staff:
- Access "Review Posts" to see pending submissions
- Use Approve/Reject buttons on pending posts
- Moderate inappropriate comments
- All patient features available

#### For Admins/Managers:
- Access "Manage All" for complete blog oversight
- View "Analytics" for system statistics
- Direct publish posts without approval
- Edit/delete any user's content

### 3. Blog Statuses
- **Draft**: Work in progress (not visible to others)
- **Pending**: Awaiting review by doctors/staff
- **Published**: Approved and visible to all users
- **Rejected**: Not approved for publication
- **Archived**: Removed from active display

## API Integration

The system expects these API endpoints:

### Blog Operations
- `GET /api/SocialBlog/GetAllBlog` - Fetch all blogs
- `POST /api/SocialBlog/CreateBlog` - Create new blog
- `PUT /api/SocialBlog/UpdateBlog/{id}` - Update existing blog
- `DELETE /api/SocialBlog/DeleteBlog/{id}` - Delete blog

### Moderation
- `POST /api/SocialBlog/ApproveBlog/{id}` - Approve pending blog
- `POST /api/SocialBlog/RejectBlog/{id}` - Reject pending blog

### Interactions
- `POST /api/SocialBlog/ReactToBlog` - Like/dislike blog
- `POST /api/SocialBlog/AddComment` - Add comment to blog

## Security Features

1. **Role-based permissions**: Each action is checked against user role
2. **Token authentication**: API calls include authorization headers
3. **Input validation**: All user inputs are validated
4. **Anonymous posting**: Option to post without revealing identity
5. **Content moderation**: Multi-level approval process

## Testing the System

### Test Users (Examples)
- **Patient**: ID 1, Role: patient
- **Doctor**: ID 2, Role: doctor  
- **Admin**: ID 3, Role: admin

### Test Scenarios
1. **Patient Flow**:
   - Login as patient
   - Create a blog post
   - Verify it shows as "Pending"
   - Try to edit/delete own post

2. **Doctor Flow**:
   - Login as doctor
   - Review pending posts
   - Approve/reject posts
   - Verify status changes

3. **Admin Flow**:
   - Login as admin
   - View all posts regardless of status
   - Edit any post
   - View analytics

## Customization

### Adding New Roles
1. Update `ROLE_TYPES` in `role-config.js`
2. Define permissions in `ROLE_PERMISSIONS`
3. Update UI to show new role options

### Modifying Permissions
1. Edit `ROLE_PERMISSIONS` object in `role-config.js`
2. Update permission checks in blog functions
3. Test all affected functionality

## Troubleshooting

### Common Issues
1. **Authentication fails**: Check User ID and role selection
2. **Posts not showing**: Verify role permissions for blog status
3. **Actions disabled**: Confirm user has required permissions
4. **API errors**: Check network connectivity and API endpoints

### Debug Mode
- Check browser console for detailed error messages
- Verify localStorage for stored user data
- Monitor network tab for API call responses

## Support

For technical support or questions about the role-based blog system:
1. Check browser console for error messages
2. Verify user authentication status
3. Confirm API endpoint availability
4. Review role permissions in role-config.js
