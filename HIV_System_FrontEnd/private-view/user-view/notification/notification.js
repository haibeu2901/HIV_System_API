document.addEventListener('DOMContentLoaded', () => {
    // Check if token exists
    const token = localStorage.getItem("token");
    if (!token) {
        console.error('No authentication token found');
        alert('You need to be logged in to view notifications. Redirecting to login...');
        window.location.href = '/public-view/landingpage.html';
        return;
    }
    
    console.log('Token found:', token ? 'Present' : 'Missing');
    
    // For main notification page
    const list = document.getElementById('notification-list');
    
    if (list) {
        loadMainNotifications();
    }

    // For popup notification bell
    const bell = document.getElementById('notificationBell');
    const popup = document.getElementById('notification-popup');
    const closeBtn = document.getElementById('closeNotificationPopup');
    const popupList = document.getElementById('popup-notification-list');
    const tabAll = document.getElementById('tab-all');
    const tabUnread = document.getElementById('tab-unread');
    const markAllReadBtn = document.getElementById('mark-all-read-btn');
    const unreadCountBadge = document.getElementById('unread-count');
    let notifications = [];
    let unreadNotifications = [];
    let currentFilter = 'all';

    // Load main page notifications
    async function loadMainNotifications() {
        list.innerHTML = "<p>Loading...</p>";
        
        try {
            // Try fetching notifications one by one to identify which endpoint is failing
            console.log('Fetching all notifications...');
            const allNotifications = await fetchNotifications('/api/Notification/GetPersonalNotifications');
            console.log('All notifications fetched successfully:', allNotifications);
            
            console.log('Fetching unread notifications...');
            const unreadNotifications = await fetchNotifications('/api/Notification/GetUnreadNotifications');
            console.log('Unread notifications fetched successfully:', unreadNotifications);
            
            console.log('All notifications:', allNotifications);
            console.log('Unread notifications:', unreadNotifications);
            
            if (!allNotifications || allNotifications.length === 0) {
                list.innerHTML = `
                    <div class="no-notifications">
                        <div class="no-notifications-icon">
                            <i class="fa-solid fa-bell-slash"></i>
                        </div>
                        <h3>No Notifications</h3>
                        <p>You don't have any notifications yet.</p>
                    </div>
                `;
                return;
            }
            
            // Sort notifications by sendAt date (newest first)
            allNotifications.sort((a, b) => new Date(b.sendAt) - new Date(a.sendAt));
            
            // Create a set of unread notification IDs for quick lookup
            const unreadIds = new Set(unreadNotifications.map(n => n.notiId));
            
            // Add controls
            const controlsHTML = `
                <div class="notification-controls">
                    <div class="filter-tabs">
                        <button class="filter-tab active" data-filter="all">All (${allNotifications.length})</button>
                        <button class="filter-tab" data-filter="unread">Unread (${unreadNotifications.length})</button>
                    </div>
                    <button class="mark-all-read-btn">
                        <i class="fa-solid fa-check-double"></i> Mark All as Read
                    </button>
                </div>
            `;
            
            list.innerHTML = controlsHTML + '<div id="notifications-container"></div>';
            
            // Add event listeners for filter tabs
            document.querySelectorAll('.filter-tab').forEach(tab => {
                tab.addEventListener('click', () => {
                    document.querySelectorAll('.filter-tab').forEach(t => t.classList.remove('active'));
                    tab.classList.add('active');
                    const filter = tab.dataset.filter;
                    filterMainNotifications(allNotifications, unreadIds, filter);
                });
            });
            
            // Initial render
            filterMainNotifications(allNotifications, unreadIds, 'all');
            
        } catch (error) {
            console.error('Error loading notifications:', error);
            list.innerHTML = `
                <div class="error-container">
                    <div class="error-icon">
                        <i class="fa-solid fa-exclamation-triangle"></i>
                    </div>
                    <h3>Error Loading Notifications</h3>
                    <p>${error.message}</p>
                    <button onclick="location.reload()" class="retry-btn">
                        <i class="fa-solid fa-refresh"></i> Try Again
                    </button>
                </div>
            `;
        }
    }

    // Filter main notifications
    function filterMainNotifications(allNotifications, unreadIds, filter) {
        const container = document.getElementById('notifications-container');
        let filteredNotifications = allNotifications;
        
        if (filter === 'unread') {
            filteredNotifications = allNotifications.filter(n => unreadIds.has(n.notiId));
        }
        
        if (filteredNotifications.length === 0) {
            container.innerHTML = `
                <div class="no-notifications">
                    <div class="no-notifications-icon">
                        <i class="fa-solid fa-bell-slash"></i>
                    </div>
                    <h3>No ${filter === 'unread' ? 'Unread' : ''} Notifications</h3>
                    <p>You don't have any ${filter === 'unread' ? 'unread' : ''} notifications.</p>
                </div>
            `;
            return;
        }
        
        container.innerHTML = filteredNotifications.map(noti => {
            const isUnread = unreadIds.has(noti.notiId);
            return `
                <div class="notification-card ${isUnread ? 'unread' : 'read'}" data-notification-id="${noti.notiId}">
                    <div class="notification-header">
                        <div class="notification-icon ${getNotificationTypeClass(noti.notiType)}">
                            <i class="${getNotificationIcon(noti.notiType)}"></i>
                        </div>
                        <div class="notification-type">${noti.notiType}</div>
                        <div class="notification-date">
                            <i class="fa-regular fa-clock"></i>
                            ${formatDateTime(noti.sendAt)}
                        </div>
                        ${isUnread ? '<div class="unread-indicator"></div>' : ''}
                    </div>
                    <div class="notification-content">
                        <div class="notification-message">${noti.notiMessage}</div>
                        <div class="notification-meta">
                            <span class="notification-id">ID: ${noti.notiId}</span>
                            <span class="notification-created">Created: ${formatDateTime(noti.createdAt)}</span>
                            <div class="notification-actions">
                                ${isUnread ? `<button class="mark-read-btn" data-notification-id="${noti.notiId}">
                                    <i class="fa-solid fa-check"></i> Mark as Read
                                </button>` : ''}
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
    }

    // Fetch notifications from API
    async function fetchNotifications(endpoint) {
        console.log(`Fetching from: https://localhost:7009${endpoint}`);
        console.log(`Token: ${token ? 'Present' : 'Missing'}`);
        
        try {
            const response = await fetch(`https://localhost:7009${endpoint}`, {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "accept": "*/*"
                }
            });
            
            console.log(`Response status for ${endpoint}:`, response.status);
            console.log(`Response ok:`, response.ok);
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error(`Error response body:`, errorText);
                throw new Error(`Failed to fetch notifications: ${response.status} - ${errorText}`);
            }
            
            const data = await response.json();
            console.log(`Data received from ${endpoint}:`, data);
            return data;
        } catch (error) {
            console.error(`Fetch error for ${endpoint}:`, error);
            throw error;
        }
    }

    // Load notifications for popup
    async function loadPopupNotifications() {
        if (!popupList) return;
        
        popupList.innerHTML = "<p style='padding:16px;'>Loading...</p>";
        try {
            const [allNotifications, unreadNotifications, unreadCount] = await Promise.all([
                fetchNotifications('/api/Notification/GetPersonalNotifications'),
                fetchNotifications('/api/Notification/GetUnreadNotifications'),
                fetchUnreadCount()
            ]);
            
            notifications = allNotifications;
            unreadNotifications = unreadNotifications;
            
            // Update unread count badge
            updateUnreadCountBadge(unreadCount);
            
            // Sort notifications by sendAt date (newest first)
            notifications.sort((a, b) => new Date(b.sendAt) - new Date(a.sendAt));
            unreadNotifications.sort((a, b) => new Date(b.sendAt) - new Date(a.sendAt));
            
            renderPopupNotifications(currentFilter);
        } catch (err) {
            console.error('Error loading popup notifications:', err);
            popupList.innerHTML = `<p style="color:red;padding:16px;">${err.message}</p>`;
        }
    }

    // Fetch unread count
    async function fetchUnreadCount() {
        const response = await fetch('https://localhost:7009/api/Notification/GetUnreadCount', {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "accept": "*/*"
            }
        });
        
        if (!response.ok) {
            throw new Error(`Failed to fetch unread count: ${response.status}`);
        }
        
        return await response.json();
    }

    // Update unread count badge
    function updateUnreadCountBadge(count) {
        if (unreadCountBadge) {
            if (count > 0) {
                unreadCountBadge.textContent = count > 99 ? '99+' : count;
                unreadCountBadge.style.display = 'block';
            } else {
                unreadCountBadge.style.display = 'none';
            }
        }
    }

    // Render popup notifications
    function renderPopupNotifications(type) {
        if (!popupList) return;
        
        let filtered = type === 'unread' ? unreadNotifications : notifications;
        const unreadIds = new Set(unreadNotifications.map(n => n.notiId));
        
        if (!filtered.length) {
            popupList.innerHTML = `<p style="padding:16px;">No ${type === 'unread' ? 'unread' : ''} notifications found.</p>`;
            return;
        }
        
        popupList.innerHTML = filtered.slice(0, 10).map(noti => {
            const isUnread = unreadIds.has(noti.notiId);
            return `
                <div class="popup-notification-card ${isUnread ? 'unread' : 'read'}" 
                     data-notification-id="${noti.notiId}">
                    <div class="popup-avatar ${getNotificationTypeClass(noti.notiType)}">
                        <i class="${getNotificationIcon(noti.notiType)}"></i>
                    </div>
                    <div class="popup-content">
                        <div class="popup-type">${noti.notiType}</div>
                        <div class="popup-message">${noti.notiMessage}</div>
                        <div class="popup-time">${formatDateTime(noti.sendAt)}</div>
                    </div>
                    ${isUnread ? '<div class="popup-unread-indicator"></div>' : ''}
                </div>
            `;
        }).join('');
    }

    // Mark single notification as read
    async function markAsRead(notificationId) {
        console.log(`Marking notification ${notificationId} as read`);
        console.log(`Token: ${token ? 'Present' : 'Missing'}`);
        
        try {
            const response = await fetch(`https://localhost:7009/api/Notification/MarkAsRead/${notificationId}`, {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "accept": "*/*"
                }
            });
            
            console.log(`Mark as read response status:`, response.status);
            console.log(`Mark as read response ok:`, response.ok);
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error(`Mark as read error response:`, errorText);
                throw new Error(`Failed to mark notification as read: ${response.status} - ${errorText}`);
            }
            
            console.log(`Successfully marked notification ${notificationId} as read`);
            
            // Refresh the page
            location.reload();
        } catch (error) {
            console.error('Error marking notification as read:', error);
            alert('Failed to mark notification as read. Please try again.');
        }
    }

    // Mark all notifications as read
    async function markAllAsRead() {
        console.log(`Marking all notifications as read`);
        console.log(`Token: ${token ? 'Present' : 'Missing'}`);
        
        try {
            const response = await fetch('https://localhost:7009/api/Notification/MarkAllAsRead', {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "accept": "*/*"
                }
            });
            
            console.log(`Mark all as read response status:`, response.status);
            console.log(`Mark all as read response ok:`, response.ok);
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error(`Mark all as read error response:`, errorText);
                throw new Error(`Failed to mark all notifications as read: ${response.status} - ${errorText}`);
            }
            
            console.log(`Successfully marked all notifications as read`);
            
            // Refresh the page
            location.reload();
        } catch (error) {
            console.error('Error marking all notifications as read:', error);
            alert('Failed to mark all notifications as read. Please try again.');
        }
    }

    // Mark as read and navigate (for popup clicks)
    async function markAsReadAndNavigate(notificationId) {
        await markAsRead(notificationId);
        // You can add navigation logic here if needed
        window.location.href = '/private-view/user-view/notification/notification.html';
    }

    // Chỉ thêm event listener một lần duy nhất
    document.addEventListener('click', function(e) {
        // Handle mark single notification as read
        if (e.target.classList.contains('mark-read-btn') || e.target.closest('.mark-read-btn')) {
            const button = e.target.classList.contains('mark-read-btn') ? e.target : e.target.closest('.mark-read-btn');
            const notificationId = button.getAttribute('data-notification-id');
            if (notificationId) {
                markAsRead(parseInt(notificationId));
            }
        }
        
        // Handle mark all as read
        if (e.target.classList.contains('mark-all-read-btn') || e.target.closest('.mark-all-read-btn')) {
            markAllAsRead();
        }
        
        // Handle popup notification click
        const popupCard = e.target.closest('.popup-notification-card');
        if (popupCard) {
            const notificationId = popupCard.getAttribute('data-notification-id');
            if (notificationId) {
                markAsReadAndNavigate(parseInt(notificationId));
            }
        }
    });

    // Update the controlsHTML to remove onclick
    const controlsHTML = `
        <div class="notification-controls">
            <div class="filter-tabs">
                <button class="filter-tab active" data-filter="all">All (${allNotifications.length})</button>
                <button class="filter-tab" data-filter="unread">Unread (${unreadNotifications.length})</button>
            </div>
            <button class="mark-all-read-btn">
                <i class="fa-solid fa-check-double"></i> Mark All as Read
            </button>
        </div>
    `;

    // Remove event listener from filterMainNotifications function
    function filterMainNotifications(allNotifications, unreadIds, filter) {
        const container = document.getElementById('notifications-container');
        let filteredNotifications = allNotifications;
        
        if (filter === 'unread') {
            filteredNotifications = allNotifications.filter(n => unreadIds.has(n.notiId));
        }
        
        if (filteredNotifications.length === 0) {
            container.innerHTML = `
                <div class="no-notifications">
                    <div class="no-notifications-icon">
                        <i class="fa-solid fa-bell-slash"></i>
                    </div>
                    <h3>No ${filter === 'unread' ? 'Unread' : ''} Notifications</h3>
                    <p>You don't have any ${filter === 'unread' ? 'unread' : ''} notifications.</p>
                </div>
            `;
            return;
        }
        
        container.innerHTML = filteredNotifications.map(noti => {
            const isUnread = unreadIds.has(noti.notiId);
            return `
                <div class="notification-card ${isUnread ? 'unread' : 'read'}" data-notification-id="${noti.notiId}">
                    <div class="notification-header">
                        <div class="notification-icon ${getNotificationTypeClass(noti.notiType)}">
                            <i class="${getNotificationIcon(noti.notiType)}"></i>
                        </div>
                        <div class="notification-type">${noti.notiType}</div>
                        <div class="notification-date">
                            <i class="fa-regular fa-clock"></i>
                            ${formatDateTime(noti.sendAt)}
                        </div>
                        ${isUnread ? '<div class="unread-indicator"></div>' : ''}
                    </div>
                    <div class="notification-content">
                        <div class="notification-message">${noti.notiMessage}</div>
                        <div class="notification-meta">
                            <span class="notification-id">ID: ${noti.notiId}</span>
                            <span class="notification-created">Created: ${formatDateTime(noti.createdAt)}</span>
                            <div class="notification-actions">
                                ${isUnread ? `<button class="mark-read-btn" data-notification-id="${noti.notiId}">
                                    <i class="fa-solid fa-check"></i> Mark as Read
                                </button>` : ''}
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
    }

    // Update popup notifications to remove onclick
    function renderPopupNotifications(type) {
        if (!popupList) return;
        
        let filtered = type === 'unread' ? unreadNotifications : notifications;
        const unreadIds = new Set(unreadNotifications.map(n => n.notiId));
        
        if (!filtered.length) {
            popupList.innerHTML = `<p style="padding:16px;">No ${type === 'unread' ? 'unread' : ''} notifications found.</p>`;
            return;
        }
        
        popupList.innerHTML = filtered.slice(0, 10).map(noti => {
            const isUnread = unreadIds.has(noti.notiId);
            return `
                <div class="popup-notification-card ${isUnread ? 'unread' : 'read'}" 
                     data-notification-id="${noti.notiId}">
                    <div class="popup-avatar ${getNotificationTypeClass(noti.notiType)}">
                        <i class="${getNotificationIcon(noti.notiType)}"></i>
                    </div>
                    <div class="popup-content">
                        <div class="popup-type">${noti.notiType}</div>
                        <div class="popup-message">${noti.notiMessage}</div>
                        <div class="popup-time">${formatDateTime(noti.sendAt)}</div>
                    </div>
                    ${isUnread ? '<div class="popup-unread-indicator"></div>' : ''}
                </div>
            `;
        }).join('');
    }

    // Popup event handlers
    if (bell && popup && closeBtn) {
        bell.addEventListener('click', () => {
            popup.classList.toggle('hidden');
            if (!popup.classList.contains('hidden')) {
                loadPopupNotifications();
            }
        });
        
        closeBtn.addEventListener('click', () => {
            popup.classList.add('hidden');
        });
        
        document.addEventListener('click', (e) => {
            if (!popup.contains(e.target) && e.target !== bell) {
                popup.classList.add('hidden');
            }
        });
        
        popup.addEventListener('click', (e) => {
            e.stopPropagation();
        });
    }

    // Tab switching for popup
    if (tabAll && tabUnread) {
        tabAll.addEventListener('click', () => {
            tabAll.classList.add('active');
            tabUnread.classList.remove('active');
            currentFilter = 'all';
            renderPopupNotifications('all');
        });
        
        tabUnread.addEventListener('click', () => {
            tabAll.classList.remove('active');
            tabUnread.classList.add('active');
            currentFilter = 'unread';
            renderPopupNotifications('unread');
        });
    }

    // Mark all as read button in popup
    if (markAllReadBtn) {
        markAllReadBtn.addEventListener('click', markAllAsRead);
    }

    // Load unread count on page load
    if (unreadCountBadge) {
        fetchUnreadCount().then(updateUnreadCountBadge).catch(console.error);
    }

    // Helper functions
    function formatDateTime(dateStr) {
        if (!dateStr) return 'N/A';
        
        try {
            const d = new Date(dateStr);
            const now = new Date();
            const diff = now.getTime() - d.getTime();
            const minutes = Math.floor(diff / 60000);
            const hours = Math.floor(minutes / 60);
            const days = Math.floor(hours / 24);
            
            if (minutes < 1) return 'Just now';
            if (minutes < 60) return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
            if (hours < 24) return `${hours} hour${hours > 1 ? 's' : ''} ago`;
            if (days < 7) return `${days} day${days > 1 ? 's' : ''} ago`;
            
            return d.toLocaleDateString('en-GB', {
                year: 'numeric', 
                month: '2-digit', 
                day: '2-digit',
                hour: '2-digit', 
                minute: '2-digit'
            });
        } catch (error) {
            return dateStr;
        }
    }

    function getNotificationIcon(notiType) {
        const icons = {
            'Appt Confirm': 'fa-solid fa-calendar-check',
            'Appointment Update': 'fa-solid fa-calendar-pen',
            'Appointment Reminder': 'fa-solid fa-bell',
            'Test Result': 'fa-solid fa-flask',
            'Medical Record': 'fa-solid fa-file-medical',
            'System': 'fa-solid fa-gear',
            'General': 'fa-solid fa-info-circle'
        };
        return icons[notiType] || 'fa-solid fa-bell';
    }

    function getNotificationTypeClass(notiType) {
        const classes = {
            'Appt Confirm': 'type-appointment-confirm',
            'Appointment Update': 'type-appointment-update',
            'Appointment Reminder': 'type-appointment-reminder',
            'Test Result': 'type-test-result',
            'Medical Record': 'type-medical-record',
            'System': 'type-system',
            'General': 'type-general'
        };
        return classes[notiType] || 'type-general';
    }

    // Facebook-style notification popup system
    let notificationQueue = [];
    let isShowingNotification = false;
    let lastNotificationCheck = Date.now();
    let notificationSound = null;

    // Initialize notification sound
    function initializeNotificationSound() {
        // You can add a notification sound file here
        // notificationSound = new Audio('/path/to/notification-sound.mp3');
        // notificationSound.volume = 0.3;
    }

    // Create Facebook-style notification popup
    function createFacebookNotificationPopup(notification) {
        const popup = document.createElement('div');
        popup.className = 'facebook-notification-popup';
        popup.setAttribute('data-notification-id', notification.notiId);
        
        const isUnread = true; // New notifications are always unread
        const timeAgo = 'Just now';
        
        popup.innerHTML = `
            <div class="fb-notification-header">
                <div class="fb-notification-icon ${getNotificationTypeClass(notification.notiType)}">
                    <i class="${getNotificationIcon(notification.notiType)}"></i>
                </div>
                <div class="fb-notification-title">
                    <span class="fb-notification-type">${notification.notiType}</span>
                    <span class="fb-notification-time">${timeAgo}</span>
                </div>
                <button class="fb-notification-close" aria-label="Close notification">
                    <i class="fa-solid fa-times"></i>
                </button>
            </div>
            <div class="fb-notification-content">
                <div class="fb-notification-message">${notification.notiMessage}</div>
            </div>
            <div class="fb-notification-actions">
                <button class="fb-notification-action mark-read" data-notification-id="${notification.notiId}">
                    <i class="fa-solid fa-check"></i> Mark as Read
                </button>
                <button class="fb-notification-action view-all">
                    <i class="fa-solid fa-bell"></i> View All
                </button>
            </div>
        `;
        
        return popup;
    }

    // Show Facebook-style notification
    function showFacebookNotification(notification) {
        if (isShowingNotification) {
            notificationQueue.push(notification);
            return;
        }
        
        isShowingNotification = true;
        
        // Play notification sound if available
        if (notificationSound) {
            notificationSound.play().catch(console.error);
        }
        
        const popup = createFacebookNotificationPopup(notification);
        document.body.appendChild(popup);
        
        // Animate in
        setTimeout(() => {
            popup.classList.add('show');
        }, 100);
        
        // Auto-dismiss after 5 seconds
        const autoDismissTimer = setTimeout(() => {
            dismissFacebookNotification(popup);
        }, 5000);
        
        // Add event listeners
        const closeBtn = popup.querySelector('.fb-notification-close');
        const markReadBtn = popup.querySelector('.mark-read');
        const viewAllBtn = popup.querySelector('.view-all');
        
        closeBtn.addEventListener('click', () => {
            clearTimeout(autoDismissTimer);
            dismissFacebookNotification(popup);
        });
        
        markReadBtn.addEventListener('click', () => {
            clearTimeout(autoDismissTimer);
            markAsRead(notification.notiId);
            dismissFacebookNotification(popup);
        });
        
        viewAllBtn.addEventListener('click', () => {
            clearTimeout(autoDismissTimer);
            dismissFacebookNotification(popup);
            window.location.href = '/private-view/user-view/notification/notification.html';
        });
        
        // Click on notification to view all
        popup.addEventListener('click', (e) => {
            if (!e.target.closest('.fb-notification-actions') && !e.target.closest('.fb-notification-close')) {
                clearTimeout(autoDismissTimer);
                dismissFacebookNotification(popup);
                window.location.href = '/private-view/user-view/notification/notification.html';
            }
        });
        
        // Store timer reference for cleanup
        popup.autoDismissTimer = autoDismissTimer;
    }

    // Dismiss Facebook-style notification
    function dismissFacebookNotification(popup) {
        popup.classList.add('hide');
        
        setTimeout(() => {
            if (popup.parentNode) {
                popup.parentNode.removeChild(popup);
            }
            isShowingNotification = false;
            
            // Show next notification in queue
            if (notificationQueue.length > 0) {
                const nextNotification = notificationQueue.shift();
                setTimeout(() => showFacebookNotification(nextNotification), 300);
            }
        }, 300);
    }

    // Check for new notifications periodically
    async function checkForNewNotifications() {
        try {
            const currentNotifications = await fetchNotifications('/api/Notification/GetPersonalNotifications');
            
            if (currentNotifications && currentNotifications.length > 0) {
                // Find new notifications (those created after last check)
                const newNotifications = currentNotifications.filter(notification => {
                    const notificationTime = new Date(notification.sendAt).getTime();
                    return notificationTime > lastNotificationCheck;
                });
                
                // Show new notifications as Facebook-style popups
                newNotifications.forEach(notification => {
                    showFacebookNotification(notification);
                });
                
                // Update last check time
                lastNotificationCheck = Date.now();
            }
        } catch (error) {
            console.error('Error checking for new notifications:', error);
        }
    }

    // Start periodic notification checking
    function startNotificationPolling() {
        // Check every 10 seconds for new notifications
        setInterval(checkForNewNotifications, 10000);
        
        // Also check when page becomes visible again
        document.addEventListener('visibilitychange', () => {
            if (!document.hidden) {
                checkForNewNotifications();
            }
        });
    }

    // Initialize Facebook-style notifications
    function initializeFacebookNotifications() {
        initializeNotificationSound();
        startNotificationPolling();
        
        // Add CSS styles for Facebook-style notifications
        if (!document.getElementById('facebook-notification-styles')) {
            const styles = document.createElement('style');
            styles.id = 'facebook-notification-styles';
            styles.textContent = `
                .facebook-notification-popup {
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    width: 320px;
                    background: #fff;
                    border-radius: 8px;
                    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
                    z-index: 10000;
                    opacity: 0;
                    transform: translateX(100%);
                    transition: all 0.3s ease;
                    cursor: pointer;
                    border-left: 4px solid #1877f2;
                }
                
                .facebook-notification-popup.show {
                    opacity: 1;
                    transform: translateX(0);
                }
                
                .facebook-notification-popup.hide {
                    opacity: 0;
                    transform: translateX(100%);
                }
                
                .facebook-notification-popup:hover {
                    box-shadow: 0 6px 25px rgba(0, 0, 0, 0.2);
                }
                
                .fb-notification-header {
                    display: flex;
                    align-items: center;
                    padding: 12px 16px 8px;
                    gap: 8px;
                }
                
                .fb-notification-icon {
                    width: 32px;
                    height: 32px;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    font-size: 14px;
                    color: white;
                }
                
                .fb-notification-icon.type-appointment-confirm,
                .fb-notification-icon.type-appointment-update,
                .fb-notification-icon.type-appointment-reminder {
                    background: #1877f2;
                }
                
                .fb-notification-icon.type-test-result {
                    background: #42b883;
                }
                
                .fb-notification-icon.type-medical-record {
                    background: #e74c3c;
                }
                
                .fb-notification-icon.type-system {
                    background: #95a5a6;
                }
                
                .fb-notification-icon.type-general {
                    background: #1877f2;
                }
                
                .fb-notification-title {
                    flex: 1;
                    display: flex;
                    flex-direction: column;
                    gap: 2px;
                }
                
                .fb-notification-type {
                    font-weight: 600;
                    font-size: 14px;
                    color: #1c1e21;
                }
                
                .fb-notification-time {
                    font-size: 12px;
                    color: #65676b;
                }
                
                .fb-notification-close {
                    background: none;
                    border: none;
                    font-size: 16px;
                    color: #65676b;
                    cursor: pointer;
                    padding: 4px;
                    border-radius: 4px;
                    transition: background-color 0.2s;
                }
                
                .fb-notification-close:hover {
                    background-color: #f0f2f5;
                }
                
                .fb-notification-content {
                    padding: 0 16px 8px;
                }
                
                .fb-notification-message {
                    font-size: 14px;
                    color: #1c1e21;
                    line-height: 1.4;
                    word-wrap: break-word;
                }
                
                .fb-notification-actions {
                    display: flex;
                    gap: 8px;
                    padding: 8px 16px 12px;
                }
                
                .fb-notification-action {
                    flex: 1;
                    background: #f0f2f5;
                    border: none;
                    border-radius: 6px;
                    padding: 8px 12px;
                    font-size: 13px;
                    font-weight: 500;
                    color: #1c1e21;
                    cursor: pointer;
                    transition: background-color 0.2s;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    gap: 4px;
                }
                
                .fb-notification-action:hover {
                    background: #e4e6ea;
                }
                
                .fb-notification-action.mark-read {
                    background: #e7f3ff;
                    color: #1877f2;
                }
                
                .fb-notification-action.mark-read:hover {
                    background: #d0e7ff;
                }
                
                /* Animation for stacking notifications */
                .facebook-notification-popup:nth-child(n+2) {
                    top: 40px;
                    transform: translateX(5px) scale(0.95);
                    opacity: 0.8;
                }
                
                .facebook-notification-popup:nth-child(n+3) {
                    top: 60px;
                    transform: translateX(10px) scale(0.9);
                    opacity: 0.6;
                }
                
                /* Mobile responsive */
                @media (max-width: 768px) {
                    .facebook-notification-popup {
                        width: 300px;
                        right: 10px;
                        top: 10px;
                    }
                }
                
                @media (max-width: 480px) {
                    .facebook-notification-popup {
                        width: calc(100vw - 20px);
                        right: 10px;
                        left: 10px;
                    }
                }
            `;
            document.head.appendChild(styles);
        }
    }

    // Manual trigger for testing Facebook-style notifications
    function triggerTestNotification() {
        const testNotification = {
            notiId: Date.now(),
            notiType: 'Test Notification',
            notiMessage: 'This is a test Facebook-style notification popup!',
            sendAt: new Date().toISOString(),
            createdAt: new Date().toISOString()
        };
        
        showFacebookNotification(testNotification);
    }

    // Add test button for development (you can remove this in production)
    function addTestButton() {
        const testBtn = document.createElement('button');
        testBtn.innerHTML = '<i class="fa-solid fa-bell"></i> Test FB Notification';
        testBtn.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            background: #1877f2;
            color: white;
            border: none;
            border-radius: 25px;
            padding: 12px 20px;
            font-size: 14px;
            cursor: pointer;
            z-index: 9999;
            box-shadow: 0 4px 12px rgba(24, 119, 242, 0.3);
            transition: all 0.3s ease;
        `;
        
        testBtn.addEventListener('click', triggerTestNotification);
        testBtn.addEventListener('mouseenter', () => {
            testBtn.style.background = '#166fe5';
            testBtn.style.transform = 'translateY(-2px)';
        });
        testBtn.addEventListener('mouseleave', () => {
            testBtn.style.background = '#1877f2';
            testBtn.style.transform = 'translateY(0)';
        });
        
        document.body.appendChild(testBtn);
    }

    // Add test button for development
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
        addTestButton();
    }

    // ...existing code...
});