// Global variables
let currentStep = 1;
let userEmail = '';
let verificationCode = '';
let countdownInterval;
let timeLeft = 240; // 4 minutes in seconds

// DOM elements
const steps = document.querySelectorAll('.step');
const stepIndicators = document.querySelectorAll('.step-item');
const emailForm = document.getElementById('email-form');
const codeForm = document.getElementById('code-form');
const passwordForm = document.getElementById('password-form');
const timerElement = document.getElementById('timer');
const resendBtn = document.getElementById('resend-code-btn');

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    setupEventListeners();
    showStep(1);
});

// Setup event listeners
function setupEventListeners() {
    emailForm.addEventListener('submit', handleEmailSubmit);
    codeForm.addEventListener('submit', handleCodeSubmit);
    passwordForm.addEventListener('submit', handlePasswordSubmit);
    resendBtn.addEventListener('click', handleResendCode);
    
    // Real-time validation for code input
    document.getElementById('code').addEventListener('input', function(e) {
        e.target.value = e.target.value.replace(/[^0-9]/g, '');
    });
}

// Handle email submission (Step 1)
async function handleEmailSubmit(e) {
    e.preventDefault();
    
    const email = document.getElementById('email').value.trim();
    const submitBtn = document.getElementById('send-code-btn');
    
    if (!email) {
        showMessage('Please enter your email address.', 'error', 1);
        return;
    }
    
    // Validate email format
    if (!isValidEmail(email)) {
        showMessage('Please enter a valid email address.', 'error', 1);
        return;
    }
    
    try {
        // Show loading state
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Sending...';
        
        const response = await fetch('https://localhost:7009/api/Account/Forgot-Password', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                email: email
            })
        });
        
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `Server error: ${response.status}`);
        }
        
        // Success - move to step 2
        userEmail = email;
        showMessage('Verification code sent successfully! Check your email.', 'success', 1);
        
        setTimeout(() => {
            showStep(2);
            startCountdown();
        }, 1500);
        
    } catch (error) {
        console.error('Error sending reset code:', error);
        showMessage(error.message || 'Failed to send reset code. Please try again.', 'error', 1);
    } finally {
        // Reset button state
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="fas fa-paper-plane"></i> Send Reset Code';
    }
}

// Handle code verification (Step 2)
async function handleCodeSubmit(e) {
    e.preventDefault();
    
    const code = document.getElementById('code').value.trim();
    const submitBtn = document.getElementById('verify-code-btn');
    
    if (!code) {
        showMessage('Please enter the verification code.', 'error', 2);
        return;
    }
    
    if (code.length !== 6) {
        showMessage('Verification code must be 6 digits.', 'error', 2);
        return;
    }
    
    try {
        // Show loading state
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Verifying...';
        
        // Store the code for the password reset step
        verificationCode = code;
        
        // Test the code by attempting to reset with a dummy password
        // This is just to verify the code is correct
        const testResponse = await fetch(`https://localhost:7009/api/Account/Reset-Password?email=${encodeURIComponent(userEmail)}&code=${code}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                password: 'TestPassword123!',
                confirmPassword: 'TestPassword123!'
            })
        });
        
        // If we get here and the response is about password requirements or success, the code is valid
        if (testResponse.ok || testResponse.status === 400) {
            const responseData = await testResponse.json().catch(() => ({}));
            
            // Check if the error is about password requirements (code is valid)
            if (testResponse.status === 400 && responseData.message && 
                (responseData.message.includes('password') || responseData.message.includes('Password'))) {
                // Code is valid, move to step 3
                clearInterval(countdownInterval);
                showMessage('Code verified successfully!', 'success', 2);
                
                setTimeout(() => {
                    showStep(3);
                }, 1500);
            } else if (testResponse.ok) {
                // Shouldn't happen with test password, but just in case
                showStep(3);
            } else {
                throw new Error(responseData.message || 'Invalid verification code.');
            }
        } else {
            const errorData = await testResponse.json().catch(() => ({}));
            throw new Error(errorData.message || 'Invalid verification code.');
        }
        
    } catch (error) {
        console.error('Error verifying code:', error);
        showMessage(error.message || 'Invalid verification code. Please try again.', 'error', 2);
    } finally {
        // Reset button state
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="fas fa-check"></i> Verify Code';
    }
}

// Handle password reset (Step 3)
async function handlePasswordSubmit(e) {
    e.preventDefault();
    
    const password = document.getElementById('new-password').value;
    const confirmPassword = document.getElementById('confirm-password').value;
    const submitBtn = document.getElementById('reset-password-btn');
    
    // Client-side validation
    if (!password || !confirmPassword) {
        showMessage('Please fill in all fields.', 'error', 3);
        return;
    }
    
    if (password !== confirmPassword) {
        showMessage('Passwords do not match.', 'error', 3);
        return;
    }
    
    if (!isValidPassword(password)) {
        showMessage('Password does not meet requirements.', 'error', 3);
        return;
    }
    
    try {
        // Show loading state
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Resetting...';
        
        const response = await fetch(`https://localhost:7009/api/Account/Reset-Password?email=${encodeURIComponent(userEmail)}&code=${verificationCode}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                password: password,
                confirmPassword: confirmPassword
            })
        });
        
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `Server error: ${response.status}`);
        }
        
        // Success
        showMessage('Password reset successfully! You can now login with your new password.', 'success', 3);
        
        // Redirect to login page after 3 seconds
        setTimeout(() => {
            window.location.href = '../landingpage.html';
        }, 3000);
        
    } catch (error) {
        console.error('Error resetting password:', error);
        showMessage(error.message || 'Failed to reset password. Please try again.', 'error', 3);
    } finally {
        // Reset button state
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="fas fa-save"></i> Reset Password';
    }
}

// Handle resend code
async function handleResendCode() {
    const resendBtn = document.getElementById('resend-code-btn');
    
    try {
        // Show loading state
        resendBtn.disabled = true;
        resendBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Resending...';
        
        const response = await fetch('https://localhost:7009/api/Account/Forgot-Password', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                email: userEmail
            })
        });
        
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `Server error: ${response.status}`);
        }
        
        // Success - restart countdown
        showMessage('New verification code sent successfully!', 'success', 2);
        timeLeft = 240; // Reset to 4 minutes
        startCountdown();
        
    } catch (error) {
        console.error('Error resending code:', error);
        showMessage(error.message || 'Failed to resend code. Please try again.', 'error', 2);
    } finally {
        // Reset button state
        resendBtn.disabled = true;
        resendBtn.innerHTML = '<i class="fas fa-redo"></i> Resend Code';
    }
}

// Show specific step
function showStep(step) {
    // Hide all steps
    steps.forEach(s => s.classList.remove('active'));
    
    // Show current step
    document.getElementById(`step-${step}`).classList.add('active');
    
    // Update step indicators
    stepIndicators.forEach((indicator, index) => {
        indicator.classList.remove('active', 'completed');
        if (index + 1 < step) {
            indicator.classList.add('completed');
        } else if (index + 1 === step) {
            indicator.classList.add('active');
        }
    });
    
    currentStep = step;
    
    // Clear messages when switching steps
    clearMessages();
}

// Start countdown timer
function startCountdown() {
    resendBtn.disabled = true;
    
    countdownInterval = setInterval(() => {
        timeLeft--;
        
        const minutes = Math.floor(timeLeft / 60);
        const seconds = timeLeft % 60;
        
        timerElement.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        
        if (timeLeft <= 0) {
            clearInterval(countdownInterval);
            timerElement.textContent = 'Code expired';
            timerElement.style.color = '#e74c3c';
            resendBtn.disabled = false;
            showMessage('Verification code has expired. Please request a new one.', 'error', 2);
        }
    }, 1000);
}

// Show message in specified step
function showMessage(message, type, step) {
    const container = document.getElementById(`message-container-${step}`);
    container.innerHTML = `
        <div class="${type}-message">
            <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-triangle' : 'info-circle'}"></i>
            ${message}
        </div>
    `;
    
    // Auto-hide success messages
    if (type === 'success') {
        setTimeout(() => {
            container.innerHTML = '';
        }, 5000);
    }
}

// Clear all messages
function clearMessages() {
    for (let i = 1; i <= 3; i++) {
        const container = document.getElementById(`message-container-${i}`);
        if (container) {
            container.innerHTML = '';
        }
    }
}

// Validate email format
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Validate password strength
function isValidPassword(password) {
    const minLength = 8;
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumbers = /\d/.test(password);
    const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(password);
    
    return password.length >= minLength && hasUpperCase && hasLowerCase && hasNumbers && hasSpecial;
}

// Cleanup on page unload
window.addEventListener('beforeunload', function() {
    if (countdownInterval) {
        clearInterval(countdownInterval);
    }
});
