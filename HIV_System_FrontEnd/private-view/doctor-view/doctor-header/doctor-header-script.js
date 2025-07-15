// Doctor Header Loader and Logic

// Dynamically load the header HTML into #header-placeholder
function loadDoctorHeader() {
    // Use an absolute path so it works from any doctor-view subdirectory
    fetch('/HIV_System_FrontEnd/private-view/doctor-view/doctor-header/doctor-header.html')
        .then(res => res.text())
        .then(html => {
            const placeholder = document.getElementById('header-placeholder');
            if (placeholder) {
                placeholder.innerHTML = html;
                setActiveDoctorNav();
                setLogoutHandler();
                setHamburgerHandler();
                setDropdownHandler();
                setNotificationBellHandler();
                updateDoctorUnreadBadge();
            }
        });
}

// Highlight the active nav link based on current page
function setActiveDoctorNav() {
    const path = window.location.pathname;
    const navLinks = [
        { id: 'nav-dashboard', path: '/HIV_System_FrontEnd/private-view/doctor-view/doctor-dashboard/doctor-dashboard.html' },
        { id: 'nav-work-schedule', path: '/HIV_System_FrontEnd/private-view/doctor-view/doctor-work-schedule/doctor-work-schedule.html' },
        { id: 'nav-appointments', path: '/HIV_System_FrontEnd/private-view/doctor-view/appointment-list/appoiment-list.html' },
        { id: 'nav-patient-list', path: '/HIV_System_FrontEnd/private-view/doctor-view/patient-list/patient-list.html' },
        { id: 'nav-medical-resources', path: '/HIV_System_FrontEnd/private-view/doctor-view/medical-resources/medical-resources.html' },
        { id: 'nav-notifications', path: '/HIV_System_FrontEnd/private-view/doctor-view/doctor-notifications/doctor-notifications.html' },
        { id: 'nav-profile', path: '/HIV_System_FrontEnd/private-view/doctor-view/doctor-profile/doctor-profile.html' }
    ];
    navLinks.forEach(link => {
        const el = document.getElementById(link.id);
        if (el && path.includes(link.path)) {
            el.classList.add('active');
        }
    });
}

// Logout handler
function setLogoutHandler() {
    const logoutBtn = document.getElementById('nav-logout');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', (e) => {
            e.preventDefault();
            localStorage.clear();
            sessionStorage.clear();
            window.location.href = '/HIV_System_FrontEnd/public-view/landingpage.html';
        });
    }
}

// Hamburger menu handler
function setHamburgerHandler() {
    const hamburger = document.getElementById('doctor-header-hamburger');
    const navLinks = document.getElementById('doctor-header-links');
    if (hamburger && navLinks) {
        hamburger.addEventListener('click', () => {
            navLinks.classList.toggle('show');
        });
        // Close menu when a link is clicked (for mobile UX)
        navLinks.querySelectorAll('a,button').forEach(link => {
            link.addEventListener('click', () => {
                navLinks.classList.remove('show');
            });
        });
    }
}

// Dropdown menu handler for doctor menu
function setDropdownHandler() {
    const dropdown = document.getElementById('doctorDropdown');
    const btn = document.getElementById('doctorDropdownBtn');
    if (dropdown && btn) {
        btn.addEventListener('click', function(e) {
            e.stopPropagation();
            dropdown.classList.toggle('open');
        });
        document.addEventListener('click', function(e) {
            if (!dropdown.contains(e.target)) {
                dropdown.classList.remove('open');
            }
        });
        const dropdownContent = dropdown.querySelector('.dropdown-content');
        if (dropdownContent) {
            dropdownContent.addEventListener('click', function(e) {
                e.stopPropagation();
            });
        }
    }
}

// Notification bell popup handler
function setNotificationBellHandler() {
    const bell = document.getElementById('notificationBell');
    const popup = document.getElementById('notification-popup');
    const closeBtn = document.getElementById('closeNotificationPopup');
    if (bell && popup && closeBtn) {
        bell.addEventListener('click', (e) => {
            e.stopPropagation();
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
}

// Update unread badge for notifications
function updateDoctorUnreadBadge() {
    const token = localStorage.getItem('token');
    if (!token) return;
    fetch('https://localhost:7009/api/Notification/GetUnreadCount', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'accept': '*/*'
        }
    })
    .then(res => res.ok ? res.json() : 0)
    .then(count => {
        const badge = document.getElementById('doctor-unread-badge');
        const badgeHeader = document.getElementById('doctor-unread-badge-header');
        if (badge) {
            if (count > 0) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.style.display = 'inline-flex';
            } else {
                badge.style.display = 'none';
            }
        }
        if (badgeHeader) {
            if (count > 0) {
                badgeHeader.textContent = count > 99 ? '99+' : count;
                badgeHeader.style.display = 'inline-flex';
            } else {
                badgeHeader.style.display = 'none';
            }
        }
    })
    .catch(() => {});
}

// Load notifications for popup (reuse logic from doctor-notifications-script.js if needed)
function loadPopupNotifications() {
    const token = localStorage.getItem('token');
    const popupList = document.getElementById('popup-notification-list');
    if (!popupList || !token) return;
    popupList.innerHTML = "<p style='padding:16px;'>Loading...</p>";
    fetch('https://localhost:7009/api/Notification/GetPersonalNotifications', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'accept': '*/*'
        }
    })
    .then(res => res.ok ? res.json() : [])
    .then(notifications => {
        if (!notifications.length) {
            popupList.innerHTML = `<div class='no-notifications'>No notifications found.</div>`;
            return;
        }
        notifications.sort((a, b) => new Date(b.sendAt) - new Date(a.sendAt));
        popupList.innerHTML = notifications.slice(0, 10).map(noti => {
            const isUnread = noti.status === 1 || noti.status === '1' || noti.isUnread;
            return `
                <div class="notification-card ${isUnread ? 'unread' : 'read'}">
                    <div style="display:flex;align-items:center;gap:10px;">
                        <i class="fas fa-bell"></i>
                        <span style="font-weight:600;">${noti.notiType || 'Notification'}</span>
                        <span style="margin-left:auto;font-size:0.9rem;color:#888;">${formatDateTime(noti.sendAt)}</span>
                    </div>
                    <div style="margin-top:6px;">${noti.notiMessage}</div>
                </div>
            `;
        }).join('');
    })
    .catch(() => {
        popupList.innerHTML = `<div class='no-notifications'>Failed to load notifications.</div>`;
    });
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

// Load header on DOMContentLoaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', loadDoctorHeader);
} else {
    loadDoctorHeader();
}
