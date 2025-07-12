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

    // Notification bell popup logic
    const bell = document.getElementById('notificationBell');
    const popup = document.getElementById('notification-popup');
    const closeBtn = document.getElementById('closeNotificationPopup');
    if (bell && popup && closeBtn) {
      bell.addEventListener('click', (e) => {
        e.stopPropagation();
        popup.classList.toggle('hidden');
        // Optionally: load notifications here
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
  });

// Make logout globally available
function logout() {
  localStorage.clear();
  sessionStorage.clear();
  window.location.href = '/public-view/landingpage.html';
}