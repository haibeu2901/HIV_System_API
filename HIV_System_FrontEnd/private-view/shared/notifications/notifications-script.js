// Notifications Script
// Adapted from patient notification.js

document.addEventListener('DOMContentLoaded', () => {
    // Check if token exists
    const token = localStorage.getItem("token");
    if (!token) {
        alert('You need to be logged in to view notifications. Redirecting to login...');
        window.location.href = '/public-view/landingpage.html';
        return;
    }

    // Main notification list
    const list = document.getElementById('notification-list');
    if (list) {
        loadMainNotifications();
    }

    // Load notifications for main page
    async function loadMainNotifications() {
        list.innerHTML = "<p>Loading...</p>";
        try {
            const allNotifications = await fetchNotifications('/api/Notification/GetPersonalNotifications');
            const unreadNotifications = await fetchNotifications('/api/Notification/GetUnreadNotifications');
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
            const unreadIds = new Set(unreadNotifications.map(n => n.notiId));
            // Controls
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
            // Mark all as read
            document.querySelector('.mark-all-read-btn').addEventListener('click', markAllAsRead);
        } catch (error) {
            list.innerHTML = `<div class="error-container"><h3>Error Loading Notifications</h3><p>${error.message}</p></div>`;
        }
    }

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
        // Add event listeners for mark as read
        container.querySelectorAll('.mark-read-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const notificationId = btn.getAttribute('data-notification-id');
                if (notificationId) markAsRead(parseInt(notificationId));
            });
        });
    }

    async function fetchNotifications(endpoint) {
        const response = await fetch(`https://localhost:7009${endpoint}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "accept": "*/*"
            }
        });
        if (!response.ok) throw new Error(`Failed to fetch notifications: ${response.status}`);
        return await response.json();
    }

    async function markAsRead(notificationId) {
        try {
            const response = await fetch(`https://localhost:7009/api/Notification/MarkAsRead/${notificationId}`, {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "accept": "*/*"
                }
            });
            if (!response.ok) throw new Error('Failed to mark notification as read.');
            // Refresh
            loadMainNotifications();
        } catch (error) {
            alert('Failed to mark notification as read. Please try again.');
        }
    }

    async function markAllAsRead() {
        try {
            const response = await fetch('https://localhost:7009/api/Notification/MarkAllAsRead', {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "accept": "*/*"
                }
            });
            if (!response.ok) throw new Error('Failed to mark all notifications as read.');
            loadMainNotifications();
        } catch (error) {
            alert('Failed to mark all notifications as read. Please try again.');
        }
    }

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
                year: 'numeric', month: '2-digit', day: '2-digit',
                hour: '2-digit', minute: '2-digit'
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
});
