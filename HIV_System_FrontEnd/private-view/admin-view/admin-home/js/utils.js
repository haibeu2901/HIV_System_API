// Modal Manager
class ModalManager {
    constructor() {
        this.openModals = new Set();
    }

    // Close modal function
    closeModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'none';
            this.openModals.delete(modalId);
            
            // Clear form data when closing modal
            const form = modal.querySelector('form');
            if (form) {
                form.reset();
            }
        }
    }

    // Show modal
    showModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'flex';
            this.openModals.add(modalId);
        }
    }

    // Initialize modal functionality
    init() {
        // Close modals when clicking outside
        window.addEventListener('click', (event) => {
            const modals = document.querySelectorAll('.modal');
            modals.forEach(modal => {
                if (event.target === modal) {
                    this.closeModal(modal.id);
                }
            });
        });
        
        // Close modals with Escape key
        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape') {
                const openModal = document.querySelector('.modal[style*="flex"]');
                if (openModal) {
                    this.closeModal(openModal.id);
                }
            }
        });
    }
}

// Utilities Manager
class UtilsManager {
    // Button loader functions
    showButtonLoader(button, text = 'Loading...') {
        button.disabled = true;
        button.classList.add('btn-loading');
        button.dataset.originalText = button.textContent;
        button.innerHTML = `
            <span style="opacity: 0;">${button.dataset.originalText}</span>
            <div class="small-loader" style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);"></div>
        `;
    }

    hideButtonLoader(button) {
        button.disabled = false;
        button.classList.remove('btn-loading');
        button.innerHTML = button.dataset.originalText || 'Submit';
    }

    // Format date helper
    formatDate(dateString) {
        if (!dateString) return 'N/A';
        
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            });
        } catch (error) {
            return dateString;
        }
    }

    // Show toast notification
    showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        
        document.body.appendChild(toast);
        
        setTimeout(() => {
            toast.classList.add('show');
        }, 100);
        
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => {
                document.body.removeChild(toast);
            }, 300);
        }, 3000);
    }

    // Debounce function
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Validate email
    validateEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // Validate phone number
    validatePhone(phone) {
        const phoneRegex = /^\+?[\d\s-()]{10,}$/;
        return phoneRegex.test(phone);
    }

    // Generate random ID
    generateId(length = 8) {
        const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        let result = '';
        for (let i = 0; i < length; i++) {
            result += chars.charAt(Math.floor(Math.random() * chars.length));
        }
        return result;
    }

    // Copy to clipboard
    async copyToClipboard(text) {
        try {
            await navigator.clipboard.writeText(text);
            this.showToast('Copied to clipboard!', 'success');
        } catch (err) {
            console.error('Failed to copy text: ', err);
            this.showToast('Failed to copy text', 'error');
        }
    }

    // Format currency
    formatCurrency(amount, currency = 'VND') {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: currency
        }).format(amount);
    }

    // Sanitize HTML
    sanitizeHtml(html) {
        const div = document.createElement('div');
        div.textContent = html;
        return div.innerHTML;
    }
}

// Export for use in other modules
window.ModalManager = ModalManager;
window.UtilsManager = UtilsManager;
