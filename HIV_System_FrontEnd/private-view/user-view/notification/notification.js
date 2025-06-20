document.addEventListener('DOMContentLoaded', async () => {
    const token = localStorage.getItem("token");
    const list = document.getElementById('notification-list');
    list.innerHTML = "<p>Loading...</p>";

    try {
        const res = await fetch("https://localhost:7009/api/Notification/GetAllPersonalNotifications", {
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

function formatDateTime(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleString('en-GB', {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit'
    });
}