// Get token from localStorage
const token = localStorage.getItem('token');

// Function to format payment method display text
function formatPaymentMethod(method) {
    if (!method) return '-';
    
    // Clean the method string first - more aggressive cleaning
    let cleanMethod = method.toLowerCase().trim();
    
    // Replace common encoding issues and special characters more aggressively
    cleanMethod = cleanMethod
        .replace(/\?/g, '') // Remove question marks
        .replace(/[\u00BF\u003F\uFFFD]/g, '') // Remove various question mark variants and replacement chars
        .replace(/[^\w\s\u00C0-\u024F\u1E00-\u1EFF]/g, '') // Keep only word chars, spaces, and Vietnamese chars
        .replace(/\s+/g, ' ') // Normalize spaces
        .trim();
    
    // Direct English method mapping (from API)
    const englishMethodMap = {
        'cash': 'Tiền mặt',
        'transfer': 'Chuyển khoản',
        'card': 'Thẻ tín dụng',
        'stripe': 'Thanh toán trực tuyến (Stripe)',
        'paypal': 'PayPal',
        'momo': 'MoMo',
        'zalopay': 'ZaloPay',
        'vnpay': 'VNPay'
    };
    
    // Check for direct English mapping first
    if (englishMethodMap[cleanMethod]) {
        return englishMethodMap[cleanMethod];
    }
    
    // Handle Vietnamese variations (for legacy data or manual input)
    // Handle "tiền mặt" variations
    if (cleanMethod.match(/ti[e\u00EA\u1EBF]n\s*m[a\u0103\u00E2\u1EAD\u1EAF\u1EB1\u1EB3\u1EB5]t/i) || 
        cleanMethod.includes('tien') && cleanMethod.includes('mat') ||
        cleanMethod.includes('tien') && cleanMethod.includes('m')) {
        return 'Tiền mặt';
    }
    
    // Handle "chuyển khoản" variations  
    if (cleanMethod.match(/chuy[e\u00EA\u1EBF]n\s*kho[a\u0103\u00E2\u1EA3\u1EA5]n/i) ||
        cleanMethod.includes('chuyen') && cleanMethod.includes('khoan')) {
        return 'Chuyển khoản';
    }
    
    // Handle "thẻ tín dụng" variations
    if (cleanMethod.match(/th[e\u00EA\u1EBF]\s*t[i\u00ED]n\s*d[u\u00F9\u00FA\u0169\u1EE5]ng/i) ||
        cleanMethod.includes('the') && cleanMethod.includes('tin') ||
        cleanMethod.includes('credit')) {
        return 'Thẻ tín dụng';
    }
    
    if (cleanMethod === 'string') {
        return 'Tiền mặt'; // Assuming 'string' means cash
    }
    
    // Enhanced mapping as fallback with more variations
    const paymentMethodMap = {
        // Cash variations - including encoding issues
        'tiền mặt': 'Tiền mặt',
        'tien mat': 'Tiền mặt',
        'tien mt': 'Tiền mặt',
        'tien m': 'Tiền mặt',
        'tienmat': 'Tiền mặt',
        'tien': 'Tiền mặt',
        'cash': 'Tiền mặt',
        
        // Bank transfer variations - including encoding issues  
        'chuyển khoản': 'Chuyển khoản',
        'chuyen khoan': 'Chuyển khoản',
        'chuyen kho': 'Chuyển khoản',
        'chuyen khn': 'Chuyển khoản',
        'chuyenkhoan': 'Chuyển khoản',
        'transfer': 'Chuyển khoản',
        'bank transfer': 'Chuyển khoản',
        
        // Credit card variations - including encoding issues
        'thẻ tín dụng': 'Thẻ tín dụng',
        'the tin dung': 'Thẻ tín dụng',
        'the tin d': 'Thẻ tín dụng',
        'the tin dng': 'Thẻ tín dụng',
        'thetindung': 'Thẻ tín dụng',
        'credit card': 'Thẻ tín dụng',
        'coin card': 'Thẻ tín dụng',
        'card': 'Thẻ tín dụng',
        
        // Online payment variations
        'stripe': 'Thanh toán trực tuyến (Stripe)',
        'paypal': 'PayPal',
        'momo': 'MoMo',
        'zalopay': 'ZaloPay',
        'vnpay': 'VNPay',
        
        // Other variations
        'string': 'Tiền mặt',
        'khác': 'Khác',
        'other': 'Khác'
    };
    
    // Check if we have a mapping for this method
    if (paymentMethodMap[cleanMethod]) {
        return paymentMethodMap[cleanMethod];
    }
    
    // Additional fallback for special characters
    for (const [key, value] of Object.entries(paymentMethodMap)) {
        if (cleanMethod.includes(key) || key.includes(cleanMethod)) {
            return value;
        }
    }
    
    // If no mapping found, return the original method with proper capitalization
    const result = method.charAt(0).toUpperCase() + method.slice(1);
    return result;
}

async function fetchPatients() {
    try {
        const response = await fetch('https://localhost:7009/api/Patient/GetAllPatients', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch patients');
        return await response.json();
    } catch (error) {
        console.error('Error fetching patients:', error);
        return [];
    }
}

async function fetchPatientPayments(pmrId) {
    try {
        const response = await fetch(`https://localhost:7009/api/Payment/GetPaymentsByPmrId/${pmrId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch patient payments');
        return await response.json();
    } catch (error) {
        console.error('Error fetching patient payments:', error);
        return [];
    }
}

let allPatients = [];

function renderPatients(patients) {
    const section = document.getElementById('patientMedicalRecordSection');
    let html = `
        <table class="patient-table">
            <thead>
                <tr>
                    <th>STT</th>
                    <th>Họ tên</th>
                    <th>Email</th>
                    <th>Giới tính</th>
                    <th>Ngày sinh</th>
                    <th>Chi tiết</th>
                    <th>Thanh toán</th>
                </tr>
            </thead>
            <tbody>
                ${patients.map((p, idx) => `
                    <tr>
                        <td>${idx + 1}</td>
                        <td>${p.account.fullname}</td>
                        <td>${p.account.email}</td>
                        <td>${p.account.gender ? 'Nam' : 'Nữ'}</td>
                        <td>${p.account.dob}</td>
                        <td><button class="view-details-btn" data-patient-id="${p.patientId}">Xem</button></td>
                    </tr>
                `).join('')}
            </tbody>
        </table>
    `;
    section.innerHTML = html;
    
    // Add event listeners for view details buttons
    document.querySelectorAll('.view-details-btn').forEach(btn => {
        btn.onclick = function() {
            const patientId = this.getAttribute('data-patient-id');
            // Redirect to patient medical record page with patient ID
            window.location.href = `../patient-medical-record/patient-medical-record.html?patientId=${patientId}`;
        };
    });

    // Modal close logic
    const closeModalBtn = document.getElementById('closePatientModal');
    if (closeModalBtn) {
        closeModalBtn.onclick = function() {
            document.getElementById('patientDetailModal').style.display = 'none';
        };
    }
        
    window.onclick = function(event) {
        const modal = document.getElementById('patientDetailModal');
        const paymentModal = document.getElementById('paymentModal');
        if (event.target === modal) {
            modal.style.display = 'none';
        }
        if (event.target === paymentModal) {
            paymentModal.style.display = 'none';
        }
    };
}

// Show payment modal with patient's payment history
async function showPaymentModal(pmrId, patientName) {
    window._lastPmrId = pmrId;
    window._lastPatientName = patientName;
    try {
        // Show loading state
        createPaymentModal();
        const modal = document.getElementById('paymentModal');
        const content = document.getElementById('paymentModalContent');
        
        content.innerHTML = `
            <div class="loading">
                <i class="fas fa-spinner fa-spin"></i>
                <p>Đang tải thông tin thanh toán...</p>
            </div>
        `;
        modal.style.display = 'block';
        
        // Fetch payment data
        const payments = await fetchPatientPayments(pmrId);
        
        // Render payments
        content.innerHTML = `
            <h3>Lịch sử thanh toán - ${patientName}</h3>
            ${payments.length === 0 ? 
                '<p class="no-payments">Bệnh nhân chưa có giao dịch thanh toán nào.</p>' :
                `<div class="payments-container">
                    ${payments.map(payment => {
                        const isCash = formatPaymentMethod(payment.paymentMethod) === 'Tiền mặt';
                        const canComplete = (window.isDoctor || window.isStaff) && payment.paymentStatus === 1 && isCash;
                        return `
                        <div class="payment-card">
                            <div class="payment-header">
                                <div class="payment-id">
                                    <strong>Mã thanh toán:</strong> ${payment.payId}
                                    <span class="payment-status status-${payment.paymentStatus}">
                                        ${paymentStatusMap[payment.paymentStatus] || 'Không xác định'}
                                    </span>
                                </div>
                                <div class="payment-amount">
                                    ${formatCurrency(payment.amount)} ${payment.currency}
                                </div>
                            </div>
                            <div class="payment-details">
                                <div class="payment-info">
                                    <p><strong>Ngày thanh toán:</strong> ${formatDate(payment.paymentDate)}</p>
                                    <p><strong>Phương thức:</strong> ${formatPaymentMethod(payment.paymentMethod)}</p>
                                    <p style="display: flex; align-items: center;"><strong>Mô tả:</strong> <span style="margin-left: 4px;">${payment.description}</span>
                                        ${canComplete && payment.paymentIntentId ? `<button class="btn-complete-cash-payment" onclick="completeCashPayment('${payment.paymentIntentId}', this)" style="background: #28a745; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; margin-left: 12px;"><i class='fas fa-check'></i> Hoàn thành thanh toán tiền mặt</button>` : ''}
                                    </p>
                                    ${payment.serviceName ? `<p><strong>Dịch vụ:</strong> ${payment.serviceName}</p>` : ''}
                                    ${payment.servicePrice ? `<p><strong>Giá dịch vụ:</strong> ${formatCurrency(payment.servicePrice)} VND</p>` : ''}
                                </div>
                                <div class="payment-metadata">
                                    <small><strong>Tạo lúc:</strong> ${formatDate(payment.createdAt)}</small>
                                    <small><strong>Cập nhật:</strong> ${formatDate(payment.updatedAt)}</small>
                                    ${payment.paymentIntentId ? `<small><strong>Intent ID:</strong> ${payment.paymentIntentId}</small>` : ''}
                                </div>
                            </div>
                        </div>
                        `;
                    }).join('')}
                </div>`
            }
        `;
        
    } catch (error) {
        console.error('Error showing payment modal:', error);
        const content = document.getElementById('paymentModalContent');
        content.innerHTML = `
            <div class="error">
                <i class="fas fa-exclamation-triangle"></i>
                <p>Có lỗi khi tải thông tin thanh toán. Vui lòng thử lại.</p>
                <button onclick="showPaymentModal('${pmrId}', '${patientName}')" class="retry-btn">Thử lại</button>
            </div>
        `;
    }
}

// Create payment modal if it doesn't exist
function createPaymentModal() {
    if (document.getElementById('paymentModal')) return;
    
    const modalHTML = `
        <div id="paymentModal" class="modal" style="display: none;">
            <div class="modal-content payment-modal-content">
                <span id="closePaymentModal" class="close">&times;</span>
                <div id="paymentModalContent">
                    <!-- Payment content will be loaded here -->
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', modalHTML);
}

// Function to complete cash payment (for doctor/staff)
async function completeCashPayment(paymentIntentId, button) {
    if (!paymentIntentId) {
        alert('Không tìm thấy Payment Intent ID');
        return;
    }
    if (!confirm('Bạn có chắc chắn muốn đánh dấu thanh toán tiền mặt này là đã hoàn thành?')) {
        return;
    }
    try {
        const originalText = button.innerHTML;
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
        button.disabled = true;
        const response = await fetch(`https://localhost:7009/api/Payment/MarkCashPaymentSuccess/${paymentIntentId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });
        if (response.ok) {
            alert('Thanh toán tiền mặt đã được đánh dấu hoàn thành!');
            // Reload payment modal
            if (window._lastPmrId && window._lastPatientName) {
                await showPaymentModal(window._lastPmrId, window._lastPatientName);
            } else {
                window.location.reload();
            }
        } else {
            const errorResult = await response.json();
            alert(`Lỗi khi hoàn thành thanh toán: ${errorResult.message || 'Không xác định'}`);
            button.innerHTML = originalText;
            button.disabled = false;
        }
    } catch (error) {
        alert('Có lỗi xảy ra khi hoàn thành thanh toán. Vui lòng thử lại.');
        button.innerHTML = '<i class="fas fa-check"></i> Hoàn thành thanh toán tiền mặt';
        button.disabled = false;
    }
}


// Utility functions for formatting
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount);
}

function formatDate(dateString) {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Helper to normalize text: remove accents, trim, collapse spaces, lowercase
function normalizeText(str) {
    return str
        .normalize('NFD')
        .replace(/\p{Diacritic}/gu, '')
        .replace(/\s+/g, ' ')
        .trim()
        .toLowerCase();
}

// Appointment status mapping
const appointmentStatusMap = {
    1: 'Chờ xác nhận',
    2: 'Đã xác nhận',
    3: 'Đã xác nhận lại',
    4: 'Đã hủy',
    5: 'Đã hoàn thành'
};

window.addEventListener('DOMContentLoaded', async () => {
    allPatients = await fetchPatients();
    renderPatients(allPatients);
    const searchInput = document.getElementById('patientSearchInput');
    if (searchInput) {
        searchInput.oninput = function() {
            const filter = normalizeText(this.value);
            const filtered = allPatients.filter(p => {
                const name = normalizeText(p.account.fullname);
                const email = normalizeText(p.account.email);
                return name.includes(filter) || email.includes(filter);
            });
            renderPatients(filtered);
        };
    }
});
