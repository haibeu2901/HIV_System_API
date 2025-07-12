document.addEventListener('DOMContentLoaded', () => {
    // For main notification page
    const list = document.getElementById('notification-list');
    const token = localStorage.getItem("token");
    if (list) {
        list.innerHTML = "<p>Loading...</p>";
        fetch("https://localhost:7009/api/Notification/GetAllPersonalNotifications", {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({})
        })
        .then(res => {
            if (!res.ok) throw new Error("Failed to fetch notifications");
            return res.json();
        })
        .then(data => {
            if (!data.length) {
                list.innerHTML = "<p>No notifications found.</p>";
                return;
            }
            list.innerHTML = data.map(noti => `
                <div class="notification-card">
                    <span class="notification-icon"><i class="fa-solid fa-bell"></i></span>
                    <div class="notification-content">
                        <div class="notification-message">${noti.notiMessage}</div>
                        <div class="notification-time">
                            <i class="fa-regular fa-clock"></i>
                            ${formatDateTime(noti.sendAt)}
                        </div>
                    </div>
                </div>
            `).join('');
        })
        .catch(err => {
            list.innerHTML = `<p style="color:red;">${err.message}</p>`;
        });
    }

    // For popup notification bell
    const bell = document.getElementById('notificationBell');
    const popup = document.getElementById('notification-popup');
    const closeBtn = document.getElementById('closeNotificationPopup');
    const popupList = document.getElementById('popup-notification-list');
    const tabAll = document.getElementById('tab-all');
    const tabUnread = document.getElementById('tab-unread');
    let notifications = [];

    async function loadNotifications() {
        popupList.innerHTML = "<p style='padding:16px;'>Loading...</p>";
        try {
            const res = await fetch("https://localhost:7009/api/Notification/GetAllPersonalNotifications", {
                method: "POST",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({})
            });
            if (!res.ok) throw new Error("Failed to fetch notifications");
            notifications = await res.json();
            renderNotifications('all');
        } catch (err) {
            popupList.innerHTML = `<p style="color:red;padding:16px;">${err.message}</p>`;
        }
    }

    function renderNotifications(type) {
        let filtered = notifications;
        if (type === 'unread') {
            filtered = notifications.filter(n => !n.isRead);
        }
        if (!filtered.length) {
            popupList.innerHTML = `<p style="padding:16px;">No notifications found.</p>`;
            return;
        }
        popupList.innerHTML = filtered.map(noti => `
            <div class="popup-notification-card${!noti.isRead ? ' unread' : ''}">
                <div class="popup-avatar"><i class="fas fa-bell"></i></div>
                <div class="popup-content">
                    <div class="popup-message">${noti.notiMessage}</div>
                    <div class="popup-time">${formatDateTime(noti.sendAt)}</div>
                </div>
            </div>
        `).join('');
    }

    if (bell && popup && closeBtn) {
        bell.addEventListener('click', () => {
            popup.classList.toggle('hidden');
            if (!popup.classList.contains('hidden')) {
                loadNotifications();
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

    // Tab switching
    if (tabAll && tabUnread) {
        tabAll.addEventListener('click', () => {
            tabAll.classList.add('active');
            tabUnread.classList.remove('active');
            renderNotifications('all');
        });
        tabUnread.addEventListener('click', () => {
            tabAll.classList.remove('active');
            tabUnread.classList.add('active');
            renderNotifications('unread');
        });
    }

    function formatDateTime(dateStr) {
        const d = new Date(dateStr);
        return d.toLocaleString('en-GB', {
            year: 'numeric', month: '2-digit', day: '2-digit',
            hour: '2-digit', minute: '2-digit'
        });
    }
});