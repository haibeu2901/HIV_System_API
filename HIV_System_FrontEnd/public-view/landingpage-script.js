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
    // Simulate login process
    const submitButton = this.querySelector('button[type="submit"]');
    const originalText = submitButton.textContent;

    submitButton.textContent = "Logging in..."; 
    submitButton.disabled = true;

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
          throw new Error("Network response was not ok");
        }

        const data = await response.json();
        const fullName = data.account.fullName;
        const role = data.account.roles;
        localStorage.setItem("token", data.token);
        console.log("Login successful:", data);
        console.log("Role:", role);
        localStorage.setItem("accId", data.account.accId); // Store account ID in localStorage
        if (role == 3) {
          window.location.href =
            "/private-view/user-view/booking/appointment-booking.html"; // Redirect to the appointment booking page after successful login
        } else if (role == 2) {
          window.location.href = "/private-view/doctor-view/doctor-dashboard.html";
=======
          window.location.href = "/HIV_System_FrontEnd/private-view/doctor-view/doctor-dashboard/doctor-dashboard.html";
        } // Redirect to the doctor dashboard page after successful login
      } catch (error) {
        console.error("Error during login:", error);
        throw error;
      }
    };

    logginFunction(username, password)
      .then(() => {
        closeModal("loginModal");
        this.reset();
        submitButton.textContent = originalText;
        submitButton.disabled = false;
      })
      .catch((error) => {
        showError("loginUsername", "Invalid email or password");
        submitButton.textContent = originalText;
        submitButton.disabled = false;
      });
  }
});

// Register Form Validation and Submission
document
  .getElementById("registerForm")
  .addEventListener("submit", async function (e) {
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
      if (age < 13) {
        showError("dateOfBirth", "You must be at least 13 years old");
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
      const originalText = submitButton.textContent;

      submitButton.textContent = "Creating Account...";
      submitButton.disabled = true;

      // Prepare data for API
      const genderValue = gender.value === "male" ? true : false; // true for male, false for female/other
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
          const errorData = await response.json();
          throw new Error(errorData.message || "Registration failed");
        }

        // Success: show modal and reset form
       closeModal("registerModal");
        openVerifyModal(email);
        this.reset();
      } catch (error) {
        alert("Registration failed: " + error.message);
      } finally {
        submitButton.textContent = originalText;
        submitButton.disabled = false;
      }
    }
  });

// Contact Form Submission
document.getElementById("contactForm").addEventListener("submit", function (e) {
  e.preventDefault();

  const submitButton = this.querySelector('button[type="submit"]');
  const originalText = submitButton.textContent;

  submitButton.textContent = "Sending...";
  submitButton.disabled = true;

  setTimeout(() => {
    alert("Thank you for your message! We will get back to you soon.");
    this.reset();
    submitButton.textContent = originalText;
    submitButton.disabled = false;
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
  this.disabled = true;
  try {
    await fetch("https://localhost:7009/api/Account/resend-verification", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email })
    });
    countdownTime = 240;
    startCountdown();
    alert("Verification code resent!");
  } catch {
    alert("Failed to resend code. Please try again.");
    this.disabled = false;
  }
});

// Verify code
document.getElementById("verifyForm").addEventListener("submit", async function (e) {
  e.preventDefault();
  clearError("verifyCode");
  const email = document.getElementById("verifyEmail").value;
  const code = document.getElementById("verifyCode").value.trim();

  if (!code) {
    showError("verifyCode", "Please enter the verification code.");
    return;
  }

  try {
    const res = await fetch("https://localhost:7009/api/Account/verify-registration", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, code })
    });
    if (!res.ok) {
      showError("verifyCode", "Invalid or expired code.");
      return;
    }
    clearInterval(countdownInterval);
    closeModal("verifyModal");
    openModal("successModal");
  } catch {
    showError("verifyCode", "Verification failed. Try again.");
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
    }, 100);
  }
});
