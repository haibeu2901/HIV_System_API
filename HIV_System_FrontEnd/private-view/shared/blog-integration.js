// Shared Blog Integration System
// This file provides blog functionality that can be integrated into any view

class BlogIntegration {
    constructor(containerId, options = {}) {
        this.containerId = containerId;
        this.container = document.getElementById(containerId);
        this.options = {
            showCreateButton: options.showCreateButton !== false, // Default true
            showFilters: options.showFilters !== false, // Default true
            showStats: options.showStats !== false, // Default true
            maxPosts: options.maxPosts || null, // No limit by default
            userRole: options.userRole || 'public', // public, patient, doctor, staff, admin, manager
            authToken: options.authToken || null,
            compact: options.compact || false, // Compact view for sidebars
            ...options
        };
        
        this.allBlogs = [];
        this.filteredBlogs = [];
        this.currentUser = null;
        this.isAuthenticated = !!this.options.authToken;
        
        if (this.options.authToken) {
            this.currentUser = { token: this.options.authToken };
        }
        
        this.init();
    }

    async init() {
        if (!this.container) {
            console.error(`Blog container with ID '${this.containerId}' not found`);
            return;
        }

        this.renderBlogContainer();
        await this.loadBlogs();
        this.setupEventListeners();
    }

    renderBlogContainer() {
        const compactClass = this.options.compact ? 'blog-compact' : '';
        
        this.container.innerHTML = `
            <div class="blog-integration ${compactClass}">
                ${this.options.showStats ? this.renderStatsSection() : ''}
                ${this.renderHeaderSection()}
                ${this.options.showFilters ? this.renderFiltersSection() : ''}
                <div class="blog-content">
                    <div id="${this.containerId}_loading" class="loading-container" style="display: none;">
                        <div class="loading-spinner">
                            <i class="fas fa-spinner fa-spin"></i>
                            <p>Loading community stories...</p>
                        </div>
                    </div>
                    <div id="${this.containerId}_grid" class="blog-grid">
                        <!-- Blog posts will be rendered here -->
                    </div>
                </div>
                ${this.renderBlogModal()}
                ${this.isAuthenticated && this.options.showCreateButton ? this.renderCreateModal() : ''}
            </div>
        `;

        this.injectStyles();
    }

    renderStatsSection() {
        if (this.options.compact) return '';
        
        return `
            <div class="blog-stats">
                <div class="stat-card">
                    <div class="stat-icon"><i class="fas fa-comments"></i></div>
                    <div class="stat-info">
                        <span class="stat-value" id="${this.containerId}_totalBlogs">0</span>
                        <span class="stat-label">Stories</span>
                    </div>
                </div>
                <div class="stat-card">
                    <div class="stat-icon"><i class="fas fa-users"></i></div>
                    <div class="stat-info">
                        <span class="stat-value" id="${this.containerId}_totalAuthors">0</span>
                        <span class="stat-label">Contributors</span>
                    </div>
                </div>
                <div class="stat-card">
                    <div class="stat-icon"><i class="fas fa-heart"></i></div>
                    <div class="stat-info">
                        <span class="stat-value" id="${this.containerId}_totalLikes">0</span>
                        <span class="stat-label">Likes</span>
                    </div>
                </div>
            </div>
        `;
    }

    renderHeaderSection() {
        return `
            <div class="blog-header">
                <div class="blog-title">
                    <h2><i class="fas fa-comments"></i> Community Stories</h2>
                    <p>Share experiences and support each other</p>
                </div>
                ${this.isAuthenticated && this.options.showCreateButton ? `
                    <button class="btn-create-blog" onclick="blogIntegrations['${this.containerId}'].openCreateModal()">
                        <i class="fas fa-pen"></i> Share Your Story
                    </button>
                ` : ''}
            </div>
        `;
    }

    renderFiltersSection() {
        if (this.options.compact) return '';
        
        return `
            <div class="blog-filters">
                <div class="filter-tabs">
                    <button class="filter-tab active" data-filter="all">All Stories</button>
                    <button class="filter-tab" data-filter="hot">Most Liked</button>
                    <button class="filter-tab" data-filter="new">Recent</button>
                    <button class="filter-tab" data-filter="anonymous">Anonymous</button>
                </div>
                <div class="search-box">
                    <i class="fas fa-search"></i>
                    <input type="text" id="${this.containerId}_search" placeholder="Search stories...">
                </div>
            </div>
        `;
    }

    renderBlogModal() {
        return `
            <div id="${this.containerId}_blogModal" class="blog-modal" style="display: none;">
                <div class="modal-overlay" onclick="blogIntegrations['${this.containerId}'].closeBlogModal()"></div>
                <div class="modal-content">
                    <div class="modal-header">
                        <h2 id="${this.containerId}_modalTitle"></h2>
                        <button class="modal-close" onclick="blogIntegrations['${this.containerId}'].closeBlogModal()">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    <div class="modal-meta">
                        <span id="${this.containerId}_modalAuthor"></span>
                        <span id="${this.containerId}_modalDate"></span>
                    </div>
                    <div class="modal-body">
                        <div id="${this.containerId}_modalContent"></div>
                    </div>
                    <div id="${this.containerId}_modalEngagement"></div>
                    <div id="${this.containerId}_modalComments"></div>
                </div>
            </div>
        `;
    }

    renderCreateModal() {
        return `
            <div id="${this.containerId}_createModal" class="blog-modal" style="display: none;">
                <div class="modal-overlay" onclick="blogIntegrations['${this.containerId}'].closeCreateModal()"></div>
                <div class="modal-content create-modal">
                    <div class="modal-header">
                        <h2><i class="fas fa-pen"></i> Share Your Story</h2>
                        <button class="modal-close" onclick="blogIntegrations['${this.containerId}'].closeCreateModal()">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    <div class="create-form">
                        <div id="${this.containerId}_formStatus" class="form-status" style="display: none;"></div>
                        
                        <form id="${this.containerId}_createForm">
                            <div class="form-group">
                                <label for="${this.containerId}_blogTitle">
                                    <i class="fas fa-heading"></i> Title
                                </label>
                                <input type="text" id="${this.containerId}_blogTitle" maxlength="200" required>
                                <span id="${this.containerId}_titleCounter" class="char-count">0/200 characters</span>
                            </div>
                            
                            <div class="form-group">
                                <label for="${this.containerId}_blogContent">
                                    <i class="fas fa-edit"></i> Your Story
                                </label>
                                <textarea id="${this.containerId}_blogContent" maxlength="5000" required></textarea>
                                <span id="${this.containerId}_contentCounter" class="char-count">0/5000 characters</span>
                            </div>
                            
                            <div class="form-group privacy-group">
                                <div class="privacy-option">
                                    <input type="checkbox" id="${this.containerId}_isPublic" checked>
                                    <label for="${this.containerId}_isPublic" class="checkbox-label">
                                        <span><i class="fas fa-globe"></i> Make this story public</span>
                                        <small>Public stories can be seen by all community members</small>
                                    </label>
                                </div>
                            </div>
                            
                            <div class="form-actions">
                                <button type="button" class="form-btn cancel-btn" onclick="blogIntegrations['${this.containerId}'].closeCreateModal()">
                                    <i class="fas fa-times"></i> Cancel
                                </button>
                                <button type="button" class="form-btn submit-btn" id="${this.containerId}_submitBtn" onclick="blogIntegrations['${this.containerId}'].submitBlog()">
                                    <i class="fas fa-paper-plane"></i> Share Story
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        `;
    }

    async loadBlogs() {
        const loadingContainer = document.getElementById(`${this.containerId}_loading`);
        const blogGrid = document.getElementById(`${this.containerId}_grid`);
        
        try {
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
            
            // Filter for published blogs
            this.allBlogs = data.filter(blog => blog.blogStatus === 4);
            
            // Apply max posts limit if specified
            if (this.options.maxPosts) {
                this.allBlogs = this.allBlogs.slice(0, this.options.maxPosts);
            }
            
            this.filteredBlogs = [...this.allBlogs];
            this.renderBlogs();
            this.updateStatistics();
            
            loadingContainer.style.display = 'none';
            blogGrid.style.display = 'block';
            
        } catch (error) {
            console.error('Error loading blogs:', error);
            blogGrid.innerHTML = `
                <div class="error-container">
                    <div class="error-icon"><i class="fas fa-exclamation-triangle"></i></div>
                    <h3>Stories Temporarily Unavailable</h3>
                    <p>We're currently unable to load the community stories. Please try again later.</p>
                    <button class="btn-retry" onclick="blogIntegrations['${this.containerId}'].loadBlogs()">
                        <i class="fas fa-refresh"></i> Try Again
                    </button>
                </div>
            `;
            loadingContainer.style.display = 'none';
            blogGrid.style.display = 'block';
        }
    }

    renderBlogs() {
        const blogGrid = document.getElementById(`${this.containerId}_grid`);
        
        if (this.filteredBlogs.length === 0) {
            blogGrid.innerHTML = `
                <div class="no-posts">
                    <div class="no-posts-icon"><i class="fas fa-comments"></i></div>
                    <h3>No stories yet</h3>
                    <p>Be the first to share your story with the community!</p>
                </div>
            `;
            return;
        }

        const postClass = this.options.compact ? 'blog-post-compact' : 'blog-post';
        
        blogGrid.innerHTML = this.filteredBlogs.map(blog => `
            <div class="${postClass}">
                <div class="post-header">
                    <div class="post-meta">
                        <span class="author">${blog.isAnonymous ? 'Anonymous' : 'Community Member'}</span>
                        <span class="separator">•</span>
                        <span class="timestamp">${this.formatDate(blog.publishedDate)}</span>
                        ${blog.isAnonymous ? '<span class="anonymous-badge"><i class="fas fa-user-secret"></i></span>' : ''}
                    </div>
                </div>
                <div class="post-body">
                    <h3 class="post-title" onclick="blogIntegrations['${this.containerId}'].openBlogModal(${blog.id})">${blog.title}</h3>
                    <div class="post-text">
                        ${this.truncateText(blog.content, this.options.compact ? 150 : 300)}
                    </div>
                </div>
                <div class="post-footer">
                    <div class="post-actions">
                        <button class="action-btn">
                            <i class="fas fa-heart"></i>
                            <span>${blog.likesCount || 0}</span>
                        </button>
                        <button class="action-btn" onclick="blogIntegrations['${this.containerId}'].openBlogModal(${blog.id})">
                            <i class="fas fa-comment"></i>
                            <span>Read More</span>
                        </button>
                    </div>
                </div>
            </div>
        `).join('');
    }

    setupEventListeners() {
        // Search functionality
        const searchInput = document.getElementById(`${this.containerId}_search`);
        if (searchInput) {
            searchInput.addEventListener('input', (e) => this.handleSearch(e));
        }

        // Filter tabs
        const filterTabs = this.container.querySelectorAll('.filter-tab');
        filterTabs.forEach(tab => {
            tab.addEventListener('click', () => {
                const filter = tab.getAttribute('data-filter');
                this.handleFilter(filter);
                
                filterTabs.forEach(t => t.classList.remove('active'));
                tab.classList.add('active');
            });
        });

        // Character counters for create form
        const titleInput = document.getElementById(`${this.containerId}_blogTitle`);
        const contentInput = document.getElementById(`${this.containerId}_blogContent`);
        
        if (titleInput) {
            titleInput.addEventListener('input', () => {
                this.updateCharCounter(titleInput, `${this.containerId}_titleCounter`, 200);
            });
        }
        
        if (contentInput) {
            contentInput.addEventListener('input', () => {
                this.updateCharCounter(contentInput, `${this.containerId}_contentCounter`, 5000);
            });
        }
    }

    handleSearch(e) {
        const searchTerm = e.target.value.toLowerCase();
        
        if (searchTerm.trim() === '') {
            this.filteredBlogs = [...this.allBlogs];
        } else {
            this.filteredBlogs = this.allBlogs.filter(blog => 
                blog.title.toLowerCase().includes(searchTerm) ||
                blog.content.toLowerCase().includes(searchTerm)
            );
        }
        
        this.renderBlogs();
    }

    handleFilter(filter) {
        switch (filter) {
            case 'hot':
                this.filteredBlogs = [...this.allBlogs].sort((a, b) => (b.likesCount || 0) - (a.likesCount || 0));
                break;
            case 'new':
                this.filteredBlogs = [...this.allBlogs].sort((a, b) => new Date(b.publishedDate) - new Date(a.publishedDate));
                break;
            case 'anonymous':
                this.filteredBlogs = this.allBlogs.filter(blog => blog.isAnonymous);
                break;
            default:
                this.filteredBlogs = [...this.allBlogs];
        }
        
        this.renderBlogs();
    }

    openBlogModal(blogId) {
        const blog = this.allBlogs.find(b => b.id === blogId);
        if (!blog) return;

        const modal = document.getElementById(`${this.containerId}_blogModal`);
        if (!modal) return;

        document.getElementById(`${this.containerId}_modalTitle`).textContent = blog.title;
        document.getElementById(`${this.containerId}_modalAuthor`).textContent = blog.isAnonymous ? 'Anonymous Community Member' : 'Community Member';
        document.getElementById(`${this.containerId}_modalDate`).textContent = this.formatDate(blog.publishedDate);
        document.getElementById(`${this.containerId}_modalContent`).innerHTML = this.formatContent(blog.content);
        
        modal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
    }

    closeBlogModal() {
        const modal = document.getElementById(`${this.containerId}_blogModal`);
        if (modal) {
            modal.style.display = 'none';
            document.body.style.overflow = 'auto';
        }
    }

    openCreateModal() {
        if (!this.isAuthenticated) {
            alert('Please log in to share your story.');
            return;
        }

        const modal = document.getElementById(`${this.containerId}_createModal`);
        if (modal) {
            modal.style.display = 'flex';
            document.body.style.overflow = 'hidden';
        }
    }

    closeCreateModal() {
        const modal = document.getElementById(`${this.containerId}_createModal`);
        if (modal) {
            modal.style.display = 'none';
            document.body.style.overflow = 'auto';
            
            // Reset form
            const form = document.getElementById(`${this.containerId}_createForm`);
            if (form) form.reset();
            
            // Reset counters
            const titleCounter = document.getElementById(`${this.containerId}_titleCounter`);
            const contentCounter = document.getElementById(`${this.containerId}_contentCounter`);
            if (titleCounter) titleCounter.textContent = '0/200 characters';
            if (contentCounter) contentCounter.textContent = '0/5000 characters';
        }
    }

    async submitBlog() {
        if (!this.isAuthenticated || !this.currentUser) {
            this.showFormStatus('Please log in to share your story.', 'error');
            return;
        }

        const titleInput = document.getElementById(`${this.containerId}_blogTitle`);
        const contentInput = document.getElementById(`${this.containerId}_blogContent`);
        const isPublicCheckbox = document.getElementById(`${this.containerId}_isPublic`);
        const submitBtn = document.getElementById(`${this.containerId}_submitBtn`);

        const title = titleInput.value.trim();
        const content = contentInput.value.trim();
        const isPublic = isPublicCheckbox.checked;

        if (!title || !content) {
            this.showFormStatus('Please fill in all fields.', 'error');
            return;
        }

        const blogData = { title, content, isPublic };

        submitBtn.disabled = true;
        submitBtn.classList.add('loading');
        this.showFormStatus('Sharing your story...', 'info');

        try {
            const response = await fetch('/api/SocialBlog/CreateBlog', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.currentUser.token}`
                },
                body: JSON.stringify(blogData)
            });

            if (response.ok) {
                this.showFormStatus('Story shared successfully!', 'success');
                setTimeout(() => {
                    this.closeCreateModal();
                    this.loadBlogs();
                }, 2000);
            } else {
                throw new Error('Failed to create blog post');
            }
        } catch (error) {
            console.error('Error creating blog:', error);
            this.showFormStatus('Failed to share story. Please try again.', 'error');
        } finally {
            submitBtn.disabled = false;
            submitBtn.classList.remove('loading');
        }
    }

    showFormStatus(message, type) {
        const statusDiv = document.getElementById(`${this.containerId}_formStatus`);
        if (statusDiv) {
            statusDiv.textContent = message;
            statusDiv.className = `form-status ${type}`;
            statusDiv.style.display = 'block';
            
            if (type === 'success' || type === 'info') {
                setTimeout(() => {
                    statusDiv.style.display = 'none';
                }, 5000);
            }
        }
    }

    updateCharCounter(textarea, counterId, maxLength) {
        const counter = document.getElementById(counterId);
        if (counter) {
            const currentLength = textarea.value.length;
            counter.textContent = `${currentLength}/${maxLength} characters`;
            
            counter.className = 'char-count';
            if (currentLength > maxLength * 0.9) {
                counter.classList.add('danger');
            } else if (currentLength > maxLength * 0.7) {
                counter.classList.add('warning');
            }
        }
    }

    updateStatistics() {
        const totalBlogs = this.allBlogs.length;
        const uniqueAuthors = new Set(this.allBlogs.map(b => b.authorId)).size;
        const totalLikes = this.allBlogs.reduce((sum, blog) => sum + (blog.likesCount || 0), 0);

        const elements = {
            totalBlogs: document.getElementById(`${this.containerId}_totalBlogs`),
            totalAuthors: document.getElementById(`${this.containerId}_totalAuthors`),
            totalLikes: document.getElementById(`${this.containerId}_totalLikes`)
        };
        
        if (elements.totalBlogs) elements.totalBlogs.textContent = totalBlogs;
        if (elements.totalAuthors) elements.totalAuthors.textContent = uniqueAuthors;
        if (elements.totalLikes) elements.totalLikes.textContent = totalLikes;
    }

    // Utility functions
    truncateText(text, maxLength) {
        if (!text) return '';
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength) + '...';
    }

    formatContent(content) {
        if (!content) return '';
        return content.replace(/\n/g, '<br>');
    }

    formatDate(dateString) {
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

    injectStyles() {
        if (document.getElementById('blog-integration-styles')) return;

        const styles = `
            <style id="blog-integration-styles">
                .blog-integration {
                    margin: 20px 0;
                }

                .blog-stats {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
                    gap: 15px;
                    margin-bottom: 20px;
                }

                .stat-card {
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    padding: 20px;
                    border-radius: 12px;
                    display: flex;
                    align-items: center;
                    gap: 15px;
                }

                .stat-icon {
                    font-size: 24px;
                }

                .stat-value {
                    font-size: 24px;
                    font-weight: bold;
                    display: block;
                }

                .stat-label {
                    font-size: 12px;
                    opacity: 0.9;
                }

                .blog-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin-bottom: 20px;
                    flex-wrap: wrap;
                    gap: 15px;
                }

                .blog-title h2 {
                    margin: 0;
                    color: #2c3e50;
                    display: flex;
                    align-items: center;
                    gap: 10px;
                }

                .blog-title p {
                    margin: 5px 0 0 0;
                    color: #7f8c8d;
                    font-size: 14px;
                }

                .btn-create-blog {
                    background: linear-gradient(135deg, #27ae60 0%, #229954 100%);
                    color: white;
                    border: none;
                    padding: 12px 24px;
                    border-radius: 8px;
                    cursor: pointer;
                    font-weight: 600;
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    transition: all 0.3s ease;
                }

                .btn-create-blog:hover {
                    background: linear-gradient(135deg, #229954 0%, #1e7e34 100%);
                    transform: translateY(-1px);
                }

                .blog-filters {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin-bottom: 20px;
                    flex-wrap: wrap;
                    gap: 15px;
                }

                .filter-tabs {
                    display: flex;
                    gap: 10px;
                    flex-wrap: wrap;
                }

                .filter-tab {
                    background: #f8f9fa;
                    border: 1px solid #dee2e6;
                    padding: 8px 16px;
                    border-radius: 20px;
                    cursor: pointer;
                    transition: all 0.3s ease;
                    font-size: 14px;
                }

                .filter-tab.active {
                    background: #667eea;
                    color: white;
                    border-color: #667eea;
                }

                .search-box {
                    position: relative;
                    display: flex;
                    align-items: center;
                }

                .search-box i {
                    position: absolute;
                    left: 12px;
                    color: #6c757d;
                    z-index: 1;
                }

                .search-box input {
                    padding: 8px 12px 8px 35px;
                    border: 1px solid #dee2e6;
                    border-radius: 20px;
                    width: 250px;
                    font-size: 14px;
                }

                .blog-grid {
                    display: grid;
                    gap: 20px;
                }

                .blog-post, .blog-post-compact {
                    background: white;
                    border: 1px solid #e9ecef;
                    border-radius: 12px;
                    padding: 20px;
                    transition: all 0.3s ease;
                }

                .blog-post-compact {
                    padding: 15px;
                }

                .blog-post:hover, .blog-post-compact:hover {
                    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                    transform: translateY(-2px);
                }

                .post-meta {
                    color: #6c757d;
                    font-size: 12px;
                    margin-bottom: 10px;
                }

                .post-title {
                    margin: 0 0 10px 0;
                    color: #2c3e50;
                    cursor: pointer;
                    transition: color 0.3s ease;
                }

                .post-title:hover {
                    color: #667eea;
                }

                .post-text {
                    color: #495057;
                    line-height: 1.6;
                    margin-bottom: 15px;
                }

                .post-actions {
                    display: flex;
                    gap: 15px;
                }

                .action-btn {
                    background: none;
                    border: none;
                    color: #6c757d;
                    cursor: pointer;
                    display: flex;
                    align-items: center;
                    gap: 5px;
                    font-size: 12px;
                    transition: color 0.3s ease;
                }

                .action-btn:hover {
                    color: #667eea;
                }

                .anonymous-badge {
                    background: #6c757d;
                    color: white;
                    padding: 2px 6px;
                    border-radius: 10px;
                    font-size: 10px;
                }

                .blog-modal {
                    position: fixed;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    background: rgba(0,0,0,0.5);
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    z-index: 1000;
                }

                .modal-overlay {
                    position: absolute;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                }

                .modal-content {
                    background: white;
                    border-radius: 12px;
                    max-width: 800px;
                    max-height: 90vh;
                    overflow-y: auto;
                    position: relative;
                    margin: 20px;
                    width: 100%;
                }

                .modal-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    padding: 20px;
                    border-bottom: 1px solid #e9ecef;
                }

                .modal-close {
                    background: none;
                    border: none;
                    font-size: 24px;
                    cursor: pointer;
                    color: #6c757d;
                }

                .modal-meta {
                    padding: 0 20px 10px;
                    color: #6c757d;
                    font-size: 14px;
                }

                .modal-body {
                    padding: 20px;
                    line-height: 1.6;
                }

                .blog-compact .blog-stats,
                .blog-compact .blog-filters {
                    display: none;
                }

                .blog-compact .blog-grid {
                    gap: 10px;
                }

                .loading-container {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    padding: 40px;
                    text-align: center;
                }

                .loading-spinner i {
                    font-size: 24px;
                    margin-bottom: 10px;
                    color: #667eea;
                }

                .error-container, .no-posts {
                    text-align: center;
                    padding: 40px;
                    color: #6c757d;
                }

                .btn-retry {
                    background: #667eea;
                    color: white;
                    border: none;
                    padding: 10px 20px;
                    border-radius: 8px;
                    cursor: pointer;
                    margin-top: 15px;
                }

                /* Form styles */
                .create-form {
                    padding: 20px;
                }

                .form-group {
                    margin-bottom: 20px;
                }

                .form-group label {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    margin-bottom: 8px;
                    font-weight: 600;
                    color: #2c3e50;
                }

                .form-group input,
                .form-group textarea {
                    width: 100%;
                    padding: 12px;
                    border: 2px solid #e9ecef;
                    border-radius: 8px;
                    font-family: inherit;
                    box-sizing: border-box;
                }

                .form-group textarea {
                    min-height: 120px;
                    resize: vertical;
                }

                .char-count {
                    font-size: 12px;
                    color: #6c757d;
                    text-align: right;
                    display: block;
                    margin-top: 5px;
                }

                .char-count.warning { color: #f39c12; }
                .char-count.danger { color: #e74c3c; }

                .form-actions {
                    display: flex;
                    gap: 15px;
                    justify-content: flex-end;
                    margin-top: 25px;
                    padding-top: 20px;
                    border-top: 1px solid #e9ecef;
                }

                .form-btn {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    padding: 12px 24px;
                    border: none;
                    border-radius: 8px;
                    cursor: pointer;
                    font-weight: 600;
                    transition: all 0.3s ease;
                }

                .cancel-btn {
                    background: #6c757d;
                    color: white;
                }

                .submit-btn {
                    background: linear-gradient(135deg, #27ae60 0%, #229954 100%);
                    color: white;
                }

                .form-status {
                    padding: 12px;
                    border-radius: 8px;
                    margin-bottom: 20px;
                }

                .form-status.success { background: #d4edda; color: #155724; }
                .form-status.error { background: #f8d7da; color: #721c24; }
                .form-status.info { background: #d1ecf1; color: #0c5460; }

                @media (max-width: 768px) {
                    .blog-header {
                        flex-direction: column;
                        align-items: stretch;
                    }

                    .blog-filters {
                        flex-direction: column;
                        align-items: stretch;
                    }

                    .search-box input {
                        width: 100%;
                    }

                    .modal-content {
                        margin: 10px;
                        max-height: 95vh;
                    }
                }
            </style>
        `;

        document.head.insertAdjacentHTML('beforeend', styles);
    }
}

// Global registry for blog integrations
window.blogIntegrations = window.blogIntegrations || {};

// Helper function to initialize blog integration
function initializeBlogIntegration(containerId, options = {}) {
    if (window.blogIntegrations[containerId]) {
        console.warn(`Blog integration for '${containerId}' already exists`);
        return window.blogIntegrations[containerId];
    }

    const integration = new BlogIntegration(containerId, options);
    window.blogIntegrations[containerId] = integration;
    return integration;
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { BlogIntegration, initializeBlogIntegration };
}
