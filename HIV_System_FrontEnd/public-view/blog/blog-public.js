// Global variables
let allBlogs = [];
let filteredBlogs = [];
let currentFilter = 'all';
let isLoading = false;

// Initialize the blog system
document.addEventListener('DOMContentLoaded', async () => {
    await loadBlogs();
    setupEventListeners();
    updateStatistics();
});

// Load blogs from API
async function loadBlogs() {
    const blogGrid = document.getElementById('blogGrid');
    const loadingContainer = document.getElementById('loadingContainer');
    
    try {
        isLoading = true;
        loadingContainer.style.display = 'flex';
        blogGrid.style.display = 'none';

        const response = await fetch('https://localhost:7009/api/SocialBlog/GetAllBlog', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('Raw API response:', data); // Debug log
        
        // Filter for published (status 4) and pending (status 2) blogs
        allBlogs = data.filter(blog => blog.blogStatus === 4 || blog.blogStatus === 2);
        filteredBlogs = [...allBlogs];
        
        renderBlogs();
        
        loadingContainer.style.display = 'none';
        blogGrid.style.display = 'block';
        
    } catch (error) {
        console.error('Error loading blogs:', error);
        loadingContainer.innerHTML = `
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
    } finally {
        isLoading = false;
    }
}

// Render blogs in the grid
function renderBlogs() {
    const blogGrid = document.getElementById('blogGrid');
    
    if (filteredBlogs.length === 0) {
        blogGrid.innerHTML = `
            <div class="no-posts">
                <div class="no-posts-icon">
                    <i class="fas fa-comments"></i>
                </div>
                <h3>No stories yet</h3>
                <p>Be the first to share your story with the community!</p>
                <a href="../landingpage.html#contact" class="cta-button">
                    <i class="fas fa-pen"></i> Share Your Story
                </a>
            </div>
        `;
        return;
    }

    blogGrid.innerHTML = filteredBlogs.map(blog => `
        <div class="reddit-post">
            <div class="post-voting">
                <button class="vote-btn upvote" onclick="handleVote(${blog.id}, true)">
                    <i class="fas fa-arrow-up"></i>
                </button>
                <span class="vote-count">${blog.likesCount || 0}</span>
                <button class="vote-btn downvote" onclick="handleVote(${blog.id}, false)">
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
                        ${getBlogStatusBadge(blog.blogStatus)}
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
                </div>
            </div>
        </div>
    `).join('');
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

// Handle vote (like/dislike)
function handleVote(blogId, isLike) {
    alert('Please log in to vote on posts.');
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

// Open blog modal
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

    // Update modal comments
    const modalComments = document.getElementById('modalComments');
    if (modalComments) {
        const comments = blog.blogReaction ? blog.blogReaction.filter(r => r.comment) : [];
        
        if (comments.length > 0) {
            modalComments.innerHTML = `
                <div class="comments-section">
                    <h4><i class="fas fa-comments"></i> Comments (${comments.length})</h4>
                    <div class="comments-list">
                        ${comments.map(comment => `
                            <div class="comment-item">
                                <div class="comment-header">
                                    <div class="comment-author">
                                        <i class="fas fa-user-circle"></i>
                                        <span>u/user${comment.accountId}</span>
                                    </div>
                                    <div class="comment-date">
                                        <i class="fas fa-clock"></i>
                                        <span>${formatDate(comment.reactedAt)}</span>
                                    </div>
                                </div>
                                <div class="comment-content">
                                    ${comment.comment}
                                </div>
                            </div>
                        `).join('')}
                    </div>
                </div>
            `;
        } else {
            modalComments.innerHTML = `
                <div class="no-comments">
                    <i class="fas fa-comment-slash"></i>
                    <p>No comments yet. Be the first to share your thoughts!</p>
                </div>
            `;
        }
    }

    // Show modal
    modal.style.display = 'flex';
    document.body.style.overflow = 'hidden';
}

// Close blog modal
function closeBlogModal() {
    const modal = document.getElementById('blogModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
}

// Update statistics
function updateStatistics() {
    const totalBlogs = allBlogs.length;
    const uniqueAuthors = new Set(allBlogs.map(b => b.authorId)).size;
    const totalLikes = allBlogs.reduce((sum, blog) => sum + (blog.likesCount || 0), 0);
    const totalComments = allBlogs.reduce((sum, blog) => {
        return sum + (blog.blogReaction ? blog.blogReaction.filter(r => r.comment).length : 0);
    }, 0);
    const anonymousPosts = allBlogs.filter(blog => blog.isAnonymous).length;

    const elements = {
        totalBlogs: document.getElementById('totalBlogs'),
        totalAuthors: document.getElementById('totalAuthors'),
        totalLikes: document.getElementById('totalLikes'),
        totalComments: document.getElementById('totalComments'),
        anonymousPosts: document.getElementById('anonymousPosts')
    };
    
    if (elements.totalBlogs) elements.totalBlogs.textContent = totalBlogs;
    if (elements.totalAuthors) elements.totalAuthors.textContent = uniqueAuthors;
    if (elements.totalLikes) elements.totalLikes.textContent = totalLikes;
    if (elements.totalComments) elements.totalComments.textContent = totalComments;
    if (elements.anonymousPosts) elements.anonymousPosts.textContent = anonymousPosts;
}

// Modal action handlers
function handleModalLike() {
    alert('Please log in to like this story.');
}

function handleModalComment() {
    alert('Please log in to comment on this story.');
}

function handleModalShare() {
    if (navigator.share) {
        navigator.share({
            title: document.getElementById('modalTitle').textContent,
            text: 'Check out this community story',
            url: window.location.href
        });
    } else {
        navigator.clipboard.writeText(window.location.href).then(() => {
            alert('Story link copied to clipboard!');
        });
    }
}

// Modal management functions
function showCreatePostModal() {
    const modal = document.getElementById('createPostModal');
    if (modal) {
        modal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
    }
}

function closeCreatePostModal() {
    const modal = document.getElementById('createPostModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
}

// Get blog status badge
function getBlogStatusBadge(status) {
    switch (status) {
        case 1: return '<span class="status-badge draft">Draft</span>';
        case 2: return '<span class="status-badge pending">Pending</span>';
        case 3: return '<span class="status-badge rejected">Rejected</span>';
        case 4: return '<span class="status-badge published">Published</span>';
        case 5: return '<span class="status-badge archived">Archived</span>';
        default: return '';
    }
}

// Utility functions
function formatDate(dateString) {
    if (!dateString) return 'Unknown date';
    
    try {
        const date = new Date(dateString);
        const now = new Date();
        const diffInHours = Math.floor((now - date) / (1000 * 60 * 60));
        
        if (diffInHours < 1) return 'Just now';
        if (diffInHours < 24) return `${diffInHours}h ago`;
        if (diffInHours < 24 * 7) return `${Math.floor(diffInHours / 24)}d ago`;
        
        return date.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric'
        });
    } catch (error) {
        return 'Unknown date';
    }
}

function truncateText(text, maxLength) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
}

function formatContent(content) {
    if (!content) return '';
    return content.replace(/\n/g, '<br>');
}
