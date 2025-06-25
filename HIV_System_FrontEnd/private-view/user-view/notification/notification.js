document.addEventListener('DOMContentLoaded', async () => {
    const token = localStorage.getItem("token");
    const list = document.getElementById('notification-list');
    list.innerHTML = "<p>Loading...</p>";

    try {
        const res = await fetch("https://localhost:7009/api/Notification/GetAllPersonalNotifications", {
            method: "POST", 
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({}) // send an empty object if the API expects a body
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (!res.ok) throw new Error("Failed to fetch notifications");
        const data = await res.json();

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
    } catch (err) {
        list.innerHTML = `<p style="color:red;">${err.message}</p>`;
    }
});

// Show/hide notification popup
document.addEventListener('DOMContentLoaded', () => {
    const bell = document.getElementById('notificationBell');
    const popup = document.getElementById('notification-popup');
    const closeBtn = document.getElementById('closeNotificationPopup');
    const popupList = document.getElementById('popup-notification-list');
    const token = localStorage.getItem("token");

    // Fetch and render notifications when popup is opened
    async function loadPopupNotifications() {
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
            const data = await res.json();
            if (!data.length) {
                popupList.innerHTML = "<p style='padding:16px;'>No notifications found.</p>";
                return;
            }
            popupList.innerHTML = data.map(noti => `
                <div class="popup-notification-card">
                    <div class="popup-avatar"><i class="fas fa-bell"></i></div>
                    <div class="popup-content">
                        <div class="popup-message">${noti.notiMessage}</div>
                        <div class="popup-time">${formatDateTime(noti.sendAt)}</div>
                    </div>
                </div>
            `).join('');
        } catch (err) {
            popupList.innerHTML = `<p style="color:red;padding:16px;">${err.message}</p>`;
        }
    }

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
        // Optional: Hide popup when clicking outside
        document.addEventListener('click', (e) => {
            if (!popup.contains(e.target) && e.target !== bell) {
                popup.classList.add('hidden');
            }
        });
    }
});

=======
function formatDateTime(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleString('en-GB', {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit'
    });
}