fetch('../header/header.html')
  .then(response => response.text())
  .then(data => {
    document.getElementById('header-placeholder').innerHTML = data;

    // Dropdown toggle logic
    const dropdown = document.getElementById('viewDropdown');
    const btn = document.getElementById('viewDropdownBtn');
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

    // Initialize notification system
    initializeNotificationSystem();
  });

// Initialize notification system
function initializeNotificationSystem() {
  const token = localStorage.getItem("token");
  if (!token) {
    console.error('No authentication token found');
    return;
  }

  const bell = document.getElementById('notificationBell');
  const unreadCountBadge = document.getElementById('unread-count');
  
  if (!bell) {
    console.error('Notification bell not found');
    return;
  }

  // Add click handler to redirect to notifications page
  bell.addEventListener('click', () => {
    window.location.href = '../notification/notification.html';
  });

  // Fetch unread count
  async function fetchUnreadCount() {
    try {
      const response = await fetch('https://localhost:7009/api/Notification/GetUnreadNotifications', {
        method: "GET",
        headers: {
          "Authorization": `Bearer ${token}`,
          "accept": "*/*"
        }
      });
      
      if (!response.ok) {
        throw new Error(`Failed to fetch unread count: ${response.status}`);
      }
      
      const unreadNotifications = await response.json();
      return Array.isArray(unreadNotifications) ? unreadNotifications.length : 0;
    } catch (error) {
      console.error('Error fetching unread count:', error);
      return 0;
    }
  }

  // Update unread count badge
  function updateUnreadCountBadge(count) {
    if (unreadCountBadge) {
      if (count > 0) {
        unreadCountBadge.textContent = count > 99 ? '99+' : count;
        unreadCountBadge.style.display = 'block';
      } else {
        unreadCountBadge.style.display = 'none';
      }
    }
  }

  // Show Facebook-style notification popup
  function showFacebookNotificationPopup() {
    // Create popup if it doesn't exist
    let popup = document.getElementById('facebook-notification-popup');
    if (!popup) {
      popup = document.createElement('div');
      popup.id = 'facebook-notification-popup';
      popup.className = 'facebook-notification-popup-container';
      popup.innerHTML = `
        <div class="fb-popup-header">
          <h3><i class="fa-solid fa-bell"></i> Thông báo</h3>
          <button class="fb-popup-close" aria-label="Close notifications">
            <i class="fa-solid fa-times"></i>
          </button>
        </div>
        <div class="fb-popup-tabs">
          <button class="fb-popup-tab active" data-filter="all">Tất cả</button>
          <button class="fb-popup-tab" data-filter="unread">Chưa đọc</button>
        </div>
        <div class="fb-popup-content">
          <div class="fb-popup-loading">Đang tải thông báo...</div>
        </div>
        <div class="fb-popup-footer">
          <button class="fb-popup-mark-all-read">Đánh dấu tất cả đã đọc</button>
          <button class="fb-popup-view-all">Xem tất cả thông báo</button>
        </div>
      `;
      document.body.appendChild(popup);

      // Add event listeners
      const closeBtn = popup.querySelector('.fb-popup-close');
      const markAllReadBtn = popup.querySelector('.fb-popup-mark-all-read');
      const viewAllBtn = popup.querySelector('.fb-popup-view-all');
      const tabs = popup.querySelectorAll('.fb-popup-tab');

      closeBtn.addEventListener('click', () => {
        popup.classList.remove('show');
      });

      markAllReadBtn.addEventListener('click', async () => {
        await markAllAsRead();
        popup.classList.remove('show');
      });

      viewAllBtn.addEventListener('click', () => {
        popup.classList.remove('show');
        window.location.href = '/private-view/user-view/notification/notification.html';
      });

      tabs.forEach(tab => {
        tab.addEventListener('click', () => {
          tabs.forEach(t => t.classList.remove('active'));
          tab.classList.add('active');
          const filter = tab.dataset.filter;
          loadPopupNotifications(filter);
        });
      });

      // Close popup when clicking outside
      document.addEventListener('click', (e) => {
        if (!popup.contains(e.target) && e.target !== bell && !bell.contains(e.target)) {
          popup.classList.remove('show');
        }
      });

      popup.addEventListener('click', (e) => {
        e.stopPropagation();
      });
    }

    // Position popup relative to notification bell
    if (bell) {
      const bellRect = bell.getBoundingClientRect();
      const scrollY = window.scrollY;
      
      // Position popup below the bell, aligned to the right
      popup.style.position = 'fixed';
      popup.style.top = `${bellRect.bottom + 10}px`;
      popup.style.right = `${window.innerWidth - bellRect.right}px`;
      popup.style.left = 'auto';
      
      // Ensure popup doesn't go off-screen
      if (bellRect.right < 400) {
        popup.style.right = '10px';
      }
    }

    // Toggle popup visibility
    popup.classList.toggle('show');
    
    // Load notifications if showing
    if (popup.classList.contains('show')) {
      loadPopupNotifications('all');
    }
  }

  // Load notifications for popup
  async function loadPopupNotifications(filter = 'all') {
    const popup = document.getElementById('facebook-notification-popup');
    const content = popup.querySelector('.fb-popup-content');
    
    if (!content) return;
    
    content.innerHTML = '<div class="fb-popup-loading">Loading notifications...</div>';
    
    try {
      // Use Promise.allSettled instead of Promise.all to handle failures gracefully
      const [allNotificationsResult, unreadNotificationsResult] = await Promise.allSettled([
        fetchNotifications('/api/Notification/GetPersonalNotifications'),
        fetchNotifications('/api/Notification/GetUnreadNotifications')
      ]);
      
      // Handle results with fallbacks
      let allNotifications = [];
      let unreadNotifications = [];
      
      if (allNotificationsResult.status === 'fulfilled') {
        allNotifications = allNotificationsResult.value || [];
      } else {
        console.error('Failed to fetch all notifications:', allNotificationsResult.reason);
      }
      
      if (unreadNotificationsResult.status === 'fulfilled') {
        unreadNotifications = unreadNotificationsResult.value || [];
      } else {
        console.error('Failed to fetch unread notifications:', unreadNotificationsResult.reason);
      }
      
      // If both requests failed, show error
      if (allNotificationsResult.status === 'rejected' && unreadNotificationsResult.status === 'rejected') {
        content.innerHTML = `
          <div class="fb-popup-error">
            <i class="fa-solid fa-exclamation-triangle"></i>
            <p>Unable to load notifications</p>
            <button onclick="loadPopupNotifications('${filter}')" class="fb-popup-retry">
              <i class="fa-solid fa-refresh"></i> Try Again
            </button>
          </div>
        `;
        return;
      }
      
      let notifications = filter === 'unread' ? unreadNotifications : allNotifications;
      const unreadIds = new Set(unreadNotifications.map(n => n.notiId));
      
      if (!notifications || notifications.length === 0) {
        content.innerHTML = `
          <div class="fb-popup-empty">
            <i class="fa-solid fa-bell-slash"></i>
            <p>Không có thông báo ${filter === 'unread' ? 'chưa đọc' : 'nào'}</p>
          </div>
        `;
        return;
      }
      
      // Sort by date (newest first)
      notifications.sort((a, b) => new Date(b.sendAt) - new Date(a.sendAt));
      
      content.innerHTML = notifications.slice(0, 10).map(noti => {
        const isUnread = unreadIds.has(noti.notiId);
        return `
          <div class="fb-popup-notification ${isUnread ? 'unread' : 'read'}" 
               data-notification-id="${noti.notiId}">
            <div class="fb-popup-noti-icon ${getNotificationTypeClass(noti.notiType)}">
              <i class="${getNotificationIcon(noti.notiType)}"></i>
            </div>
            <div class="fb-popup-noti-content">
              <div class="fb-popup-noti-type">${noti.notiType}</div>
              <div class="fb-popup-noti-message">${noti.notiMessage}</div>
              <div class="fb-popup-noti-time">${formatDateTime(noti.sendAt)}</div>
            </div>
            ${isUnread ? '<div class="fb-popup-unread-indicator"></div>' : ''}
          </div>
        `;
      }).join('');
      
      // Add click handlers for individual notifications
      const notiElements = content.querySelectorAll('.fb-popup-notification');
      notiElements.forEach(element => {
        element.addEventListener('click', async () => {
          const notificationId = element.dataset.notificationId;
          if (notificationId) {
            await markAsRead(parseInt(notificationId));
            popup.classList.remove('show');
            window.location.href = '/private-view/user-view/notification/notification.html';
          }
        });
      });
      
    } catch (error) {
      console.error('Error loading popup notifications:', error);
      content.innerHTML = `          <div class="fb-popup-error">
          <i class="fa-solid fa-exclamation-triangle"></i>
          <p>Error loading notifications</p>
          <button onclick="loadPopupNotifications('${filter}')" class="fb-popup-retry">
            <i class="fa-solid fa-refresh"></i> Try Again
          </button>
        </div>
      `;
    }
  }

  // Rate limiting for notification requests
  let lastNotificationRequest = 0;
  const MIN_REQUEST_INTERVAL = 2000; // 2 seconds between requests
  
  function canMakeRequest() {
    const now = Date.now();
    if (now - lastNotificationRequest < MIN_REQUEST_INTERVAL) {
      return false;
    }
    lastNotificationRequest = now;
    return true;
  }
  
  // Wrap loadPopupNotifications with rate limiting
  const originalLoadPopupNotifications = loadPopupNotifications;
  
  async function loadPopupNotifications(filter) {
    if (!canMakeRequest()) {
      console.log('Rate limited - skipping notification request');
      return;
    }
    
    return originalLoadPopupNotifications(filter);
  }

  // Add global exposure for retry button
  window.loadPopupNotifications = loadPopupNotifications;

  // Helper functions
  async function fetchNotifications(endpoint, retryCount = 2) {
    for (let i = 0; i < retryCount; i++) {
      try {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 8000); // 8 second timeout
        
        const response = await fetch(`https://localhost:7009${endpoint}`, {
          method: "GET",
          headers: {
            "Authorization": `Bearer ${token}`,
            "accept": "*/*"
          },
          signal: controller.signal
        });
        
        clearTimeout(timeoutId);
        
        if (!response.ok) {
          if (response.status === 401) {
            // Token expired
            localStorage.removeItem('token');
            localStorage.removeItem('userRole');
            localStorage.removeItem('accId');
            throw new Error('Authentication expired');
          }
          
          // Don't retry on 4xx errors (except 401)
          if (response.status >= 400 && response.status < 500) {
            throw new Error(`Client error: ${response.status}`);
          }
          
          // Retry on 5xx errors
          if (i === retryCount - 1) {
            throw new Error(`Server error: ${response.status}`);
          }
          
          // Wait before retry
          await new Promise(resolve => setTimeout(resolve, (i + 1) * 1000));
          continue;
        }
        
        return await response.json();
        
      } catch (error) {
        if (error.name === 'AbortError') {
          console.error(`Request timeout for ${endpoint}`);
          if (i === retryCount - 1) {
            throw new Error('Request timeout');
          }
        } else {
          console.error(`Error fetching ${endpoint} (attempt ${i + 1}):`, error);
          if (i === retryCount - 1) {
            throw error;
          }
        }
        
        // Wait before retry
        await new Promise(resolve => setTimeout(resolve, (i + 1) * 1000));
      }
    }
    
    return [];
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
      
      if (response.ok) {
        // Update unread count
        const count = await fetchUnreadCount();
        updateUnreadCountBadge(count);
      }
    } catch (error) {
      console.error('Error marking notification as read:', error);
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
      
      if (response.ok) {
        // Update unread count
        updateUnreadCountBadge(0);
      }
    } catch (error) {
      console.error('Error marking all notifications as read:', error);
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
      
      if (minutes < 1) return 'Vừa xong';
      if (minutes < 60) return `${minutes} phút trước`;
      if (hours < 24) return `${hours} giờ trước`;
      if (days < 7) return `${days} ngày trước`;
      
      return d.toLocaleDateString();
    } catch (error) {
      return dateStr;
    }
  }

  function getNotificationIcon(notiType) {
    const icons = {
      'Xác nhận lịch hẹn': 'fa-solid fa-calendar-check',
      'Hủy lịch hẹn': 'fa-solid fa-calendar-times',
      'Cập nhật lịch hẹn': 'fa-solid fa-calendar-pen',
      'Nhắc nhở lịch hẹn': 'fa-solid fa-bell',
      'Kết quả xét nghiệm': 'fa-solid fa-flask',
      'Hồ sơ y tế': 'fa-solid fa-file-medical',
      'Phê duyệt bài viết': 'fa-solid fa-check-circle',
      'Hệ thống': 'fa-solid fa-gear',
      'Chung': 'fa-solid fa-info-circle'
    };
    return icons[notiType] || 'fa-solid fa-bell';
  }

  function getNotificationTypeClass(notiType) {
    const classes = {
      'Xác nhận lịch hẹn': 'type-appointment-confirm',
      'Hủy lịch hẹn': 'type-appointment-cancel',
      'Cập nhật lịch hẹn': 'type-appointment-update',
      'Nhắc nhở lịch hẹn': 'type-appointment-reminder',
      'Kết quả xét nghiệm': 'type-test-result',
      'Hồ sơ y tế': 'type-medical-record',
      'Phê duyệt bài viết': 'type-blog-approval',
      'Hệ thống': 'type-system',
      'Chung': 'type-general'
    };
    return classes[notiType] || 'type-general';
  }

  // Add CSS styles for Facebook-style popup
  if (!document.getElementById('facebook-popup-styles')) {
    const styles = document.createElement('style');
    styles.id = 'facebook-popup-styles';
    styles.textContent = `
      .facebook-notification-popup-container {
        position: fixed;
        top: 60px;
        right: 20px;
        width: 380px;
        max-height: 500px;
        background: #fff;
        border-radius: 8px;
        box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
        z-index: 10000;
        opacity: 0;
        transform: translateY(-10px);
        transition: all 0.3s ease;
        pointer-events: none;
        border: 1px solid #e4e6ea;
      }
      
      .facebook-notification-popup-container.show {
        opacity: 1;
        transform: translateY(0);
        pointer-events: auto;
      }
      
      .fb-popup-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 16px 20px;
        border-bottom: 1px solid #e4e6ea;
      }
      
      .fb-popup-header h3 {
        margin: 0;
        font-size: 20px;
        font-weight: 600;
        color: #1c1e21;
        display: flex;
        align-items: center;
        gap: 8px;
      }
      
      .fb-popup-close {
        background: none;
        border: none;
        font-size: 16px;
        color: #65676b;
        cursor: pointer;
        padding: 8px;
        border-radius: 50%;
        width: 32px;
        height: 32px;
        display: flex;
        align-items: center;
        justify-content: center;
      }
      
      .fb-popup-close:hover {
        background-color: #f0f2f5;
      }
      
      .fb-popup-tabs {
        display: flex;
        border-bottom: 1px solid #e4e6ea;
      }
      
      .fb-popup-tab {
        flex: 1;
        background: none;
        border: none;
        padding: 12px 16px;
        font-size: 14px;
        font-weight: 500;
        color: #65676b;
        cursor: pointer;
        border-bottom: 2px solid transparent;
        transition: all 0.2s;
      }
      
      .fb-popup-tab.active {
        color: #1877f2;
        border-bottom-color: #1877f2;
      }
      
      .fb-popup-tab:hover {
        background-color: #f0f2f5;
      }
      
      .fb-popup-content {
        max-height: 300px;
        overflow-y: auto;
      }
      
      .fb-popup-loading, .fb-popup-empty, .fb-popup-error {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        padding: 40px 20px;
        color: #65676b;
        text-align: center;
      }
      
      .fb-popup-empty i, .fb-popup-error i {
        font-size: 32px;
        margin-bottom: 8px;
        color: #bcc0c4;
      }
      
      .fb-popup-notification {
        display: flex;
        align-items: flex-start;
        gap: 12px;
        padding: 12px 16px;
        cursor: pointer;
        border-bottom: 1px solid #f0f2f5;
        position: relative;
        transition: background-color 0.2s;
      }
      
      .fb-popup-notification:hover {
        background-color: #f0f2f5;
      }
      
      .fb-popup-notification.unread {
        background-color: #e7f3ff;
      }
      
      .fb-popup-notification.unread:hover {
        background-color: #d0e7ff;
      }
      
      .fb-popup-noti-icon {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 16px;
        color: white;
        flex-shrink: 0;
      }
      
      .type-appointment-confirm,
      .type-appointment-update,
      .type-appointment-reminder {
        background: #1877f2;
      }
      
      .type-appointment-cancel {
        background: #e74c3c;
      }
      
      .type-test-result {
        background: #42b883;
      }
      
      .type-medical-record {
        background: #e74c3c;
      }
      
      .type-blog-approval {
        background: #27ae60;
      }
      
      .type-system {
        background: #95a5a6;
      }
      
      .type-general {
        background: #1877f2;
      }
      
      .fb-popup-noti-content {
        flex: 1;
        min-width: 0;
      }
      
      .fb-popup-noti-type {
        font-weight: 600;
        font-size: 14px;
        color: #1c1e21;
        margin-bottom: 4px;
      }
      
      .fb-popup-noti-message {
        font-size: 13px;
        color: #65676b;
        line-height: 1.4;
        margin-bottom: 4px;
        word-wrap: break-word;
      }
      
      .fb-popup-noti-time {
        font-size: 12px;
        color: #8a8d91;
      }
      
      .fb-popup-unread-indicator {
        position: absolute;
        top: 50%;
        right: 16px;
        transform: translateY(-50%);
        width: 8px;
        height: 8px;
        background: #1877f2;
        border-radius: 50%;
      }
      
      .fb-popup-footer {
        display: flex;
        gap: 8px;
        padding: 12px 16px;
        border-top: 1px solid #e4e6ea;
      }
      
      .fb-popup-mark-all-read,
      .fb-popup-view-all {
        flex: 1;
        background: #f0f2f5;
        border: none;
        border-radius: 6px;
        padding: 8px 12px;
        font-size: 13px;
        font-weight: 500;
        color: #1c1e21;
        cursor: pointer;
        transition: background-color 0.2s;
      }
      
      .fb-popup-mark-all-read:hover,
      .fb-popup-view-all:hover {
        background: #e4e6ea;
      }
      
      .fb-popup-view-all {
        background: #e7f3ff;
        color: #1877f2;
      }
      
      .fb-popup-view-all:hover {
        background: #d0e7ff;
      }
      
      /* Mobile responsive */
      @media (max-width: 768px) {
        .facebook-notification-popup-container {
          width: 350px;
          right: 10px;
          top: 70px;
        }
      }
      
      @media (max-width: 480px) {
        .facebook-notification-popup-container {
          width: calc(100vw - 20px);
          right: 10px;
          left: 10px;
          top: 70px;
        }
      }
      
      /* Ensure popup appears above everything */
      .facebook-notification-popup-container {
        z-index: 99999 !important;
      }
    `;
    document.head.appendChild(styles);
  }

  // Bell click handler
  if (bell) {
    bell.addEventListener('click', (e) => {
      e.stopPropagation();
      showFacebookNotificationPopup();
    });
  }

  // Load initial unread count
  fetchUnreadCount().then(updateUnreadCountBadge).catch(console.error);

  // Refresh unread count every 30 seconds
  setInterval(async () => {
    const count = await fetchUnreadCount();
    updateUnreadCountBadge(count);
  }, 30000);
}

// Make logout globally available
function logout() {
  localStorage.clear();
  sessionStorage.clear();
  window.location.href = '/public-view/landingpage.html';
}