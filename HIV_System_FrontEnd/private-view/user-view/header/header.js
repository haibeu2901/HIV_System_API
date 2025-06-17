fetch('../header/header.html')
  .then(response => response.text())
  .then(data => {
    document.getElementById('header-placeholder').innerHTML = data;

    // Dropdown toggle logic (runs after header is loaded)
    const dropdown = document.getElementById('viewDropdown');
    const btn = document.getElementById('viewDropdownBtn');
    if (dropdown && btn) {
      btn.addEventListener('click', function(e) {
        e.stopPropagation();
        dropdown.classList.toggle('open');
      });

      // Close dropdown when clicking outside the dropdown
      document.addEventListener('click', function(e) {
        // Only close if the click is outside the dropdown
        if (!dropdown.contains(e.target)) {
          dropdown.classList.remove('open');
        }
      });

      // Prevent closing when clicking inside the dropdown content
      const dropdownContent = dropdown.querySelector('.dropdown-content');
      if (dropdownContent) {
        dropdownContent.addEventListener('click', function(e) {
          e.stopPropagation();
        });
      }
    }
  });

// Make logout globally available
function logout() {
  localStorage.clear();
  sessionStorage.clear();
  window.location.href = 'http://127.0.0.1:5500/public-view/landingpage.html';
}