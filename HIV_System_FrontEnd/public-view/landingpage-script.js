// Loading Functions
function showPageLoader() {
  const loader = document.createElement('div');
  loader.className = 'page-loading';
  loader.innerHTML = `
    <div class="loader"></div>
    <div class="loading-text">Loading CareFirst HIV Clinic...</div>
  `;
  document.body.appendChild(loader);
  return loader;
}

function hidePageLoader() {
  const loader = document.querySelector('.page-loading');
  if (loader) {
    loader.classList.add('fade-out');
    setTimeout(() => {
      loader.remove();
    }, 500);
  }
}

// Medical Services Functions
async function fetchMedicalServices() {
  try {
    console.log('Fetching medical services from API...');
    
    const response = await fetch('https://localhost:7009/api/MedicalService/GetAllMedicalServices', {
      method: 'GET',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const services = await response.json();
    console.log('Medical services fetched successfully:', services);
    
    return services.filter(service => service.isAvailable);
  } catch (error) {
    console.error('Error fetching medical services:', error);
    // Return fallback services if API fails
    return [
      {
        serviceId: 1,
        serviceName: "Xét nghiệm HIV Combo",
        serviceDescription: "Sàng lọc HIV thế hệ 4, phát hiện kháng thể và kháng nguyên p24.",
        price: 250000,
        isAvailable: true
      },
      {
        serviceId: 2,
        serviceName: "Tư vấn PrEP",
        serviceDescription: "Tư vấn và cung cấp thuốc dự phòng trước phơi nhiễm.",
        price: 500000,
        isAvailable: true
      },
      {
        serviceId: 3,
        serviceName: "Điều trị ARV",
        serviceDescription: "Đánh giá lâm sàng và liệu pháp ARV cá nhân hóa.",
        price: 300000,
        isAvailable: true
      }
    ];
  }
}

function formatPrice(price) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(price);
}

function getServiceIcon(serviceName) {
  const iconMap = {
    'Xét nghiệm HIV Combo': 'fas fa-vial',
    'Tư vấn PrEP': 'fas fa-shield-alt',
    'Điều trị ARV': 'fas fa-pills',
    'Xét nghiệm tải lượng HIV': 'fas fa-microscope',
    'Đếm CD4': 'fas fa-dna',
    'Tư vấn HIV': 'fas fa-comments',
    'Tư vấn PEP': 'fas fa-first-aid'
  };
  
  return iconMap[serviceName] || 'fas fa-stethoscope';
}

async function renderMedicalServices() {
  const servicesGrid = document.querySelector('.services-grid');
  if (!servicesGrid) {
    console.error('Services grid not found');
    return;
  }

  // Show loading state
  servicesGrid.innerHTML = `
    <div class="services-loading">
      <i class="fas fa-spinner fa-spin"></i>
      <p>Đang tải dịch vụ...</p>
    </div>
  `;

  try {
    const services = await fetchMedicalServices();
    
    // Clear loading and render services
    servicesGrid.innerHTML = '';
    
    services.forEach(service => {
      const serviceCard = document.createElement('div');
      serviceCard.className = 'service-card medical-service-card';
      serviceCard.innerHTML = `
        <i class="${getServiceIcon(service.serviceName)}"></i>
        <h3>${service.serviceName}</h3>
        <p>${service.serviceDescription}</p>
        <div class="service-price">${formatPrice(service.price)}</div>
        <button class="service-book-btn" onclick="openModal('loginModal')">
          <i class="fas fa-calendar-plus"></i> Đặt lịch
        </button>
      `;
      servicesGrid.appendChild(serviceCard);
    });

    // Add the additional service cards (ARV medications and Community Blog)
    addAdditionalServiceCards(servicesGrid);

  } catch (error) {
    console.error('Error rendering medical services:', error);
    servicesGrid.innerHTML = `
      <div class="services-error">
        <i class="fas fa-exclamation-triangle"></i>
        <p>Không thể tải dịch vụ. Vui lòng thử lại sau.</p>
        <button onclick="renderMedicalServices()" class="retry-btn">
          <i class="fas fa-redo"></i> Thử lại
        </button>
      </div>
    `;
  }
}

function addAdditionalServiceCards(servicesGrid) {
  // ARV Medications card
  const arvCard = document.createElement('div');
  arvCard.className = 'service-card medication-info-card';
  arvCard.innerHTML = `
    <i class="fas fa-pills"></i>
    <h3>Thuốc ARV</h3>
    <p>Tìm hiểu về các loại thuốc kháng vi-rút hiện có và công dụng của chúng trong điều trị HIV.</p>
    <a href="arv-medications-public.html" class="service-link">
      <i class="fas fa-arrow-right"></i> Xem Thuốc
    </a>
  `;
  servicesGrid.appendChild(arvCard);

  // Community Blog card
  const blogCard = document.createElement('div');
  blogCard.className = 'service-card';
  blogCard.innerHTML = `
    <i class="fas fa-blog"></i>
    <h3>Câu chuyện cộng đồng</h3>
    <p>Đọc những câu chuyện và trải nghiệm đầy cảm hứng từ cộng đồng hỗ trợ HIV của chúng tôi.</p>
    <a href="./blog/blog-public.html" class="service-link">
      <i class="fas fa-arrow-right"></i> Đọc Blog
    </a>
  `;
  servicesGrid.appendChild(blogCard);
}

function showButtonLoader(button, text = 'Loading...') {
  button.disabled = true;
  button.classList.add('btn-loading');
  button.dataset.originalText = button.textContent;
  button.innerHTML = `
    <span style="opacity: 0;">${button.dataset.originalText}</span>
    <div class="small-loader" style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);"></div>
  `;
}

function hideButtonLoader(button) {
  button.disabled = false;
  button.classList.remove('btn-loading');
  button.innerHTML = button.dataset.originalText || 'Submit';
}

// Mobile Navigation Toggle
const hamburger = document.querySelector(".hamburger");
const navMenu = document.querySelector(".nav-menu");

hamburger.addEventListener("click", () => {
  hamburger.classList.toggle("active");
  navMenu.classList.toggle("active");
});

// Close mobile menu when clicking on a link
document.querySelectorAll(".nav-menu a").forEach((n) =>
  n.addEventListener("click", () => {
    hamburger.classList.remove("active");
    navMenu.classList.remove("active");
  })
);

// Modal Functions
function openModal(modalId) {
  const modal = document.getElementById(modalId);
  modal.style.display = "block";
  document.body.style.overflow = "hidden";

  // Focus on first input field
  setTimeout(() => {
    const firstInput = modal.querySelector("input");
    if (firstInput) firstInput.focus();
  }, 100);
}

function closeModal(modalId) {
  const modal = document.getElementById(modalId);
  modal.style.display = "none";
  document.body.style.overflow = "auto";
  clearFormErrors(modalId);
}

function switchModal(currentModalId, targetModalId) {
  closeModal(currentModalId);
  setTimeout(() => openModal(targetModalId), 100);
}

// Close modal when clicking outside
window.addEventListener("click", (event) => {
  const modals = document.querySelectorAll(".modal");
  modals.forEach((modal) => {
    if (event.target === modal) {
      closeModal(modal.id);
    }
  });
});

// Close modal with Escape key
document.addEventListener("keydown", (event) => {
  if (event.key === "Escape") {
    const openModal = document.querySelector('.modal[style*="block"]');
    if (openModal) {
      closeModal(openModal.id);
    }
  }
});
// Username Validation Function
function validateUsername(username) {
  const usernameRegex = /^[a-zA-Z0-9_]{3,20}$/;
  return usernameRegex.test(username);
}

// Smooth Scrolling
function scrollToSection(sectionId) {
  const section = document.getElementById(sectionId);
  if (section) {
    section.scrollIntoView({
      behavior: "smooth",
      block: "start",
    });
  }
}

// Form Validation Functions
function validateEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

function validatePhone(phone) {
  const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
  return phoneRegex.test(phone.replace(/[\s\-$$$$]/g, ""));
}

function validatePassword(password) {
  return password.length >= 8;
}

function validateName(name) {
  return name.trim().length >= 2;
}

function showError(fieldId, message) {
  const errorElement = document.getElementById(fieldId + "Error");
  const inputElement = document.getElementById(fieldId);

  if (errorElement) {
    errorElement.textContent = message;
    errorElement.style.display = "block";
  }

  if (inputElement) {
    inputElement.style.borderColor = "#e74c3c";
  }
}

function clearError(fieldId) {
  const errorElement = document.getElementById(fieldId + "Error");
  const inputElement = document.getElementById(fieldId);

  if (errorElement) {
    errorElement.textContent = "";
    errorElement.style.display = "none";
  }

  if (inputElement) {
    inputElement.style.borderColor = "#e0e0e0";
  }
}

function clearFormErrors(modalId) {
  const modal = document.getElementById(modalId);
  const errorElements = modal.querySelectorAll(".error-message");
  const inputElements = modal.querySelectorAll("input");

  errorElements.forEach((error) => {
    error.textContent = "";
    error.style.display = "none";
  });

  inputElements.forEach((input) => {
    input.style.borderColor = "#e0e0e0";
  });
}

// Login Form Validation and Submission
document.getElementById("loginForm").addEventListener("submit", function (e) {
  e.preventDefault();

  const username = document.getElementById("loginUsername").value;
  const password = document.getElementById("loginPassword").value;

  let isValid = true;

  // Clear previous errors
  clearError("loginUsername");
  clearError("loginPassword");

  // Validate email
  if (!username) {
    showError("loginUsername", "Email is required");
    isValid = false;
  }

  // Validate password
  if (!password) {
    showError("loginPassword", "Password is required");
    isValid = false;
  }

  if (isValid) {
    const submitButton = this.querySelector('button[type="submit"]');
    
    // Show loading state
    showButtonLoader(submitButton, 'Logging in...');

    const logginFunction = async (userName, password) => {
      try {
        const response = await fetch(
          `https://localhost:7009/api/Account/GetAccountByLogin`,
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              accUsername: userName,
              accPassword: password,
            }),
          }
        );

        if (!response.ok) {
          let errorMessage = "Login failed";
          
          try {
            // Try to parse error response as JSON
            const errorData = await response.json();
            errorMessage = errorData.message || errorData.error || errorMessage;
          } catch (parseError) {
            // If parsing fails, try to get text response
            try {
              const errorText = await response.text();
              errorMessage = errorText || `Login failed (Status: ${response.status})`;
            } catch (textError) {
              errorMessage = `Login failed (Status: ${response.status})`;
            }
          }
          
          throw new Error(errorMessage);
        }

        const data = await response.json();
        const fullName = data.account.fullName;
        const role = data.account.roles;
        localStorage.setItem("token", data.token);
        localStorage.setItem("accId", data.account.accId);
        localStorage.setItem("userId", data.account.accId); // Use accId as userId
        localStorage.setItem("userRole", role); // Store user role
        localStorage.setItem("fullName", fullName); // Store full name
        
        // Show success message briefly
        submitButton.innerHTML = `
          <span style="opacity: 0;">${submitButton.dataset.originalText}</span>
          <div style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); color: #27ae60;">
            <i class="fas fa-check"></i> Success!
          </div>
        `;
        
        // Redirect after showing success
        setTimeout(() => {
          if (role == 1) {
            window.location.href = "../private-view/admin-view/admin-home/admin-home.html";
          } else if (role == 5) {
            window.location.href = "../private-view/manager-view/manager-home/manager-home.html";
          } else if (role == 3) {
            window.location.href = "../private-view/user-view/booking/appointment-booking.html";
          } else if (role == 2) {
            window.location.href = "../private-view/doctor-view/doctor-dashboard/doctor-dashboard.html";
          } else if (role== 4){
            window.location.href = "../private-view/staff-view/staff-dashboard/staff-dashboard.html";
          }
        }, 1000);
        
      } catch (error) {
        console.error("Error during login:", error);
        throw error;
      }
    };

    logginFunction(username, password)
      .then(() => {
        // Success handled above
      })
      .catch((error) => {
        hideButtonLoader(submitButton);
        console.error("Login error:", error);
        
        // Show error message under username field instead of generic message
        const errorMessage = error.message || "Invalid email or password";
        showError("loginUsername", errorMessage);
      });
  }
});

// Register Form Validation and Submission
document.getElementById("registerForm").addEventListener("submit", async function (e) {
  e.preventDefault();

  const username = document.getElementById("username").value;
  const fullName = document.getElementById("fullName").value;
  const email = document.getElementById("registerEmail").value;
  const dateOfBirth = document.getElementById("dateOfBirth").value;
  const gender = document.querySelector('input[name="gender"]:checked');
  const password = document.getElementById("registerPassword").value;
  const confirmPassword = document.getElementById("confirmPassword").value;
  const termsAccepted = document.querySelector('input[name="terms"]').checked;

  let isValid = true;

  // Clear previous errors
  [
    "username",
    "fullName",
    "registerEmail",
    "dateOfBirth",
    "registerPassword",
    "confirmPassword",
  ].forEach(clearError);

  // Validate username
  if (!username) {
    showError("username", "Username is required");
    isValid = false;
  } else if (!validateUsername(username)) {
    showError(
      "username",
      "Username must be 3-20 characters and contain only letters, numbers, and underscores"
    );
    isValid = false;
  }

  // Validate full name
  if (!fullName) {
    showError("fullName", "Full name is required");
    isValid = false;
  } else if (!validateName(fullName)) {
    showError("fullName", "Full name must be at least 2 characters");
    isValid = false;
  }

  // Validate email
  if (!email) {
    showError("registerEmail", "Email is required");
    isValid = false;
  } else if (!validateEmail(email)) {
    showError("registerEmail", "Please enter a valid email address");
    isValid = false;
  }

  // Validate date of birth
  if (!dateOfBirth) {
    showError("dateOfBirth", "Date of birth is required");
    isValid = false;
  } else {
    const birthDate = new Date(dateOfBirth);
    const today = new Date();
    const age = today.getFullYear() - birthDate.getFullYear();
    if (age < 18) {
      showError("dateOfBirth", "You must be at least 18 years old");
      isValid = false;
    }
  }

  // Validate gender
  if (!gender) {
    showError("gender", "Please select your gender");
    isValid = false;
  }

  // Validate password
  if (!password) {
    showError("registerPassword", "Password is required");
    isValid = false;
  } else if (!validatePassword(password)) {
    showError(
      "registerPassword",
      "Password must be at least 8 characters long"
    );
    isValid = false;
  }

  // Validate confirm password
  if (!confirmPassword) {
    showError("confirmPassword", "Please confirm your password");
    isValid = false;
  } else if (password !== confirmPassword) {
    showError("confirmPassword", "Passwords do not match");
    isValid = false;
  }

  // Validate terms acceptance
  if (!termsAccepted) {
    alert(
      "Please accept the Terms of Service and Privacy Policy to continue."
    );
    isValid = false;
  }

  if (isValid) {
    const submitButton = this.querySelector('button[type="submit"]');
    
    // Show loading state
    showButtonLoader(submitButton, 'Creating Account...');

    // Prepare data for API
    const genderValue = gender.value === "male" ? true : false;
    const dobISO = new Date(dateOfBirth).toISOString();

    const patientData = {
      accUsername: username,
      accPassword: password,
      email: email,
      fullname: fullName,
      dob: dobISO,
      gender: genderValue,
    };

    try {
      const response = await fetch(
        "https://localhost:7009/api/Account/RegisterPatient",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(patientData),
        }
      );

      if (!response.ok) {
        let errorMessage = "Registration failed";
        
        try {
          // Try to parse error response as JSON
          const errorData = await response.json();
          errorMessage = errorData.message || errorData.error || errorMessage;
        } catch (parseError) {
          // If parsing fails, try to get text response
          try {
            const errorText = await response.text();
            errorMessage = errorText || `Registration failed (Status: ${response.status})`;
          } catch (textError) {
            errorMessage = `Registration failed (Status: ${response.status})`;
          }
        }
        
        // Handle specific error cases based on status code and message
        if (response.status === 409) {
          // Conflict - username or email already exists
          if (errorMessage.toLowerCase().includes('username')) {
            showError("username", errorMessage);
          } else if (errorMessage.toLowerCase().includes('email')) {
            showError("registerEmail", errorMessage);
          } else {
            showError("username", errorMessage);
          }
        } else if (response.status === 400) {
          // Bad request - validation errors
          if (errorMessage.toLowerCase().includes('username')) {
            showError("username", errorMessage);
          } else if (errorMessage.toLowerCase().includes('email')) {
            showError("registerEmail", errorMessage);
          } else if (errorMessage.toLowerCase().includes('password')) {
            showError("registerPassword", errorMessage);
          } else if (errorMessage.toLowerCase().includes('fullname') || errorMessage.toLowerCase().includes('full name')) {
            showError("fullName", errorMessage);
          } else if (errorMessage.toLowerCase().includes('date') || errorMessage.toLowerCase().includes('birth')) {
            showError("dateOfBirth", errorMessage);
          } else {
            // Show general error if we can't determine the field
            showError("username", errorMessage);
          }
        } else {
          // Other status codes - show general error
          showError("username", `Registration failed: ${errorMessage}`);
        }
        
        hideButtonLoader(submitButton);
        return;
      }

      // Show success state
      submitButton.innerHTML = `
        <span style="opacity: 0;">${submitButton.dataset.originalText}</span>
        <div style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); color: #27ae60;">
          <i class="fas fa-check"></i> Success!
        </div>
      `;

      // Success: show modal and reset form after delay
      setTimeout(() => {
        closeModal("registerModal");
        openVerifyModal(email);
        this.reset();
        hideButtonLoader(submitButton);
      }, 1500);

    } catch (error) {
      hideButtonLoader(submitButton);
      console.error("Registration error:", error);
      alert("Registration failed: Network error or server unavailable. Please try again later.");
    }
  }
});

//
document.getElementById("contactForm").addEventListener("submit", function (e) {
  e.preventDefault();

  const submitButton = this.querySelector('button[type="submit"]');
  
  // Show loading state
  showButtonLoader(submitButton, 'Sending...');

  setTimeout(() => {
    // Show success state
    submitButton.innerHTML = `
      <span style="opacity: 0;">${submitButton.dataset.originalText}</span>
      <div style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); color: #27ae60;">
        <i class="fas fa-check"></i> Sent!
      </div>
    `;
    
    setTimeout(() => {
      alert("Thank you for your message! We will get back to you soon.");
      this.reset();
      hideButtonLoader(submitButton);
    }, 1500);
  }, 2000);
});

// Verification Modal Functions
let countdownInterval;
let countdownTime = 240; // 4 minutes in seconds

function openVerifyModal(email) {
  document.getElementById("verifyEmail").value = email;
  document.getElementById("verifyCode").value = "";
  clearError("verifyCode");
  document.getElementById("resendBtn").disabled = true;
  openModal("verifyModal");
  startCountdown();
}

function startCountdown() {
  clearInterval(countdownInterval);
  countdownTime = 240;
  updateCountdown();
  countdownInterval = setInterval(() => {
    countdownTime--;
    updateCountdown();
    if (countdownTime <= 0) {
      clearInterval(countdownInterval);
      document.getElementById("countdown").textContent = "Code expired.";
      document.getElementById("resendBtn").disabled = false;
    }
  }, 1000);
}

function updateCountdown() {
  const min = Math.floor(countdownTime / 60);
  const sec = countdownTime % 60;
  document.getElementById("countdown").textContent =
    `Code expires in ${min}:${sec.toString().padStart(2, "0")}`;
  document.getElementById("resendBtn").disabled = countdownTime > 0;
}

// Resend code
document.getElementById("resendBtn").addEventListener("click", async function () {
  const email = document.getElementById("verifyEmail").value;
  
  // Show loading state
  showButtonLoader(this, 'Sending...');
  
  try {
    await fetch("https://localhost:7009/api/Account/resend-verification", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email })
    });
    
    // Show success state
    this.innerHTML = `
      <span style="opacity: 0;">${this.dataset.originalText}</span>
      <div style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); color: #27ae60;">
        <i class="fas fa-check"></i> Sent!
      </div>
    `;
    
    setTimeout(() => {
      countdownTime = 240;
      startCountdown();
      hideButtonLoader(this);
      alert("Verification code resent!");
    }, 1500);
    
  } catch {
    hideButtonLoader(this);
    alert("Failed to resend code. Please try again.");
  }
});

// Verify code
document.getElementById("verifyForm").addEventListener("submit", async function (e) {
  e.preventDefault();
  clearError("verifyCode");
  
  const email = document.getElementById("verifyEmail").value;
  const code = document.getElementById("verifyCode").value.trim();
  const submitButton = this.querySelector('button[type="submit"]');

  if (!code) {
    showError("verifyCode", "Please enter the verification code.");
    return;
  }

  // Show loading state
  showButtonLoader(submitButton, 'Verifying...');

  try {
    console.log('Sending verification request:', { email, code }); // Debug log
    
    const res = await fetch("https://localhost:7009/api/Account/verify-registration", {
      method: "POST",
      headers: { 
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
      body: JSON.stringify({ 
        email: email, 
        code: code 
      })
    });
    
    console.log('Verification response status:', res.status); // Debug log
    console.log('Verification response:', res); // Debug log
    
    // Check if response is OK
    if (!res.ok) {
      // Try to get error details from response
      let errorMessage = "Invalid or expired code.";
      try {
        const errorData = await res.json();
        console.log('Error data:', errorData); // Debug log
        errorMessage = errorData.message || errorMessage;
      } catch (e) {
        console.log('Could not parse error response'); // Debug log
      }
      
      hideButtonLoader(submitButton);
      showError("verifyCode", errorMessage);
      return;
    }
    
    // Try to parse the response
    let responseData;
    try {
      responseData = await res.json();
      console.log('Success response data:', responseData); // Debug log
    } catch (e) {
      console.log('Response might be empty or not JSON'); // Debug log
      responseData = { success: true };
    }
    
    // Show success state
    submitButton.innerHTML = `
      <span style="opacity: 0;">${submitButton.dataset.originalText}</span>
      <div style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); color: #27ae60;">
        <i class="fas fa-check"></i> Verified!
      </div>
    `;
    
    setTimeout(() => {
      clearInterval(countdownInterval);
      closeModal("verifyModal");
      openModal("successModal");
      hideButtonLoader(submitButton);
    }, 1500);
    
  } catch (error) {
    console.error('Network error during verification:', error); // Debug log
    hideButtonLoader(submitButton);
    showError("verifyCode", "Network error. Please check your connection and try again.");
  }
});

// Real-time validation for better UX
document.addEventListener("input", function (e) {
  const field = e.target;
  const fieldId = field.id;

  // Clear error when user starts typing
  if (field.tagName === "INPUT") {
    clearError(fieldId);
  }

  // Real-time validation for specific fields
  switch (fieldId) {
    case "username":
      if (field.value && !validateUsername(field.value)) {
        showError(
          fieldId,
          "Username must be 3-20 characters and contain only letters, numbers, and underscores"
        );
      }
      break;

    case "loginEmail":
    case "registerEmail":
      if (field.value && !validateEmail(field.value)) {
        showError(fieldId, "Please enter a valid email address");
      }
      break;

    case "registerPassword":
      if (field.value && !validatePassword(field.value)) {
        showError(fieldId, "Password must be at least 8 characters long");
      }
      break;

    case "confirmPassword":
      const password = document.getElementById("registerPassword").value;
      if (field.value && field.value !== password) {
        showError(fieldId, "Passwords do not match");
      }
      break;
  }
});

// Navbar scroll effect
window.addEventListener("scroll", function () {
  const header = document.querySelector(".header");
  if (window.scrollY > 100) {
    header.style.background = "rgba(255, 255, 255, 0.95)";
    header.style.backdropFilter = "blur(10px)";
  } else {
    header.style.background = "#fff";
    header.style.backdropFilter = "none";
  }
});

// Animate elements on scroll
const observerOptions = {
  threshold: 0.1,
  rootMargin: "0px 0px -50px 0px",
};

const observer = new IntersectionObserver(function (entries) {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.style.opacity = "1";
      entry.target.style.transform = "translateY(0)";
    }
  });
}, observerOptions);

// Observe elements for animation
document.addEventListener("DOMContentLoaded", function () {
  const animateElements = document.querySelectorAll(
    ".service-card, .stat, .contact-item"
  );

  animateElements.forEach((el) => {
    el.style.opacity = "0";
    el.style.transform = "translateY(20px)";
    el.style.transition = "opacity 0.6s ease, transform 0.6s ease";
    observer.observe(el);
  });
});

// Accessibility: Trap focus in modals
function trapFocus(modal) {
  const focusableElements = modal.querySelectorAll(
    'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
  );
  const firstElement = focusableElements[0];
  const lastElement = focusableElements[focusableElements.length - 1];

  modal.addEventListener("keydown", function (e) {
    if (e.key === "Tab") {
      if (e.shiftKey) {
        if (document.activeElement === firstElement) {
          lastElement.focus();
          e.preventDefault();
        }
      } else {
        if (document.activeElement === lastElement) {
          firstElement.focus();
          e.preventDefault();
        }
      }
    }
  });
}

// Apply focus trap to modals
document.querySelectorAll(".modal").forEach((modal) => {
  trapFocus(modal);
});

// Initialize tooltips and other interactive elements
document.addEventListener("DOMContentLoaded", function () {
  // Add loading states to buttons
  const buttons = document.querySelectorAll("button");
  buttons.forEach((button) => {
    button.addEventListener("click", function () {
      if (this.type === "submit") {
        this.style.position = "relative";
      }
    });
  });

  // Add smooth reveal animation to hero section
  const hero = document.querySelector(".hero");
  if (hero) {
    hero.style.opacity = "0";
    hero.style.transform = "translateY(20px)";

    setTimeout(() => {
      hero.style.transition = "opacity 1s ease, transform 1s ease";
      hero.style.opacity = "1";
      hero.style.transform = "translateY(0)";
    }, 1600); // Show after page loader
  }
});

// Authentication and Token Validation
async function checkTokenAndRedirect() {
  const token = localStorage.getItem('token');
  const userRole = localStorage.getItem('userRole');
  
  if (token) {
    try {
      // Validate token by making a test API call
      const response = await fetch('https://localhost:7009/api/Account/View-profile', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        // Token is valid, redirect to appropriate dashboard
        const roleInt = parseInt(userRole);
        
        switch (roleInt) {
          case 1: // Admin
            window.location.href = "../private-view/admin-view/admin-home/admin-home.html";
            return;
          case 2: // Doctor
            window.location.href = "../private-view/doctor-view/doctor-dashboard/doctor-dashboard.html";
            return;
          case 3: // Patient
            window.location.href = "../private-view/user-view/booking/appointment-booking.html";
            return;
          case 4: // Staff
            window.location.href = "../private-view/staff-view/staff-dashboard/staff-dashboard.html";
            return;
          case 5: // Manager
            window.location.href = "../private-view/manager-view/manager-home/manager-home.html";
            return;
          default:
            console.warn('Unknown role:', userRole);
            clearInvalidToken();
        }
      } else if (response.status === 401 || response.status === 403) {
        // Token is invalid or expired
        clearInvalidToken();
      }
    } catch (error) {
      console.error('Token validation failed:', error);
      // If there's a network error, we'll clear the token to be safe
      clearInvalidToken();
    }
  }
}

function clearInvalidToken() {
  localStorage.removeItem('token');
  localStorage.removeItem('userRole');
  localStorage.removeItem('accId');
  localStorage.removeItem('userId');
  localStorage.removeItem('fullName');
}

// Check for redirect messages and display them
function checkForRedirectMessage() {
  const urlParams = new URLSearchParams(window.location.search);
  const message = urlParams.get('message');
  const reason = urlParams.get('reason');
  
  if (message || reason) {
    // Create a subtle notification instead of alert
    showRedirectNotification(message || reason || 'Please log in to continue');
    
    // Clean the URL without the message parameters
    const url = new URL(window.location);
    url.searchParams.delete('message');
    url.searchParams.delete('reason');
    window.history.replaceState({}, document.title, url.toString());
  }
}

// Show subtle notification for redirect messages
function showRedirectNotification(message) {
  // Create notification element
  const notification = document.createElement('div');
  notification.className = 'redirect-notification';
  notification.innerHTML = `
    <div class="notification-content">
      <i class="fas fa-info-circle"></i>
      <span>${message}</span>
      <button onclick="this.parentElement.parentElement.remove()" class="notification-close">
        <i class="fas fa-times"></i>
      </button>
    </div>
  `;
  
  // Add styles
  notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 15px 20px;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.3);
    z-index: 10000;
    max-width: 400px;
    animation: slideInRight 0.5s ease-out;
  `;
  
  // Add animation styles to head if not exists
  if (!document.querySelector('#redirect-notification-styles')) {
    const style = document.createElement('style');
    style.id = 'redirect-notification-styles';
    style.textContent = `
      @keyframes slideInRight {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
      }
      .notification-content {
        display: flex;
        align-items: center;
        gap: 10px;
      }
      .notification-close {
        background: none;
        border: none;
        color: white;
        cursor: pointer;
        padding: 5px;
        margin-left: auto;
      }
      .notification-close:hover {
        opacity: 0.7;
      }
    `;
    document.head.appendChild(style);
  }
  
  // Add to page
  document.body.appendChild(notification);
  
  // Auto-remove after 5 seconds
  setTimeout(() => {
    if (notification.parentElement) {
      notification.remove();
    }
  }, 5000);
}

// Page Load Animation
document.addEventListener('DOMContentLoaded', async function() {
  // Check for redirect messages
  checkForRedirectMessage();
  
  // Check token validity first
  await checkTokenAndRedirect();
  
  // Show page loader
  const pageLoader = showPageLoader();
  
  // Load medical services
  renderMedicalServices();
  
  // Simulate loading time and hide loader
  setTimeout(() => {
    hidePageLoader();
  }, 1500);
  
  // Your existing DOMContentLoaded code here...
  const animateElements = document.querySelectorAll(
    ".service-card, .stat, .contact-item"
  );

  animateElements.forEach((el) => {
    el.style.opacity = "0";
    el.style.transform = "translateY(20px)";
    el.style.transition = "opacity 0.6s ease, transform 0.6s ease";
    observer.observe(el);
  });

  // Add loading states to buttons
  const buttons = document.querySelectorAll("button");
  buttons.forEach((button) => {
    button.addEventListener("click", function () {
      if (this.type === "submit") {
        this.style.position = "relative";
      }
    });
  });

  // Add smooth reveal animation to hero section
  const hero = document.querySelector(".hero");
  if (hero) {
    hero.style.opacity = "0";
    hero.style.transform = "translateY(20px)";

    setTimeout(() => {
      hero.style.transition = "opacity 1s ease, transform 1s ease";
      hero.style.opacity = "1";
      hero.style.transform = "translateY(0)";
    }, 1600); // Show after page loader
  }
});

// Add window focus event to check token validity when user returns to page
window.addEventListener('focus', async function() {
  await checkTokenAndRedirect();
});

// Add periodic token validation (every 5 minutes)
setInterval(async function() {
  await checkTokenAndRedirect();
}, 5 * 60 * 1000); // 5 minutes
