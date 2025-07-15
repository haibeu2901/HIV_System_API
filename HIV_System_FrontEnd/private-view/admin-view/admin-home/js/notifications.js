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
                    <button class="btn-small btn-primary" onclick="notificationManager.editNotification(${notification.notiId})">
                        <i class="fas fa-edit"></i> Edit
                    </button>
                    <button class="btn-small btn-danger" onclick="notificationManager.deleteNotification(${notification.notiId})">
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
        if (window.modalManager && window.modalManager.showModal) {
            window.modalManager.showModal('createNotificationModal');
        } else {
            // Fallback if modalManager is not available
            const modal = document.getElementById('createNotificationModal');
            if (modal) {
                modal.style.display = 'flex';
            }
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
                        // Fallback if modalManager is not available
                        const modal = document.getElementById('editNotificationModal');
                        if (modal) {
                            modal.style.display = 'flex';
                        }
                    }
                } else {
                    if (window.utils && window.utils.showToast) {
                        window.utils.showToast('Notification not found', 'error');
                    } else {
                        alert('Notification not found');
                    }
                }
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading notification for edit:', error);
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Error loading notification details. Please try again.', 'error');
            } else {
                alert('Error loading notification details. Please try again.');
            }
        }
    }

    // Handle edit notification form submission
    async handleEditNotification(e) {
        e.preventDefault();
        
        const notificationId = document.getElementById('edit-notification-id').value;
        const notiType = document.getElementById('edit-notification-type').value;
        const notiMessage = document.getElementById('edit-notification-message').value;
        
        if (!notificationId || !notiType || !notiMessage) {
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Please fill in all required fields', 'error');
            } else {
                alert('Please fill in all required fields');
            }
            return;
        }
        
        const token = this.authManager.getToken();
        
        const requestBody = {
            notiType: notiType,
            notiMessage: notiMessage,
            sendAt: new Date().toISOString()
        };
        
        try {
            const submitButton = e.target.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Updating...';
            }
            
            console.log('Updating notification with data:', requestBody);
            
            const response = await fetch(`https://localhost:7009/api/Notification/UpdateNotification/${notificationId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'accept': '*/*'
                },
                body: JSON.stringify(requestBody)
            });
            
            if (response.ok) {
                if (window.utils && window.utils.showToast) {
                    window.utils.showToast('Notification updated successfully!', 'success');
                } else {
                    alert('Notification updated successfully!');
                }
                this.closeEditModal();
                this.loadNotifications(); // Reload the notifications list
            } else {
                const errorData = await response.text();
                console.error('Update failed:', errorData);
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error updating notification:', error);
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Error updating notification. Please try again.', 'error');
            } else {
                alert('Error updating notification. Please try again.');
            }
        } finally {
            const submitButton = e.target.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-save"></i> Update Notification';
            }
        }
    }

    // Close edit modal
    closeEditModal() {
        if (window.modalManager && window.modalManager.closeModal) {
            window.modalManager.closeModal('editNotificationModal');
        } else {
            // Fallback if modalManager is not available
            const modal = document.getElementById('editNotificationModal');
            if (modal) {
                modal.style.display = 'none';
            }
        }
        
        // Reset form
        const form = document.getElementById('editNotificationForm');
        if (form) {
            form.reset();
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
                    if (window.utils && window.utils.showToast) {
                        window.utils.showToast('Notification deleted successfully!', 'success');
                    } else {
                        alert('Notification deleted successfully!');
                    }
                    this.loadNotifications(); // Reload the notifications list
                } else {
                    const errorData = await response.text();
                    throw new Error(errorData || `HTTP error! status: ${response.status}`);
                }
            } catch (error) {
                console.error('Error deleting notification:', error);
                if (window.utils && window.utils.showToast) {
                    window.utils.showToast('Error deleting notification. Please try again.', 'error');
                } else {
                    alert('Error deleting notification. Please try again.');
                }
            }
        }
    }

    // Get role name from role ID
    getRoleName(roleId) {
        const roles = {
            '1': 'Patients',
            '2': 'Doctors', 
            '3': 'Managers',
            '4': 'Admins'
        };
        return roles[roleId] || 'Unknown Role';
    }

    // Validate notification data
    validateNotificationData(notiType, notiMessage, sendTo, accountId = null) {
        if (!notiType || notiType.trim() === '') {
            return 'Please select a notification type';
        }
        
        if (!notiMessage || notiMessage.trim() === '') {
            return 'Please enter a notification message';
        }
        
        if (!sendTo || sendTo.trim() === '') {
            return 'Please select who to send the notification to';
        }
        
        // If sending to specific account, validate account ID
        if (sendTo === 'account' && (!accountId || accountId.trim() === '')) {
            return 'Please enter a valid account ID';
        }
        
        return null; // No validation errors
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
                if (window.utils && window.utils.showToast) {
                    window.utils.showToast('Notification sent to role successfully!', 'success');
                } else {
                    alert('Notification sent to role successfully!');
                }
                return true;
            } else {
                const errorData = await response.text();
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error sending notification to role:', error);
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Error sending notification to role. Please try again.', 'error');
            } else {
                alert('Error sending notification to role. Please try again.');
            }
            return false;
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
                if (window.utils && window.utils.showToast) {
                    window.utils.showToast('Notification sent to account successfully!', 'success');
                } else {
                    alert('Notification sent to account successfully!');
                }
                return true;
            } else {
                const errorData = await response.text();
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error sending notification to account:', error);
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Error sending notification to account. Please try again.', 'error');
            } else {
                alert('Error sending notification to account. Please try again.');
            }
            return false;
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
                if (window.utils && window.utils.showToast) {
                    window.utils.showToast('Notification sent to all users successfully!', 'success');
                } else {
                    alert('Notification sent to all users successfully!');
                }
                return true;
            } else {
                const errorData = await response.text();
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error sending notification to all:', error);
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Error sending notification to all users. Please try again.', 'error');
            } else {
                alert('Error sending notification to all users. Please try again.');
            }
            return false;
        }
    }

    // Handle create notification form
    async handleCreateNotification(e) {
        e.preventDefault();
        
        const notiType = document.getElementById('notification-type').value;
        const notiMessage = document.getElementById('notification-message').value;
        const sendTo = document.getElementById('send-to').value; // 'all', 'account', or role ID
        const accountId = document.getElementById('account-id') ? document.getElementById('account-id').value : null;
        
        // Validate input data
        const validationError = this.validateNotificationData(notiType, notiMessage, sendTo, accountId);
        if (validationError) {
            if (window.utils && window.utils.showToast) {
                window.utils.showToast(validationError, 'error');
            } else {
                alert(validationError);
            }
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
            } else if (sendTo === 'account') {
                console.log(`Sending notification to account ID: ${accountId}`);
                success = await this.sendNotificationToAccount(accountId, notiType, notiMessage);
            } else {
                // Send to specific role
                const roleName = this.getRoleName(sendTo);
                console.log(`Sending notification to ${roleName} (Role ID: ${sendTo})`);
                success = await this.sendNotificationToRole(sendTo, notiType, notiMessage);
            }
            
            if (success) {
                // Close modal and reset form
                if (window.modalManager && window.modalManager.closeModal) {
                    window.modalManager.closeModal('createNotificationModal');
                } else {
                    // Fallback if modalManager is not available
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
                
                // Hide account ID field if it was shown
                const accountIdGroup = document.getElementById('account-id-group');
                if (accountIdGroup) {
                    accountIdGroup.style.display = 'none';
                }
                
                // Reload notifications
                this.loadNotifications();
            }
        } catch (error) {
            console.error('Error creating notification:', error);
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Error creating notification. Please try again.', 'error');
            } else {
                alert('Error creating notification. Please try again.');
            }
        } finally {
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-paper-plane"></i> Send Notification';
            }
        }
    }

    // Initialize
    init() {
        console.log('Initializing NotificationManager...');
        console.log('window.utils:', window.utils);
        console.log('window.modalManager:', window.modalManager);
        
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
        
        // Send-to dropdown change handler
        const sendToDropdown = document.getElementById('send-to');
        if (sendToDropdown) {
            sendToDropdown.addEventListener('change', () => this.handleSendToChange());
        }
        
        // Form handlers
        const createNotificationForm = document.getElementById('createNotificationForm');
        if (createNotificationForm) {
            createNotificationForm.addEventListener('submit', (e) => this.handleCreateNotification(e));
        }
        
        const editNotificationForm = document.getElementById('editNotificationForm');
        if (editNotificationForm) {
            editNotificationForm.addEventListener('submit', (e) => this.handleEditNotification(e));
        }
        
        console.log('NotificationManager initialized successfully');
    }

    // Handle send-to dropdown change
    handleSendToChange() {
        const sendTo = document.getElementById('send-to').value;
        const accountIdGroup = document.getElementById('account-id-group');
        
        if (accountIdGroup) {
            if (sendTo === 'account') {
                accountIdGroup.style.display = 'block';
            } else {
                accountIdGroup.style.display = 'none';
                // Clear account ID when hidden
                const accountIdField = document.getElementById('account-id');
                if (accountIdField) {
                    accountIdField.value = '';
                }
            }
        }
    }
}

// Export for use in other modules
window.NotificationManager = NotificationManager;
