// Patient Community JavaScript
// Global variables
let communityInitialized = false;

// Initialize the community page
function initializeCommunity() {
    if (communityInitialized) return;
    
    console.log('Initializing patient community...');
    
    // Initialize header first
    initializeHeader();
    
    // Initialize blog system after header loads
    setTimeout(() => {
        initializeBlogSystem();
    }, 500);
    
    communityInitialized = true;
}

// Initialize header
function initializeHeader() {
    console.log('Loading header...');
    
    fetch('../header/header.html')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.text();
        })
        .then(html => {
            const headerPlaceholder = document.getElementById('header-placeholder');
            if (headerPlaceholder) {
                headerPlaceholder.innerHTML = html;
                console.log('Header HTML loaded successfully');
                
                // Load header script after HTML is inserted
                loadHeaderScript();
            } else {
                console.error('Header placeholder not found');
            }
        })
        .catch(error => {
            console.error('Error loading header:', error);
            // Show fallback header
            showFallbackHeader();
        });
}

// Load header script
function loadHeaderScript() {
    const script = document.createElement('script');
    script.src = '../header/header.js';
    script.onload = () => {
        console.log('Header script loaded successfully');
    };
    script.onerror = () => {
        console.error('Error loading header script');
    };
    document.head.appendChild(script);
}

// Show fallback header if loading fails
function showFallbackHeader() {
    const headerPlaceholder = document.getElementById('header-placeholder');
    if (headerPlaceholder) {
        headerPlaceholder.innerHTML = `
            <header style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 1rem; color: white;">
                <div style="display: flex; justify-content: space-between; align-items: center; max-width: 1200px; margin: 0 auto;">
                    <div style="display: flex; align-items: center; gap: 1rem;">
                        <i class="fas fa-clinic-medical"></i>
                        <span style="font-weight: 600;">Phòng khám HIV CareFirst</span>
                    </div>
                    <nav style="display: flex; gap: 1rem; align-items: center;">
                        <a href="../view/user-home.html" style="color: white; text-decoration: none; padding: 0.5rem 1rem; border-radius: 4px; transition: background 0.3s;">
                            <i class="fas fa-home"></i> Trang chủ
                        </a>
                        <a href="../community/patient-community.html" style="color: white; text-decoration: none; padding: 0.5rem 1rem; border-radius: 4px; background: rgba(255,255,255,0.2);">
                            <i class="fas fa-users"></i> Cộng đồng
                        </a>
                        <button onclick="logout()" style="background: rgba(255,255,255,0.2); border: none; color: white; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer;">
                            <i class="fas fa-sign-out-alt"></i> Đăng xuất
                        </button>
                    </nav>
                </div>
            </header>
        `;
    }
}

// Initialize blog system
function initializeBlogSystem() {
    console.log('Initializing blog system...');
    
    // Check if blog scripts are loaded
    if (typeof window.BlogRoles === 'undefined' || typeof loadBlogs === 'undefined') {
        console.log('Blog scripts not loaded, loading them...');
        loadBlogScripts().then(() => {
            setTimeout(setupBlogSystem, 200);
        });
    } else {
        setupBlogSystem();
    }
}

// Load blog scripts
function loadBlogScripts() {
    return new Promise((resolve, reject) => {
        console.log('Loading blog role config only (skipping blog-public.js to avoid conflicts)...');
        
        // Only load blog role config, skip blog-public.js to avoid function conflicts
        const roleScript = document.createElement('script');
        roleScript.src = '../../../public-view/blog/blog-role-config.js';
        roleScript.onload = () => {
            console.log('Blog role config loaded');
            resolve();
        };
        roleScript.onerror = () => {
            console.error('Error loading blog-role-config.js');
            resolve(); // Continue anyway
        };
        document.head.appendChild(roleScript);
    });
}

// Setup blog system
function setupBlogSystem() {
    try {
        console.log('Setting up blog system...');
        
        // Clear debug info
        const debugInfo = document.getElementById('debugInfo');
        if (debugInfo) {
            debugInfo.style.display = 'none';
        }

        // Get patient's auth token
        const patientToken = localStorage.getItem('token') || sessionStorage.getItem('token') || localStorage.getItem('authToken');
        console.log('Patient token:', patientToken ? 'Found' : 'Not found');
        
        if (patientToken) {
            // Set up authentication for the blog system
            const userData = {
                userId: parseInt(localStorage.getItem('accId')) || 1,
                role: 'patient',
                token: patientToken,
                username: localStorage.getItem('username') || 'Patient User',
                email: 'patient@example.com'
            };
            
            // Store user data for blog system
            localStorage.setItem('userData', JSON.stringify(userData));
            
            // Check if BlogRoles is available
            if (typeof window.BlogRoles !== 'undefined') {
                // Initialize the blog system
                window.currentUser = new window.BlogRoles.BlogUser(userData);
                window.isAuthenticated = true;
                console.log('Blog system authenticated');
            }
        }
        
        // Always load blogs using our custom implementation
        console.log('Loading blogs using direct fetch...');
        fetchBlogsDirectly();
        
    } catch (error) {
        console.error('Error setting up blog system:', error);
        showBlogError('Error: ' + error.message);
    }
}

// Show blog error
function showBlogError(message) {
    const debugInfo = document.getElementById('debugInfo');
    if (debugInfo) {
        debugInfo.innerHTML = `
            <div style="text-align: center; padding: 20px; color: #e74c3c;">
                <i class="fas fa-exclamation-triangle" style="font-size: 32px; margin-bottom: 12px;"></i>
                <p style="margin: 8px 0;">${message}</p>
                <button onclick="retryBlogLoad()" style="margin-top: 12px; padding: 8px 16px; background: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer;">
                    <i class="fas fa-refresh"></i> Retry
                </button>
            </div>
        `;
        debugInfo.style.display = 'block';
    }
}

// Retry blog loading
function retryBlogLoad() {
    console.log('Retrying blog load...');
    
    // Always use our custom implementation
    fetchBlogsDirectly();
}

// Direct blog fetch fallback
async function fetchBlogsDirectly() {
    console.log('Attempting direct blog fetch...');
    const blogGrid = document.getElementById('blogGrid');
    const loadingContainer = document.getElementById('loadingContainer');
    
    if (loadingContainer) {
        loadingContainer.style.display = 'flex';
    }
    
    try {
        // Use SocialBlog API with correct endpoint and CORS handling
        const response = await fetch('https://localhost:7009/api/SocialBlog/GetAllBlog', {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            },
            mode: 'cors',
            credentials: 'omit'
        });
        console.log('API Response status:', response.status);
        
        if (response.ok) {
            const blogs = await response.json();
            console.log('Fetched blogs:', blogs.length);
            
            if (blogGrid) {
                if (blogs.length === 0) {
                    blogGrid.innerHTML = `
                        <div class="empty-state">
                            <i class="fas fa-newspaper"></i>
                            <h3>No stories yet</h3>
                            <p>Be the first to share your story with the community!</p>
                            <button onclick="showCreatePostModal()" class="btn-primary">
                                <i class="fas fa-pen"></i> Share Your Story
                            </button>
                        </div>
                    `;
                } else {
                    // Display blogs with proper styling
                    const blogHTML = blogs.map(blog => `
                        <div class="blog-card">
                            <div class="blog-header">
                                <h3 class="blog-title">${blog.title || 'Untitled'}</h3>
                                <div class="blog-meta">
                                    <span class="blog-author">
                                        <i class="fas fa-user"></i>
                                        ${blog.isAnonymous ? 'Anonymous' : (blog.userName || 'Unknown Author')}
                                    </span>
                                    <span class="blog-date">
                                        <i class="fas fa-calendar"></i>
                                        ${blog.dateCreated ? new Date(blog.dateCreated).toLocaleDateString() : 'Unknown date'}
                                    </span>
                                </div>
                            </div>
                            <div class="blog-content">
                                <p>${(blog.content || '').substring(0, 200)}${blog.content && blog.content.length > 200 ? '...' : ''}</p>
                            </div>
                            <div class="blog-actions">
                                <button class="blog-action-btn" onclick="openBlog(${blog.blogId})">
                                    <i class="fas fa-book-open"></i> Read More
                                </button>
                                <div class="blog-stats">
                                    <span><i class="fas fa-heart"></i> ${blog.likeCount || 0}</span>
                                    <span><i class="fas fa-comment"></i> ${blog.commentCount || 0}</span>
                                </div>
                            </div>
                        </div>
                    `).join('');
                    
                    blogGrid.innerHTML = blogHTML;
                    
                    // Update statistics
                    updateBlogStats(blogs);
                }
            }
        } else {
            throw new Error(`API returned ${response.status}`);
        }
    } catch (error) {
        console.error('Direct fetch error:', error);
        
        // Try HTTP fallback if HTTPS fails
        if (error.message.includes('Failed to fetch') || error.name === 'TypeError') {
            console.log('Attempting HTTP fallback for blog fetch...');
            
            try {
                const fallbackResponse = await fetch('http://localhost:7009/api/SocialBlog/GetAllBlog', {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json'
                    },
                    mode: 'cors',
                    credentials: 'omit'
                });
                
                if (fallbackResponse.ok) {
                    const blogs = await fallbackResponse.json();
                    console.log('Fetched blogs via HTTP fallback:', blogs.length);
                    
                    if (blogGrid) {
                        if (blogs.length === 0) {
                            blogGrid.innerHTML = `
                                <div class="empty-state">
                                    <i class="fas fa-newspaper"></i>
                                    <h3>No stories yet</h3>
                                    <p>Be the first to share your story with the community!</p>
                                    <button onclick="showCreatePostModal()" class="btn-primary">
                                        <i class="fas fa-pen"></i> Share Your Story
                                    </button>
                                </div>
                            `;
                        } else {
                            const blogHTML = blogs.map(blog => `
                                <div class="blog-card">
                                    <div class="blog-header">
                                        <h3 class="blog-title">${blog.title || 'Untitled'}</h3>
                                        <div class="blog-meta">
                                            <span class="blog-author">
                                                <i class="fas fa-user"></i>
                                                ${blog.isAnonymous ? 'Anonymous' : (blog.userName || 'Unknown Author')}
                                            </span>
                                            <span class="blog-date">
                                                <i class="fas fa-calendar"></i>
                                                ${blog.dateCreated ? new Date(blog.dateCreated).toLocaleDateString() : 'Unknown date'}
                                            </span>
                                        </div>
                                    </div>
                                    <div class="blog-content">
                                        <p>${(blog.content || '').substring(0, 200)}${blog.content && blog.content.length > 200 ? '...' : ''}</p>
                                    </div>
                                    <div class="blog-actions">
                                        <button class="blog-action-btn" onclick="openBlog(${blog.blogId})">
                                            <i class="fas fa-book-open"></i> Read More
                                        </button>
                                        <div class="blog-stats">
                                            <span><i class="fas fa-heart"></i> ${blog.likeCount || 0}</span>
                                            <span><i class="fas fa-comment"></i> ${blog.commentCount || 0}</span>
                                        </div>
                                    </div>
                                </div>
                            `).join('');
                            
                            blogGrid.innerHTML = blogHTML;
                            updateBlogStats(blogs);
                        }
                    }
                    
                    if (loadingContainer) {
                        loadingContainer.style.display = 'none';
                    }
                    return; // Success, exit function
                }
            } catch (fallbackError) {
                console.error('HTTP fallback for blog fetch also failed:', fallbackError);
            }
        }
        
        if (blogGrid) {
            blogGrid.innerHTML = `
                <div class="error-state">
                    <i class="fas fa-exclamation-triangle"></i>
                    <h3>Unable to load stories</h3>
                    <p>Please check your connection and try again.</p>
                    <div style="margin-top: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px; font-size: 0.9em; color: #666;">
                        <strong>Troubleshooting:</strong><br>
                        • Make sure the API server is running on localhost:7009<br>
                        • Check if CORS is enabled on the server<br>
                        • Try refreshing the page
                    </div>
                    <button onclick="retryBlogLoad()" class="btn-primary" style="margin-top: 15px;">
                        <i class="fas fa-refresh"></i> Retry
                    </button>
                </div>
            `;
        }
    } finally {
        if (loadingContainer) {
            loadingContainer.style.display = 'none';
        }
    }
}

// Update blog statistics
function updateBlogStats(blogs) {
    const totalBlogs = document.getElementById('totalBlogs');
    const totalAuthors = document.getElementById('totalAuthors');
    const totalLikes = document.getElementById('totalLikes');
    
    if (totalBlogs) totalBlogs.textContent = blogs.length;
    
    if (totalAuthors) {
        const uniqueAuthors = new Set(blogs.filter(b => !b.isAnonymous).map(b => b.userName));
        totalAuthors.textContent = uniqueAuthors.size;
    }
    
    if (totalLikes) {
        const likes = blogs.reduce((sum, blog) => sum + (blog.likeCount || 0), 0);
        totalLikes.textContent = likes;
    }
}

// Modal functions
function showCreatePostModal() {
    console.log('Showing create post modal...');
    
    // Show modal directly
    const modal = document.getElementById('createPostModal');
    if (modal) {
        modal.style.display = 'block';
        console.log('Modal shown successfully');
        
        // Initialize character counters and event listeners
        setTimeout(() => {
            initializeModalEventListeners();
            updateCharacterCounters();
        }, 100);
    } else {
        console.error('Create post modal not found');
    }
}

// Initialize event listeners for the modal
function initializeModalEventListeners() {
    const titleInput = document.getElementById('blogTitle');
    const contentInput = document.getElementById('blogContent');
    const notesInput = document.getElementById('blogNotes');
    
    // Add input event listeners for character counting
    if (titleInput) {
        titleInput.addEventListener('input', updateCharacterCounters);
    }
    
    if (contentInput) {
        contentInput.addEventListener('input', updateCharacterCounters);
    }
    
    if (notesInput) {
        notesInput.addEventListener('input', updateCharacterCounters);
    }
    
    console.log('Modal event listeners initialized');
}

function closeCreatePostModal() {
    const modal = document.getElementById('createPostModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

// Create a new blog post using the SocialBlog API
async function createBlog() {
    console.log('Creating blog post...');
    
    // Get form elements
    const titleInput = document.getElementById('blogTitle');
    const contentInput = document.getElementById('blogContent');
    const notesInput = document.getElementById('blogNotes');
    const isAnonymousInput = document.getElementById('isAnonymous');
    const formStatus = document.getElementById('formStatus');
    
    // Validate required fields
    if (!titleInput || !contentInput) {
        console.error('Required form elements not found');
        return;
    }
    
    const title = titleInput.value.trim();
    const content = contentInput.value.trim();
    const notes = notesInput ? notesInput.value.trim() : '';
    const isAnonymous = isAnonymousInput ? isAnonymousInput.checked : false;
    
    // Validation
    if (!title) {
        showFormStatus('Please enter a title for your story', 'error');
        titleInput.focus();
        return;
    }
    
    if (!content) {
        showFormStatus('Please enter your story content', 'error');
        contentInput.focus();
        return;
    }
    
    if (title.length > 200) {
        showFormStatus('Title must be 200 characters or less', 'error');
        titleInput.focus();
        return;
    }
    
    if (notes.length > 500) {
        showFormStatus('Notes must be 500 characters or less', 'error');
        notesInput.focus();
        return;
    }
    
    try {
        // Get user ID from localStorage or use default
        const userData = JSON.parse(localStorage.getItem('userData') || '{}');
        const authorId = userData.userId || 1; // Default to 1 if no user data found
        
        // Show loading status
        showFormStatus('Creating your story...', 'loading');
        
        // Prepare the request body
        const requestBody = {
            authorId: authorId,
            title: title,
            content: content,
            isAnonymous: isAnonymous,
            notes: notes || ""
        };
        
        console.log('Sending blog creation request:', requestBody);
        
        // Make API call with CORS handling
        const response = await fetch('https://localhost:7009/api/SocialBlog/CreateBlog', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            mode: 'cors',
            credentials: 'omit',
            body: JSON.stringify(requestBody)
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error('API Error:', response.status, errorText);
            throw new Error(`Failed to create blog: ${response.status} ${response.statusText}`);
        }
        
        const result = await response.json();
        console.log('Blog created successfully:', result);
        
        // Show success message
        showFormStatus('Your story has been shared successfully!', 'success');
        
        // Clear form
        titleInput.value = '';
        contentInput.value = '';
        if (notesInput) notesInput.value = '';
        if (isAnonymousInput) isAnonymousInput.checked = false;
        
        // Update character counters
        updateCharacterCounters();
        
        // Close modal after a short delay
        setTimeout(() => {
            closeCreatePostModal();
            // Refresh the blog list
            fetchBlogsDirectly();
        }, 2000);
        
    } catch (error) {
        console.error('Error creating blog:', error);
        
        // Check if it's a CORS or network error and suggest solutions
        if (error.message.includes('Failed to fetch') || error.name === 'TypeError') {
            console.log('Attempting fallback to HTTP...');
            
            try {
                // Try HTTP instead of HTTPS
                const fallbackResponse = await fetch('http://localhost:7009/api/SocialBlog/CreateBlog', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    mode: 'cors',
                    credentials: 'omit',
                    body: JSON.stringify(requestBody)
                });
                
                if (fallbackResponse.ok) {
                    const result = await fallbackResponse.json();
                    console.log('Blog created successfully via HTTP fallback:', result);
                    
                    showFormStatus('Your story has been shared successfully!', 'success');
                    
                    // Clear form
                    titleInput.value = '';
                    contentInput.value = '';
                    if (notesInput) notesInput.value = '';
                    if (isAnonymousInput) isAnonymousInput.checked = false;
                    
                    updateCharacterCounters();
                    
                    setTimeout(() => {
                        closeCreatePostModal();
                        fetchBlogsDirectly();
                    }, 2000);
                    
                    return; // Success, exit function
                }
            } catch (fallbackError) {
                console.error('HTTP fallback also failed:', fallbackError);
            }
            
            showFormStatus(`Error: Unable to connect to server. Please check:\n1. API server is running on localhost:7009\n2. CORS is enabled on the server\n3. Try refreshing the page`, 'error');
        } else {
            showFormStatus(`Error creating story: ${error.message}`, 'error');
        }
    }
}

// Show form status messages
function showFormStatus(message, type) {
    const formStatus = document.getElementById('formStatus');
    if (!formStatus) return;
    
    // Handle multi-line messages
    const formattedMessage = message.replace(/\n/g, '<br>');
    formStatus.innerHTML = formattedMessage;
    formStatus.className = `form-status ${type}`;
    formStatus.style.display = 'block';
    
    // Auto-hide success messages
    if (type === 'success') {
        setTimeout(() => {
            formStatus.style.display = 'none';
        }, 3000);
    }
}

// Update character counters for form inputs
function updateCharacterCounters() {
    const titleInput = document.getElementById('blogTitle');
    const contentInput = document.getElementById('blogContent');
    const notesInput = document.getElementById('blogNotes');
    
    const titleCounter = document.getElementById('titleCharCount');
    const contentCounter = document.getElementById('contentCharCount');
    const notesCounter = document.getElementById('notesCharCount');
    
    if (titleInput && titleCounter) {
        titleCounter.textContent = `${titleInput.value.length}/200`;
    }
    
    if (contentInput && contentCounter) {
        contentCounter.textContent = `${contentInput.value.length}`;
    }
    
    if (notesInput && notesCounter) {
        notesCounter.textContent = `${notesInput.value.length}/500`;
    }
}

function closeBlogModal() {
    const modal = document.getElementById('blogModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

function openBlog(blogId) {
    console.log('Opening blog:', blogId);
    // This would typically open the blog in a modal or navigate to a detail page
    // For now, just show an alert
    alert(`Opening blog ${blogId} - Feature coming soon!`);
}

// Global logout function
function logout() {
    localStorage.clear();
    sessionStorage.clear();
    window.location.href = '/public-view/landingpage.html';
}

// Test API connection
async function testApiConnection() {
    console.log('Testing API connection...');
    
    const endpoints = [
        'https://localhost:7009/api/SocialBlog/GetAllBlog',
        'http://localhost:7009/api/SocialBlog/GetAllBlog'
    ];
    
    for (const endpoint of endpoints) {
        try {
            console.log(`Testing ${endpoint}...`);
            const response = await fetch(endpoint, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                },
                mode: 'cors',
                credentials: 'omit'
            });
            
            if (response.ok) {
                console.log(`✅ API connection successful: ${endpoint}`);
                const data = await response.json();
                console.log(`✅ Response data:`, data);
                return endpoint;
            } else {
                console.log(`❌ API response not OK: ${response.status} ${response.statusText}`);
            }
        } catch (error) {
            console.log(`❌ API connection failed for ${endpoint}:`, error.message);
        }
    }
    
    console.log('❌ All API endpoints failed');
    return null;
}

// Debug function to test blog loading
function debugLoadBlogs() {
    console.log('Debug: Force loading blogs...');
    fetchBlogsDirectly();
}

// Debug function to test blog creation
async function debugCreateBlog() {
    console.log('Debug: Testing blog creation...');
    
    const testBlog = {
        authorId: 1,
        title: "Test Blog Post",
        content: "This is a test blog post to verify the API is working correctly.",
        isAnonymous: false,
        notes: "Created via debug function"
    };
    
    const endpoints = [
        'https://localhost:7009/api/SocialBlog/CreateBlog',
        'http://localhost:7009/api/SocialBlog/CreateBlog'
    ];
    
    for (const endpoint of endpoints) {
        try {
            console.log(`Testing blog creation at ${endpoint}...`);
            const response = await fetch(endpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                mode: 'cors',
                credentials: 'omit',
                body: JSON.stringify(testBlog)
            });
            
            console.log('Debug create response status:', response.status);
            
            if (response.ok) {
                const result = await response.json();
                console.log('Debug create result:', result);
                alert(`✅ Test blog created successfully at ${endpoint}!`);
                // Refresh the blog list
                fetchBlogsDirectly();
                return;
            } else {
                const errorText = await response.text();
                console.error(`Debug create error at ${endpoint}:`, errorText);
            }
        } catch (error) {
            console.error(`Debug create exception at ${endpoint}:`, error);
        }
    }
    
    alert('❌ All endpoints failed. Please check if the API server is running and CORS is enabled.');
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded, initializing community...');
    initializeCommunity();
});

// Also initialize on window load as backup
window.addEventListener('load', function() {
    console.log('Window loaded, checking community initialization...');
    if (!communityInitialized) {
        setTimeout(initializeCommunity, 100);
    }
});
