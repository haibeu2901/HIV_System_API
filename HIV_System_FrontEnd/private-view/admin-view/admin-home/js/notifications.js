// Notifications Module
class NotificationManager {
    constructor(authManager) {
        this.authManager = authManager;
    }

    // Load notifications
    async loadNotifications() {
        const notificationsList = document.getElementById('notifications-list');
        notificationsList.innerHTML = '<div class="loader"></div>';
        
        // Mock notifications data
        setTimeout(() => {
            const notifications = [
                { id: 1, title: 'System Update', message: 'System will be updated tonight', type: 'system', created: '2024-01-15' },
                { id: 2, title: 'Appointment Reminder', message: 'Multiple appointments scheduled for tomorrow', type: 'appointment', created: '2024-01-14' },
                { id: 3, title: 'Monthly Report', message: 'Monthly report is ready for review', type: 'reminder', created: '2024-01-13' }
            ];
            
            notificationsList.innerHTML = notifications.map(notification => `
                <div class="list-item">
                    <div class="item-content">
                        <div class="item-title">${notification.title}</div>
                        <div class="item-subtitle">${notification.message}</div>
                        <small>Created: ${notification.created}</small>
                    </div>
                    <div class="item-actions">
                        <button class="btn-small btn-edit" onclick="window.notificationManager.editNotification(${notification.id})">Edit</button>
                        <button class="btn-small btn-delete" onclick="window.notificationManager.deleteNotification(${notification.id})">Delete</button>
                    </div>
                </div>
            `).join('');
        }, 1000);
    }

    // Show create notification modal
    showCreateNotificationModal() {
        const modal = document.getElementById('createNotificationModal');
        if (modal) {
            modal.style.display = 'block';
        }
    }

    // Filter notifications
    filterNotifications() {
        const filter = document.getElementById('notification-filter').value;
        console.log('Filtering notifications by:', filter);
        this.loadNotifications();
    }

    // Edit notification
    editNotification(id) {
        console.log('Edit notification:', id);
        alert('Edit notification functionality will be implemented here');
    }

    // Delete notification
    deleteNotification(id) {
        if (confirm('Are you sure you want to delete this notification?')) {
            console.log('Delete notification:', id);
            this.loadNotifications();
        }
    }

    // Handle create notification form
    handleCreateNotification(e) {
        e.preventDefault();
        
        const title = document.getElementById('notification-title').value;
        const message = document.getElementById('notification-message').value;
        const type = document.getElementById('notification-type').value;
        
        console.log('Creating notification:', { title, message, type });
        
        alert('Notification created successfully!');
        window.modalManager.closeModal('createNotificationModal');
        this.loadNotifications();
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
