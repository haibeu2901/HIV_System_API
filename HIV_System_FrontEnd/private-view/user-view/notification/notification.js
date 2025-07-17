document.addEventListener('DOMContentLoaded', () => {
    // Get authentication token from localStorage
    const token = localStorage.getItem("token");
    const userRole = localStorage.getItem("userRole");
    const accId = localStorage.getItem("accId");
    
    console.log('🔐 Authentication Check:');
    console.log('Token:', token ? 'Present' : 'Missing');
    console.log('User Role:', userRole);
    console.log('Account ID:', accId);
    
    // Redirect to login if no token
    if (!token) {
        console.error('❌ No authentication token found');
        alert('You need to be logged in to view notifications. Redirecting to login...');
        window.location.href = '/public-view/landingpage.html';
        return;
    }
    
    // DOM elements
    const notificationList = document.getElementById('notification-list');
    
    // State management
    let notifications = [];
    let unreadNotifications = [];
    let currentFilter = 'all';
    let isLoading = false;
    
    // Initialize notification system
    init();
    
    async function init() {
        console.log('🚀 Initializing Facebook-like notification system...');
        
        // Test token validity
        await testTokenValidity();
        
        // Load notifications
        await loadNotifications();
        
        // Start periodic checking for new notifications
        startPeriodicCheck();
    }
    
    // Test token validity
    async function testTokenValidity() {
        try {
            console.log('🔄 Testing token validity...');
            const response = await fetch('https://localhost:7009/api/Notification/GetUnreadNotifications', {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json",
                    "accept": "*/*"
                }
            });
            
            if (response.status === 401) {
                console.error('❌ Token is invalid or expired');
                localStorage.removeItem('token');
                localStorage.removeItem('userRole');
                localStorage.removeItem('accId');
                alert('Your session has expired. Please log in again.');
                window.location.href = '/public-view/landingpage.html';
                return;
            }
            
            console.log('✅ Token is valid');
        } catch (error) {
            console.error('❌ Token validation failed:', error);
        }
    }
    
    // Load all notifications
    async function loadNotifications() {
        if (isLoading) return;
        isLoading = true;
        
        try {
            showLoadingState();
            
            console.log('🔄 Loading notifications...');
            
            // Fetch both personal and unread notifications
            const [personalResult, unreadResult] = await Promise.allSettled([
                fetchNotifications('/api/Notification/GetPersonalNotifications'),
                fetchNotifications('/api/Notification/GetUnreadNotifications')
            ]);
            
            // Process results
            notifications = personalResult.status === 'fulfilled' ? personalResult.value : [];
            unreadNotifications = unreadResult.status === 'fulfilled' ? unreadResult.value : [];
            
            console.log('✅ Loaded notifications:', {
                total: notifications.length,
                unread: unreadNotifications.length
            });
            
            // Render notifications
            renderNotifications();
            
        } catch (error) {
            console.error('❌ Error loading notifications:', error);
            showErrorState(error);
        } finally {
            isLoading = false;
        }
    }
    
    // Fetch notifications from API
    async function fetchNotifications(endpoint) {
        console.log(`🔄 Fetching from: https://localhost:7009${endpoint}`);
        
        const response = await fetch(`https://localhost:7009${endpoint}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json",
                "accept": "*/*"
            }
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error(`❌ API Error ${response.status}:`, errorText);
            
            if (response.status === 401) {
                localStorage.removeItem('token');
                localStorage.removeItem('userRole');
                localStorage.removeItem('accId');
                alert('Your session has expired. Please log in again.');
                window.location.href = '/public-view/landingpage.html';
                return [];
            }
            
            throw new Error(`API Error ${response.status}: ${errorText}`);
        }
        
        const data = await response.json();
        return Array.isArray(data) ? data : [];
    }
    
    // Show loading state
    function showLoadingState() {
        notificationList.innerHTML = `
            <div class="loading-container">
                <div class="loading-spinner"></div>
                <p>Loading notifications...</p>
            </div>
        `;
    }
    
    // Show error state
    function showErrorState(error) {
        notificationList.innerHTML = `
            <div class="error-container">
                <div class="error-icon">
                    <i class="fa-solid fa-exclamation-triangle"></i>
                </div>
                <h3>Failed to Load Notifications</h3>
                <p>${error.message}</p>
                <button onclick="location.reload()" class="retry-btn">
                    <i class="fa-solid fa-refresh"></i> Try Again
                </button>
            </div>
        `;
    }
    
    // Render notifications with Facebook-like UI
    function renderNotifications() {
        if (notifications.length === 0) {
            notificationList.innerHTML = `
                <div class="empty-state">
                    <div class="empty-icon">
                        <i class="fa-solid fa-bell-slash"></i>
                    </div>
                    <h3>No Notifications</h3>
                    <p>You don't have any notifications yet.</p>
                </div>
            `;
            return;
        }
        
        // Create unread IDs set for quick lookup
        const unreadIds = new Set(unreadNotifications.map(n => n.notiId));
        
        // Sort notifications by date (newest first)
        const sortedNotifications = [...notifications].sort((a, b) => 
            new Date(b.sendAt) - new Date(a.sendAt)
        );
        
        // Create controls
        const controlsHTML = createControlsHTML();
        
        // Create notification cards
        const notificationsHTML = sortedNotifications.map(notification => 
            createNotificationCard(notification, unreadIds.has(notification.notiId))
        ).join('');
        
        notificationList.innerHTML = controlsHTML + notificationsHTML;
        
        // Add event listeners
        addEventListeners();
    }
    
    // Create controls HTML
    function createControlsHTML() {
        const totalCount = notifications.length;
        const unreadCount = unreadNotifications.length;
        
        return `
            <div class="notification-controls">
                <div class="notification-header">
                    <h2>
                        <i class="fa-solid fa-bell"></i>
                        Notifications
                    </h2>
                    <div class="notification-stats">
                        <span class="total-count">${totalCount} total</span>
                        <span class="unread-count">${unreadCount} unread</span>
                    </div>
                </div>
                
                <div class="notification-actions">
                    <div class="filter-tabs">
                        <button class="filter-tab ${currentFilter === 'all' ? 'active' : ''}" data-filter="all">
                            <i class="fa-solid fa-list"></i>
                            All (${totalCount})
                        </button>
                        <button class="filter-tab ${currentFilter === 'unread' ? 'active' : ''}" data-filter="unread">
                            <i class="fa-solid fa-circle-dot"></i>
                            Unread (${unreadCount})
                        </button>
                    </div>
                    
                    <div class="action-buttons">
                        <button class="refresh-btn" onclick="loadNotifications()">
                            <i class="fa-solid fa-refresh"></i>
                            Refresh
                        </button>
                        ${unreadCount > 0 ? `
                            <button class="mark-all-read-btn" onclick="markAllAsRead()">
                                <i class="fa-solid fa-check-double"></i>
                                Mark All Read
                            </button>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
    }
    
    // Create notification card HTML
    function createNotificationCard(notification, isUnread) {
        const timeAgo = formatTimeAgo(notification.sendAt);
        const typeClass = getNotificationTypeClass(notification.notiType);
        const typeIcon = getNotificationTypeIcon(notification.notiType);
        
        return `
            <div class="notification-card ${isUnread ? 'unread' : 'read'}" data-id="${notification.notiId}">
                <div class="notification-avatar">
                    <div class="avatar-icon ${typeClass}">
                        <i class="${typeIcon}"></i>
                    </div>
                    ${isUnread ? '<div class="unread-indicator"></div>' : ''}
                </div>
                
                <div class="notification-content">
                    <div class="notification-header">
                        <div class="notification-type">
                            ${notification.notiType}
                        </div>
                        <div class="notification-time">
                            <i class="fa-regular fa-clock"></i>
                            ${timeAgo}
                        </div>
                    </div>
                    
                    <div class="notification-message">
                        ${notification.notiMessage}
                    </div>
                    
                    <div class="notification-footer">
                        <div class="notification-meta">
                            <span class="notification-id">ID: ${notification.notiId}</span>
                            <span class="notification-date">
                                Created: ${formatDateTime(notification.createdAt)}
                            </span>
                        </div>
                        
                        ${isUnread ? `
                            <div class="notification-actions">
                                <button class="mark-read-btn" data-id="${notification.notiId}">
                                    <i class="fa-solid fa-check"></i>
                                    Mark as Read
                                </button>
                            </div>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
    }
    
    // Add event listeners
    function addEventListeners() {
        // Filter tab listeners
        document.querySelectorAll('.filter-tab').forEach(tab => {
            tab.addEventListener('click', (e) => {
                const filter = e.currentTarget.dataset.filter;
                switchFilter(filter);
            });
        });
        
        // Mark as read button listeners
        document.querySelectorAll('.mark-read-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const notificationId = e.currentTarget.dataset.id;
                markAsRead(notificationId);
            });
        });
        
        // Notification card click listeners
        document.querySelectorAll('.notification-card').forEach(card => {
            card.addEventListener('click', (e) => {
                // Don't trigger if clicking on buttons
                if (e.target.closest('.mark-read-btn') || e.target.closest('.notification-actions')) {
                    return;
                }
                
                const notificationId = card.dataset.id;
                const isUnread = card.classList.contains('unread');
                
                if (isUnread) {
                    markAsRead(notificationId);
                }
            });
        });
    }
    
    // Switch filter
    function switchFilter(filter) {
        currentFilter = filter;
        
        // Update active tab
        document.querySelectorAll('.filter-tab').forEach(tab => {
            tab.classList.remove('active');
        });
        document.querySelector(`[data-filter="${filter}"]`).classList.add('active');
        
        // Filter notifications
        const unreadIds = new Set(unreadNotifications.map(n => n.notiId));
        const filteredNotifications = filter === 'all' 
            ? notifications 
            : notifications.filter(n => unreadIds.has(n.notiId));
        
        // Re-render filtered notifications
        renderFilteredNotifications(filteredNotifications, unreadIds);
    }
    
    // Render filtered notifications
    function renderFilteredNotifications(filteredNotifications, unreadIds) {
        if (filteredNotifications.length === 0) {
            const emptyMessage = currentFilter === 'unread' 
                ? 'No unread notifications' 
                : 'No notifications found';
            
            document.querySelector('.notification-controls').nextSibling.innerHTML = `
                <div class="empty-state">
                    <div class="empty-icon">
                        <i class="fa-solid fa-bell-slash"></i>
                    </div>
                    <h3>${emptyMessage}</h3>
                    <p>You're all caught up!</p>
                </div>
            `;
            return;
        }
        
        // Sort and render
        const sortedNotifications = [...filteredNotifications].sort((a, b) => 
            new Date(b.sendAt) - new Date(a.sendAt)
        );
        
        const notificationsHTML = sortedNotifications.map(notification => 
            createNotificationCard(notification, unreadIds.has(notification.notiId))
        ).join('');
        
        // Update notifications container
        const controlsElement = document.querySelector('.notification-controls');
        controlsElement.insertAdjacentHTML('afterend', notificationsHTML);
        
        // Remove old notification cards
        const oldCards = document.querySelectorAll('.notification-card');
        oldCards.forEach(card => {
            if (!card.closest('.notification-controls')) {
                card.remove();
            }
        });
        
        // Re-add event listeners
        addEventListeners();
    }
    
    // Mark single notification as read
    async function markAsRead(notificationId) {
        try {
            console.log(`🔄 Marking notification ${notificationId} as read...`);
            
            const response = await fetch(`https://localhost:7009/api/Notification/MarkAsRead/${notificationId}`, {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "accept": "*/*"
                }
            });
            
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Failed to mark notification as read: ${response.status} - ${errorText}`);
            }
            
            console.log(`✅ Notification ${notificationId} marked as read`);
            
            // Update UI immediately
            updateNotificationUI(notificationId, false);
            
            // Refresh notifications to sync with server
            await loadNotifications();
            
        } catch (error) {
            console.error('❌ Error marking notification as read:', error);
            alert('Failed to mark notification as read. Please try again.');
        }
    }
    
    // Mark all notifications as read
    async function markAllAsRead() {
        try {
            console.log('🔄 Marking all notifications as read...');
            
            const response = await fetch('https://localhost:7009/api/Notification/MarkAllAsRead', {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "accept": "*/*"
                }
            });
            
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Failed to mark all notifications as read: ${response.status} - ${errorText}`);
            }
            
            console.log('✅ All notifications marked as read');
            
            // Show success message
            showSuccessMessage('All notifications marked as read!');
            
            // Refresh notifications
            await loadNotifications();
            
        } catch (error) {
            console.error('❌ Error marking all notifications as read:', error);
            alert('Failed to mark all notifications as read. Please try again.');
        }
    }
    
    // Update notification UI
    function updateNotificationUI(notificationId, isUnread) {
        const card = document.querySelector(`[data-id="${notificationId}"]`);
        if (card) {
            card.classList.toggle('unread', isUnread);
            card.classList.toggle('read', !isUnread);
            
            // Remove/add unread indicator
            const indicator = card.querySelector('.unread-indicator');
            if (isUnread && !indicator) {
                card.querySelector('.notification-avatar').insertAdjacentHTML('beforeend', '<div class="unread-indicator"></div>');
            } else if (!isUnread && indicator) {
                indicator.remove();
            }
            
            // Remove/add mark as read button
            const markReadBtn = card.querySelector('.mark-read-btn');
            if (!isUnread && markReadBtn) {
                markReadBtn.remove();
            }
        }
    }
    
    // Show success message
    function showSuccessMessage(message) {
        const successDiv = document.createElement('div');
        successDiv.className = 'success-message';
        successDiv.innerHTML = `
            <i class="fa-solid fa-check-circle"></i>
            ${message}
        `;
        
        document.body.appendChild(successDiv);
        
        setTimeout(() => {
            successDiv.remove();
        }, 3000);
    }
    
    // Start periodic check for new notifications
    function startPeriodicCheck() {
        setInterval(async () => {
            try {
                await loadNotifications();
            } catch (error) {
                console.error('❌ Periodic check failed:', error);
            }
        }, 30000); // Check every 30 seconds
    }
    
    // Utility functions
    function formatTimeAgo(dateString) {
        const now = new Date();
        const date = new Date(dateString);
        const diffInSeconds = Math.floor((now - date) / 1000);
        
        if (diffInSeconds < 60) return 'Just now';
        if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m ago`;
        if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h ago`;
        if (diffInSeconds < 604800) return `${Math.floor(diffInSeconds / 86400)}d ago`;
        
        return date.toLocaleDateString();
    }
    
    function formatDateTime(dateString) {
        return new Date(dateString).toLocaleString();
    }
    
    function getNotificationTypeClass(type) {
        const typeMap = {
            'Appt Confirm': 'type-appointment',
            'Appointment Update': 'type-appointment',
            'Appointment Reminder': 'type-reminder',
            'Test Result': 'type-test',
            'Medical Record': 'type-medical',
            'System': 'type-system'
        };
        return typeMap[type] || 'type-general';
    }
    
    function getNotificationTypeIcon(type) {
        const iconMap = {
            'Appt Confirm': 'fa-solid fa-calendar-check',
            'Appointment Update': 'fa-solid fa-calendar-pen',
            'Appointment Reminder': 'fa-solid fa-bell',
            'Test Result': 'fa-solid fa-flask',
            'Medical Record': 'fa-solid fa-file-medical',
            'System': 'fa-solid fa-gear'
        };
        return iconMap[type] || 'fa-solid fa-info-circle';
    }
    
    // Make functions globally available
    window.loadNotifications = loadNotifications;
    window.markAllAsRead = markAllAsRead;
});
