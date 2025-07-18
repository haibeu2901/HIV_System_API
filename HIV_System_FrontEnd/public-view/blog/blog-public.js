// Role-Based HIV Support Blog System
// Global variables
let allBlogs = [];
let filteredBlogs = [];
let currentFilter = 'all';
let isLoading = false;

// Global authentication state using the role system
let currentUser = new window.BlogRoles.BlogUser();
let isAuthenticated = false;

// API Base URL - using same URL as your Swagger
const API_BASE_URL = 'https://localhost:7009/api';

// Initialize the blog system
document.addEventListener('DOMContentLoaded', async () => {
    // Check for stored authentication
    await checkStoredAuthentication();
    
    // Initialize UI
    await loadBlogs();
    setupEventListeners();
    updateStatistics();
    updateUIForRole();
    
    // Set up character counters for blog creation
    setupCharacterCounters();
});

// Authentication functions
async function checkStoredAuthentication() {
    const storedUserData = localStorage.getItem('userData');
    if (storedUserData) {
        try {
            const userData = JSON.parse(storedUserData);
            currentUser = new window.BlogRoles.BlogUser(userData);
            isAuthenticated = true;
            updateUIForRole();
        } catch (error) {
            console.error('Error parsing stored user data:', error);
            localStorage.removeItem('userData');
        }
    }
}

async function authenticateUser() {
    const userId = document.getElementById('userIdInput').value;
    const role = document.getElementById('userRoleSelect').value;
    
    if (!userId || !role) {
        alert('Please enter both User ID and select a role');
        return;
    }
    
    try {
        // In a real application, this would be an API call to authenticate
        // For now, we'll simulate authentication
        const userData = {
            userId: parseInt(userId),
            role: role,
            token: `mock_token_${userId}_${role}`,
            username: `user${userId}`,
            email: `user${userId}@example.com`
        };
        
        // Store user data
        localStorage.setItem('userData', JSON.stringify(userData));
        
        // Update current user
        currentUser = new window.BlogRoles.BlogUser(userData);
        isAuthenticated = true;
        
        // Update UI
        updateUIForRole();
        
        // Reload blogs with new permissions
        await loadBlogs();
        
        alert(`Successfully authenticated as ${currentUser.getRoleDisplayName()}`);
        
    } catch (error) {
        console.error('Authentication error:', error);
        alert('Authentication failed. Please try again.');
    }
}

function logoutUser() {
    // Clear stored data
    localStorage.removeItem('userData');
    
    // Reset user state
    currentUser = new window.BlogRoles.BlogUser();
    isAuthenticated = false;
    
    // Update UI
    updateUIForRole();
    
    // Reload blogs
    loadBlogs();
    
    alert('Successfully logged out');
}

function updateUIForRole() {
    const guestSection = document.getElementById('guestSection');
    const authenticatedSection = document.getElementById('authenticatedSection');
    const userName = document.getElementById('userName');
    const userRole = document.getElementById('userRole');
    const roleActions = document.getElementById('roleActions');
    
    if (isAuthenticated) {
        if (guestSection) guestSection.style.display = 'none';
        if (authenticatedSection) authenticatedSection.style.display = 'block';
        
        if (userName) {
            userName.textContent = currentUser.userData.username || `User ${currentUser.userId}`;
        }
        if (userRole) {
            userRole.innerHTML = `<span class="user-role-badge ${currentUser.role}">${currentUser.getRoleDisplayName()}</span>`;
        }
        
        // Update role-specific actions
        updateRoleActions();
        
    } else {
        if (guestSection) guestSection.style.display = 'block';
        if (authenticatedSection) authenticatedSection.style.display = 'none';
    }
    
    // Update create post button visibility
    const createPostPrompt = document.querySelector('.create-post-prompt');
    if (createPostPrompt) {
        if (currentUser.hasPermission(window.BlogRoles.PERMISSIONS.CREATE_BLOG)) {
            createPostPrompt.style.display = 'flex';
        } else {
            createPostPrompt.style.display = 'none';
        }
    }
}

function updateRoleActions() {
    const roleActions = document.getElementById('roleActions');
    if (!roleActions) return; // Exit if element doesn't exist
    
    let actionsHTML = '';
    
    if (currentUser.hasPermission(window.BlogRoles.PERMISSIONS.CREATE_BLOG)) {
        actionsHTML += `
            <a href="#" class="role-action-btn" onclick="showCreatePostModal()">
                <i class="fas fa-pen"></i> Create Story
            </a>
        `;
    }
    
    if (currentUser.hasPermission(window.BlogRoles.PERMISSIONS.VERIFY_BLOG)) {
        actionsHTML += `
            <a href="#" class="role-action-btn" onclick="showPendingBlogs()">
                <i class="fas fa-clipboard-check"></i> Review Posts
            </a>
        `;
    }
    
    if (currentUser.hasPermission(window.BlogRoles.PERMISSIONS.MANAGE_ALL_BLOGS)) {
        actionsHTML += `
            <a href="#" class="role-action-btn" onclick="showBlogManagement()">
                <i class="fas fa-cogs"></i> Manage All
            </a>
        `;
    }
    
    if (currentUser.hasPermission(window.BlogRoles.PERMISSIONS.VIEW_ANALYTICS)) {
        actionsHTML += `
            <a href="#" class="role-action-btn" onclick="showAnalytics()">
                <i class="fas fa-chart-bar"></i> Analytics
            </a>
        `;
    }
    
    roleActions.innerHTML = actionsHTML;
}

// Load blogs from API with role-based filtering
async function loadBlogs() {
    const blogGrid = document.getElementById('blogGrid');
    const loadingContainer = document.getElementById('loadingContainer');
    
    // Check if required elements exist
    if (!blogGrid || !loadingContainer) {
        console.warn('Required blog elements not found in DOM');
        return;
    }
    
    try {
        isLoading = true;
        loadingContainer.style.display = 'flex';
        blogGrid.style.display = 'none';

        const headers = {
            'Content-Type': 'application/json'
        };
        
        // Add authorization header if authenticated
        if (isAuthenticated && currentUser.token) {
            headers['Authorization'] = `Bearer ${currentUser.token}`;
        }

        const response = await fetch(`${API_BASE_URL}/SocialBlog/GetAllBlog`, {
            method: 'GET',
            headers: headers
        });

        console.log('Response status:', response.status);
        console.log('Response headers:', response.headers);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('Raw API response:', data);
        
        // Process the blog data to match our expected structure
        allBlogs = data.map(blog => ({
            id: blog.id,
            authorId: blog.authorId,
            staffId: blog.staffId,
            title: blog.title,
            content: blog.content,
            publishedDate: blog.publishedDate,
            isAnonymous: blog.isAnonymous,
            notes: blog.notes,
            blogStatus: blog.blogStatus || 'published', // Default to published if no status
            blogReaction: blog.blogReaction || [],
            likesCount: blog.likesCount || 0,
            dislikesCount: blog.dislikesCount || 0,
            authorName: blog.authorName
        }));
        
        console.log('Processed blogs:', allBlogs);
        
        // Filter blogs based on user role permissions
        const visibleStatuses = currentUser.getVisibleBlogStatuses();
        console.log('Visible statuses for user role:', visibleStatuses);
        
        // For now, let's show all blogs regardless of status to see if they appear
        // allBlogs = allBlogs.filter(blog => visibleStatuses.includes(blog.blogStatus));
        
        filteredBlogs = [...allBlogs];
        
        console.log('Filtered blogs to display:', filteredBlogs.length);
        
        renderBlogs();
        
        loadingContainer.style.display = 'none';
        blogGrid.style.display = 'block';
        
    } catch (error) {
        console.error('Error loading blogs:', error);
        console.error('Error name:', error.name);
        console.error('Error message:', error.message);
        console.error('Error stack:', error.stack);
        
        // Different error messages based on error type
        let errorHTML = '';
        if (error.message.includes('Failed to fetch') || error.name === 'TypeError') {
            errorHTML = `
                <div class="error-container">
                    <div class="error-icon">
                        <i class="fas fa-server"></i>
                    </div>
                    <h3>Server Connection Issue</h3>
                    <p>Unable to connect to the blog server. The server might be offline or there could be a network issue.</p>
                    <div class="error-details">
                        <p><strong>Possible solutions:</strong></p>
                        <ul>
                            <li>Check if the API server is running on port 7009</li>
                            <li>Verify your internet connection</li>
                            <li>Contact the system administrator if the problem persists</li>
                        </ul>
                    </div>
                    <button class="btn-retry" onclick="window.location.reload()">
                        <i class="fas fa-refresh"></i> Try Again
                    </button>
                </div>
            `;
        } else {
            errorHTML = `
                <div class="error-container">
                    <div class="error-icon">
                        <i class="fas fa-exclamation-triangle"></i>
                    </div>
                    <h3>Stories Temporarily Unavailable</h3>
                    <p>We're currently unable to load the community stories. Please try again later or contact our support team.</p>
                    <button class="btn-retry" onclick="window.location.reload()">
                        <i class="fas fa-refresh"></i> Try Again
                    </button>
                    <a href="../landingpage.html#contact" class="btn-contact">
                        <i class="fas fa-phone"></i> Contact Support
                    </a>
                </div>
            `;
        }
        
        loadingContainer.innerHTML = errorHTML;
        
    } finally {
        isLoading = false;
    }
}

// Render blogs with role-based actions
function renderBlogs() {
    console.log('renderBlogs called with', filteredBlogs.length, 'blogs');
    const blogGrid = document.getElementById('blogGrid');
    
    if (!blogGrid) {
        console.error('blogGrid element not found!');
        return;
    }
    
    if (filteredBlogs.length === 0) {
        console.log('No blogs to display, showing empty state');
        blogGrid.innerHTML = `
            <div class="no-posts">
                <div class="no-posts-icon">
                    <i class="fas fa-comments"></i>
                </div>
                <h3>No stories yet</h3>
                <p>Be the first to share your story with the community!</p>
                ${currentUser.hasPermission(window.BlogRoles.PERMISSIONS.CREATE_BLOG) ? 
                    '<button class="cta-button" onclick="showCreatePostModal()"><i class="fas fa-pen"></i> Share Your Story</button>' :
                    '<a href="../landingpage.html#contact" class="cta-button"><i class="fas fa-phone"></i> Contact Us</a>'
                }
            </div>
        `;
        return;
    }

    console.log('Rendering', filteredBlogs.length, 'blogs');
    blogGrid.innerHTML = filteredBlogs.map(blog => {
        const availableActions = currentUser.getAvailableActions(blog);
        const adminActionsHTML = generateAdminActions(blog, availableActions);
        
        return `
            <div class="reddit-post">
                <div class="post-voting">
                    <button class="vote-btn upvote ${availableActions.includes('like') ? '' : 'disabled'}" 
                            onclick="handleVote(${blog.id}, true)" 
                            ${!availableActions.includes('like') ? 'disabled' : ''}>
                        <i class="fas fa-arrow-up"></i>
                    </button>
                    <span class="vote-count">${blog.likesCount || 0}</span>
                    <button class="vote-btn downvote ${availableActions.includes('dislike') ? '' : 'disabled'}" 
                            onclick="handleVote(${blog.id}, false)"
                            ${!availableActions.includes('dislike') ? 'disabled' : ''}>
                        <i class="fas fa-arrow-down"></i>
                    </button>
                </div>
                <div class="post-content">
                    <div class="post-header">
                        <div class="post-meta">
                            <span class="community">r/HIVSupport</span>
                            <span class="separator">•</span>
                            <span class="author">Posted by ${blog.isAnonymous ? 'Anonymous' : 'u/user' + blog.authorId}</span>
                            <span class="separator">•</span>
                            <span class="timestamp">${formatDate(blog.publishedDate)}</span>
                            ${blog.isAnonymous ? '<span class="anonymous-badge"><i class="fas fa-user-secret"></i> Anonymous</span>' : ''}
                            ${window.BlogRoles.getBlogStatusBadge(blog.blogStatus)}
                        </div>
                    </div>
                    <div class="post-body">
                        <h3 class="post-title" onclick="openBlogModal(${blog.id})">${blog.title}</h3>
                        <div class="post-text">
                            ${truncateText(blog.content, 300)}
                        </div>
                    </div>
                    <div class="post-footer">
                        <div class="post-actions">
                            <button class="action-btn" onclick="openBlogModal(${blog.id})">
                                <i class="fas fa-comment"></i>
                                <span>${blog.blogReaction ? blog.blogReaction.filter(r => r.comment).length : 0} Comments</span>
                            </button>
                            <button class="action-btn" onclick="handleShare(${blog.id})">
                                <i class="fas fa-share"></i>
                                <span>Share</span>
                            </button>
                            <button class="action-btn" onclick="openBlogModal(${blog.id})">
                                <i class="fas fa-expand"></i>
                                <span>Read More</span>
                            </button>
                        </div>
                        ${adminActionsHTML}
                    </div>
                </div>
            </div>
        `;
    }).join('');
    
    console.log('Blogs rendered successfully');
}

function generateAdminActions(blog, availableActions) {
    if (availableActions.length === 0) return '';
    
    let actionsHTML = '<div class="post-admin-actions">';
    
    if (availableActions.includes('edit')) {
        actionsHTML += `<button class="admin-action-btn edit" onclick="editBlog(${blog.id})">
            <i class="fas fa-edit"></i> Edit
        </button>`;
    }
    
    if (availableActions.includes('delete')) {
        actionsHTML += `<button class="admin-action-btn delete" onclick="deleteBlog(${blog.id})">
            <i class="fas fa-trash"></i> Delete
        </button>`;
    }
    
    if (availableActions.includes('approve')) {
        actionsHTML += `<button class="admin-action-btn approve" onclick="approveBlog(${blog.id})">
            <i class="fas fa-check"></i> Approve
        </button>`;
    }
    
    if (availableActions.includes('reject')) {
        actionsHTML += `<button class="admin-action-btn reject" onclick="rejectBlog(${blog.id})">
            <i class="fas fa-times"></i> Reject
        </button>`;
    }
    
    actionsHTML += '</div>';
    return actionsHTML;
}

// Role-based blog actions
async function editBlog(blogId) {
    const blog = allBlogs.find(b => b.id === blogId);
    if (!blog) return;

    if (!currentUser.canEditBlog(blog)) {
        alert('You do not have permission to edit this blog post.');
        return;
    }

    // Pre-fill the create modal with existing data for editing
    showCreatePostModal();
    
    // Set the form to edit mode
    document.getElementById('blogTitle').value = blog.title;
    document.getElementById('blogContent').value = blog.content;
    document.getElementById('blogNotes').value = blog.notes || '';
    document.getElementById('isAnonymous').checked = blog.isAnonymous;
    
    // Change button text and function
    const submitBtn = document.querySelector('.submit-btn');
    submitBtn.innerHTML = '<i class="fas fa-save"></i> Update Story';
    submitBtn.onclick = () => updateBlog(blogId);
    
    // Update modal title
    document.querySelector('#createPostModal .modal-title-area h2').textContent = 'Edit Your Story';
}

async function updateBlog(blogId) {
    if (!currentUser.hasPermission(window.BlogRoles.PERMISSIONS.UPDATE_BLOG)) {
        alert('You do not have permission to update blog posts.');
        return;
    }

    const title = document.getElementById('blogTitle').value.trim();
    const content = document.getElementById('blogContent').value.trim();
    const notes = document.getElementById('blogNotes').value.trim();
    const isAnonymous = document.getElementById('isAnonymous').checked;

    if (!title || !content) {
        showFormStatus('Please fill in all required fields.', 'error');
        return;
    }

    const submitBtn = document.querySelector('.submit-btn');
    submitBtn.disabled = true;
    submitBtn.classList.add('loading');

    try {
        const blogData = {
            id: blogId,
            title: title,
            content: content,
            notes: notes,
            isAnonymous: isAnonymous,
            authorId: currentUser.userId
        };

        const response = await fetch(`${API_BASE_URL}/SocialBlog/UpdateBlog/${blogId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${currentUser.token}`
            },
            body: JSON.stringify(blogData)
        });

        if (response.ok) {
            showFormStatus('Blog post updated successfully!', 'success');
            setTimeout(() => {
                closeCreatePostModal();
                loadBlogs();
            }, 2000);
        } else {
            const error = await response.text();
            showFormStatus('Failed to update blog post: ' + error, 'error');
        }
    } catch (error) {
        console.error('Error updating blog:', error);
        showFormStatus('Failed to update blog post. Please try again.', 'error');
    } finally {
        submitBtn.disabled = false;
        submitBtn.classList.remove('loading');
    }
}

async function deleteBlog(blogId) {
    const blog = allBlogs.find(b => b.id === blogId);
    if (!blog) return;

    if (!currentUser.canDeleteBlog(blog)) {
        alert('You do not have permission to delete this blog post.');
        return;
    }

    if (!confirm('Are you sure you want to delete this blog post? This action cannot be undone.')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/SocialBlog/DeleteBlog/${blogId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${currentUser.token}`
            }
        });

        if (response.ok) {
            alert('Blog post deleted successfully.');
            await loadBlogs();
        } else {
            const error = await response.text();
            alert('Failed to delete blog post: ' + error);
        }
    } catch (error) {
        console.error('Error deleting blog:', error);
        alert('Failed to delete blog post. Please try again.');
    }
}

async function approveBlog(blogId) {
    if (!currentUser.hasPermission(window.BlogRoles.PERMISSIONS.APPROVE_BLOG)) {
        alert('You do not have permission to approve blog posts.');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/SocialBlog/ApproveBlog/${blogId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${currentUser.token}`
            }
        });

        if (response.ok) {
            alert('Blog post approved successfully.');
            await loadBlogs();
        } else {
            const error = await response.text();
            alert('Failed to approve blog post: ' + error);
        }
    } catch (error) {
        console.error('Error approving blog:', error);
        alert('Failed to approve blog post. Please try again.');
    }
}

async function rejectBlog(blogId) {
    if (!currentUser.hasPermission(window.BlogRoles.PERMISSIONS.REJECT_BLOG)) {
        alert('You do not have permission to reject blog posts.');
        return;
    }

    const reason = prompt('Please provide a reason for rejection (optional):');
    
    try {
        const response = await fetch(`${API_BASE_URL}/SocialBlog/RejectBlog/${blogId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${currentUser.token}`
            },
            body: JSON.stringify({ reason: reason || 'No reason provided' })
        });

        if (response.ok) {
            alert('Blog post rejected successfully.');
            await loadBlogs();
        } else {
            const error = await response.text();
            alert('Failed to reject blog post: ' + error);
        }
    } catch (error) {
        console.error('Error rejecting blog:', error);
        alert('Failed to reject blog post. Please try again.');
    }
}

// Role-specific view functions
function showPendingBlogs() {
    currentFilter = 'pending';
    filteredBlogs = allBlogs.filter(blog => blog.blogStatus === window.BlogRoles.BLOG_STATUS.PENDING);
    renderBlogs();
    
    // Update filter tab
    document.querySelectorAll('.sort-tab').forEach(tab => tab.classList.remove('active'));
    alert(`Showing ${filteredBlogs.length} pending blog posts for review.`);
}

function showBlogManagement() {
    if (!currentUser.hasPermission(window.BlogRoles.PERMISSIONS.MANAGE_ALL_BLOGS)) {
        alert('You do not have permission to access blog management.');
        return;
    }
    
    // Show all blogs regardless of status
    filteredBlogs = [...allBlogs];
    renderBlogs();
    alert('Showing all blog posts for management. You can edit, delete, approve, or reject any post.');
}

function showAnalytics() {
    if (!currentUser.hasPermission(window.BlogRoles.PERMISSIONS.VIEW_ANALYTICS)) {
        alert('You do not have permission to view analytics.');
        return;
    }
    
    // Calculate analytics
    const totalBlogs = allBlogs.length;
    const publishedBlogs = allBlogs.filter(b => b.blogStatus === window.BlogRoles.BLOG_STATUS.PUBLISHED).length;
    const pendingBlogs = allBlogs.filter(b => b.blogStatus === window.BlogRoles.BLOG_STATUS.PENDING).length;
    const totalLikes = allBlogs.reduce((sum, blog) => sum + (blog.likesCount || 0), 0);
    const anonymousBlogs = allBlogs.filter(b => b.isAnonymous).length;
    
    const analyticsMessage = `
Blog Analytics:
• Total Posts: ${totalBlogs}
• Published: ${publishedBlogs}
• Pending Review: ${pendingBlogs}
• Total Likes: ${totalLikes}
• Anonymous Posts: ${anonymousBlogs}
• Anonymous Percentage: ${totalBlogs > 0 ? Math.round((anonymousBlogs / totalBlogs) * 100) : 0}%
    `;
    
    alert(analyticsMessage);
}
function setupEventListeners() {
    // Search functionality
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', handleSearch);
    }

    // Filter tabs
    const sortTabs = document.querySelectorAll('.sort-tab');
    sortTabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const filter = tab.getAttribute('data-sort');
            handleFilter(filter);
            
            // Update active tab
            sortTabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
        });
    });

    // Modal close functionality
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeBlogModal();
            closeCreatePostModal();
        }
    });
}

// Handle search
function handleSearch(e) {
    const searchTerm = e.target.value.toLowerCase();
    
    if (searchTerm.trim() === '') {
        filteredBlogs = [...allBlogs];
    } else {
        filteredBlogs = allBlogs.filter(blog => 
            blog.title.toLowerCase().includes(searchTerm) ||
            blog.content.toLowerCase().includes(searchTerm)
        );
    }
    
    renderBlogs();
}

// Handle filter
function handleFilter(filter) {
    currentFilter = filter;
    
    switch (filter) {
        case 'hot':
            filteredBlogs = [...allBlogs].sort((a, b) => (b.likesCount || 0) - (a.likesCount || 0));
            break;
        case 'new':
            filteredBlogs = [...allBlogs].sort((a, b) => new Date(b.publishedDate) - new Date(a.publishedDate));
            break;
        case 'top':
            filteredBlogs = [...allBlogs].sort((a, b) => (b.likesCount || 0) - (a.likesCount || 0));
            break;
        case 'anonymous':
            filteredBlogs = allBlogs.filter(blog => blog.isAnonymous);
            break;
        default:
            filteredBlogs = [...allBlogs];
    }
    
    renderBlogs();
}

// Setup event listeners
function setupEventListeners() {
    // Search functionality
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', handleSearch);
    }

    // Filter tabs
    const sortTabs = document.querySelectorAll('.sort-tab');
    sortTabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const filter = tab.getAttribute('data-sort');
            handleFilter(filter);
            
            // Update active tab
            sortTabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
        });
    });

    // Modal close functionality
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeBlogModal();
            closeCreatePostModal();
        }
    });
}

// Handle search
function handleSearch(e) {
    const searchTerm = e.target.value.toLowerCase();
    
    if (searchTerm.trim() === '') {
        filteredBlogs = [...allBlogs];
    } else {
        filteredBlogs = allBlogs.filter(blog => 
            blog.title.toLowerCase().includes(searchTerm) ||
            blog.content.toLowerCase().includes(searchTerm)
        );
    }
    
    renderBlogs();
}

// Handle filter
function handleFilter(filter) {
    currentFilter = filter;
    
    switch (filter) {
        case 'hot':
            filteredBlogs = [...allBlogs].sort((a, b) => (b.likesCount || 0) - (a.likesCount || 0));
            break;
        case 'new':
            filteredBlogs = [...allBlogs].sort((a, b) => new Date(b.publishedDate) - new Date(a.publishedDate));
            break;
        case 'top':
            filteredBlogs = [...allBlogs].sort((a, b) => (b.likesCount || 0) - (a.likesCount || 0));
            break;
        case 'anonymous':
            filteredBlogs = allBlogs.filter(blog => blog.isAnonymous);
            break;
        default:
            filteredBlogs = [...allBlogs];
    }
    
    renderBlogs();
}

// Handle vote (like/dislike) with role-based access
async function handleVote(blogId, isLike) {
    if (!currentUser.hasPermission(window.BlogRoles.PERMISSIONS.LIKE_BLOG)) {
        alert('Please log in to vote on posts.');
        return;
    }

    try {
        // Show loading feedback
        const likeBtn = document.querySelector(`[onclick="handleVote(${blogId}, true)"]`);
        const dislikeBtn = document.querySelector(`[onclick="handleVote(${blogId}, false)"]`);
        
        if (isLike && likeBtn) {
            likeBtn.disabled = true;
            likeBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
        } else if (!isLike && dislikeBtn) {
            dislikeBtn.disabled = true;
            dislikeBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
        }

        // Prepare request body as specified in your API
        const requestBody = {
            blogId: parseInt(blogId),
            accountId: currentUser.id || currentUser.userId,
            reactionType: isLike ? "like" : "dislike"
        };

        const response = await fetch(`${API_BASE_URL}/BlogReaction/UpdateBlogReaction`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${currentUser.token}`
            },
            body: JSON.stringify(requestBody)
        });

        if (response.ok) {
            // Reload blogs to update vote counts
            await loadBlogs();
            showToast(`Successfully ${isLike ? 'liked' : 'disliked'} the post!`, 'success');
        } else {
            const error = await response.text();
            showToast('Failed to register vote: ' + error, 'error');
        }
    } catch (error) {
        console.error('Error voting:', error);
        showToast('Failed to register vote. Please try again.', 'error');
    } finally {
        // Restore button states
        const likeBtn = document.querySelector(`[onclick="handleVote(${blogId}, true)"]`);
        const dislikeBtn = document.querySelector(`[onclick="handleVote(${blogId}, false)"]`);
        
        if (likeBtn) {
            likeBtn.disabled = false;
            likeBtn.innerHTML = '<i class="fas fa-heart"></i>';
        }
        if (dislikeBtn) {
            dislikeBtn.disabled = false;
            dislikeBtn.innerHTML = '<i class="fas fa-heart-broken"></i>';
        }
    }
}

// Handle share
function handleShare(blogId) {
    const blog = allBlogs.find(b => b.id === blogId);
    if (!blog) return;

    if (navigator.share) {
        navigator.share({
            title: blog.title,
            text: truncateText(blog.content, 100),
            url: window.location.href
        });
    } else {
        const shareText = `${blog.title}\n\n${truncateText(blog.content, 100)}\n\nRead more at: ${window.location.href}`;
        navigator.clipboard.writeText(shareText).then(() => {
            alert('Story link copied to clipboard!');
        });
    }
}

// Open blog modal with role-based commenting
function openBlogModal(blogId) {
    const blog = allBlogs.find(b => b.id === blogId);
    if (!blog) return;

    const modal = document.getElementById('blogModal');
    if (!modal) return;

    // Update modal content
    document.getElementById('modalTitle').textContent = blog.title;
    document.getElementById('modalAuthor').textContent = blog.isAnonymous ? 'Anonymous Community Member' : `u/user${blog.authorId}`;
    document.getElementById('modalDate').textContent = formatDate(blog.publishedDate);
    document.getElementById('modalContent').innerHTML = formatContent(blog.content);
    
    // Update modal engagement
    const modalEngagement = document.getElementById('modalEngagement');
    if (modalEngagement) {
        modalEngagement.innerHTML = `
            <div class="engagement-stats">
                <div class="stat-item">
                    <i class="fas fa-arrow-up"></i>
                    <span>${blog.likesCount || 0}</span>
                    <small>upvotes</small>
                </div>
                <div class="stat-item">
                    <i class="fas fa-arrow-down"></i>
                    <span>${blog.dislikesCount || 0}</span>
                    <small>downvotes</small>
                </div>
                <div class="stat-item">
                    <i class="fas fa-comments"></i>
                    <span>${blog.blogReaction ? blog.blogReaction.filter(r => r.comment).length : 0}</span>
                    <small>comments</small>
                </div>
            </div>
        `;
    }

    // Update modal comments with role-based commenting
    const modalComments = document.getElementById('modalComments');
    if (modalComments) {
        let commentsHTML = '<div class="comments-section"><h4>Comments</h4>';
        
        if (blog.blogReaction && blog.blogReaction.length > 0) {
            const comments = blog.blogReaction.filter(r => r.comment);
            commentsHTML += comments.map(comment => `
                <div class="comment">
                    <div class="comment-author">
                        <i class="fas fa-user"></i>
                        <span>u/user${comment.userId}</span>
                        <span class="comment-date">${formatDate(comment.createdDate)}</span>
                    </div>
                    <div class="comment-text">${comment.comment}</div>
                </div>
            `).join('');
        } else {
            commentsHTML += '<p class="no-comments">No comments yet. Be the first to share your thoughts!</p>';
        }
        
        // Add comment form if user has permission
        if (currentUser.hasPermission(window.BlogRoles.PERMISSIONS.COMMENT_BLOG)) {
            commentsHTML += `
                <div class="comment-form">
                    <textarea id="newComment" placeholder="Share your thoughts..."></textarea>
                    <button onclick="addComment(${blog.id})" class="comment-btn">
                        <i class="fas fa-paper-plane"></i> Post Comment
                    </button>
                </div>
            `;
        }
        
        commentsHTML += '</div>';
        modalComments.innerHTML = commentsHTML;
    }

    // Show modal
    modal.style.display = 'flex';
    document.body.style.overflow = 'hidden';
}

async function addComment(blogId) {
    if (!currentUser.hasPermission(window.BlogRoles.PERMISSIONS.COMMENT_BLOG)) {
        alert('You do not have permission to comment on posts.');
        return;
    }

    const commentText = document.getElementById('newComment').value.trim();
    if (!commentText) {
        alert('Please enter a comment.');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/SocialBlog/AddComment`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${currentUser.token}`
            },
            body: JSON.stringify({
                blogId: blogId,
                userId: currentUser.userId,
                comment: commentText
            })
        });

        if (response.ok) {
            // Reload blogs and reopen modal to show new comment
            await loadBlogs();
            openBlogModal(blogId);
            alert('Comment added successfully!');
        } else {
            const error = await response.text();
            alert('Failed to add comment: ' + error);
        }
    } catch (error) {
        console.error('Error adding comment:', error);
        alert('Failed to add comment. Please try again.');
    }
}

function closeBlogModal() {
    const modal = document.getElementById('blogModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
}

// Modal action handlers with role-based permissions
function handleModalLike() {
    alert('Like functionality will be implemented with the modal blog ID');
}

function handleModalComment() {
    const commentForm = document.querySelector('.comment-form textarea');
    if (commentForm) {
        commentForm.focus();
    }
}

function handleModalShare() {
    alert('Share functionality for modal');
}

// Show create post modal with role-based access
function showCreatePostModal() {
    if (!currentUser.hasPermission(window.BlogRoles.PERMISSIONS.CREATE_BLOG)) {
        alert('Please log in to create a blog post.');
        return;
    }

    const modal = document.getElementById('createPostModal');
    if (!modal) return;

    // Reset form
    document.getElementById('blogTitle').value = '';
    document.getElementById('blogContent').value = '';
    document.getElementById('blogNotes').value = '';
    document.getElementById('isAnonymous').checked = false;
    
    // Reset submit button
    const submitBtn = document.querySelector('.submit-btn');
    submitBtn.innerHTML = '<i class="fas fa-paper-plane"></i> Share Story';
    submitBtn.onclick = createBlog;
    
    // Reset modal title
    document.querySelector('#createPostModal .modal-title-area h2').textContent = 'Share Your Story';
    
    // Hide auth section and show form
    document.getElementById('authSection').style.display = 'none';
    document.getElementById('createForm').style.display = 'block';
    
    // Show modal
    modal.style.display = 'flex';
    document.body.style.overflow = 'hidden';
}

function closeCreatePostModal() {
    const modal = document.getElementById('createPostModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
    
    // Clear form status
    hideFormStatus();
}

// Create blog with role-based permissions
async function createBlog() {
    if (!currentUser.hasPermission(window.BlogRoles.PERMISSIONS.CREATE_BLOG)) {
        alert('You do not have permission to create blog posts.');
        return;
    }

    const title = document.getElementById('blogTitle').value.trim();
    const content = document.getElementById('blogContent').value.trim();
    const notes = document.getElementById('blogNotes').value.trim();
    const isAnonymous = document.getElementById('isAnonymous').checked;

    // Validation
    if (!title || !content) {
        showFormStatus('Please fill in all required fields.', 'error');
        return;
    }

    if (title.length > 200) {
        showFormStatus('Title must be 200 characters or less.', 'error');
        return;
    }

    const submitBtn = document.querySelector('.submit-btn');
    submitBtn.disabled = true;
    submitBtn.classList.add('loading');

    try {
        const blogData = {
            title: title,
            content: content,
            notes: notes,
            isAnonymous: isAnonymous,
            authorId: currentUser.userId,
            blogStatus: currentUser.role === 'admin' || currentUser.role === 'manager' ? 
                       window.BlogRoles.BLOG_STATUS.PUBLISHED : 
                       window.BlogRoles.BLOG_STATUS.PENDING
        };

        const response = await fetch(`${API_BASE_URL}/SocialBlog/CreateBlog`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${currentUser.token}`
            },
            body: JSON.stringify(blogData)
        });

        if (response.ok) {
            const statusMessage = blogData.blogStatus === window.BlogRoles.BLOG_STATUS.PUBLISHED ? 
                                'Blog post created and published successfully!' : 
                                'Blog post created successfully! It will be reviewed before publication.';
            showFormStatus(statusMessage, 'success');
            
            setTimeout(() => {
                closeCreatePostModal();
                loadBlogs();
            }, 2000);
        } else {
            const error = await response.text();
            console.error('Blog creation failed:', error);
            showFormStatus('Failed to create blog post: ' + error, 'error');
        }
    } catch (error) {
        console.error('Error creating blog:', error);
        let errorMessage = 'Failed to create blog post. ';
        if (error.name === 'TypeError' && error.message.includes('fetch')) {
            errorMessage += 'Please check your internet connection or contact support.';
        } else {
            errorMessage += 'Please try again or contact support if the problem persists.';
        }
        showFormStatus(errorMessage, 'error');
    } finally {
        submitBtn.disabled = false;
        submitBtn.classList.remove('loading');
    }
}

// Character counter setup
function setupCharacterCounters() {
    const titleInput = document.getElementById('blogTitle');
    const contentInput = document.getElementById('blogContent');
    const notesInput = document.getElementById('blogNotes');
    
    if (titleInput) {
        titleInput.addEventListener('input', function() {
            updateCharCounter('titleCount', this.value.length, 200);
        });
    }
    
    if (contentInput) {
        contentInput.addEventListener('input', function() {
            updateCharCounter('contentCount', this.value.length, null);
        });
    }
    
    if (notesInput) {
        notesInput.addEventListener('input', function() {
            updateCharCounter('notesCount', this.value.length, 100);
        });
    }
}

function updateCharCounter(counterId, currentLength, maxLength) {
    const counter = document.getElementById(counterId);
    if (!counter) return;
    
    if (maxLength) {
        counter.textContent = `${currentLength}/${maxLength}`;
        counter.className = 'char-count';
        
        if (currentLength > maxLength * 0.9) {
            counter.classList.add('warning');
        }
        if (currentLength > maxLength) {
            counter.classList.add('danger');
        }
    } else {
        counter.textContent = `${currentLength} characters`;
    }
}

// Form status management
function showFormStatus(message, type) {
    const statusElement = document.getElementById('formStatus');
    if (statusElement) {
        statusElement.textContent = message;
        statusElement.className = `form-status ${type}`;
        statusElement.style.display = 'block';
    }
}

function hideFormStatus() {
    const statusElement = document.getElementById('formStatus');
    if (statusElement) {
        statusElement.style.display = 'none';
    }
}

// Update statistics
function updateStatistics() {
    const totalBlogs = document.getElementById('totalBlogs');
    const totalAuthors = document.getElementById('totalAuthors');
    const totalLikes = document.getElementById('totalLikes');
    const totalComments = document.getElementById('totalComments');
    const anonymousPosts = document.getElementById('anonymousPosts');
    
    if (totalBlogs) totalBlogs.textContent = allBlogs.length;
    
    if (totalAuthors) {
        const uniqueAuthors = new Set(allBlogs.map(blog => blog.authorId));
        totalAuthors.textContent = uniqueAuthors.size;
    }
    
    if (totalLikes) {
        const likes = allBlogs.reduce((sum, blog) => sum + (blog.likesCount || 0), 0);
        totalLikes.textContent = likes;
    }
    
    if (totalComments) {
        const comments = allBlogs.reduce((sum, blog) => {
            return sum + (blog.blogReaction ? blog.blogReaction.filter(r => r.comment).length : 0);
        }, 0);
        totalComments.textContent = comments;
    }
    
    if (anonymousPosts) {
        const anonymous = allBlogs.filter(blog => blog.isAnonymous).length;
        anonymousPosts.textContent = anonymous;
    }
}

// User menu function (placeholder)
function showUserMenu() {
    alert(`User Menu for ${currentUser.getRoleDisplayName()}\n\nFeatures coming soon:\n• Profile settings\n• Notification preferences\n• Privacy settings`);
}

// Legacy functions for compatibility
function showCreatePostModal() {
    showCreatePostModal();
}

function closeCreatePostModal() {
    closeCreatePostModal();
}

function getBlogStatusBadge(status) {
    return window.BlogRoles.getBlogStatusBadge(status);
}

// Utility functions
function truncateText(text, maxLength) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
}

function formatContent(content) {
    if (!content) return '';
    return content.replace(/\n/g, '<br>');
}

function formatDate(dateString) {
    if (!dateString) return 'Unknown date';
    
    try {
        const date = new Date(dateString);
        const now = new Date();
        const diffInHours = Math.floor((now - date) / (1000 * 60 * 60));
        
        if (diffInHours < 1) return 'Just now';
        if (diffInHours < 24) return `${diffInHours} hours ago`;
        if (diffInHours < 48) return 'Yesterday';
        
        const diffInDays = Math.floor(diffInHours / 24);
        if (diffInDays < 7) return `${diffInDays} days ago`;
        if (diffInDays < 30) return `${Math.floor(diffInDays / 7)} weeks ago`;
        
        return date.toLocaleDateString();
    } catch (error) {
        console.error('Error formatting date:', error);
        return 'Unknown date';
    }
}

// Show toast notification
function showToast(message, type = 'info') {
    // Remove existing toast if any
    const existingToast = document.querySelector('.toast-notification');
    if (existingToast) {
        existingToast.remove();
    }

    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type}`;
    toast.innerHTML = `
        <div class="toast-content">
            <i class="fas ${getToastIcon(type)}"></i>
            <span>${message}</span>
        </div>
        <button class="toast-close" onclick="this.parentElement.remove()">
            <i class="fas fa-times"></i>
        </button>
    `;

    // Add styles if not exists
    if (!document.getElementById('toast-styles')) {
        const style = document.createElement('style');
        style.id = 'toast-styles';
        style.textContent = `
            .toast-notification {
                position: fixed;
                top: 20px;
                right: 20px;
                background: white;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                padding: 16px;
                display: flex;
                align-items: center;
                gap: 12px;
                z-index: 10000;
                min-width: 300px;
                max-width: 500px;
                animation: slideIn 0.3s ease-out;
            }
            .toast-success { border-left: 4px solid #10B981; }
            .toast-error { border-left: 4px solid #EF4444; }
            .toast-info { border-left: 4px solid #3B82F6; }
            .toast-content {
                display: flex;
                align-items: center;
                gap: 8px;
                flex: 1;
            }
            .toast-close {
                background: none;
                border: none;
                color: #666;
                cursor: pointer;
                padding: 4px;
            }
            @keyframes slideIn {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
        `;
        document.head.appendChild(style);
    }

    document.body.appendChild(toast);

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (toast.parentElement) {
            toast.remove();
        }
    }, 5000);
}

function getToastIcon(type) {
    switch (type) {
        case 'success': return 'fa-check-circle';
        case 'error': return 'fa-exclamation-circle';
        case 'info': 
        default: return 'fa-info-circle';
    }
}
