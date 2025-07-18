// Shared Header Loader and Logic (adapted from doctor-header)
function loadSharedHeader() {
    fetch('/private-view/shared/header/header.html')
        .then(res => res.text())
        .then(html => {
            const placeholder = document.getElementById('header-placeholder');
            if (placeholder) {
                placeholder.innerHTML = html;
                setActiveSharedNav();
                setLogoutHandler();
                setHamburgerHandler();
                setDropdownHandler();
                setNotificationBellHandler();
                setBlogNavigationHandler();
                updateSharedUnreadBadge();
                // Role-based menu logic
                const userRoleId = window.roleUtils && window.roleUtils.getUserRole ? window.roleUtils.getUserRole() : null;
                const userRoleName = (window.roleUtils && window.roleUtils.ROLE_NAMES && userRoleId) ? window.roleUtils.ROLE_NAMES[userRoleId] : 'guest';
                // Hide Work Schedule for all roles except doctor
                if (userRoleName !== 'doctor') {
                    document.getElementById('nav-work-schedule')?.classList.add('hidden');
                } else {
                    document.getElementById('nav-work-schedule')?.classList.remove('hidden');
                }
                // Set dashboard link and dropdown label per role (only doctor and staff for now)
                let dashboardLink = '/private-view/doctor-view/doctor-dashboard/doctor-dashboard.html';
                let dropdownLabel = '<i class="fas fa-user-md"></i> Bác sĩ <i class="fas fa-caret-down"></i>';
                if (userRoleName === 'staff') {
                    dashboardLink = '/private-view/staff-view/staff-dashboard/staff-dashboard.html';
                    dropdownLabel = '<i class="fas fa-user"></i> Nhân viên <i class="fas fa-caret-down"></i>';
                }
                // For admin and manager, use doctor dashboard and label for now
                const dashboardAnchor = document.getElementById('nav-dashboard');
                if (dashboardAnchor) dashboardAnchor.setAttribute('href', dashboardLink);
                document.getElementById('sharedDropdownBtn').innerHTML = dropdownLabel;
            }
        });
}

function setActiveSharedNav() {
    const path = window.location.pathname;
    const navLinks = [
        { id: 'nav-dashboard', path: '/private-view/doctor-view/doctor-dashboard/doctor-dashboard.html' },
        { id: 'nav-work-schedule', path: '/private-view/doctor-view/doctor-work-schedule/doctor-work-schedule.html' },
        { id: 'nav-appointments', path: '/private-view/shared/appointment-list/appoiment-list.html' },
        { id: 'nav-patient-list', path: '/private-view/doctor-view/patient-list/patient-list.html' },
        { id: 'nav-medical-resources', path: '/private-view/shared/medical-resources/medical-resources.html' },
        { id: 'nav-blog', path: '/blog' }, // Dynamic based on role
        { id: 'nav-notifications', path: '/private-view/shared/notifications/notifications.html' },
        { id: 'nav-profile', path: '/private-view/shared/user-profile/user-profile.html' }
    ];
    navLinks.forEach(link => {
        const el = document.getElementById(link.id);
        if (el && path.includes(link.path)) {
            el.classList.add('active');
        }
    });
}

function setLogoutHandler() {
    const logoutBtn = document.getElementById('nav-logout');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', (e) => {
            e.preventDefault();
            localStorage.clear();
            sessionStorage.clear();
            window.location.href = '/public-view/landingpage.html';
        });
    }
}

function setHamburgerHandler() {
    const hamburger = document.getElementById('shared-header-hamburger');
    const navLinks = document.getElementById('shared-header-links');
    if (hamburger && navLinks) {
        hamburger.addEventListener('click', () => {
            navLinks.classList.toggle('show');
        });
        navLinks.querySelectorAll('a,button').forEach(link => {
            link.addEventListener('click', () => {
                navLinks.classList.remove('show');
            });
        });
    }
}

function setDropdownHandler() {
    const dropdown = document.getElementById('sharedDropdown');
    const btn = document.getElementById('sharedDropdownBtn');
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

function updateSharedUnreadBadge() {
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
        const badge = document.getElementById('shared-unread-badge');
        const badgeHeader = document.getElementById('shared-unread-badge-header');
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

// Blog Navigation Handler - Routes to appropriate blog page based on user role
function setBlogNavigationHandler() {
    const blogLink = document.getElementById('nav-blog');
    if (blogLink) {
        blogLink.addEventListener('click', (e) => {
            e.preventDefault();
            
            // Get user role
            const userRoleId = window.roleUtils && window.roleUtils.getUserRole ? window.roleUtils.getUserRole() : null;
            const userRoleName = (window.roleUtils && window.roleUtils.ROLE_NAMES && userRoleId) ? window.roleUtils.ROLE_NAMES[userRoleId] : 'guest';
            
            // Route to appropriate blog page based on role
            let blogUrl = '/public-view/blog/blog-public.html'; // default fallback
            
            switch (userRoleName) {
                case 'doctor':
                    blogUrl = '/private-view/doctor-view/doctor-dashboard/doctor-dashboard-with-blog.html';
                    break;
                case 'admin':
                    blogUrl = '/private-view/admin-view/blog-management.html';
                    break;
                case 'staff':
                    blogUrl = '/private-view/staff-view/staff-community.html';
                    break;
                case 'manager':
                    blogUrl = '/private-view/manager-view/manager-blog-overview.html';
                    break;
                default:
                    // For unknown roles, try public blog
                    blogUrl = '/public-view/blog/blog-public.html';
            }
            
            window.location.href = blogUrl;
        });
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', loadSharedHeader);
} else {
    loadSharedHeader();
} 