document.addEventListener('DOMContentLoaded', () => {
    const blogGrid = document.getElementById('blogGrid');
    const loadingContainer = document.getElementById('loadingContainer');
    const searchInput = document.getElementById('searchInput');
    const filterButtons = document.querySelectorAll('.filter-btn');
    
    let allBlogs = [];
    let filteredBlogs = [];

    // Show loading state
    loadingContainer.style.display = 'flex';
    blogGrid.style.display = 'none';

    // Fetch all blogs (public access - no authentication needed)
    fetch("https://localhost:7009/api/SocialBlog/GetAllBlog", {
        method: "GET",
        headers: {
            "Content-Type": "application/json"
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        // Filter only published blogs (status 4)
        allBlogs = data.filter(blog => blog.blogStatus === 4);
        filteredBlogs = allBlogs;
        
        // Hide loading and show content
        loadingContainer.style.display = 'none';
        blogGrid.style.display = 'grid';
        
        // Update statistics
        updateStatistics(allBlogs);
        
        // Display blogs
        displayBlogs(filteredBlogs);
    })
    .catch(error => {
        console.error('Error fetching blogs:', error);
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
    });

    // Display blogs in grid
    function displayBlogs(blogs) {
        if (!blogs || blogs.length === 0) {
            blogGrid.innerHTML = `
                <div class="no-blogs">
                    <div class="no-blogs-icon">
                        <i class="fas fa-blog"></i>
                    </div>
                    <h3>No Stories Found</h3>
                    <p>No community stories match your current search. Try adjusting your search terms or check back later for new stories.</p>
                    <a href="../landingpage.html#contact" class="btn-share">
                        <i class="fas fa-pen"></i> Share Your Story
                    </a>
                </div>
            `;
            return;
        }

        blogGrid.innerHTML = blogs.map(blog => `
            <div class="blog-card" data-anonymous="${blog.isAnonymous}">
                <div class="blog-card-header">
                    <h3 class="blog-title">${blog.title}</h3>
                    <div class="blog-meta">
                        <div class="author-info">
                            <i class="fas fa-user"></i>
                            <span>${blog.isAnonymous ? 'Anonymous Community Member' : `Author ID: ${blog.authorId}`}</span>
                        </div>
                        <div class="publish-date">
                            <i class="fas fa-calendar"></i>
                            <span>${formatDate(blog.publishedDate)}</span>
                        </div>
                    </div>
                </div>
                
                <div class="blog-content">
                    <p class="blog-excerpt">${truncateText(blog.content, 150)}</p>
                </div>
                
                <div class="blog-card-footer">
                    <div class="blog-tags">
                        ${blog.isAnonymous ? '<span class="tag anonymous">Anonymous</span>' : '<span class="tag named">Named Author</span>'}
                        <span class="tag published">Published</span>
                    </div>
                    <button class="btn-read-more" onclick="openBlogModal(${blog.id})">
                        <i class="fas fa-book-open"></i> Read Full Story
                    </button>
                </div>
            </div>
        `).join('');
    }

    // Update statistics
    function updateStatistics(blogs) {
        const totalBlogs = blogs.length;
        const uniqueAuthors = new Set(blogs.map(b => b.authorId)).size;

        document.getElementById('totalBlogs').textContent = totalBlogs;
        document.getElementById('totalAuthors').textContent = uniqueAuthors;
    }

    // Search functionality
    searchInput.addEventListener('input', (e) => {
        const searchTerm = e.target.value.toLowerCase();
        filteredBlogs = allBlogs.filter(blog => 
            blog.title.toLowerCase().includes(searchTerm) ||
            blog.content.toLowerCase().includes(searchTerm)
        );
        displayBlogs(filteredBlogs);
    });

    // Filter functionality
    filterButtons.forEach(button => {
        button.addEventListener('click', () => {
            // Update active button
            filterButtons.forEach(btn => btn.classList.remove('active'));
            button.classList.add('active');
            
            const filter = button.getAttribute('data-filter');
            
            switch (filter) {
                case 'all':
                    filteredBlogs = allBlogs;
                    break;
                case 'anonymous':
                    filteredBlogs = allBlogs.filter(blog => blog.isAnonymous);
                    break;
                case 'named':
                    filteredBlogs = allBlogs.filter(blog => !blog.isAnonymous);
                    break;
                case 'recent':
                    filteredBlogs = [...allBlogs].sort((a, b) => new Date(b.publishedDate) - new Date(a.publishedDate));
                    break;
            }
            
            displayBlogs(filteredBlogs);
        });
    });

    // Open blog modal
    window.openBlogModal = function(blogId) {
        const blog = allBlogs.find(b => b.id === blogId);
        if (blog) {
            document.getElementById('modalTitle').textContent = blog.title;
            document.getElementById('modalAuthor').textContent = blog.isAnonymous ? 'Anonymous Community Member' : `Author ID: ${blog.authorId}`;
            document.getElementById('modalDate').textContent = formatDate(blog.publishedDate);
            document.getElementById('modalContent').innerHTML = formatContent(blog.content);
            
            document.getElementById('blogModal').style.display = 'flex';
            document.body.style.overflow = 'hidden';
        }
    };

    // Close blog modal
    window.closeBlogModal = function() {
        document.getElementById('blogModal').style.display = 'none';
        document.body.style.overflow = 'auto';
    };

    // Helper functions
    function formatDate(dateString) {
        if (!dateString) return 'Unknown date';
        
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
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
        
        // Convert line breaks to HTML
        return content.replace(/\n/g, '<br>');
    }

    // Get blog status text
    function getBlogStatusText(status) {
        switch (status) {
            case 1: return 'Draft';
            case 2: return 'Pending';
            case 3: return 'Rejected';
            case 4: return 'Published';
            case 5: return 'Archived';
            default: return 'Unknown';
        }
    }

    // Close modal when clicking outside
    document.addEventListener('click', (e) => {
        const modal = document.getElementById('blogModal');
        if (e.target === modal) {
            closeBlogModal();
        }
    });

    // Close modal with Escape key
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeBlogModal();
        }
    });
});
