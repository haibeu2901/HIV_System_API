document.addEventListener('DOMContentLoaded', () => {
    // Configuration
    const CONFIG = {
        API_BASE_URL: 'https://localhost:7009',
        LOGIN_PAGE: '/public-view/landingpage.html',
        ENDPOINTS: {
            GET_PERSONAL_NOTIFICATIONS: '/api/Notification/GetPersonalNotifications',
            GET_UNREAD_NOTIFICATIONS: '/api/Notification/GetUnreadNotifications',
            MARK_AS_READ: '/api/Notification/MarkAsRead',
            MARK_ALL_AS_READ: '/api/Notification/MarkAllAsRead'
        },
        NAVIGATION: {
            PROFILE: '../profile/profile.html',
            APPOINTMENTS: '../appointment-view/view-appointment.html',
            MEDICAL_RECORDS: '../medical-record/medical-record.html',
            FIND_DOCTOR: '../view-doctor/view-doctor.html',
            ARV_MEDICATIONS: '../ARV/arv-medications.html',
            BOOKING: '../booking/appointment-booking.html'
        },
        MESSAGES: {
            LOGIN_REQUIRED: 'You need to be logged in to view notifications. Redirecting to login...',
            SESSION_EXPIRED: 'Your session has expired. Please log in again.',
            MARK_READ_FAILED: 'Failed to mark notification as read. Please try again.',
            MARK_ALL_READ_FAILED: 'Failed to mark all notifications as read. Please try again.'
        },
        TIMEOUTS: {
            REQUEST_TIMEOUT: 10000,
            OPERATION_TIMEOUT: 15000
        },
        POLLING: {
            INTERVAL: 30000 // 30 seconds
        },
        RETRY: {
            MAX_ATTEMPTS: 3,
            DELAY_MULTIPLIER: 1000
        }
    };

    // Get authentication token from localStorage
    const token = localStorage.getItem("token");
    const userRole = localStorage.getItem("userRole");
    const accId = localStorage.getItem("accId");
    
    // Redirect to login if no token
    if (!token) {
        alert(CONFIG.MESSAGES.LOGIN_REQUIRED);
        window.location.href = CONFIG.LOGIN_PAGE;
        return;
    }
    
    // DOM elements
    const notificationList = document.getElementById('notification-list');
    
    // State management
    let notifications = [];
    let unreadNotifications = [];
    let currentFilter = 'appointment'; // Default to appointment confirmations
    let isLoading = false;
    let connectionStatus = 'online';
    let periodicCheckInterval;
    
    // Initialize notification system
    init();
    
    async function init() {
        await testTokenValidity();
        await loadNotifications();
        startPeriodicCheck();
        updateStats();
    }
    
    // Test token validity
    async function testTokenValidity() {
        try {
            const response = await fetch(`${CONFIG.API_BASE_URL}${CONFIG.ENDPOINTS.GET_UNREAD_NOTIFICATIONS}`, {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json",
                    "accept": "*/*"
                }
            });
            
            if (response.status === 401) {
                localStorage.removeItem('token');
                localStorage.removeItem('userRole');
                localStorage.removeItem('accId');
                alert(CONFIG.MESSAGES.SESSION_EXPIRED);
                window.location.href = CONFIG.LOGIN_PAGE;
                return;
            }
        } catch (error) {
            // Token validation failed, continue silently
        }
    }
    
    // Load all notifications
    async function loadNotifications() {
        if (isLoading) return;
        
        isLoading = true;
        
        try {
            showLoadingState();
            
            const timeoutPromise = new Promise((_, reject) => 
                setTimeout(() => reject(new Error('Request timeout')), CONFIG.TIMEOUTS.OPERATION_TIMEOUT)
            );
            
            // Fetch both personal and unread notifications
            const [personalResult, unreadResult] = await Promise.allSettled([
                Promise.race([
                    fetchNotifications(CONFIG.ENDPOINTS.GET_PERSONAL_NOTIFICATIONS),
                    timeoutPromise
                ]),
                Promise.race([
                    fetchNotifications(CONFIG.ENDPOINTS.GET_UNREAD_NOTIFICATIONS),
                    timeoutPromise
                ])
            ]);
            
            // Process results
            if (personalResult.status === 'fulfilled' && Array.isArray(personalResult.value)) {
                notifications = personalResult.value;
            } else {
                notifications = [];
            }
            
            if (unreadResult.status === 'fulfilled' && Array.isArray(unreadResult.value)) {
                unreadNotifications = unreadResult.value;
            } else {
                unreadNotifications = [];
            }
            
            renderNotifications();
            updateStats();
            updateHeaderNotificationCount();
            
        } catch (error) {
            handleNotificationError(error);
        } finally {
            isLoading = false;
        }
    }
    
    // Fetch notifications from API with retry logic
    async function fetchNotifications(endpoint, retryCount = CONFIG.RETRY.MAX_ATTEMPTS) {
        for (let i = 0; i < retryCount; i++) {
            try {
                const controller = new AbortController();
                const timeoutId = setTimeout(() => controller.abort(), CONFIG.TIMEOUTS.REQUEST_TIMEOUT);
                
                const response = await fetch(`${CONFIG.API_BASE_URL}${endpoint}`, {
                    method: "GET",
                    headers: {
                        "Authorization": `Bearer ${token}`,
                        "Content-Type": "application/json",
                        "accept": "*/*"
                    },
                    signal: controller.signal
                });
                
                clearTimeout(timeoutId);
                
                if (!response.ok) {
                    const errorText = await response.text();
                    
                    if (response.status === 401) {
                        localStorage.removeItem('token');
                        localStorage.removeItem('userRole');
                        localStorage.removeItem('accId');
                        alert(CONFIG.MESSAGES.SESSION_EXPIRED);
                        window.location.href = CONFIG.LOGIN_PAGE;
                        return [];
                    }
                    
                    // Don't retry on 4xx errors (except 401)
                    if (response.status >= 400 && response.status < 500 && response.status !== 401) {
                        throw new Error(`API Error ${response.status}: ${errorText}`);
                    }
                    
                    // Retry on 5xx errors or network issues
                    if (i === retryCount - 1) {
                        throw new Error(`API Error ${response.status}: ${errorText}`);
                    }
                    
                    await new Promise(resolve => setTimeout(resolve, (i + 1) * CONFIG.RETRY.DELAY_MULTIPLIER));
                    continue;
                }
                
                const data = await response.json();
                return Array.isArray(data) ? data : [];
                
            } catch (error) {
                if (error.name === 'AbortError') {
                    if (i === retryCount - 1) {
                        throw new Error('Request timeout - please check your connection');
                    }
                } else {
                    if (i === retryCount - 1) {
                        throw error;
                    }
                }
                
                await new Promise(resolve => setTimeout(resolve, (i + 1) * CONFIG.RETRY.DELAY_MULTIPLIER));
            }
        }
        
        return [];
    }
    
    // Update notification statistics
    function updateStats() {
        const totalCount = document.querySelector('.total-count');
        const unreadCount = document.querySelector('.unread-count');
        
        // Count notifications by category
        const appointmentCount = notifications.filter(n => {
            const categoryInfo = categorizeNotification(n);
            return categoryInfo.category === 'appointment';
        }).length;
        
        const unreadAppointmentCount = unreadNotifications.filter(n => {
            const categoryInfo = categorizeNotification(n);
            return categoryInfo.category === 'appointment';
        }).length;
        
        if (totalCount) {
            totalCount.innerHTML = `${appointmentCount} <small>APPOINTMENTS</small>`;
        }
        
        if (unreadCount) {
            unreadCount.innerHTML = `${unreadAppointmentCount} <small>UNREAD</small>`;
        }
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
        const errorMessage = error.message || 'Unknown error occurred';
        const isNetworkError = errorMessage.includes('timeout') || 
                              errorMessage.includes('Failed to fetch') ||
                              errorMessage.includes('Network error');
        
        const isServerError = errorMessage.includes('500') || 
                             errorMessage.includes('Internal Server Error') ||
                             errorMessage.includes('DbContext');
        
        notificationList.innerHTML = `
            <div class="error-container">
                <div class="error-icon">
                    <i class="fa-solid fa-server"></i>
                </div>
                <h3>Service Temporarily Unavailable</h3>
                <p>We're experiencing technical difficulties with our notification service.</p>
                
                ${isServerError ? `
                    <div class="error-suggestions">
                        <p><strong>This is a temporary server issue. Please:</strong></p>
                        <ul>
                            <li>Wait a few moments and try again</li>
                            <li>Check back in a few minutes</li>
                            <li>Contact support if the issue persists</li>
                        </ul>
                    </div>
                ` : isNetworkError ? `
                    <div class="error-suggestions">
                        <p><strong>Connection issue detected:</strong></p>
                        <ul>
                            <li>Check your internet connection</li>
                            <li>Refresh the page</li>
                            <li>Try again in a few moments</li>
                        </ul>
                    </div>
                ` : `
                    <div class="error-suggestions">
                        <p><strong>Something went wrong:</strong></p>
                        <ul>
                            <li>Please try refreshing the page</li>
                            <li>Check your connection</li>
                            <li>Contact support if the issue continues</li>
                        </ul>
                    </div>
                `}
                
                <div class="error-actions">
                    <button onclick="location.reload()" class="retry-btn">
                        <i class="fa-solid fa-refresh"></i> Refresh Page
                    </button>
                    <button onclick="window.loadNotifications()" class="retry-btn secondary">
                        <i class="fa-solid fa-redo"></i> Try Again
                    </button>
                </div>
            </div>
        `;
    }
    
    // Show offline view
    function showOfflineView() {
        notificationList.innerHTML = `
            <div class="offline-container">
                <div class="offline-header">
                    <i class="fa-solid fa-wifi-slash"></i>
                    <h3>Notifications Unavailable</h3>
                    <p>Unable to connect to notification service</p>
                </div>
                
                <div class="offline-content">
                    <div class="offline-message">
                        <h4>What you can do:</h4>
                        <ul>
                            <li>Check your important notifications later</li>
                            <li>Visit other sections of the app</li>
                            <li>Try refreshing the page</li>
                        </ul>
                    </div>
                    
                    <div class="offline-navigation">
                        <h4>Go to:</h4>
                        <div class="nav-buttons">
                            <a href="${CONFIG.NAVIGATION.PROFILE}" class="nav-btn">
                                <i class="fa-solid fa-user"></i> Profile
                            </a>
                            <a href="${CONFIG.NAVIGATION.APPOINTMENTS}" class="nav-btn">
                                <i class="fa-solid fa-calendar"></i> Appointments
                            </a>
                            <a href="${CONFIG.NAVIGATION.MEDICAL_RECORDS}" class="nav-btn">
                                <i class="fa-solid fa-file-medical"></i> Medical Records
                            </a>
                            <a href="${CONFIG.NAVIGATION.FIND_DOCTOR}" class="nav-btn">
                                <i class="fa-solid fa-user-doctor"></i> Find Doctor
                            </a>
                        </div>
                    </div>
                </div>
                
                <div class="offline-actions">
                    <button onclick="window.loadNotifications()" class="retry-btn">
                        <i class="fa-solid fa-refresh"></i> Try Again
                    </button>
                    <button onclick="location.reload()" class="retry-btn secondary">
                        <i class="fa-solid fa-redo"></i> Refresh Page
                    </button>
                </div>
            </div>
        `;
    }
    
    // Handle notification errors
    function handleNotificationError(error) {
        const errorMessage = error.message || '';
        const isServerError = errorMessage.includes('500') || 
                             errorMessage.includes('Internal Server Error') ||
                             errorMessage.includes('DbContext') ||
                             errorMessage.includes('Server error');
        
        if (isServerError) {
            showOfflineView();
        } else {
            showErrorState(error);
        }
    }
    
    // Categorize notification type
    function categorizeNotification(notification) {
        const message = notification.notiMessage ? notification.notiMessage.toLowerCase() : '';
        const type = notification.notiType ? notification.notiType.toLowerCase() : '';
        
        // Check for appointment-related notifications
        if (message.includes('appointment') || message.includes('cuộc hẹn') || 
            type.includes('appointment') || type.includes('cuộc hẹn') ||
            message.includes('confirmed') || message.includes('xác nhận') ||
            message.includes('scheduled') || message.includes('lịch hẹn') ||
            message.includes('booking') || message.includes('đặt lịch') ||
            message.includes('cancel') || message.includes('hủy') ||
            message.includes('reschedule') || message.includes('đổi lịch')) {
            return {
                category: 'appointment',
                icon: 'fa-calendar-check',
                displayType: 'APPOINTMENT',
                navigationUrl: CONFIG.NAVIGATION.APPOINTMENTS,
                actionText: 'View Appointments'
            };
        }
        
        // Check for ARV-related notifications
        if (message.includes('arv') || message.includes('antiretroviral') ||
            message.includes('regimen') || message.includes('phác đồ') ||
            message.includes('arv medication') || message.includes('thuốc arv') ||
            type.includes('arv') || type.includes('regimen')) {
            return {
                category: 'arv',
                icon: 'fa-pills',
                displayType: 'ARV REGIMEN',
                navigationUrl: CONFIG.NAVIGATION.ARV_MEDICATIONS,
                actionText: 'View ARV Medications'
            };
        }
        
        // Check for general medication notifications
        if (message.includes('medication') || message.includes('medicine') ||
            message.includes('thuốc') || message.includes('prescription') ||
            message.includes('đơn thuốc') || message.includes('dosage') ||
            message.includes('liều lượng') || type.includes('medication') ||
            type.includes('medicine') || type.includes('prescription')) {
            return {
                category: 'medication',
                icon: 'fa-capsules',
                displayType: 'MEDICATION',
                navigationUrl: CONFIG.NAVIGATION.MEDICAL_RECORDS,
                actionText: 'View Medical Records'
            };
        }
        
        // Check for medical/health related
        if (message.includes('medical') || message.includes('health') || 
            message.includes('doctor') || message.includes('bác sĩ') ||
            message.includes('treatment') || message.includes('điều trị') ||
            message.includes('test result') || message.includes('kết quả xét nghiệm') ||
            message.includes('lab') || message.includes('xét nghiệm')) {
            return {
                category: 'medical',
                icon: 'fa-user-doctor',
                displayType: 'MEDICAL',
                navigationUrl: CONFIG.NAVIGATION.MEDICAL_RECORDS,
                actionText: 'View Medical Records'
            };
        }
        
        // Check for payment-related notifications
        if (message.includes('payment') || message.includes('thanh toán') ||
            message.includes('bill') || message.includes('hóa đơn') ||
            message.includes('invoice') || message.includes('fee') ||
            message.includes('phí') || type.includes('payment')) {
            return {
                category: 'payment',
                icon: 'fa-credit-card',
                displayType: 'PAYMENT',
                navigationUrl: CONFIG.NAVIGATION.MEDICAL_RECORDS,
                actionText: 'View Payment History'
            };
        }
        
        // Check for system notifications
        if (message.includes('system') || message.includes('update') ||
            message.includes('maintenance') || message.includes('security') ||
            type.includes('system')) {
            return {
                category: 'system',
                icon: 'fa-cog',
                displayType: 'SYSTEM',
                navigationUrl: null,
                actionText: null
            };
        }
        
        // Default to appointment if unclear
        return {
            category: 'appointment',
            icon: 'fa-bell',
            displayType: 'NOTIFICATION',
            navigationUrl: CONFIG.NAVIGATION.APPOINTMENTS,
            actionText: 'View Details'
        };
    }
    
    // Render notifications
    function renderNotifications() {
        const filteredNotifications = filterNotifications(notifications, currentFilter);
        
        if (filteredNotifications.length === 0) {
            const filterText = currentFilter === 'appointment' ? 'appointment confirmations' : 
                              currentFilter === 'medical' ? 'medical notifications' :
                              currentFilter === 'system' ? 'system notifications' :
                              currentFilter === 'unread' ? 'unread notifications' : 'notifications';
            
            notificationList.innerHTML = `
                <div class="no-notifications">
                    <i class="fa-solid fa-bell-slash"></i>
                    <h3>No ${filterText}</h3>
                    <p>You're all caught up!</p>
                </div>
            `;
            return;
        }
        
        notificationList.innerHTML = filteredNotifications.map(notification => {
            const categoryInfo = categorizeNotification(notification);
            const isUnread = unreadNotifications.find(u => u.notiId === notification.notiId);
            
            return `
                <div class="notification-item ${isUnread ? 'unread' : 'read'}" 
                     data-id="${notification.notiId}" 
                     ${categoryInfo.navigationUrl ? `onclick="handleNotificationClick(${notification.notiId}, '${categoryInfo.navigationUrl}')"` : ''}>
                    <div class="notification-content">
                        <div class="notification-icon ${categoryInfo.category}">
                            <i class="fa-solid ${categoryInfo.icon}"></i>
                        </div>
                        <div class="notification-body">
                            <div class="notification-header-info">
                                <span class="notification-type ${categoryInfo.category}">${categoryInfo.displayType}</span>
                                <span class="notification-time">${formatDate(notification.createdAt)}</span>
                            </div>
                            <h3 class="notification-title">${notification.notiType || 'Notification'}</h3>
                            <p class="notification-message">${notification.notiMessage || 'No message available'}</p>
                            <div class="notification-actions">
                                ${isUnread ? `
                                    <button onclick="event.stopPropagation(); markAsRead(${notification.notiId})" class="mark-read-btn">
                                        <i class="fa-solid fa-check"></i> Mark as Read
                                    </button>
                                ` : ''}
                                ${categoryInfo.navigationUrl ? `
                                    <button onclick="event.stopPropagation(); navigateToSection('${categoryInfo.navigationUrl}')" class="action-btn">
                                        <i class="fa-solid fa-external-link-alt"></i> ${categoryInfo.actionText}
                                    </button>
                                ` : ''}
                            </div>
                        </div>
                    </div>
                    ${categoryInfo.navigationUrl ? '<div class="notification-click-hint">Click to view details</div>' : ''}
                </div>
            `;
        }).join('');
    }
    
    // Filter notifications
    function filterNotifications(notifications, filter) {
        switch (filter) {
            case 'appointment':
                return notifications.filter(n => {
                    const categoryInfo = categorizeNotification(n);
                    return categoryInfo.category === 'appointment';
                });
            case 'arv':
                return notifications.filter(n => {
                    const categoryInfo = categorizeNotification(n);
                    return categoryInfo.category === 'arv';
                });
            case 'medication':
                return notifications.filter(n => {
                    const categoryInfo = categorizeNotification(n);
                    return categoryInfo.category === 'medication';
                });
            case 'medical':
                return notifications.filter(n => {
                    const categoryInfo = categorizeNotification(n);
                    return categoryInfo.category === 'medical';
                });
            case 'payment':
                return notifications.filter(n => {
                    const categoryInfo = categorizeNotification(n);
                    return categoryInfo.category === 'payment';
                });
            case 'system':
                return notifications.filter(n => {
                    const categoryInfo = categorizeNotification(n);
                    return categoryInfo.category === 'system';
                });
            case 'unread':
                return notifications.filter(n => unreadNotifications.find(u => u.notiId === n.notiId));
            case 'important':
                return notifications.filter(n => n.priority === 'high' || n.type === 'important');
            default:
                return notifications;
        }
    }
    
    // Mark notification as read
    async function markAsRead(notificationId) {
        try {
            const response = await fetch(`${CONFIG.API_BASE_URL}${CONFIG.ENDPOINTS.MARK_AS_READ}/${notificationId}`, {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json",
                    "accept": "*/*"
                }
            });
            
            if (response.ok) {
                // Remove from unread list
                unreadNotifications = unreadNotifications.filter(n => n.notiId !== notificationId);
                
                // Re-render and update stats
                renderNotifications();
                updateStats();
                updateHeaderNotificationCount();
                
                // Update notification count in header if function exists
                if (typeof updateNotificationCount === 'function') {
                    updateNotificationCount();
                }
            }
        } catch (error) {
            alert(CONFIG.MESSAGES.MARK_READ_FAILED);
        }
    }
    
    // Mark all notifications as read
    async function markAllAsRead() {
        try {
            const response = await fetch(`${CONFIG.API_BASE_URL}${CONFIG.ENDPOINTS.MARK_ALL_AS_READ}`, {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json",
                    "accept": "*/*"
                }
            });
            
            if (response.ok) {
                // Clear unread list
                unreadNotifications = [];
                
                // Re-render and update stats
                renderNotifications();
                updateStats();
                updateHeaderNotificationCount();
                
                // Update notification count in header if function exists
                if (typeof updateNotificationCount === 'function') {
                    updateNotificationCount();
                }
            }
        } catch (error) {
            alert(CONFIG.MESSAGES.MARK_ALL_READ_FAILED);
        }
    }
    
    // Update header notification count
    function updateHeaderNotificationCount() {
        const unreadCount = unreadNotifications.length;
        const badge = document.getElementById('unread-count');
        if (badge) {
            if (unreadCount > 0) {
                badge.textContent = unreadCount;
                badge.style.display = 'block';
            } else {
                badge.style.display = 'none';
            }
        }
    }
    
    // Format date for display
    function formatDate(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diff = now - date;
        const seconds = Math.floor(diff / 1000);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
        const days = Math.floor(hours / 24);
        
        if (days > 0) {
            return `${days} day${days > 1 ? 's' : ''} ago`;
        } else if (hours > 0) {
            return `${hours} hour${hours > 1 ? 's' : ''} ago`;
        } else if (minutes > 0) {
            return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
        } else {
            return 'Just now';
        }
    }
    
    // Start periodic checking
    function startPeriodicCheck() {
        if (periodicCheckInterval) {
            clearInterval(periodicCheckInterval);
        }
        
        periodicCheckInterval = setInterval(() => {
            loadNotifications();
        }, CONFIG.POLLING.INTERVAL); // Check every 30 seconds
    }
    
    // Filter button event handlers
    document.addEventListener('click', (e) => {
        if (e.target.classList.contains('filter-btn')) {
            // Update active filter
            document.querySelectorAll('.filter-btn').forEach(btn => btn.classList.remove('active'));
            e.target.classList.add('active');
            
            // Update current filter and re-render
            currentFilter = e.target.dataset.filter;
            renderNotifications();
        }
    });
    
    // Expose functions globally
    window.loadNotifications = loadNotifications;
    window.markAsRead = markAsRead;
    window.markAllAsRead = markAllAsRead;
    
    // Navigation functions
    function handleNotificationClick(notificationId, navigationUrl) {
        // Mark as read if unread
        const isUnread = unreadNotifications.find(u => u.notiId === notificationId);
        if (isUnread) {
            markAsRead(notificationId);
        }
        
        // Navigate to the appropriate section
        navigateToSection(navigationUrl);
    }
    
    function navigateToSection(url) {
        if (url) {
            window.location.href = url;
        }
    }
    
    // Extract appointment ID from notification for direct navigation
    function extractAppointmentId(notification) {
        const message = notification.notiMessage || '';
        const match = message.match(/appointment[^\d]*(\d+)/i) || 
                     message.match(/cuộc hẹn[^\d]*(\d+)/i) ||
                     message.match(/id[^\d]*(\d+)/i);
        return match ? match[1] : null;
    }
    
    // Enhanced navigation for appointments with specific ID
    function navigateToAppointment(notificationId) {
        const notification = notifications.find(n => n.notiId === notificationId);
        if (notification) {
            const appointmentId = extractAppointmentId(notification);
            if (appointmentId) {
                window.location.href = `${CONFIG.NAVIGATION.APPOINTMENTS}?id=${appointmentId}`;
            } else {
                window.location.href = CONFIG.NAVIGATION.APPOINTMENTS;
            }
        }
    }
    
    // Expose navigation functions globally
    window.handleNotificationClick = handleNotificationClick;
    window.navigateToSection = navigateToSection;
    window.navigateToAppointment = navigateToAppointment;
    
    // Cleanup on page unload
    window.addEventListener('beforeunload', () => {
        if (periodicCheckInterval) {
            clearInterval(periodicCheckInterval);
        }
    });
});
