// Get token from localStorage
const token = localStorage.getItem('token');

// Appointment status mapping
const appointmentStatusMap = {
    1: 'Chờ xác nhận',
    2: 'Đã xác nhận',
    3: 'Đã xác nhận lại',
    4: 'Đã hủy',
    5: 'Đã hoàn thành'
};

// ARV Regimen level and status mapping
const regimenLevelMap = {
    1: "Level 1",
    2: "Level 2",
    3: "Level 3",
    4: "Special Case"
};
const regimenStatusMap = {
    1: "Planned",
    2: "Active",
    3: "Paused",
    4: "Failed",
    5: "Completed"
};

// Payment status mapping
const paymentStatusMap = {
    1: 'Đang chờ',
    2: 'Đã thanh toán',
    3: 'Thất bại'
};

// Fetch patient medical record data
async function fetchPatientMedicalData() {
    try {
        const response = await fetch('https://localhost:7009/api/PatientMedicalRecord/GetPersonalMedicalRecord', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            if (response.status === 404) {
                return null; // No medical records found
            }
            throw new Error('Failed to fetch medical data');
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching medical data:', error);
        return null;
    }
}

// Fetch ARV regimens for the current patient
async function fetchPatientArvRegimens() {
    try {
        const response = await fetch('https://localhost:7009/api/PatientArvRegimen/GetPersonalArvRegimens', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            if (response.status === 404) {
                return []; // No ARV regimens found
            }
            throw new Error('Failed to fetch ARV regimens');
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching ARV regimens:', error);
        return [];
    }
}

// Fetch patient payment history
async function fetchPatientPayments() {
    try {
        const response = await fetch('https://localhost:7009/api/Payment/GetAllPersonalPayments', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            if (response.status === 404) {
                return []; // No payments found
            }
            throw new Error('Failed to fetch payment history');
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching payment history:', error);
        return [];
    }
}

// Render appointments
function renderAppointments(appointments) {
    const section = document.getElementById('appointmentsContent');
    
    if (!appointments || appointments.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-calendar-times"></i>
                <p>No appointments found.</p>
                <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                    <i class="fas fa-calendar-plus"></i> Book an Appointment
                </button>
            </div>
        `;
        return;
    }

    let html = `
        <div class="appointments-stats">
            <div class="stat-card">
                <div class="stat-number">${appointments.length}</div>
                <div class="stat-label">Total Appointments</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${appointments.filter(a => a.apmStatus === 5).length}</div>
                <div class="stat-label">Completed</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${appointments.filter(a => a.apmStatus === 2).length}</div>
                <div class="stat-label">Confirmed</div>
            </div>
        </div>
        <table class="appointments-table">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Time</th>
                    <th>Doctor</th>
                    <th>Status</th>
                    <th>Notes</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
    `;

    appointments.forEach(appt => {
        const statusLabel = appointmentStatusMap[appt.apmStatus] || 'Unknown';
        const statusClass = `status-${appt.apmStatus}`;
        
        html += `
            <tr>
                <td>${appt.apmtDate}</td>
                <td>${appt.apmTime ? appt.apmTime.slice(0, 5) : '-'}</td>
                <td>${appt.doctorName || '-'}</td>
                <td><span class="appointment-status ${statusClass}">${statusLabel}</span></td>
                <td>${appt.notes || '-'}</td>
                <td>
                    <button class="action-btn" onclick="window.location.href='../appointment-view/view-appointment.html?id=${appt.appointmentId}'">
                        <i class="fas fa-eye"></i> View
                    </button>
                </td>
            </tr>
        `;
    });

    html += `
            </tbody>
        </table>
    `;
    
    section.innerHTML = html;
}

// Render test results
function renderTestResults(testResults) {
    const section = document.getElementById('testResultsContent');
    
    if (!testResults || testResults.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-flask"></i>
                <p>No test results found.</p>
                <p class="empty-state-subtitle">Test results will appear here after your laboratory tests.</p>
            </div>
        `;
        return;
    }

    let html = `
        <div class="test-results-stats">
            <div class="stat-card">
                <div class="stat-number">${testResults.length}</div>
                <div class="stat-label">Total Tests</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${testResults.filter(t => t.result === true).length}</div>
                <div class="stat-label">Positive</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${testResults.filter(t => t.result === false).length}</div>
                <div class="stat-label">Negative</div>
            </div>
        </div>
        <div class="test-results-grid">
    `;
    
    testResults.forEach(testResult => {
        const resultClass = testResult.result ? 'test-result-positive' : 'test-result-negative';
        const resultText = testResult.result ? 'Positive' : 'Negative';

        html += `
            <div class="test-result-card">
                <div class="test-result-header">
                    <span class="test-result-date">Test Date: ${testResult.testDate}</span>
                    <span class="test-result-overall ${resultClass}">${resultText}</span>
                </div>
        `;

        if (testResult.notes) {
            html += `
                <div class="test-result-notes">
                    <strong>Notes:</strong> ${testResult.notes}
                </div>
            `;
        }

        if (testResult.componentTestResults && testResult.componentTestResults.length > 0) {
            html += `
                <div class="component-test-results">
                    <h4>Component Tests</h4>
                    <table class="component-tests-table">
                        <thead>
                            <tr>
                                <th>Component</th>
                                <th>Result Value</th>
                                <th>Notes</th>
                            </tr>
                        </thead>
                        <tbody>
            `;
            testResult.componentTestResults.forEach(component => {
                html += `
                    <tr>
                        <td>${component.componentTestResultName}</td>
                        <td>${component.resultValue}</td>
                        <td>${component.notes || '-'}</td>
                    </tr>
                `;
            });
            html += `
                        </tbody>
                    </table>
                </div>
            `;
        }

        html += `</div>`;
    });
    
    html += `</div>`;
    section.innerHTML = html;
}

// Render payment history
function renderPayments(payments) {
    const section = document.getElementById('paymentsContent');
    
    if (!payments || payments.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-credit-card"></i>
                <p>No payment history found.</p>
                <p class="empty-state-subtitle">Payment records will appear here after transactions are completed.</p>
            </div>
        `;
        return;
    }

    let html = `
        <div class="payments-stats">
            <div class="stat-card">
                <div class="stat-number">${payments.length}</div>
                <div class="stat-label">Total Payments</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${payments.filter(p => p.paymentStatus === 2).length}</div>
                <div class="stat-label">Completed</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${payments.filter(p => p.paymentStatus === 1).length}</div>
                <div class="stat-label">Pending</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${payments.filter(p => p.paymentStatus === 3).length}</div>
                <div class="stat-label">Failed</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${payments.reduce((total, p) => p.paymentStatus === 2 ? total + p.amount : total, 0).toLocaleString()}</div>
                <div class="stat-label">Total Paid (VND)</div>
            </div>
        </div>
        <div class="payments-grid">
    `;

    payments.forEach(payment => {
        const statusClass = `payment-status-${payment.paymentStatus}`;
        const statusText = paymentStatusMap[payment.paymentStatus] || 'Không xác định';
        
        html += `
            <div class="payment-card">
                <div class="payment-card-header">
                    <div class="payment-main-info">
                        <div class="payment-id">
                            <span class="payment-label">Mã thanh toán</span>
                            <span class="payment-value">#${payment.payId}</span>
                        </div>
                        <div class="payment-amount">
                            <span class="amount-value">${payment.amount.toLocaleString()}</span>
                            <span class="amount-currency">${payment.currency}</span>
                        </div>
                    </div>
                    <div class="payment-status-container">
                        <span class="payment-status ${statusClass}">${statusText}</span>
                    </div>
                </div>
                
                <div class="payment-card-body">
                    <div class="payment-info-grid">
                        <div class="payment-info-item">
                            <span class="info-label">Bệnh nhân</span>
                            <span class="info-value">${payment.patientName}</span>
                        </div>
                        
                        <div class="payment-info-item">
                            <span class="info-label">Email</span>
                            <span class="info-value">${payment.patientEmail}</span>
                        </div>
                        
                        <div class="payment-info-item">
                            <span class="info-label">Ngày thanh toán</span>
                            <span class="info-value">${formatDateTime(payment.paymentDate)}</span>
                        </div>
                        
                        <div class="payment-info-item">
                            <span class="info-label">Phương thức</span>
                            <span class="info-value">${payment.paymentMethod}</span>
                        </div>
                        
                        <div class="payment-info-item full-width">
                            <span class="info-label">Mô tả</span>
                            <span class="info-value">${payment.description}</span>
                        </div>
                        
                        ${payment.serviceName ? `
                            <div class="payment-info-item">
                                <span class="info-label">Dịch vụ</span>
                                <span class="info-value service-name">${payment.serviceName}</span>
                            </div>
                        ` : ''}
                        
                        ${payment.servicePrice ? `
                            <div class="payment-info-item">
                                <span class="info-label">Giá dịch vụ</span>
                                <span class="info-value service-price">${payment.servicePrice.toLocaleString()} VND</span>
                            </div>
                        ` : ''}
                    </div>
                    
                    <div class="payment-metadata">
                        <div class="metadata-row">
                            <span class="metadata-label">Tạo lúc:</span>
                            <span class="metadata-value">${formatDateTime(payment.createdAt)}</span>
                        </div>
                        <div class="metadata-row">
                            <span class="metadata-label">Cập nhật:</span>
                            <span class="metadata-value">${formatDateTime(payment.updatedAt)}</span>
                        </div>
                        ${payment.paymentIntentId ? `
                            <div class="metadata-row">
                                <span class="metadata-label">Intent ID:</span>
                                <span class="metadata-value intent-id">${payment.paymentIntentId}</span>
                            </div>
                        ` : ''}
                    </div>
                    
                    ${payment.paymentIntentId && payment.paymentStatus === 1 ? `
                        <div class="payment-actions">
                            <button class="payment-action-btn confirm-btn" onclick="openCardPaymentModal('${payment.paymentIntentId}')">
                                <i class="fas fa-check"></i> Xác nhận thanh toán
                            </button>
                        </div>
                    ` : ''}
                </div>
            </div>
        `;
    });

    html += `</div>`;
    section.innerHTML = html;
}

// Utility function for formatting date time
function formatDateTime(dateString) {
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

// Payment confirmation function
async function confirmPayment(paymentIntentId) {
    if (!paymentIntentId) {
        alert('Không tìm thấy Payment Intent ID');
        return;
    }

    // Show confirmation dialog
    if (!confirm('Bạn có chắc chắn muốn xác nhận thanh toán này?')) {
        return;
    }

    try {
        // Show loading state
        const button = event.target;
        const originalText = button.innerHTML;
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
        button.disabled = true;

        const response = await fetch(`https://localhost:7009/api/Payment/confirm-payment/${paymentIntentId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        const result = await response.json();

        if (response.ok) {
            alert(`Thanh toán đã được xác nhận thành công!\nTrạng thái: ${result.Status}\nPayment Intent ID: ${result.PaymentIntentId}`);
            
            // Reload payments to reflect the new status
            await loadPaymentData();
        } else {
            alert(`Lỗi xác nhận thanh toán: ${result.Error || 'Không xác định'}`);
        }
    } catch (error) {
        console.error('Error confirming payment:', error);
        alert('Có lỗi xảy ra khi xác nhận thanh toán. Vui lòng thử lại.');
    }
}

// Payment failure function
async function failPayment(paymentIntentId) {
    if (!paymentIntentId) {
        alert('Không tìm thấy Payment Intent ID');
        return;
    }

    // Show confirmation dialog
    if (!confirm('Bạn có chắc chắn muốn đánh dấu thanh toán này là thất bại?')) {
        return;
    }

    try {
        // Show loading state
        const button = event.target;
        const originalText = button.innerHTML;
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
        button.disabled = true;

        const response = await fetch(`https://localhost:7009/api/Payment/fail-payment/${paymentIntentId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        const result = await response.json();

        if (response.ok) {
            alert(`Thanh toán đã được đánh dấu thất bại!\nTrạng thái: ${result.Status}\nPayment Intent ID: ${result.PaymentIntentId}`);
            
            // Reload payments to reflect the new status
            await loadPaymentData();
        } else {
            alert(`Lỗi khi đánh dấu thanh toán thất bại: ${result.Error || 'Không xác định'}`);
        }
    } catch (error) {
        console.error('Error failing payment:', error);
        alert('Có lỗi xảy ra khi đánh dấu thanh toán thất bại. Vui lòng thử lại.');
    }
}

// Function to reload payment data
async function loadPaymentData() {
    try {
        const payments = await fetchPatientPayments();
        renderPayments(payments);
    } catch (error) {
        console.error('Error reloading payment data:', error);
    }
}

// Render ARV regimens with embedded medications
function renderARVRegimens(regimens) {
    const section = document.getElementById('arvRegimensContent');
    
    if (!regimens || regimens.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-pills"></i>
                <p>No ARV regimens found.</p>
                <p class="empty-state-subtitle">ARV treatment regimens will appear here when prescribed by your doctor.</p>
            </div>
        `;
        return;
    }

    let html = `
        <div class="regimens-stats">
            <div class="stat-card">
                <div class="stat-number">${regimens.length}</div>
                <div class="stat-label">Total Regimens</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${regimens.filter(r => r.regimenStatus === 2).length}</div>
                <div class="stat-label">Active</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${regimens.filter(r => r.regimenStatus === 5).length}</div>
                <div class="stat-label">Completed</div>
            </div>
        </div>
        <div class="regimens-grid">
    `;
    
    regimens.forEach(regimen => {
        let statusClass = '';
        switch (regimen.regimenStatus) {
            case 1: statusClass = 'regimen-planned'; break;
            case 2: statusClass = 'regimen-active'; break;
            case 3: statusClass = 'regimen-paused'; break;
            case 4: statusClass = 'regimen-failed'; break;
            case 5: statusClass = 'regimen-completed'; break;
            default: statusClass = 'regimen-unknown'; break;
        }
        
        const statusText = regimenStatusMap[regimen.regimenStatus] || regimen.regimenStatus;
        const levelText = regimenLevelMap[regimen.regimenLevel] || regimen.regimenLevel;
        
        // Use embedded medications from the regimen
        const regimenMeds = regimen.arvMedications || [];
        
        html += `
            <div class="regimen-card">
                <div class="regimen-header">
                    <span class="regimen-id">Regimen ID: ${regimen.patientArvRegiId}</span>
                    <span class="regimen-status ${statusClass}">${statusText}</span>
                </div>
                <div class="regimen-details">
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">Start Date</div>
                        <div class="regimen-detail-value">${regimen.startDate}</div>
                    </div>
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">End Date</div>
                        <div class="regimen-detail-value">${regimen.endDate || 'Ongoing'}</div>
                    </div>
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">Regimen Level</div>
                        <div class="regimen-detail-value">${levelText}</div>
                    </div>
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">Total Cost</div>
                        <div class="regimen-detail-value">${regimen.totalCost ? regimen.totalCost.toLocaleString() + ' VND' : 'Not specified'}</div>
                    </div>
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">Created At</div>
                        <div class="regimen-detail-value">${new Date(regimen.createdAt).toLocaleDateString()}</div>
                    </div>
                </div>
                ${regimen.notes ? `
                    <div class="regimen-notes">
                        <strong>Notes:</strong> ${regimen.notes}
                    </div>
                ` : ''}
                
                <div class="regimen-medications">
                    <h4>Medications (${regimenMeds.length})</h4>
                    ${regimenMeds.length > 0 ? `
                        <table class="medications-table">
                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>Dosage</th>
                                    <th>Quantity</th>
                                    <th>Unit Price</th>
                                    <th>Subtotal</th>
                                    <th>Manufacturer</th>
                                    <th>Description</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${regimenMeds.map(med => `
                                    <tr>
                                        <td data-label="Name"><strong>${med.medicationDetail.arvMedicationName}</strong></td>
                                        <td data-label="Dosage">${med.medicationDetail.arvMedicationDosage}</td>
                                        <td data-label="Quantity">${med.quantity}</td>
                                        <td data-label="Unit Price">${med.medicationDetail.arvMedicationPrice ? med.medicationDetail.arvMedicationPrice.toLocaleString() + ' VND' : 'N/A'}</td>
                                        <td data-label="Subtotal">${med.medicationDetail.arvMedicationPrice ? (med.medicationDetail.arvMedicationPrice * med.quantity).toLocaleString() + ' VND' : 'N/A'}</td>
                                        <td data-label="Manufacturer">${med.medicationDetail.arvMedicationManufacturer}</td>
                                        <td data-label="Description"><small>${med.medicationDetail.arvMedicationDescription}</small></td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    ` : `<div class='empty-state'><i class='fas fa-capsules'></i> No medications for this regimen.</div>`}
                </div>
            </div>
        `;
    });
    
    html += `</div>`;
    section.innerHTML = html;
}

// Main function to load all patient data
async function loadPatientData() {
    // Show loading state
    document.body.innerHTML = `
        <div class="loading-container">
            <div class="loading-spinner"></div>
            <p>Đang tải hồ sơ bệnh án của bạn...</p>
        </div>
    `;

    try {
        // Fetch medical record data
        const medicalData = await fetchPatientMedicalData();
        
        // Fetch ARV regimens (new API)
        const arvRegimens = await fetchPatientArvRegimens();
        
        // Fetch payment history
        const payments = await fetchPatientPayments();

        // Render the page structure
        document.body.innerHTML = `
            <div class="medical-record-container">
                <div class="page-header">
                    <h1>My Medical Records</h1>
                    <button class="btn-back" onclick="window.history.back()">
                        <i class="fas fa-arrow-left"></i> Back
                    </button>
                </div>
                
                <!-- Section Navigation -->
                <div class="section-navigation">
                    <button class="nav-btn active" onclick="showSection('appointments')" id="appointmentsNav">
                        <i class="fas fa-calendar-alt"></i> Appointments
                    </button>
                    <button class="nav-btn" onclick="showSection('testResults')" id="testResultsNav">
                        <i class="fas fa-flask"></i> Test Results
                    </button>
                    <button class="nav-btn" onclick="showSection('arvRegimens')" id="arvRegimensNav">
                        <i class="fas fa-pills"></i> ARV Regimens
                    </button>
                    <button class="nav-btn" onclick="showSection('payments')" id="paymentsNav">
                        <i class="fas fa-credit-card"></i> Payments
                    </button>
                </div>
                
                <div class="medical-record-content">
                    <!-- Appointments Section -->
                    <section class="medical-section active" id="appointmentsSection">
                        <div class="section-header">
                            <h2><i class="fas fa-calendar-alt"></i> Appointments</h2>
                            <p class="section-description">View your scheduled and completed appointments</p>
                        </div>
                        <div id="appointmentsContent"></div>
                    </section>
                    
                    <!-- Test Results Section -->
                    <section class="medical-section" id="testResultsSection">
                        <div class="section-header">
                            <h2><i class="fas fa-flask"></i> Test Results</h2>
                            <p class="section-description">View your laboratory test results and reports</p>
                        </div>
                        <div id="testResultsContent"></div>
                    </section>
                    
                    <!-- ARV Regimens Section -->
                    <section class="medical-section" id="arvRegimensSection">
                        <div class="section-header">
                            <h2><i class="fas fa-pills"></i> ARV Regimens</h2>
                            <p class="section-description">View your current and past ARV treatment regimens</p>
                        </div>
                        <div id="arvRegimensContent"></div>
                    </section>
                    
                    <!-- Payments Section -->
                    <section class="medical-section" id="paymentsSection">
                        <div class="section-header">
                            <h2><i class="fas fa-credit-card"></i> Payment History</h2>
                            <p class="section-description">View your payment transactions and billing history</p>
                        </div>
                        <div id="paymentsContent"></div>
                    </section>
                </div>
            </div>
            
            <!-- Card Payment Modal -->
            <div id="cardPaymentModal" class="card-payment-modal">
                <div class="card-payment-modal-content">
                    <div class="card-payment-modal-header">
                        <h2 class="card-payment-modal-title">Nhập thông tin thẻ thanh toán</h2>
                        <button class="card-payment-close" onclick="closeCardPaymentModal()">&times;</button>
                    </div>
                    
                    <div class="card-payment-modal-body">
                        <!-- Test Cards Section -->
                        <div id="testCardsSection" class="test-cards-section">
                            <h3>Test Cards Available</h3>
                            <div class="loading-test-cards">
                                <i class="fas fa-spinner fa-spin"></i> Đang tải danh sách thẻ test...
                            </div>
                        </div>
                        
                        <form id="cardPaymentForm">
                            <div class="form-group">
                                <label for="card-number">Số thẻ</label>
                                <input type="text" id="card-number" placeholder="4242 4242 4242 4242" maxlength="23" required />
                            </div>
                            
                            <div class="form-row">
                                <div class="form-group">
                                    <label for="card-expiry">Ngày hết hạn</label>
                                    <input type="text" id="card-expiry" placeholder="MM/YY" maxlength="5" required />
                                </div>
                                <div class="form-group">
                                    <label for="card-cvc">CVC</label>
                                    <input type="text" id="card-cvc" placeholder="123" maxlength="4" required />
                                </div>
                            </div>
                            
                            <div class="form-group">
                                <label for="card-name">Tên chủ thẻ</label>
                                <input type="text" id="card-name" placeholder="Nguyen Van A" required />
                            </div>
                            
                            <div class="card-payment-form-actions">
                                <button type="button" class="card-payment-btn card-payment-btn-cancel" onclick="closeCardPaymentModal()">
                                    Hủy
                                </button>
                                <button type="button" id="cardPaymentSubmit" class="card-payment-btn card-payment-btn-submit" onclick="submitCardPayment()">
                                    <div class="spinner"></div>
                                    <span class="btn-text">Thanh toán</span>
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        `;

        // Check if we have medical data
        if (medicalData && medicalData.appointments && medicalData.appointments.length > 0) {
            // Render appointments
            renderAppointments(medicalData.appointments);
            
            // Render test results
            renderTestResults(medicalData.testResults);
        } else {
            // No appointments found
            document.getElementById('appointmentsContent').innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-calendar-times"></i>
                    <p>No appointments found.</p>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Đặt Lịch Hẹn
                    </button>
                </div>
            `;
            
            // No test results
            document.getElementById('testResultsContent').innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-flask"></i>
                    <p>No test results found.</p>
                    <p class="empty-state-subtitle">Test results will appear here after your laboratory tests.</p>
                </div>
            `;
        }

        // Render ARV regimens (using new API)
        renderARVRegimens(arvRegimens);
        
        // Render payment history
        renderPayments(payments);

        // Add section navigation functionality
        window.showSection = function(sectionName) {
            // Hide all sections
            document.querySelectorAll('.medical-section').forEach(section => {
                section.classList.remove('active');
            });
            
            // Remove active class from all nav buttons
            document.querySelectorAll('.nav-btn').forEach(btn => {
                btn.classList.remove('active');
            });
            
            // Show selected section
            document.getElementById(sectionName + 'Section').classList.add('active');
            
            // Activate corresponding nav button
            document.getElementById(sectionName + 'Nav').classList.add('active');
        };

        // If no data at all, show a comprehensive message in appointments section
        if ((!medicalData || !medicalData.appointments || medicalData.appointments.length === 0) && 
            (!arvRegimens || arvRegimens.length === 0) && 
            (!payments || payments.length === 0)) {
            
            document.getElementById('appointmentsContent').innerHTML = `
                <div class="no-records-container">
                    <div class="no-records-icon">
                        <i class="fas fa-clipboard-list"></i>
                    </div>
                    <h3>No Medical Records Found</h3>
                    <p>You don't have any medical records yet. Medical records will appear here after your appointments with doctors.</p>
                    <div class="suggestions">
                        <p><strong>What you can do:</strong></p>
                        <ul>
                            <li>Book an appointment with a doctor</li>
                            <li>Complete your scheduled appointments</li>
                            <li>Your medical records will be created after consultations</li>
                        </ul>
                    </div>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Đặt Lịch Hẹn
                    </button>
                </div>
            `;
        }

    } catch (error) {
        console.error('Error loading patient data:', error);
        document.body.innerHTML = `
            <div class="error-state">
                <i class="fas fa-exclamation-triangle"></i>
                <p>Error loading medical records. Please try again.</p>
                <button class="btn-retry" onclick="window.location.reload()">
                    <i class="fas fa-refresh"></i> Thử Lại
                </button>
            </div>
        `;
    }
}

// Initialize page when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Load patient data
    loadPatientData();
    
    // Setup modal event listeners after a delay to ensure DOM is ready
    setTimeout(() => {
        setupCardModalEventListeners();
    }, 1000);
});

// ============================
// CARD PAYMENT MODAL FUNCTIONS
// ============================

// Global variable to store test card data
let testCardData = null;

// Global variable to store current payment intent ID
let currentPaymentIntentId = null;

// Function to fetch test card numbers from API
async function fetchTestCardNumbers() {
    try {
        const response = await fetch('https://localhost:7009/api/Payment/test-card-numbers', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to fetch test card numbers');
        }
        
        const data = await response.json();
        testCardData = data;
        console.log('Test card data fetched:', testCardData);
        return data;
    } catch (error) {
        console.error('Error fetching test card numbers:', error);
        // Return fallback data if API fails
        return {
            successScenarios: [
                { cardNumber: "4242424242424242", description: "Visa - Success" },
                { cardNumber: "5555555555554444", description: "Mastercard - Success" },
                { cardNumber: "378282246310005", description: "American Express - Success" }
            ],
            failureScenarios: [
                { cardNumber: "4000000000000002", description: "Visa - Generic decline" }
            ]
        };
    }
}

// Function to open card payment modal
async function openCardPaymentModal(paymentIntentId) {
    currentPaymentIntentId = paymentIntentId;
    console.log('Opening card payment modal for payment intent:', paymentIntentId);
    
    const modal = document.getElementById('cardPaymentModal');
    if (modal) {
        modal.classList.add('show');
        modal.style.display = 'flex';
        
        // Reset form
        resetCardPaymentForm();
        
        // Load and display test cards
        await loadAndDisplayTestCards();
        
        // Setup event listeners for modal inputs immediately
        setTimeout(() => {
            setupCardModalEventListeners();
            
            // Focus on card number input
            const cardNumberInput = document.getElementById('card-number');
            if (cardNumberInput) {
                cardNumberInput.focus();
                console.log('Card number input focused');
            }
        }, 100);
        
    } else {
        console.error('Card payment modal not found!');
    }
}

// Function to load and display test cards in modal
async function loadAndDisplayTestCards() {
    const testCardsSection = document.getElementById('testCardsSection');
    if (!testCardsSection) return;
    
    try {
        // Fetch test card data if not already loaded
        if (!testCardData) {
            testCardData = await fetchTestCardNumbers();
        }
        
        let html = `
            <h3>Test Cards Available</h3>
            <div class="test-cards-container">
        `;
        
        // Success cards section
        if (testCardData.successScenarios && testCardData.successScenarios.length > 0) {
            html += `
                <div class="test-cards-group success-cards">
                    <h4><i class="fas fa-check-circle"></i> Success Cards (Confirm Payment)</h4>
                    <div class="test-cards-grid">
            `;
            
            testCardData.successScenarios.forEach(card => {
                const formattedCardNumber = formatCardNumberDisplay(card.cardNumber);
                html += `
                    <div class="test-card-item success-card" onclick="selectTestCard('${card.cardNumber}')">
                        <div class="test-card-number">${formattedCardNumber}</div>
                        <div class="test-card-description">${card.description}</div>
                        <div class="test-card-action">Click to use</div>
                    </div>
                `;
            });
            
            html += `
                    </div>
                </div>
            `;
        }
        
        // Failure cards section
        if (testCardData.failureScenarios && testCardData.failureScenarios.length > 0) {
            html += `
                <div class="test-cards-group failure-cards">
                    <h4><i class="fas fa-times-circle"></i> Failure Cards (Fail Payment)</h4>
                    <div class="test-cards-grid">
            `;
            
            testCardData.failureScenarios.forEach(card => {
                const formattedCardNumber = formatCardNumberDisplay(card.cardNumber);
                html += `
                    <div class="test-card-item failure-card" onclick="selectTestCard('${card.cardNumber}')">
                        <div class="test-card-number">${formattedCardNumber}</div>
                        <div class="test-card-description">${card.description}</div>
                        <div class="test-card-action">Click to use</div>
                    </div>
                `;
            });
            
            html += `
                    </div>
                </div>
            `;
        }
        
        html += `</div>`;
        
        testCardsSection.innerHTML = html;
        
    } catch (error) {
        console.error('Error loading test cards:', error);
        testCardsSection.innerHTML = `
            <h3>Test Cards Available</h3>
            <div class="test-cards-error">
                <i class="fas fa-exclamation-triangle"></i>
                <p>Không thể tải danh sách thẻ test. Vui lòng nhập số thẻ thủ công.</p>
            </div>
        `;
    }
}

// Function to format card number for display
function formatCardNumberDisplay(cardNumber) {
    if (!cardNumber) return '';
    // Add spaces every 4 digits
    return cardNumber.replace(/(.{4})/g, '$1 ').trim();
}

// Function to select a test card and fill the form
function selectTestCard(cardNumber) {
    console.log('Selected test card:', cardNumber);
    
    const cardNumberInput = document.getElementById('card-number');
    const expiryInput = document.getElementById('card-expiry');
    const cvcInput = document.getElementById('card-cvc');
    const nameInput = document.getElementById('card-name');
    
    if (cardNumberInput) {
        cardNumberInput.value = formatCardNumberDisplay(cardNumber);
        // Trigger input event to format the card number
        const event = new Event('input', { bubbles: true });
        cardNumberInput.dispatchEvent(event);
    }
    
    // Auto-fill other fields with default test values
    if (expiryInput) {
        expiryInput.value = '12/25';
    }
    
    if (cvcInput) {
        cvcInput.value = '123';
    }
    
    if (nameInput) {
        nameInput.value = 'Test User';
    }
    
    // Focus on the submit button for convenience
    const submitBtn = document.getElementById('cardPaymentSubmit');
    if (submitBtn) {
        submitBtn.focus();
    }
    
    // Show success message
    const notification = document.createElement('div');
    notification.className = 'test-card-notification';
    notification.innerHTML = `<i class="fas fa-check"></i> Đã chọn thẻ test: ${formatCardNumberDisplay(cardNumber)}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #28a745;
        color: white;
        padding: 10px 15px;
        border-radius: 5px;
        z-index: 10001;
        animation: slideInRight 0.3s ease-out;
    `;
    
    document.body.appendChild(notification);
    
    // Remove notification after 3 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 3000);
}
    const modal = document.getElementById('cardPaymentModal');
    if (modal) {
        modal.classList.remove('show');
        modal.style.display = 'none';
        resetCardPaymentForm();
    }
    currentPaymentIntentId = null;


// Function to reset card payment form
function resetCardPaymentForm() {
    const form = document.getElementById('cardPaymentForm');
    if (form) {
        form.reset();
        
        // Clear validation classes
        document.querySelectorAll('.form-group').forEach(group => {
            group.classList.remove('error', 'success');
        });
        
        // Clear error messages
        document.querySelectorAll('.error-message').forEach(msg => {
            msg.remove();
        });
        
        // Reset submit button
        const submitBtn = document.getElementById('cardPaymentSubmit');
        if (submitBtn) {
            submitBtn.classList.remove('loading');
            submitBtn.disabled = false;
            submitBtn.querySelector('.btn-text').textContent = 'Thanh toán';
        }
    }
}

// Function to validate card form
function validateCardForm() {
    let isValid = true;
    
    // Get form elements
    const cardNumber = document.getElementById('card-number');
    const expiry = document.getElementById('card-expiry');
    const cvc = document.getElementById('card-cvc');
    const name = document.getElementById('card-name');
    
    // Validate card number
    if (!cardNumber.value || cardNumber.value.replace(/\s/g, '').length < 13) {
        showFieldError(cardNumber, 'Vui lòng nhập số thẻ hợp lệ (13-19 số)');
        isValid = false;
    } else {
        showFieldSuccess(cardNumber);
    }
    
    // Validate expiry date
    if (!expiry.value || !expiry.value.match(/^(0[1-9]|1[0-2])\/\d{2}$/)) {
        showFieldError(expiry, 'Vui lòng nhập ngày hết hạn hợp lệ (MM/YY)');
        isValid = false;
    } else {
        // Check if expiry date is in the future
        const [month, year] = expiry.value.split('/');
        const expiryDate = new Date(2000 + parseInt(year), parseInt(month) - 1);
        const now = new Date();
        now.setDate(1); // Set to first day of current month for comparison
        
        if (expiryDate < now) {
            showFieldError(expiry, 'Thẻ đã hết hạn');
            isValid = false;
        } else {
            showFieldSuccess(expiry);
        }
    }
    
    // Validate CVC
    if (!cvc.value || cvc.value.length < 3 || cvc.value.length > 4) {
        showFieldError(cvc, 'Vui lòng nhập CVC hợp lệ (3-4 số)');
        isValid = false;
    } else {
        showFieldSuccess(cvc);
    }
    
    // Validate cardholder name
    if (!name.value || name.value.trim().length < 2) {
        showFieldError(name, 'Vui lòng nhập tên chủ thẻ');
        isValid = false;
    } else {
        showFieldSuccess(name);
    }
    
    return isValid;
}

// Function to show field error
function showFieldError(field, message) {
    const formGroup = field.closest('.form-group');
    formGroup.classList.remove('success');
    formGroup.classList.add('error');
    
    // Remove existing error message
    const existingError = formGroup.querySelector('.error-message');
    if (existingError) {
        existingError.remove();
    }
    
    // Add new error message
    const errorDiv = document.createElement('div');
    errorDiv.className = 'error-message';
    errorDiv.textContent = message;
    formGroup.appendChild(errorDiv);
}

// Function to show field success
function showFieldSuccess(field) {
    const formGroup = field.closest('.form-group');
    formGroup.classList.remove('error');
    formGroup.classList.add('success');
    
    // Remove error message
    const existingError = formGroup.querySelector('.error-message');
    if (existingError) {
        existingError.remove();
    }
}

// Function to format card number input
function formatCardNumber(input) {
    console.log('Formatting card number:', input.value);
    
    // Remove all non-digits
    let value = input.value.replace(/\D/g, '');
    
    // Limit to 16 digits
    if (value.length > 16) {
        value = value.substring(0, 16);
    }
    
    // Add spaces every 4 digits
    let formattedValue = '';
    for (let i = 0; i < value.length; i++) {
        if (i > 0 && i % 4 === 0) {
            formattedValue += ' ';
        }
        formattedValue += value[i];
    }
    
    input.value = formattedValue;
    console.log('Formatted to:', formattedValue);
}

// Function to format expiry date input
function formatExpiryDate(input) {
    console.log('Formatting expiry date:', input.value);
    
    // Remove all non-digits
    let value = input.value.replace(/\D/g, '');
    
    // Limit to 4 digits
    if (value.length > 4) {
        value = value.substring(0, 4);
    }
    
    // Add slash after month
    if (value.length >= 2) {
        value = value.substring(0, 2) + '/' + value.substring(2, 4);
    }
    
    input.value = value;
    console.log('Formatted to:', value);
}

// Function to handle card payment submission
async function submitCardPayment() {
    // Validate form
    if (!validateCardForm()) {
        return;
    }
    
    if (!currentPaymentIntentId) {
        alert('Không tìm thấy Payment Intent ID');
        return;
    }
    
    const submitBtn = document.getElementById('cardPaymentSubmit');
    
    try {
        // Show loading state
        submitBtn.classList.add('loading');
        submitBtn.disabled = true;
        
        // Get form data
        const cardNumber = document.getElementById('card-number').value.replace(/\s/g, '');
        const expiry = document.getElementById('card-expiry').value;
        const cvc = document.getElementById('card-cvc').value;
        const name = document.getElementById('card-name').value;
        
        console.log('Processing payment with card:', cardNumber);
        
        // Simulate API call delay
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        // Ensure test card data is loaded
        if (!testCardData) {
            testCardData = await fetchTestCardNumbers();
        }
        
        // Get card numbers from API response
        const successCardNumbers = testCardData.successScenarios.map(card => card.cardNumber);
        const failureCardNumbers = testCardData.failureScenarios.map(card => card.cardNumber);
        
        let apiResult;
        let cardInfo = null;
        
        // Find the card info based on the entered card number
        if (successCardNumbers.includes(cardNumber)) {
            cardInfo = testCardData.successScenarios.find(card => card.cardNumber === cardNumber);
            
            // Call confirm payment API for success cards
            console.log('Calling confirm payment API with card:', cardInfo);
            try {
                const response = await fetch(`https://localhost:7009/api/Payment/confirm-payment/${currentPaymentIntentId}`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        cardNumber: cardInfo.cardNumber,
                        description: cardInfo.description
                    })
                });

                const result = await response.json();

                if (response.ok) {
                    apiResult = {
                        success: true,
                        message: `Thanh toán được xác nhận thành công!\nMô tả: ${cardInfo.description}\nTrạng thái: ${result.Status}\nPayment Intent ID: ${result.PaymentIntentId}`,
                        type: 'confirm'
                    };
                } else {
                    apiResult = {
                        success: false,
                        message: `Lỗi xác nhận thanh toán: ${result.Error || 'Không xác định'}`,
                        type: 'confirm'
                    };
                }
            } catch (error) {
                console.error('Error calling confirm payment API:', error);
                apiResult = {
                    success: false,
                    message: 'Có lỗi xảy ra khi gọi API xác nhận thanh toán',
                    type: 'confirm'
                };
            }
        } else if (failureCardNumbers.includes(cardNumber)) {
            cardInfo = testCardData.failureScenarios.find(card => card.cardNumber === cardNumber);
            
            // Call fail payment API for failure cards
            console.log('Calling fail payment API with card:', cardInfo);
            try {
                const response = await fetch(`https://localhost:7009/api/Payment/fail-payment/${currentPaymentIntentId}`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        cardNumber: cardInfo.cardNumber,
                        description: cardInfo.description
                    })
                });

                const result = await response.json();

                if (response.ok) {
                    apiResult = {
                        success: true,
                        message: `Thanh toán được đánh dấu thất bại!\nMô tả: ${cardInfo.description}\nTrạng thái: ${result.Status}\nPayment Intent ID: ${result.PaymentIntentId}`,
                        type: 'fail'
                    };
                } else {
                    apiResult = {
                        success: false,
                        message: `Lỗi khi đánh dấu thanh toán thất bại: ${result.Error || 'Không xác định'}`,
                        type: 'fail'
                    };
                }
            } catch (error) {
                console.error('Error calling fail payment API:', error);
                apiResult = {
                    success: false,
                    message: 'Có lỗi xảy ra khi gọi API đánh dấu thất bại',
                    type: 'fail'
                };
            }
        } else {
            // Invalid card number - use test card from API
            apiResult = {
                success: false,
                message: 'Số thẻ không hợp lệ! Vui lòng sử dụng số thẻ test hợp lệ.',
                type: 'invalid'
            };
        }
        
        // Show result
        alert(apiResult.message);
        
        if (apiResult.success) {
            // Close modal after success
            setTimeout(() => {
                closeCardPaymentModal();
                // Reload payment data to reflect changes
                loadPaymentData();
            }, 2000);
        }
        
    } catch (error) {
        console.error('Error processing payment:', error);
        alert('Có lỗi xảy ra khi xử lý thanh toán. Vui lòng thử lại.');
    } finally {
        // Reset loading state
        submitBtn.classList.remove('loading');
        submitBtn.disabled = false;
    }
}

// Event listener setup for modal inputs
function setupCardModalEventListeners() {
    console.log('Setting up card modal event listeners...');
    
    // Check if modal exists in DOM
    const modal = document.getElementById('cardPaymentModal');
    if (!modal) {
        console.log('Card payment modal not found in DOM yet');
        return;
    }
    
    console.log('Card payment modal found, setting up event listeners');
    
    // Remove existing event listeners to prevent duplicates
    const cardNumberInput = document.getElementById('card-number');
    const expiryInput = document.getElementById('card-expiry');
    const cvcInput = document.getElementById('card-cvc');
    
    if (cardNumberInput) {
        // Clone node to remove all existing event listeners
        const newCardNumberInput = cardNumberInput.cloneNode(true);
        cardNumberInput.parentNode.replaceChild(newCardNumberInput, cardNumberInput);
        
        // Add new event listener
        newCardNumberInput.addEventListener('input', function(e) {
            console.log('Card number input event triggered');
            formatCardNumber(this);
        });
        
        // Allow typing numbers
        newCardNumberInput.addEventListener('keypress', function(e) {
            // Allow numbers, spaces, backspace, delete
            const allowedKeys = ['0','1','2','3','4','5','6','7','8','9',' '];
            if (!allowedKeys.includes(e.key) && !['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Tab'].includes(e.key)) {
                e.preventDefault();
            }
        });
    }
    
    if (expiryInput) {
        // Clone node to remove all existing event listeners
        const newExpiryInput = expiryInput.cloneNode(true);
        expiryInput.parentNode.replaceChild(newExpiryInput, expiryInput);
        
        // Add new event listener
        newExpiryInput.addEventListener('input', function(e) {
            console.log('Expiry input event triggered');
            formatExpiryDate(this);
        });
        
        // Allow typing numbers and slash
        newExpiryInput.addEventListener('keypress', function(e) {
            const allowedKeys = ['0','1','2','3','4','5','6','7','8','9','/'];
            if (!allowedKeys.includes(e.key) && !['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Tab'].includes(e.key)) {
                e.preventDefault();
            }
        });
    }
    
    if (cvcInput) {
        // Clone node to remove all existing event listeners
        const newCvcInput = cvcInput.cloneNode(true);
        cvcInput.parentNode.replaceChild(newCvcInput, cvcInput);
        
        // Add new event listener
        newCvcInput.addEventListener('input', function(e) {
            console.log('CVC input event triggered');
            this.value = this.value.replace(/\D/g, '').substring(0, 4);
        });
        
        // Allow typing numbers only
        newCvcInput.addEventListener('keypress', function(e) {
            const allowedKeys = ['0','1','2','3','4','5','6','7','8','9'];
            if (!allowedKeys.includes(e.key) && !['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Tab'].includes(e.key)) {
                e.preventDefault();
            }
        });
    }
    
    // Close modal when clicking outside
    modal.addEventListener('click', function(e) {
        if (e.target === modal) {
            closeCardPaymentModal();
        }
    });
    
    // Close modal with Escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            const modal = document.getElementById('cardPaymentModal');
            if (modal && modal.classList.contains('show')) {
                closeCardPaymentModal();
            }
        }
    });
}