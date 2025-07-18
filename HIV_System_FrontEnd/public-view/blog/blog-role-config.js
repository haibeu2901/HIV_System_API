// Role-Based Blog System Configuration
(function(window) {
    'use strict';

    // Blog-specific roles and permissions
    const BLOG_ROLES = {
        PATIENT: 'patient',
        DOCTOR: 'doctor', 
        STAFF: 'staff',
        ADMIN: 'admin',
        MANAGER: 'manager',
        GUEST: 'guest'
    };

    const BLOG_STATUS = {
        DRAFT: 'draft',
        PENDING: 'pending',
        PUBLISHED: 'published',
        REJECTED: 'rejected',
        ARCHIVED: 'archived'
    };

    const PERMISSIONS = {
        // Basic permissions
        VIEW_BLOGS: 'view_blogs',
        CREATE_BLOG: 'create_blog',
        UPDATE_BLOG: 'update_blog',
        DELETE_BLOG: 'delete_blog',
        
        // Interaction permissions
        LIKE_BLOG: 'like_blog',
        COMMENT_BLOG: 'comment_blog',
        SHARE_BLOG: 'share_blog',
        
        // Moderation permissions
        VERIFY_BLOG: 'verify_blog',
        APPROVE_BLOG: 'approve_blog',
        REJECT_BLOG: 'reject_blog',
        
        // Management permissions
        MANAGE_ALL_BLOGS: 'manage_all_blogs',
        VIEW_ANALYTICS: 'view_analytics',
        MANAGE_USERS: 'manage_users'
    };

    // Role permission mappings
    const ROLE_PERMISSIONS = {
        [BLOG_ROLES.GUEST]: [
            PERMISSIONS.VIEW_BLOGS
        ],
        [BLOG_ROLES.PATIENT]: [
            PERMISSIONS.VIEW_BLOGS,
            PERMISSIONS.CREATE_BLOG,
            PERMISSIONS.UPDATE_BLOG,
            PERMISSIONS.LIKE_BLOG,
            PERMISSIONS.COMMENT_BLOG,
            PERMISSIONS.SHARE_BLOG
        ],
        [BLOG_ROLES.DOCTOR]: [
            PERMISSIONS.VIEW_BLOGS,
            PERMISSIONS.CREATE_BLOG,
            PERMISSIONS.UPDATE_BLOG,
            PERMISSIONS.LIKE_BLOG,
            PERMISSIONS.COMMENT_BLOG,
            PERMISSIONS.SHARE_BLOG,
            PERMISSIONS.VERIFY_BLOG,
            PERMISSIONS.APPROVE_BLOG,
            PERMISSIONS.REJECT_BLOG
        ],
        [BLOG_ROLES.STAFF]: [
            PERMISSIONS.VIEW_BLOGS,
            PERMISSIONS.CREATE_BLOG,
            PERMISSIONS.UPDATE_BLOG,
            PERMISSIONS.LIKE_BLOG,
            PERMISSIONS.COMMENT_BLOG,
            PERMISSIONS.SHARE_BLOG,
            PERMISSIONS.VERIFY_BLOG,
            PERMISSIONS.APPROVE_BLOG,
            PERMISSIONS.REJECT_BLOG
        ],
        [BLOG_ROLES.ADMIN]: [
            PERMISSIONS.VIEW_BLOGS,
            PERMISSIONS.CREATE_BLOG,
            PERMISSIONS.UPDATE_BLOG,
            PERMISSIONS.DELETE_BLOG,
            PERMISSIONS.LIKE_BLOG,
            PERMISSIONS.COMMENT_BLOG,
            PERMISSIONS.SHARE_BLOG,
            PERMISSIONS.VERIFY_BLOG,
            PERMISSIONS.APPROVE_BLOG,
            PERMISSIONS.REJECT_BLOG,
            PERMISSIONS.MANAGE_ALL_BLOGS,
            PERMISSIONS.VIEW_ANALYTICS,
            PERMISSIONS.MANAGE_USERS
        ],
        [BLOG_ROLES.MANAGER]: [
            PERMISSIONS.VIEW_BLOGS,
            PERMISSIONS.CREATE_BLOG,
            PERMISSIONS.UPDATE_BLOG,
            PERMISSIONS.DELETE_BLOG,
            PERMISSIONS.LIKE_BLOG,
            PERMISSIONS.COMMENT_BLOG,
            PERMISSIONS.SHARE_BLOG,
            PERMISSIONS.VERIFY_BLOG,
            PERMISSIONS.APPROVE_BLOG,
            PERMISSIONS.REJECT_BLOG,
            PERMISSIONS.MANAGE_ALL_BLOGS,
            PERMISSIONS.VIEW_ANALYTICS,
            PERMISSIONS.MANAGE_USERS
        ]
    };

    // BlogUser class
    class BlogUser {
        constructor(userData = null) {
            if (userData) {
                this.id = userData.id || userData.userId;
                this.userId = userData.userId || userData.id;
                this.role = userData.role || BLOG_ROLES.GUEST;
                this.token = userData.token;
                this.username = userData.username;
                this.email = userData.email;
                this.userData = userData;
            } else {
                this.id = null;
                this.userId = null;
                this.role = BLOG_ROLES.GUEST;
                this.token = null;
                this.username = null;
                this.email = null;
                this.userData = null;
            }
        }

        // Check if user has a specific permission
        hasPermission(permission) {
            const rolePermissions = ROLE_PERMISSIONS[this.role] || [];
            return rolePermissions.includes(permission);
        }

        // Get user's role display name
        getRoleDisplayName() {
            const roleNames = {
                [BLOG_ROLES.PATIENT]: 'Patient',
                [BLOG_ROLES.DOCTOR]: 'Doctor',
                [BLOG_ROLES.STAFF]: 'Staff',
                [BLOG_ROLES.ADMIN]: 'Administrator', 
                [BLOG_ROLES.MANAGER]: 'Manager',
                [BLOG_ROLES.GUEST]: 'Guest'
            };
            return roleNames[this.role] || 'Unknown';
        }

        // Get blog statuses visible to this role
        getVisibleBlogStatuses() {
            switch (this.role) {
                case BLOG_ROLES.ADMIN:
                case BLOG_ROLES.MANAGER:
                    return [BLOG_STATUS.PUBLISHED, BLOG_STATUS.PENDING, BLOG_STATUS.REJECTED, BLOG_STATUS.DRAFT];
                case BLOG_ROLES.DOCTOR:
                case BLOG_ROLES.STAFF:
                    return [BLOG_STATUS.PUBLISHED, BLOG_STATUS.PENDING];
                case BLOG_ROLES.PATIENT:
                default:
                    return [BLOG_STATUS.PUBLISHED];
            }
        }

        // Get available actions for a specific blog
        getAvailableActions(blog) {
            const actions = [];

            // Like/dislike actions
            if (this.hasPermission(PERMISSIONS.LIKE_BLOG)) {
                actions.push('like', 'dislike');
            }

            // Comment action
            if (this.hasPermission(PERMISSIONS.COMMENT_BLOG)) {
                actions.push('comment');
            }

            // Share action
            if (this.hasPermission(PERMISSIONS.SHARE_BLOG)) {
                actions.push('share');
            }

            // Edit/delete actions (own posts or admin)
            if (this.canEditBlog(blog)) {
                actions.push('edit');
            }

            if (this.canDeleteBlog(blog)) {
                actions.push('delete');
            }

            // Moderation actions
            if (this.hasPermission(PERMISSIONS.APPROVE_BLOG) && blog.blogStatus === BLOG_STATUS.PENDING) {
                actions.push('approve');
            }

            if (this.hasPermission(PERMISSIONS.REJECT_BLOG) && blog.blogStatus === BLOG_STATUS.PENDING) {
                actions.push('reject');
            }

            return actions;
        }

        // Check if user can edit a specific blog
        canEditBlog(blog) {
            // Own blog or admin/manager
            return (blog.authorId === this.userId && this.hasPermission(PERMISSIONS.UPDATE_BLOG)) ||
                   this.hasPermission(PERMISSIONS.MANAGE_ALL_BLOGS);
        }

        // Check if user can delete a specific blog
        canDeleteBlog(blog) {
            // Own blog (patients can't delete published posts) or admin/manager
            if (this.hasPermission(PERMISSIONS.MANAGE_ALL_BLOGS)) {
                return true;
            }
            return blog.authorId === this.userId && 
                   this.hasPermission(PERMISSIONS.DELETE_BLOG) &&
                   blog.blogStatus !== BLOG_STATUS.PUBLISHED;
        }

        // Check if user can react to blogs
        canReact() {
            return this.hasPermission(PERMISSIONS.LIKE_BLOG);
        }

        // Check if user can comment
        canComment() {
            return this.hasPermission(PERMISSIONS.COMMENT_BLOG);
        }
    }

    // Utility functions
    function getBlogStatusBadge(status) {
        const badges = {
            [BLOG_STATUS.PUBLISHED]: '<span class="status-badge published"><i class="fas fa-check-circle"></i> Published</span>',
            [BLOG_STATUS.PENDING]: '<span class="status-badge pending"><i class="fas fa-clock"></i> Pending Review</span>',
            [BLOG_STATUS.REJECTED]: '<span class="status-badge rejected"><i class="fas fa-times-circle"></i> Rejected</span>',
            [BLOG_STATUS.DRAFT]: '<span class="status-badge draft"><i class="fas fa-edit"></i> Draft</span>',
            [BLOG_STATUS.ARCHIVED]: '<span class="status-badge archived"><i class="fas fa-archive"></i> Archived</span>'
        };
        return badges[status] || '';
    }

    function getRoleColor(role) {
        const colors = {
            [BLOG_ROLES.ADMIN]: '#e74c3c',
            [BLOG_ROLES.MANAGER]: '#e67e22',
            [BLOG_ROLES.DOCTOR]: '#3498db',
            [BLOG_ROLES.STAFF]: '#9b59b6',
            [BLOG_ROLES.PATIENT]: '#27ae60',
            [BLOG_ROLES.GUEST]: '#95a5a6'
        };
        return colors[role] || '#95a5a6';
    }

    // Export to window.BlogRoles
    window.BlogRoles = {
        BLOG_ROLES,
        BLOG_STATUS,
        PERMISSIONS,
        ROLE_PERMISSIONS,
        BlogUser,
        getBlogStatusBadge,
        getRoleColor
    };

})(window);
