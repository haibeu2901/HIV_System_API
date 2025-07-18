// Get token from localStorage
const token = localStorage.getItem('token');

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
                        <td><button class="view-payments-btn" data-pmr-id="${p.pmrId}" data-patient-name="${p.account.fullname}">Xem thanh toán</button></td>
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
    
    // Add event listeners for view payments buttons
    document.querySelectorAll('.view-payments-btn').forEach(btn => {
        btn.onclick = async function() {
            const pmrId = this.getAttribute('data-pmr-id');
            const patientName = this.getAttribute('data-patient-name');
            await showPaymentModal(pmrId, patientName);
        };
    });
    
    // Modal close logic
    const closeModalBtn = document.getElementById('closePatientModal');
    if (closeModalBtn) {
        closeModalBtn.onclick = function() {
            document.getElementById('patientDetailModal').style.display = 'none';
        };
    }
    
    // Payment modal close logic
    const closePaymentModalBtn = document.getElementById('closePaymentModal');
    if (closePaymentModalBtn) {
        closePaymentModalBtn.onclick = function() {
            document.getElementById('paymentModal').style.display = 'none';
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
                    ${payments.map(payment => `
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
                                    <p><strong>Phương thức:</strong> ${payment.paymentMethod}</p>
                                    <p><strong>Mô tả:</strong> ${payment.description}</p>
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
                    `).join('')}
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

// Payment status mapping
const paymentStatusMap = {
    1: 'Chờ thanh toán',
    2: 'Đã thanh toán',
    3: 'Đã hủy',
    4: 'Đã hoàn tiền'
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
