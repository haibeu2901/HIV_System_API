// Payments Management Module
class PaymentsManager {
    constructor() {
        this.payments = [];
        this.filteredPayments = [];
        this.currentPage = 1;
        this.itemsPerPage = 20;
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        // Status filter
        const statusFilter = document.getElementById('payment-status-filter');
        if (statusFilter) {
            statusFilter.addEventListener('change', () => this.filterPayments());
        }

        // Method filter
        const methodFilter = document.getElementById('payment-method-filter');
        if (methodFilter) {
            methodFilter.addEventListener('change', () => this.filterPayments());
        }

        // Search
        const searchInput = document.getElementById('payment-search');
        if (searchInput) {
            searchInput.addEventListener('input', () => this.filterPayments());
        }
    }

    async fetchAllPayments() {
        try {
            const token = localStorage.getItem('token');
            if (!token) {
                throw new Error('No authentication token found');
            }

            const response = await fetch('https://localhost:7009/api/Payment/GetAllPayments', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            this.payments = data || [];
            this.filteredPayments = [...this.payments];
            
            this.updateStats();
            this.renderPayments();
            
            return this.payments;
        } catch (error) {
            console.error('Error fetching payments:', error);
            this.showError('Failed to load payments. Please try again.');
            return [];
        }
    }

    updateStats() {
        const totalCount = this.payments.length;
        const successfulCount = this.payments.filter(p => p.paymentStatus === 2).length;
        const pendingCount = this.payments.filter(p => p.paymentStatus === 1).length;
        const totalRevenue = this.payments
            .filter(p => p.paymentStatus === 2)
            .reduce((sum, p) => sum + (p.amount || 0), 0);

        // Update stat cards
        this.updateElementText('total-payments-count', totalCount);
        this.updateElementText('successful-payments', successfulCount);
        this.updateElementText('pending-payments', pendingCount);
        this.updateElementText('total-revenue-amount', this.formatCurrency(totalRevenue));
    }

    updateElementText(id, text) {
        const element = document.getElementById(id);
        if (element) {
            element.textContent = text;
        }
    }

    formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND',
            minimumFractionDigits: 0
        }).format(amount);
    }

    filterPayments() {
        const statusFilter = document.getElementById('payment-status-filter')?.value || 'all';
        const methodFilter = document.getElementById('payment-method-filter')?.value || 'all';
        const searchTerm = document.getElementById('payment-search')?.value.toLowerCase() || '';

        this.filteredPayments = this.payments.filter(payment => {
            // Status filter
            if (statusFilter !== 'all' && payment.paymentStatus !== parseInt(statusFilter)) {
                return false;
            }

            // Method filter
            if (methodFilter !== 'all') {
                const paymentMethod = (payment.paymentMethod || '').toLowerCase();
                if (methodFilter === 'card' && !paymentMethod.includes('card') && !paymentMethod.includes('stripe')) {
                    return false;
                }
                if (methodFilter === 'cash' && !paymentMethod.includes('cash') && !paymentMethod.includes('tiền mặt')) {
                    return false;
                }
            }

            // Search filter
            if (searchTerm) {
                const searchFields = [
                    payment.patientName || '',
                    payment.payId?.toString() || '',
                    payment.description || '',
                    payment.paymentIntentId || ''
                ].join(' ').toLowerCase();

                if (!searchFields.includes(searchTerm)) {
                    return false;
                }
            }

            return true;
        });

        this.renderPayments();
    }

    renderPayments() {
        const container = document.getElementById('payments-list');
        if (!container) return;

        if (this.filteredPayments.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-credit-card"></i>
                    <h3>No payments found</h3>
                    <p>No payments match your current filters.</p>
                </div>
            `;
            return;
        }

        const paymentsHtml = this.filteredPayments.map(payment => this.renderPaymentCard(payment)).join('');
        
        container.innerHTML = `
            <div class="payments-grid">
                ${paymentsHtml}
            </div>
        `;
    }

    renderPaymentCard(payment) {
        const statusBadge = this.getStatusBadge(payment.paymentStatus);
        const methodBadge = this.getMethodBadge(payment.paymentMethod);
        const formattedAmount = this.formatCurrency(payment.amount || 0);
        const formattedDate = this.formatDate(payment.paymentDate);

        return `
            <div class="payment-card">
                <div class="payment-header">
                    <div class="payment-id">#${payment.payId}</div>
                    <div class="payment-status">${statusBadge}</div>
                </div>
                
                <div class="payment-body">
                    <div class="payment-amount">${formattedAmount}</div>
                    <div class="payment-patient">
                        <i class="fas fa-user"></i>
                        <span>${payment.patientName || 'N/A'}</span>
                    </div>
                    <div class="payment-method">${methodBadge}</div>
                    <div class="payment-date">
                        <i class="fas fa-calendar"></i>
                        <span>${formattedDate}</span>
                    </div>
                    ${payment.description ? `
                        <div class="payment-description">
                            <i class="fas fa-file-text"></i>
                            <span>${payment.description}</span>
                        </div>
                    ` : ''}
                </div>
                
                <div class="payment-footer">
                    <button class="btn-view-details" onclick="paymentsManager.viewPaymentDetails(${payment.payId})">
                        <i class="fas fa-eye"></i> View Details
                    </button>
                    ${payment.paymentStatus === 1 && payment.paymentIntentId ? `
                        <button class="btn-process-payment" onclick="paymentsManager.processPayment('${payment.paymentIntentId}')">
                            <i class="fas fa-credit-card"></i> Process
                        </button>
                    ` : ''}
                </div>
            </div>
        `;
    }

    getStatusBadge(status) {
        const statusMap = {
            1: { text: 'Pending', class: 'status-pending' },
            2: { text: 'Paid', class: 'status-paid' },
            3: { text: 'Failed', class: 'status-failed' }
        };

        const statusInfo = statusMap[status] || { text: 'Unknown', class: 'status-unknown' };
        return `<span class="status-badge ${statusInfo.class}">${statusInfo.text}</span>`;
    }

    getMethodBadge(method) {
        if (!method) return '<span class="method-badge">N/A</span>';
        
        const methodLower = method.toLowerCase();
        let methodClass = 'method-other';
        let methodText = method;

        if (methodLower.includes('card') || methodLower.includes('stripe')) {
            methodClass = 'method-card';
            methodText = 'Card';
        } else if (methodLower.includes('cash') || methodLower.includes('tiền mặt')) {
            methodClass = 'method-cash';
            methodText = 'Cash';
        }

        return `<span class="method-badge ${methodClass}">${methodText}</span>`;
    }

    formatDate(dateString) {
        if (!dateString) return 'N/A';
        
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('vi-VN', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch (error) {
            return 'Invalid Date';
        }
    }

    viewPaymentDetails(paymentId) {
        const payment = this.payments.find(p => p.payId === paymentId);
        if (!payment) return;

        // Create and show modal with payment details
        this.showPaymentDetailsModal(payment);
    }

    showPaymentDetailsModal(payment) {
        const modalHtml = `
            <div class="modal active" id="paymentDetailsModal">
                <div class="modal-content">
                    <div class="modal-header">
                        <h3>Payment Details - #${payment.payId}</h3>
                        <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                    </div>
                    <div class="modal-body">
                        <div class="payment-details-grid">
                            <div class="detail-item">
                                <label>Payment ID:</label>
                                <span>${payment.payId}</span>
                            </div>
                            <div class="detail-item">
                                <label>Amount:</label>
                                <span>${this.formatCurrency(payment.amount || 0)}</span>
                            </div>
                            <div class="detail-item">
                                <label>Status:</label>
                                <span>${this.getStatusBadge(payment.paymentStatus)}</span>
                            </div>
                            <div class="detail-item">
                                <label>Method:</label>
                                <span>${this.getMethodBadge(payment.paymentMethod)}</span>
                            </div>
                            <div class="detail-item">
                                <label>Patient:</label>
                                <span>${payment.patientName || 'N/A'}</span>
                            </div>
                            <div class="detail-item">
                                <label>Date:</label>
                                <span>${this.formatDate(payment.paymentDate)}</span>
                            </div>
                            ${payment.paymentIntentId ? `
                                <div class="detail-item">
                                    <label>Payment Intent ID:</label>
                                    <span>${payment.paymentIntentId}</span>
                                </div>
                            ` : ''}
                            ${payment.description ? `
                                <div class="detail-item full-width">
                                    <label>Description:</label>
                                    <span>${payment.description}</span>
                                </div>
                            ` : ''}
                        </div>
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', modalHtml);
    }

    processPayment(paymentIntentId) {
        // This would typically redirect to a payment processing page
        // or open a payment modal
        console.log('Processing payment with intent ID:', paymentIntentId);
        alert(`Processing payment with intent ID: ${paymentIntentId}`);
    }

    showError(message) {
        const container = document.getElementById('payments-list');
        if (container) {
            container.innerHTML = `
                <div class="error-state">
                    <i class="fas fa-exclamation-triangle"></i>
                    <h3>Error</h3>
                    <p>${message}</p>
                    <button onclick="paymentsManager.fetchAllPayments()" class="btn-retry">
                        <i class="fas fa-retry"></i> Retry
                    </button>
                </div>
            `;
        }
    }

    // Initialize when payments section becomes active
    async initialize() {
        console.log('Initializing Payments Manager...');
        await this.fetchAllPayments();
    }
}

// Global instance
let paymentsManager;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    paymentsManager = new PaymentsManager();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = PaymentsManager;
}
