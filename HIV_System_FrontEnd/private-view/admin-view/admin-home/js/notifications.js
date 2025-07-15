// Notifications Module
class NotificationManager {
    constructor(authManager) {
        this.authManager = authManager;
    }

    // Load notifications
    async loadNotifications() {
        const notificationsList = document.getElementById('notifications-list');
        notificationsList.innerHTML = '<div class="loader"></div>';
        
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch('https://localhost:7009/api/Notification/GetAllNotifications', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const notifications = await response.json();
                console.log('Notifications data:', notifications);
                
                if (!notifications || notifications.length === 0) {
                    notificationsList.innerHTML = '<div class="no-data">No notifications found</div>';
                    return;
                }
                
                this.renderNotifications(notifications);
                
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading notifications:', error);
            notificationsList.innerHTML = '<div class="error-message">Error loading notifications. Please try again.</div>';
        }
    }

    // Render notifications
    renderNotifications(notifications) {
        const notificationsList = document.getElementById('notifications-list');
        
        // Sort notifications by sendAt date (most recent first)
        const sortedNotifications = notifications.sort((a, b) => new Date(b.sendAt) - new Date(a.sendAt));
        
        const notificationsHTML = sortedNotifications.map(notification => `
            <div class="notification-item" data-type="${notification.notiType}">
                <div class="notification-header">
                    <div class="notification-icon">
                        <i class="fas fa-${this.getNotificationIcon(notification.notiType)}"></i>
                    </div>
                    <div class="notification-info">
                        <div class="notification-type">${notification.notiType}</div>
                        <div class="notification-time">${this.formatTime(notification.sendAt)}</div>
                    </div>
                    <div class="notification-badge ${this.getNotificationBadgeClass(notification.notiType)}">
                        ${notification.notiType}
                    </div>
                </div>
                
                <div class="notification-content">
                    <div class="notification-message">${notification.notiMessage}</div>
                    <div class="notification-meta">
                        <small>Created: ${this.formatTime(notification.createdAt)}</small>
                    </div>
                </div>
                
                <div class="notification-actions">
                    <button class="btn-small btn-primary" onclick="window.notificationManager.editNotification(${notification.notiId})">
                        <i class="fas fa-edit"></i> Edit
                    </button>
                    <button class="btn-small btn-danger" onclick="window.notificationManager.deleteNotification(${notification.notiId})">
                        <i class="fas fa-trash"></i> Delete
                    </button>
                </div>
            </div>
        `).join('');
        
        notificationsList.innerHTML = notificationsHTML;
    }

    // Get notification icon based on type
    getNotificationIcon(type) {
        const icons = {
            'Appt Confirm': 'calendar-check',
            'Appointment Update': 'calendar-edit',
            'Appointment Request': 'calendar-plus',
            'Appointment Reminder': 'bell',
            'System Alert': 'exclamation-triangle',
            'ARV Consultation': 'pills',
            'Test Result': 'vial',
            'Blog Approval': 'check-circle'
        };
        return icons[type] || 'info-circle';
    }

    // Get notification badge class based on type
    getNotificationBadgeClass(type) {
        const classes = {
            'Appt Confirm': 'badge-success',
            'Appointment Update': 'badge-warning',
            'Appointment Request': 'badge-info',
            'Appointment Reminder': 'badge-primary',
            'System Alert': 'badge-danger',
            'ARV Consultation': 'badge-purple',
            'Test Result': 'badge-teal',
            'Blog Approval': 'badge-green'
        };
        return classes[type] || 'badge-default';
    }

    // Format time
    formatTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffInMs = now - date;
        const diffInMins = Math.floor(diffInMs / (1000 * 60));
        const diffInHours = Math.floor(diffInMins / 60);
        const diffInDays = Math.floor(diffInHours / 24);

        if (diffInMins < 1) return 'Just now';
        if (diffInMins < 60) return `${diffInMins} minute${diffInMins > 1 ? 's' : ''} ago`;
        if (diffInHours < 24) return `${diffInHours} hour${diffInHours > 1 ? 's' : ''} ago`;
        if (diffInDays < 7) return `${diffInDays} day${diffInDays > 1 ? 's' : ''} ago`;
        
        return date.toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // Show create notification modal
    showCreateNotificationModal() {
        const modal = document.getElementById('createNotificationModal');
        if (modal) {
            modal.style.display = 'block';
        }
    }

    // Filter notifications
    async filterNotifications() {
        const filter = document.getElementById('notification-filter').value;
        console.log('Filtering notifications by:', filter);
        
        const notificationsList = document.getElementById('notifications-list');
        notificationsList.innerHTML = '<div class="loader"></div>';
        
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch('https://localhost:7009/api/Notification/GetAllNotifications', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                let notifications = await response.json();
                
                // Apply filter
                if (filter !== 'all') {
                    notifications = notifications.filter(notification => {
                        const typeMapping = {
                            'appointment': ['Appt Confirm', 'Appointment Update', 'Appointment Request', 'Appointment Reminder'],
                            'system': ['System Alert'],
                            'medical': ['ARV Consultation', 'Test Result'],
                            'blog': ['Blog Approval']
                        };
                        
                        return typeMapping[filter] && typeMapping[filter].includes(notification.notiType);
                    });
                }
                
                if (notifications.length === 0) {
                    notificationsList.innerHTML = '<div class="no-data">No notifications found matching the criteria</div>';
                    return;
                }
                
                this.renderNotifications(notifications);
                
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error filtering notifications:', error);
            notificationsList.innerHTML = '<div class="error-message">Error filtering notifications. Please try again.</div>';
        }
    }

    // Edit notification
    async editNotification(id) {
        console.log('Edit notification:', id);
        
        // First, get the notification data
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch('https://localhost:7009/api/Notification/GetAllNotifications', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const notifications = await response.json();
                const notification = notifications.find(n => n.notiId === id);
                
                if (notification) {
                    // Populate edit form
                    document.getElementById('edit-notification-id').value = notification.notiId;
                    document.getElementById('edit-notification-type').value = notification.notiType;
                    document.getElementById('edit-notification-message').value = notification.notiMessage;
                    
                    // Show edit modal
                    if (window.modalManager && window.modalManager.showModal) {
                        window.modalManager.showModal('editNotificationModal');
                    } else {
                        const modal = document.getElementById('editNotificationModal');
                        if (modal) {
                            modal.style.display = 'block';
                        }
                    }
                } else {
                    alert('Notification not found');
                }
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading notification for edit:', error);
            alert('Error loading notification details. Please try again.');
        }
    }

    // Delete notification
    async deleteNotification(id) {
        if (confirm('Are you sure you want to delete this notification? This action cannot be undone.')) {
            const token = this.authManager.getToken();
            
            try {
                const response = await fetch(`https://localhost:7009/api/Notification/DeleteNotification/${id}`, {
                    method: 'DELETE',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'accept': '*/*'
                    }
                });
                
                if (response.ok) {
                    alert('Notification deleted successfully!');
                    this.loadNotifications(); // Reload the notifications list
                } else {
                    const errorData = await response.text();
                    throw new Error(errorData || `HTTP error! status: ${response.status}`);
                }
            } catch (error) {
                console.error('Error deleting notification:', error);
                alert('Error deleting notification. Please try again.');
            }
        }
    }

    // Handle create notification form
    async handleCreateNotification(e) {
        e.preventDefault();
        
        const notiType = document.getElementById('notification-type').value;
        const notiMessage = document.getElementById('notification-message').value;
        const sendTo = document.getElementById('send-to') ? document.getElementById('send-to').value : 'all';
        const accountId = document.getElementById('account-id') ? document.getElementById('account-id').value : null;
        
        if (!notiType || !notiMessage) {
            alert('Please fill in all required fields');
            return;
        }
        
        const submitButton = e.target.querySelector('button[type="submit"]');
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Sending...';
        }
        
        try {
            let success = false;
            
            if (sendTo === 'all') {
                success = await this.sendNotificationToAll(notiType, notiMessage);
            } else if (sendTo === 'account' && accountId) {
                success = await this.sendNotificationToAccount(accountId, notiType, notiMessage);
            } else if (sendTo && sendTo !== 'all' && sendTo !== 'account') {
                // Send to specific role
                success = await this.sendNotificationToRole(sendTo, notiType, notiMessage);
            } else {
                // Default to send to all if no specific target
                success = await this.sendNotificationToAll(notiType, notiMessage);
            }
            
            if (success) {
                alert('Notification sent successfully!');
                
                // Close modal
                if (window.modalManager && window.modalManager.closeModal) {
                    window.modalManager.closeModal('createNotificationModal');
                } else {
                    const modal = document.getElementById('createNotificationModal');
                    if (modal) {
                        modal.style.display = 'none';
                    }
                }
                
                // Reset form
                const form = document.getElementById('createNotificationForm');
                if (form) {
                    form.reset();
                }
                
                // Reload notifications
                this.loadNotifications();
            }
        } catch (error) {
            console.error('Error creating notification:', error);
            alert('Error creating notification. Please try again.');
        } finally {
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-paper-plane"></i> Send Notification';
            }
        }
    }

    // Send notification to all users
    async sendNotificationToAll(notiType, notiMessage) {
        const token = this.authManager.getToken();
        
        const requestBody = {
            notiType: notiType,
            notiMessage: notiMessage,
            sendAt: new Date().toISOString()
        };
        
        try {
            const response = await fetch('https://localhost:7009/api/Notification/CreateAndSendToAll', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'accept': '*/*'
                },
                body: JSON.stringify(requestBody)
            });
            
            if (response.ok) {
                return true;
            } else {
                const errorData = await response.text();
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error sending notification to all:', error);
            throw error;
        }
    }

    // Send notification to specific role
    async sendNotificationToRole(roleId, notiType, notiMessage) {
        const token = this.authManager.getToken();
        
        const requestBody = {
            notiType: notiType,
            notiMessage: notiMessage,
            sendAt: new Date().toISOString()
        };
        
        try {
            const response = await fetch(`https://localhost:7009/api/Notification/CreateAndSendToRole/${roleId}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'accept': '*/*'
                },
                body: JSON.stringify(requestBody)
            });
            
            if (response.ok) {
                return true;
            } else {
                const errorData = await response.text();
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error sending notification to role:', error);
            throw error;
        }
    }

    // Send notification to specific account
    async sendNotificationToAccount(accountId, notiType, notiMessage) {
        const token = this.authManager.getToken();
        
        const requestBody = {
            notiType: notiType,
            notiMessage: notiMessage,
            sendAt: new Date().toISOString()
        };
        
        try {
            const response = await fetch(`https://localhost:7009/api/Notification/CreateAndSendToAccount/${accountId}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'accept': '*/*'
                },
                body: JSON.stringify(requestBody)
            });
            
            if (response.ok) {
                return true;
            } else {
                const errorData = await response.text();
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error sending notification to account:', error);
            throw error;
        }
    }

    // Initialize
    init() {
        // Create notification button
        const createNotificationBtn = document.getElementById('create-notification-btn');
        if (createNotificationBtn) {
            createNotificationBtn.addEventListener('click', () => this.showCreateNotificationModal());
        }
        
        // Filter elements
        const notificationFilter = document.getElementById('notification-filter');
        if (notificationFilter) {
            notificationFilter.addEventListener('change', () => this.filterNotifications());
        }
        
        // Form handler
        const createNotificationForm = document.getElementById('createNotificationForm');
        if (createNotificationForm) {
            createNotificationForm.addEventListener('submit', (e) => this.handleCreateNotification(e));
        }
    }
}

// Export for use in other modules
window.NotificationManager = NotificationManager;
