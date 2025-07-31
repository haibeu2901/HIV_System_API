// Get token from localStorage
const token = localStorage.getItem('token');

let allRegimens = [];
let allRegimenMedications = [];
let allTestResults = [];
// Appointment status mapping
const appointmentStatusMap = {
    1: 'Chờ xác nhận',
    2: 'Đã lên lịch',
    3: 'Đã lên lịch lại',
    4: 'Đã hủy',
    5: 'Đã hoàn thành'
};

// ARV Regimen level and status mapping
const regimenLevelMap = {
    1: "Bậc 1",
    2: "Bậc 2",
    3: "Bậc 3",
    4: "Trường hợp đặc biệt"
};
const regimenStatusMap = {
    1: "Đã lên kế hoạch",
    2: "Đang hoạt động",
    3: "Tạm dừng",
    4: "Đã hủy",
    5: "Hoàn thành"
};

// Payment status mapping

// Set window.isStaff and window.i// Payment status mapping
const paymentStatusMap = {
    1: 'Đang chờ',
    2: 'Đã thanh toán',
    3: 'Thất bại'
};

function getVietnamDateTime() {
    const now = new Date();
    // Vietnam is UTC+7
    const vietnamOffset = 7 * 60; // in minutes
    const localOffset = now.getTimezoneOffset(); // in minutes
    const diff = vietnamOffset + localOffset;
    return new Date(now.getTime() + diff * 60 * 1000).toISOString();
}
// Set window.isStaff and window.isDoctor globally
window.isStaff = false;
window.isDoctor = false;
if (window.roleUtils && window.roleUtils.getUserRole && window.roleUtils.ROLE_NAMES) {
    const roleId = window.roleUtils.getUserRole();
    const roleName = window.roleUtils.ROLE_NAMES[roleId];
    window.isStaff = (roleName === 'staff');
    window.isDoctor = (roleName === 'doctor');
}

// Add global modal for creating patient medical record if not found
if (!document.getElementById('createPmrModal')) {
    const modalHtml = `
    <div id="createPmrModal" class="modal">
      <div class="modal-content" style="max-width:400px; margin:auto; text-align:center;">
        <span class="close" id="closeCreatePmrModal" style="float:right; font-size:24px; cursor:pointer;">&times;</span>
        <div id="createPmrModalText" style="margin: 30px 0 10px 0; font-size: 1.1em;">
          Bệnh nhân không có Hồ sơ bệnh án.<br>Bạn có muốn tạo một hồ sơ bệnh án cho bệnh nhân?
        </div>
        <div style="margin: 20px 0 10px 0;">
          <button id="createPmrYesBtn" style="margin-right: 16px; padding: 6px 18px; background:#2196f3; color:#fff; border:none; border-radius:4px; cursor:pointer;">Có</button>
          <button id="createPmrNoBtn" style="padding: 6px 18px; background:#f44336; color:#fff; border:none; border-radius:4px; cursor:pointer;">Không</button>
        </div>
      </div>
    </div>
  `;
    document.body.insertAdjacentHTML('beforeend', modalHtml);
}

function showCreatePmrModal(ptnId) {
    const modal = document.getElementById('createPmrModal');
    modal.style.display = 'block';
    document.getElementById('closeCreatePmrModal').onclick = () => { modal.style.display = 'none'; };
    document.getElementById('createPmrNoBtn').onclick = () => { modal.style.display = 'none'; };
    document.getElementById('createPmrYesBtn').onclick = async () => {
        try {
            const res = await fetch('https://localhost:7009/api/PatientMedicalRecord/CreatePatientMedicalRecord', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ ptnId })
            });
            if (!res.ok) throw new Error('Không thể tạo hồ sơ bệnh án.');
            modal.style.display = 'none';
            // Reload the page to reflect the new record
            window.location.reload();
            // setMessage('Tạo hồ sơ bệnh án thành công!', true); // This will not show after reload
        } catch (err) {
            setMessage('Lỗi khi tạo hồ sơ bệnh án.', false);
        }
    };
}

// Get patient ID from URL parameters
function getPatientIdFromUrl() {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('patientId');
}

// Fetch patient details
async function fetchPatientDetails(patientId) {
    try {
        const response = await fetch(`https://localhost:7009/api/Patient/GetPatientById/${patientId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Lỗi thất bại lấy thông tin bệnh nhân');
        return await response.json();
    } catch (error) {
        console.error('Error fetching patient details:', error);
        return null;
    }
}

// Add new fetchPatientMedicalDataByPatientId
async function fetchPatientMedicalDataByPatientId(patientId) {
    try {
        const response = await fetch(`https://localhost:7009/api/PatientMedicalRecord/GetPatientMedicalRecordByPatientId?patientId=${patientId}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.status === 404) {
            showCreatePmrModal(patientId);
            return null;
        }
        if (!response.ok) throw new Error('Lỗi thất bại lấy thông tin bệnh án bệnh nhân');
        const data = await response.json();
        if (!data || !data.pmrId) {
            showCreatePmrModal(patientId);
            return null;
        }
        return data;
    } catch (error) {
        console.error('Error fetching patient medical data:', error);
        return null;
    }
}

// Fetch all ARV medications for the patient
async function fetchPatientArvMedications(patientId) {
    try {
        const response = await fetch(`https://localhost:7009/api/PatientArvMedication/GetPatientArvMedicationsByPatientId/${patientId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Lỗi thất bại lấy thông tin phác đồ ARV của bệnh nhân');
        return await response.json();
    } catch (error) {
        console.error('Error fetching ARV medications:', error);
        return [];
    }
}

// Fetch patient payment history
async function fetchPatientPayments(pmrId) {
    try {
        const response = await fetch(`https://localhost:7009/api/Payment/GetPaymentsByPmrId/${pmrId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Lỗi thất bại lấy lịch sử thanh toán của bệnh nhân');
        return await response.json();
    } catch (error) {
        console.error('Error fetching patient payments:', error);
        return [];
    }
}

// Fetch all services for payment creation
async function fetchAllServices() {
    try {
        const response = await fetch('https://localhost:7009/api/MedicalService/GetAllMedicalServices', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Lỗi lấy danh sách dịch vụ');
        return await response.json();
    } catch (error) {
        console.error('Error fetching services:', error);
        return [];
    }
}

// Create new payment
async function createPayment(paymentData) {
    try {
        const response = await fetch('https://localhost:7009/api/Payment/CreatePayment', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(paymentData)
        });
        if (!response.ok) throw new Error('Lỗi tạo thanh toán');
        return await response.json();
    } catch (error) {
        console.error('Error creating payment:', error);
        throw error;
    }
}

// Render payment history
function renderPaymentHistory(payments) {
    const section = document.getElementById('paymentHistorySection');

    if (!payments || payments.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-credit-card"></i>
                <p>Không tìm thấy lịch sử giao dịch cho bệnh nhân này.</p>
            </div>
        `;
        return;
    }

    let html = `
        <div class="payments-container">
    `;

    payments.forEach(payment => {
        const statusClass = `payment-status-${payment.paymentStatus}`;
        const statusText = paymentStatusMap[payment.paymentStatus] || 'Không xác định';

        html += `
            <div class="payment-card">
                <div class="payment-header">
                    <div class="payment-id-info">
                        <span class="payment-id">Mã thanh toán: ${payment.payId}</span>
                        <span class="payment-status ${statusClass}">${statusText}</span>
                    </div>
                    <div class="payment-amount">
                        ${formatCurrency(payment.amount)} ${payment.currency}
                    </div>
                </div>
                <div class="payment-details">
                    <div class="payment-info">
                        <p><strong>Ngày thanh toán:</strong> ${formatDateTime(payment.paymentDate)}</p>
                        <p><strong>Phương thức:</strong> ${payment.paymentMethod}</p>
                        <p><strong>Mô tả:</strong> ${payment.description}</p>
                        ${payment.serviceName ? `<p><strong>Dịch vụ:</strong> ${payment.serviceName}</p>` : ''}
${payment.servicePrice ? `<p><strong>Giá dịch vụ:</strong> ${formatCurrency(payment.servicePrice)} VND</p>` : ''}
                    </div>
                    <div class="payment-metadata">
                        <small><strong>Tạo lúc:</strong> ${formatDateTime(payment.createdAt)}</small>
                        <small><strong>Cập nhật:</strong> ${formatDateTime(payment.updatedAt)}</small>
                        ${payment.paymentIntentId ? `<small><strong>Intent ID:</strong> ${payment.paymentIntentId}</small>` : ''}
                    </div>
                </div>
            </div>
        `;
    });

    html += `</div>`;
    section.innerHTML = html;
}

// Utility functions for formatting
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount);
}

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

// Render patient profile
function renderPatientProfile(patient) {
    if (!patient) {
        document.getElementById('patientName').textContent = 'Không tìm thấy bệnh nhân';
        return;
    }

    document.getElementById('patientName').textContent = patient.account.fullname;
    document.getElementById('patientGender').textContent = patient.account.gender ? 'Nam' : 'Nữ';
    document.getElementById('patientDOB').textContent = patient.account.dob;
    document.getElementById('patientEmail').textContent = patient.account.email;
    document.getElementById('patientId').textContent = patient.patientId;
}

// Render appointments
function renderAppointments(appointments) {
    const section = document.getElementById('appointmentsSection');

    if (!appointments || appointments.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-calendar-times"></i>
                <p>Không tìm thấy cuộc hẹn cho bệnh nhân này.</p>
            </div>
        `;
        return;
    }

    let html = `
        <table class="appointments-table">
            <thead>
                <tr>
                    <th>Ngày</th>
                    <th>Thời gian</th>
                    <th>Bác sĩ</th>
                    <th>Trạng thái</th>
                    <th>Ghi chú</th>
                </tr>
            </thead>
            <tbody>
    `;

    appointments.forEach(appt => {
        const statusLabel = appointmentStatusMap[appt.apmStatus] || 'Unknown';
        const statusClass = `status-${appt.apmStatus}`;

        html += `
            <tr>
                <td>${appt.apmtDate || appt.requestDate}</td>
                <td>${(appt.apmTime || appt.requestTime || '-').slice(0, 5)}</td>
                <td>${appt.doctorName || '-'}</td>
<td><span class="appointment-status ${statusClass}">${statusLabel}</span></td>
                <td>${appt.notes || '-'}</td>
            </tr>
        `;
    });

    html += `
            </tbody>
        </table>
    `;

    section.innerHTML = html;
}

// Render test results (array)
function renderTestResults(testResults) {
    const section = document.getElementById('testResultsSection');

    if (!testResults || testResults.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-flask"></i>
                <p>No test results found for this patient.</p>
            </div>
        `;
        return;
    }

    let html = `
        <table class="test-results-table" style="width:100%;border-collapse:collapse;">
            <thead>
                <tr>
                    <th>Ngày xét nghiệm</th>
                    <th>Kết quả</th>
                    <th>Ghi chú</th>
                    <th>Mở rộng/Đóng</th>
                </tr>
            </thead>
            <tbody>
    `;

    testResults.forEach((testResult, idx) => {
        const resultClass = testResult.result ? 'test-result-positive' : 'test-result-negative';
        const resultText = testResult.result ? 'Dương tính' : 'Âm tính';

        html += `
            <tr class="test-result-row" data-idx="${idx}" style="cursor:pointer;">
                <td>${testResult.testDate}</td>
                <td><span class="test-result-overall ${resultClass}">${resultText}</span></td>
                <td>${testResult.notes || ''}</td>
                <td>
                    <button class="toggle-test-details-btn" data-idx="${idx}">▼</button>
                </td>
            </tr>
            <tr class="test-result-details-row" id="test-result-details-${idx}" style="display:none;background:#fafbfc;">
                <td colspan="4">
                    <div><strong>Thành phần xét nghiệm:</strong></div>
                    ${testResult.componentTestResults && testResult.componentTestResults.length > 0 ? `
                        <table class="medications-table" style="width:100%;margin-top:10px;">
                            <thead>
                                <tr>
                                    <th>Tên thành phần</th>
                                    <th>Mô tả</th>
                                    <th>Giá trị</th>
                                    <th>Ghi chú</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${testResult.componentTestResults.map(comp => `
                                    <tr>
                                        <td>${comp.componentTestResultName || ''}</td>
                                        <td>${comp.ctrDescription || ''}</td>
                                        <td>${comp.resultValue || ''}</td>
                                        <td>${comp.notes || ''}</td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    ` : `<div class='empty-state'><i class='fas fa-capsules'></i> Không có thành phần xét nghiệm.</div>`}
                    <div style="margin-top:1rem;text-align:right;">
                        ${window.isStaff ? `<button class="secondary-btn update-test-result-btn" data-id="${testResult.testResultId}">Cập nhật kết quả</button>` : ''}
                    </div>
                </td>
            </tr>
        `;
    });

    html += `</tbody></table>`;
    section.innerHTML = html;

    // Toggle details on click
    document.querySelectorAll('.toggle-test-details-btn').forEach(btn => {
        btn.onclick = function(e) {
            e.stopPropagation();
            const idx = this.getAttribute('data-idx');
            const detailsRow = document.getElementById(`test-result-details-${idx}`);
            if (detailsRow.style.display === 'none') {
                detailsRow.style.display = '';
                this.textContent = '▲';
            } else {
                detailsRow.style.display = 'none';
                this.textContent = '▼';
            }
        };
    });

    // Add update event listeners as before
    if (window.isStaff) {
        section.querySelectorAll('.update-test-result-btn').forEach(btn => {
            btn.addEventListener('click', async function () {
                const testResultId = this.getAttribute('data-id');
                if (!testResultId) return;
                const testResult = testResults.find(tr => String(tr.testResultId) === String(testResultId));
                if (!testResult) {
                    alert('Không tìm thấy dữ liệu kết quả xét nghiệm.');
                    return;
                }
                document.getElementById('updateTestResultId').value = testResult.testResultId;
                document.getElementById('updateTestResultDate').value = testResult.testDate || '';
                document.getElementById('updateTestResultSelect').value = testResult.result ? 'true' : 'false';
                document.getElementById('updateTestResultNotes').value = testResult.notes || '';
                document.getElementById('updateTestResultMsg').textContent = '';
                loadComponentTestResultsFromData(testResult.componentTestResults || []);
                document.getElementById('updateTestResultModal').style.display = 'block';
            });
        });
    }

    // After rendering the test results table
    section.querySelectorAll('.component-row').forEach(row => {
        row.addEventListener('click', function () {
            // Get data from row attributes
            document.getElementById('updateComponentTestId').value = row.getAttribute('data-id');
            document.getElementById('updateComponentTestName').value = decodeURIComponent(row.getAttribute('data-name'));
            document.getElementById('updateComponentTestDesc').value = decodeURIComponent(row.getAttribute('data-desc'));
            document.getElementById('updateComponentTestValue').value = decodeURIComponent(row.getAttribute('data-value'));
            document.getElementById('updateComponentTestNotes').value = decodeURIComponent(row.getAttribute('data-notes'));
            document.getElementById('updateComponentTestMsg').textContent = '';
            document.getElementById('updateComponentTestModal').style.display = 'block';
        });
    });
}

// Render ARV regimens (array) with medications
function renderARVRegimens(regimens, medications) {
    const section = document.getElementById('arvRegimensSection');
    if (!regimens || regimens.length === 0) {
        section.innerHTML = `<div class="empty-state"><i class="fas fa-pills"></i><p>No ARV regimens found for this patient.</p></div>`;
        return;
    }

    let html = `
        <table class="regimens-table" style="width:100%;border-collapse:collapse;">
            <thead>
                <tr>
                    <th>Bậc</th>
                    <th>Trạng thái</th>
                    <th>Ngày bắt đầu</th>
                    <th>Ngày kết thúc</th>
                    <th>Ghi chú</th>
                    <th>Mở rộng/Đóng</th>
                </tr>
            </thead>
            <tbody>
    `;

    regimens.forEach((regimen, idx) => {
        const statusText = regimenStatusMap[regimen.regimenStatus] || regimen.regimenStatus;
        const levelText = regimenLevelMap[regimen.regimenLevel] || regimen.regimenLevel;
        const regimenMeds = (medications || []).filter(med => med.patientArvRegiId === regimen.patientArvRegiId);

        html += `
            <tr class="regimen-row" data-idx="${idx}" style="cursor:pointer;">
                <td>${levelText}</td>
                <td>${statusText}</td>
                <td>${regimen.startDate}</td>
                <td>${regimen.endDate || 'Đang áp dụng'}</td>
                <td>${regimen.notes ? regimen.notes : ''}</td>
                <td>
                    <button class="toggle-details-btn" data-idx="${idx}">▼</button>
                </td>
            </tr>
            <tr class="regimen-details-row" id="regimen-details-${idx}" style="display:none;background:#fafbfc;">
                <td colspan="6">
                    <div><strong>Ngày tạo:</strong> ${new Date(regimen.createdAt).toLocaleDateString()}</div>
                    <div><strong>Tổng chi phí:</strong> ${regimen.totalCost ? regimen.totalCost.toLocaleString('vi-VN') + ' VND' : 'Không xác định'}</div>
                    <h4>Thuốc</h4>
                    ${regimenMeds.length > 0 ? `
                        <table class="medications-table" style="width:100%;margin-top:10px;">
                            <thead>
                                <tr>
                                    <th>Tên thuốc</th>
                                    <th>Loại thuốc</th>
                                    <th>Liều lượng</th>
                                    <th>Số lượng</th>
                                    <th>Nhà sản xuất</th>
                                    <th>Cách sử dụng</th>
                                    <th>Giá</th>
                                    <th>Mô tả</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${regimenMeds.map(med => `
                                    <tr>
                                        <td>${med.medicationDetail.arvMedicationName}</td>
                                        <td>${med.medicationDetail.arvMedicationType || med.medicationDetail.medicationType || ''}</td>
                                        <td>${med.medicationDetail.arvMedicationDosage}</td>
                                        <td>${med.quantity}</td>
                                        <td>${med.medicationDetail.arvMedicationManufacturer}</td>
                                        <td>${med.usageInstructions || med.medicationDetail.medicationUsage || ''}</td>
                                        <td>${med.medicationDetail.arvMedicationPrice ? med.medicationDetail.arvMedicationPrice.toLocaleString() : 'N/A'}</td>
                                        <td>${med.medicationDetail.arvMedicationDescription}</td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    ` : `<div class='empty-state'><i class='fas fa-capsules'></i> No medications for this regimen.</div>`}
                    <div style="margin-top:1rem;text-align:right;">
                        ${(!window.isStaff && (regimen.regimenStatus !== 4 && regimen.regimenStatus !== 5)) ? `<button class="secondary-btn update-regimen-status-btn" data-id="${regimen.patientArvRegiId}" data-status="${regimen.regimenStatus}">Cập nhật trạng thái</button>` : ''}
                        ${(window.isDoctor && (regimen.regimenStatus !== 4 && regimen.regimenStatus !== 5)) ? `<button class="secondary-btn update-regimen-btn" data-id="${regimen.patientArvRegiId}">Cập nhật phác đồ</button>` : ''}
                    </div>
                </td>
            </tr>
        `;
    });

    html += `</tbody></table>`;
    section.innerHTML = html;

    // Toggle details on click
    document.querySelectorAll('.toggle-details-btn').forEach(btn => {
        btn.onclick = function(e) {
            e.stopPropagation();
            const idx = this.getAttribute('data-idx');
            const detailsRow = document.getElementById(`regimen-details-${idx}`);
            if (detailsRow.style.display === 'none') {
                detailsRow.style.display = '';
                this.textContent = '▲';
            } else {
                detailsRow.style.display = 'none';
                this.textContent = '▼';
            }
        };
    });

    // Add your update-regimen and update-status event listeners as before
    document.querySelectorAll('.update-regimen-status-btn').forEach(btn => {
        btn.onclick = function (e) {
            e.stopPropagation();
            const regimenId = this.getAttribute('data-id');
            const currentStatus = this.getAttribute('data-status');
            openUpdateRegimenStatusModal(regimenId, currentStatus);
        };
    });
    document.querySelectorAll('.update-regimen-btn').forEach(btn => {
        btn.onclick = async function (e) {
            e.stopPropagation();
            const regimenId = this.getAttribute('data-id');
            const regimen = regimens.find(r => String(r.patientArvRegiId) === String(regimenId));
            if (!regimen) return alert('Không tìm thấy phác đồ.');
            await loadMedicationDetails();
            regimenModal.style.display = 'block';
            // Change modal title for update
            const modalTitle = regimenModal.querySelector('h2');
            if (modalTitle) modalTitle.textContent = 'Cập nhật phác đồ ARV';
            const updateRegimenTemplate = document.getElementById('regimenTemplate');
            if (updateRegimenTemplate && updateRegimenTemplate.parentElement) {
                updateRegimenTemplate.parentElement.style.display = 'none';
                updateRegimenTemplate.removeAttribute('required');
                updateRegimenTemplate.value = '';
            }
            // Pre-fill modal fields
            regimenLevel.value = regimen.regimenLevel;
            regimenNotes.value = regimen.notes || '';
            regimenStartDate.value = regimen.startDate;
            // --- FIX: Pre-fill end date ---
            if (document.getElementById('regimenEndDate')) {
                document.getElementById('regimenEndDate').value = regimen.endDate || '';
            }
            // --- FIX: Pre-fill medications with usageInstructions ---
            selectedTemplateMedications = (regimen.arvMedications || []).map(med => ({
                arvMedicationName: med.medicationDetail.arvMedicationName,
                arvMedDetailId: med.medicationDetail.arvMedicationId,
                dosage: med.medicationDetail.arvMedicationDosage,
                quantity: med.quantity,
                manufacturer: med.medicationDetail.arvMedicationManufacturer,
                usageInstructions: (typeof med.usageInstructions === 'string' && med.usageInstructions.length > 0)
                    ? med.usageInstructions
                    : (med.medicationDetail.medicationUsage || '')
            }));
            renderMedicationRows();
            // Set update mode
            regimenForm.setAttribute('data-update-id', regimenId);

            // Change button text to "Update Regimen"
            const submitBtn = regimenForm.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.textContent = 'Cập nhật phác đồ';
            }
        };
    });
}

// Modal logic for update status
const updateRegimenStatusModal = document.getElementById('updateRegimenStatusModal');
const closeUpdateRegimenStatusModalBtn = document.getElementById('closeUpdateRegimenStatusModalBtn');
const updateRegimenStatusForm = document.getElementById('updateRegimenStatusForm');
const updateRegimenStatusSelect = document.getElementById('updateRegimenStatusSelect');
const updateRegimenStatusNotes = document.getElementById('updateRegimenStatusNotes');
const updateRegimenStatusId = document.getElementById('updateRegimenStatusId');
const updateRegimenStatusMsg = document.getElementById('updateRegimenStatusMsg');

function openUpdateRegimenStatusModal(regimenId, currentStatus) {
    updateRegimenStatusId.value = regimenId;
    updateRegimenStatusSelect.value = '';
    updateRegimenStatusNotes.value = '';
    updateRegimenStatusMsg.textContent = '';
    updateRegimenStatusModal.style.display = 'block';
}
closeUpdateRegimenStatusModalBtn.onclick = function () {
    updateRegimenStatusModal.style.display = 'none';
};
window.onclick = function (event) {
    if (event.target === updateRegimenStatusModal) {
        updateRegimenStatusModal.style.display = 'none';
    }
};

updateRegimenStatusForm.onsubmit = async function (e) {
    e.preventDefault();
    const regimenId = updateRegimenStatusId.value;
    const newStatus = +updateRegimenStatusSelect.value;
    const notes = updateRegimenStatusNotes.value.trim();
    if (!regimenId || !newStatus || !notes) {
        updateRegimenStatusMsg.textContent = 'Please fill all fields.';
        return;
    }
    // Enforce: Only one active regimen
    const section = document.getElementById('arvRegimensSection');
    const regimens = Array.from(section.querySelectorAll('.regimen-status')).map(el => el.textContent.trim());
    if (newStatus === 2 && regimens.includes('Active')) {
        updateRegimenStatusMsg.textContent = 'There is already an active regimen. Please pause/complete it first.';
        return;
    }
    try {
        const res = await fetch(`https://localhost:7009/api/PatientArvRegimen/UpdatePatientArvRegimenStatus/${regimenId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
            body: JSON.stringify({ notes, regimenStatus: newStatus })
        });
        if (!res.ok) {
            updateRegimenStatusMsg.textContent = 'Failed to update status.';
            return;
        }
        updateRegimenStatusModal.style.display = 'none';
        loadPatientData();
    } catch (err) {
        updateRegimenStatusMsg.textContent = 'Error updating status.';
    }
};

// --- Regimen Modal Logic ---
let allMedicationDetails = [];
let allTemplatesByLevel = {};
let selectedTemplateMedications = [];

// Modal open/close
const openRegimenModalBtn = document.getElementById('openRegimenModalBtn');
const regimenModal = document.getElementById('regimenModal');
const closeRegimenModalBtn = document.getElementById('closeRegimenModalBtn');
const cancelRegimenBtn = document.getElementById('cancelRegimenBtn');

// Prevent creating a new regimen if one is active
openRegimenModalBtn.onclick = async function () {
    // Check for active regimen
    const section = document.getElementById('arvRegimensSection');
    if (section && section.innerHTML.includes('regimen-active')) {
        alert('Cannot create a new regimen while one is active.');
        return;
    }
    await loadMedicationDetails();
    regimenModal.style.display = 'block';
    resetRegimenForm();
    // Set modal title and show template dropdown for create
    const modalTitle = regimenModal.querySelector('h2');
    if (modalTitle) modalTitle.textContent = 'Tạo phác đồ ARV mới';
    const createRegimenTemplate = document.getElementById('regimenTemplate');
    if (createRegimenTemplate && createRegimenTemplate.parentElement) {
        createRegimenTemplate.parentElement.style.display = '';
        createRegimenTemplate.setAttribute('required', 'required');
    }
    regimenForm.removeAttribute('data-update-id');
};
closeRegimenModalBtn.onclick = cancelRegimenBtn.onclick = function () {
    regimenModal.style.display = 'none';
    resetRegimenForm();
};
window.onclick = function (event) {
    if (event.target === regimenModal) {
        regimenModal.style.display = 'none';
        resetRegimenForm();
    }
};

// Fetch all medication details
async function loadMedicationDetails() {
    if (allMedicationDetails.length > 0) return;
    try {
        const res = await fetch('https://localhost:7009/api/ArvMedicationDetail/GetAllArvMedicationDetails', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (res.ok) {
            allMedicationDetails = await res.json();
        }
    } catch (e) { console.error('Failed to fetch medication details', e); }
}

// Fetch templates by level
async function fetchTemplatesByLevel(level) {
    if (allTemplatesByLevel[level]) return allTemplatesByLevel[level];
    try {
        const res = await fetch(`https://localhost:7009/api/RegimenTemplate/GetRegimenTemplatesByLevel?level=${level}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (res.ok) {
            const data = await res.json();
            allTemplatesByLevel[level] = data;
            return data;
        }
    } catch (e) { console.error('Failed to fetch templates', e); }
    return [];
}

// Regimen form logic
const regimenLevel = document.getElementById('regimenLevel');
const regimenTemplate = document.getElementById('regimenTemplate');
const regimenNotes = document.getElementById('regimenNotes');
const regimenStartDate = document.getElementById('regimenStartDate');
const medicationsTableBody = document.querySelector('#medicationsTable tbody');
const addMedicationBtn = document.getElementById('addMedicationBtn');
if (addMedicationBtn) {
    addMedicationBtn.onclick = function () {
        selectedTemplateMedications.push({
            arvMedicationName: '',
            arvMedDetailId: '',
            dosage: '',
            quantity: 1,
            manufacturer: '',
            usageInstructions: ''
        });
        renderMedicationRows();
    };
}
function updateAddMedicationBtnState() {
    if (addMedicationBtn) addMedicationBtn.disabled = false;
}
const regimenForm = document.getElementById('regimenForm');

regimenLevel.onchange = async function () {
    regimenTemplate.innerHTML = '<option value="">Chọn mẫu</option>';
    if (!this.value) return;
    const templates = await fetchTemplatesByLevel(this.value);
    templates.forEach(t => {
        const opt = document.createElement('option');
        opt.value = t.description;
        opt.textContent = t.description;
        opt.dataset.template = JSON.stringify(t);
        regimenTemplate.appendChild(opt);
    });
};
regimenTemplate.onchange = function () {
    if (!this.value) return;
    const selected = this.options[this.selectedIndex].dataset.template;
    if (!selected) return;
    const template = JSON.parse(selected);
    // Fill form fields
    regimenNotes.value = template.description;
    // Set start date to today in Vietnam timezone
   regimenStartDate.value = getVietnamToday();
    // Set end date to start date + duration days
    if (template.duration) {
        const endDate = new Date(vietnamToday.getTime() + template.duration * 24 * 60 * 60 * 1000);
        if (document.getElementById('regimenEndDate')) {
            document.getElementById('regimenEndDate').value = endDate.toISOString().slice(0, 10);
        }
    }
    // Fill medications: always set usageInstructions from medicationUsage
    selectedTemplateMedications = (template.medications || []).map(med => ({
        arvMedicationName: med.medicationName,
        arvMedDetailId: med.arvMedicationDetailId,
        dosage: med.dosage,
        quantity: med.quantity,
        manufacturer: '',
        usageInstructions: med.medicationUsage || ''
    }));
    renderMedicationRows();
};

function resetRegimenForm() {
    regimenForm.reset();
    medicationsTableBody.innerHTML = '';
    selectedTemplateMedications = [];

    // Reset button text to "Create Regimen"
    const submitBtn = regimenForm.querySelector('button[type="submit"]');
    if (submitBtn) {
        submitBtn.textContent = 'Tạo phác đồ';
    }
}

function renderMedicationRows() {
    medicationsTableBody.innerHTML = '';
    selectedTemplateMedications.forEach((med, idx) => {
        // Find medication detail by name
        let medDetail = null;
        if (med.arvMedicationName) {
            medDetail = allMedicationDetails.find(m => m.arvMedicationName === med.arvMedicationName);
        }
        // Use existing usageInstructions and quantity, or default
        let usageValue = (typeof med.usageInstructions === 'string' && med.usageInstructions.length > 0)
            ? med.usageInstructions
            : '';
        let quantityValue = med.quantity || 1;

        const row = document.createElement('tr');
        row.innerHTML = `
            <td>
                <select class="medication-name-select" data-idx="${idx}">
                    <option value="">Chọn</option>
                    ${allMedicationDetails.map(m =>
                        `<option value="${m.arvMedicationName}" ${m.arvMedicationName === med.arvMedicationName ? 'selected' : ''}>${m.arvMedicationName}</option>`
                    ).join('')}
                </select>
            </td>
            <td>${medDetail ? medDetail.arvMedicationDosage : ''}</td>
            <td><input type="number" min="1" value="${quantityValue}" class="medication-qty-input" data-idx="${idx}" style="width:70px;"></td>
            <td>${medDetail ? medDetail.arvMedicationManufacturer : ''}</td>
            <td><input type="text" class="medication-usage-input" data-idx="${idx}" value="${usageValue}" placeholder="Nhập cách sử dụng"></td>
            <td><button type="button" class="remove-med-btn" data-idx="${idx}"><i class="fas fa-trash"></i></button></td>
        `;
        medicationsTableBody.appendChild(row);
    });
    updateAddMedicationBtnState();
}

// Handle medication name/quantity/usage changes and remove
medicationsTableBody.onchange = function (e) {
    if (e.target.classList.contains('medication-name-select')) {
        const idx = +e.target.dataset.idx;
        const medName = e.target.value;
        const medDetail = allMedicationDetails.find(m => m.arvMedicationName === medName);
        if (medDetail) {
            selectedTemplateMedications[idx] = {
                arvMedicationName: medDetail.arvMedicationName,
                arvMedDetailId: medDetail.arvMedicationId,
                dosage: medDetail.arvMedicationDosage,
                quantity: selectedTemplateMedications[idx].quantity || 1,
                manufacturer: medDetail.arvMedicationManufacturer,
                usageInstructions: selectedTemplateMedications[idx].usageInstructions || ''
            };
        } else {
            selectedTemplateMedications[idx] = { arvMedicationName: '', arvMedDetailId: '', dosage: '', quantity: 1, manufacturer: '', usageInstructions: '' };
        }
        renderMedicationRows();
    } else if (e.target.classList.contains('medication-qty-input')) {
        const idx = +e.target.dataset.idx;
        selectedTemplateMedications[idx].quantity = +e.target.value;
    } else if (e.target.classList.contains('medication-usage-input')) {
        const idx = +e.target.dataset.idx;
        selectedTemplateMedications[idx].usageInstructions = e.target.value;
    }
};

// Regimen form submit
regimenForm.onsubmit = async function (e) {
    e.preventDefault();
    // Validation
    if (selectedTemplateMedications.length === 0) {
        alert('Vui lòng thêm ít nhất một loại thuốc.');
        return;
    }
    const medNames = selectedTemplateMedications.map(m => m.arvMedicationName);
    if (new Set(medNames).size !== medNames.length) {
        alert('Không được phép có thuốc trùng lặp.');
        return;
    }
    if (selectedTemplateMedications.some(m => !m.arvMedicationName || !m.quantity)) {
        alert('Please fill all medication fields.');
        return;
    }
    // New validation: usageInstructions must not be empty
    if (selectedTemplateMedications.some(m => !m.usageInstructions || !m.usageInstructions.trim())) {
        alert('Vui lòng nhập "Cách sử dụng" cho tất cả các thuốc.');
        return;
    }
    // Check that every medication selection is valid
    if (selectedTemplateMedications.some(m => {
        const medDetail = allMedicationDetails.find(md => md.arvMedicationName === m.arvMedicationName);
        return !medDetail;
    })) {
        alert('Vui lòng chọn thuốc hợp lệ cho mỗi dòng.');
        return;
    }
    // Prepare payloads
    const updateId = regimenForm.getAttribute('data-update-id');
    if (updateId) {
        // --- UPDATE MODE ---
        // Build medicationRequests array
        const medicationRequests = selectedTemplateMedications.map(med => {
            const medDetail = allMedicationDetails.find(md => md.arvMedicationName === med.arvMedicationName);
            let patientArvRegId = 0; // Default to 0 for new medications
            const regimen = (window._lastRegimens || []).find(r => String(r.patientArvRegiId) === String(updateId));
            if (regimen && regimen.arvMedications) {
                const match = regimen.arvMedications.find(m => m.medicationDetail.arvMedicationName === med.arvMedicationName);
                if (match) patientArvRegId = match.patientArvRegId || match.patientArvRegiId || match.patientArvRegId || 0;
            }
            return {
                patientArvRegId: patientArvRegId,
                arvMedDetailId: medDetail ? medDetail.arvMedicationId : med.arvMedDetailId,
                quantity: med.quantity,
                usageInstructions: (med.usageInstructions || '').trim()
            };
        });
        // Build regimenRequest object
        const regimen = (window._lastRegimens || []).find(r => String(r.patientArvRegiId) === String(updateId));
        const regimenRequest = {
            patientMedRecordId: window.pmrId, // Use the global pmrId from the patient's medical record
            notes: regimenNotes.value,
            regimenLevel: +regimenLevel.value,
            createdAt: getVietnamDateTime(),
            startDate: regimenStartDate.value,
            endDate: document.getElementById('regimenEndDate') ? document.getElementById('regimenEndDate').value : null,
            regimenStatus: 1,
            totalCost: 0
        };
        // Log the payload
        console.log('Submitting update payload:', { regimenRequest, medicationRequests });
        try {
            const res = await fetch(`https://localhost:7009/api/PatientArvRegimen/UpdatePatientArvRegimenWithMedications/${updateId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
                body: JSON.stringify({ regimenRequest, medicationRequests })
            });
            if (!res.ok) {
                let errorText = '';
                try { errorText = await res.text(); } catch { }
                alert('Cập nhật phác đồ thất bại. ' + errorText);
                return;
            }
            alert('Cập nhật phác đồ thành công!');
            regimenModal.style.display = 'none';
            regimenForm.removeAttribute('data-update-id');
            loadPatientData();
        } catch (err) {
            alert('Lỗi khi cập nhật phác đồ.');
        }
        return;
    }
    // --- CREATE MODE ---
    if (!window.pmrId) {
        alert('Không tìm thấy hồ sơ y tế của bệnh nhân.');
        return;
    }
    // Build medications array for API
    const medications = selectedTemplateMedications.map(med => {
        const medDetail = allMedicationDetails.find(md => md.arvMedicationName === med.arvMedicationName);
        return {
            patientArvRegId: 0,
            arvMedDetailId: medDetail.arvMedicationId, // Use arvMedicationId as arvMedDetailId
            quantity: med.quantity,
            usageInstructions: (med.usageInstructions || '').trim()
        };
    });
    // Build regimen object
    const regimen = {
        patientMedRecordId: window.pmrId,
        notes: regimenNotes.value,
        regimenLevel: +regimenLevel.value,
        createdAt: getVietnamDateTime(),
        startDate: regimenStartDate.value,
        endDate: document.getElementById('regimenEndDate') ? document.getElementById('regimenEndDate').value : null,
        regimenStatus: 1, // active
        totalCost: 0
    };
    const payload = { regimen, medications };
    // Log the payload for confirmation
    console.log('Submitting create payload:', payload);
    // Call new API
    try {
        const res = await fetch('https://localhost:7009/api/PatientArvRegimen/CreatePatientArvRegimenWithMedications', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
            body: JSON.stringify(payload)
        });
        if (!res.ok) {
            let errorMsg = 'Failed to create regimen.';
            let errorText = '';
            try {
                // Try to read as JSON
                const errorData = await res.json();
                console.error('Backend error:', errorData);
                if (errorData && errorData.message) errorMsg += '\n' + errorData.message;
                else errorText = JSON.stringify(errorData);
            } catch (e) {
                // If not JSON, try as text (but only once)
                try {
                    errorText = await res.text();
                } catch { }
            }
            if (errorText) errorMsg += '\n' + errorText;
            alert(errorMsg);
            return;
        }
        alert('Tạo phác đồ và thuốc thành công!');
        regimenModal.style.display = 'none';
        // Refresh data
        loadPatientData();
    } catch (err) {
        alert('Lỗi khi tạo phác đồ.');
        console.error('Error creating regimen:', err);
    }
};

// --- Create Test Result Modal Logic ---
document.addEventListener('DOMContentLoaded', function () {
    // Modal elements
    const openBtn = document.getElementById('openTestResultModalBtn');
    const modal = document.getElementById('testResultModal');
    const closeBtn = document.getElementById('closeTestResultModalBtn');
    const cancelBtn = document.getElementById('cancelTestResultBtn');
    const form = document.getElementById('testResultForm');
    const msgDiv = document.getElementById('testResultFormMsg');
    const dateInput = document.getElementById('testResultDate');
    const pmrIdInput = document.getElementById('testResultPatientMedicalRecordId');
    const componentTestsContainer = document.getElementById('componentTestsContainer');
    const addComponentBtn = document.getElementById('addComponentTestBtn');
    const resultSelect = document.getElementById('testResultSelect');

    // --- Component Test Fieldset Logic ---
    function createComponentTestFieldset(idx) {
        const fieldset = document.createElement('div');
        fieldset.className = 'component-test-fieldset';
        fieldset.style = 'border:1px solid #eee; padding:12px; margin-bottom:12px; border-radius:8px; position:relative;';
        fieldset.innerHTML = `
      <div class="form-group">
        <label>Tên thành phần <span style="color:red">*</span></label>
        <input type="text" name="componentTestResultName" required />
      </div>
      <div class="form-group">
        <label>Mô tả</label>
        <input type="text" name="ctrDescription" />
      </div>
      <div class="form-group">
        <label>Giá trị kết quả <span style="color:red">*</span></label>
        <input type="text" name="resultValue" required />
      </div>
      <div class="form-group">
        <label>Ghi chú <span style="color:red">*</span></label>
        <textarea name="notes" rows="2" required></textarea>
      </div>
      <button type="button" class="removeComponentBtn secondary-btn" style="position:absolute;top:8px;right:8px;">- Xóa</button>
    `;
        return fieldset;
    }

    function addComponentTestFieldset() {
        const idx = componentTestsContainer.querySelectorAll('.component-test-fieldset').length;
        const fieldset = createComponentTestFieldset(idx);
        componentTestsContainer.appendChild(fieldset);
        // Remove button logic
        fieldset.querySelector('.removeComponentBtn').onclick = function () {
            if (componentTestsContainer.querySelectorAll('.component-test-fieldset').length > 1) {
                fieldset.remove();
            }
        };
    }

    function resetComponentTests() {
        componentTestsContainer.innerHTML = '<h3>Thành phần xét nghiệm</h3>';
        addComponentTestFieldset();
    }

    // --- Modal Open/Close ---
    function openModal() {
        msgDiv.textContent = '';
        form.reset();
        resetComponentTests();
        // Set today as default date
        dateInput.value = getVietnamToday();
        // Set pmrId from global
        if (window.pmrId != null) pmrIdInput.value = window.pmrId;
        modal.style.display = 'block';
    }
    function closeModal() {
        modal.style.display = 'none';
        form.reset();
        resetComponentTests();
        msgDiv.textContent = '';
    }
    if (openBtn) openBtn.onclick = openModal;
    if (closeBtn) closeBtn.onclick = closeModal;
    if (cancelBtn) cancelBtn.onclick = closeModal;
    // Close modal on outside click
    window.onclick = function (event) {
        if (event.target === modal) closeModal();
    };

    // Add component test
    if (addComponentBtn) addComponentBtn.onclick = addComponentTestFieldset;

    // --- Form Submit ---
    form.onsubmit = async function (e) {
        e.preventDefault();
        msgDiv.textContent = '';
        // Validate required fields
        if (!pmrIdInput.value) {
            msgDiv.textContent = 'Không tìm thấy hồ sơ bệnh án.';
            return;
        }
        if (!dateInput.value) {
            msgDiv.textContent = 'Vui lòng chọn ngày xét nghiệm.';
            return;
        }
        const resultVal = resultSelect.value;
        if (!resultVal) {
            msgDiv.textContent = 'Vui lòng chọn kết quả.';
            return;
        }
        if (!form.testResultNotes.value.trim()) {
            msgDiv.textContent = 'Vui lòng nhập ghi chú cho kết quả xét nghiệm.';
            return;
        }
        // Component tests
        const componentFieldsets = componentTestsContainer.querySelectorAll('.component-test-fieldset');
        if (componentFieldsets.length === 0) {
            msgDiv.textContent = 'Cần ít nhất một thành phần xét nghiệm.';
            return;
        }
        const componentTests = [];
        for (const fs of componentFieldsets) {
            const name = fs.querySelector('input[name="componentTestResultName"]').value.trim();
            const desc = fs.querySelector('input[name="ctrDescription"]').value.trim();
            const value = fs.querySelector('input[name="resultValue"]').value.trim();
            const notes = fs.querySelector('textarea[name="notes"]').value.trim();
            if (!name) {
                msgDiv.textContent = 'Vui lòng nhập tên thành phần cho tất cả thành phần.';
                return;
            }
            if (!value) {
                msgDiv.textContent = 'Vui lòng nhập giá trị kết quả cho tất cả thành phần.';
                return;
            }
            if (!notes) {
                msgDiv.textContent = 'Vui lòng nhập ghi chú cho tất cả thành phần.';
                return;
            }
            componentTests.push({
                testResultId: 0, // Will be set by backend
                staffId: 0, // Optionally set if available
                componentTestResultName: name,
                ctrDescription: desc,
                resultValue: value,
                notes: notes
            });
        }
        // Build payload
        const payload = {
            testResult: {
                patientMedicalRecordId: Number(pmrIdInput.value),
                testDate: dateInput.value,
                result: resultVal === 'true',
                notes: form.testResultNotes.value.trim()
            },
            componentTests
        };
        // Submit
        try {
            const res = await fetch('https://localhost:7009/api/TestResult/CreateTestResultWithComponentTests', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });
            if (!res.ok) throw new Error('Tạo kết quả xét nghiệm thất bại.');
            closeModal();
            alert('Tạo kết quả xét nghiệm thành công!');
            // Reload test results (if you have a function for this)
            if (typeof loadPatientData === 'function') loadPatientData();
        } catch (err) {
            msgDiv.textContent = 'Lỗi khi tạo kết quả xét nghiệm.';
        }
    };
});

// --- Update Component Test Result Modal Logic ---
const updateComponentTestModal = document.getElementById('updateComponentTestModal');
const closeUpdateComponentTestModalBtn = document.getElementById('closeUpdateComponentTestModalBtn');
const cancelUpdateComponentTestBtn = document.getElementById('cancelUpdateComponentTestBtn');
const updateComponentTestForm = document.getElementById('updateComponentTestForm');
const updateComponentTestMsg = document.getElementById('updateComponentTestMsg');

function closeUpdateComponentTestModal() {
    updateComponentTestModal.style.display = 'none';
    updateComponentTestForm.reset();
    updateComponentTestMsg.textContent = '';
}
if (closeUpdateComponentTestModalBtn) closeUpdateComponentTestModalBtn.onclick = closeUpdateComponentTestModal;
if (cancelUpdateComponentTestBtn) cancelUpdateComponentTestBtn.onclick = closeUpdateComponentTestModal;
window.addEventListener('click', function (event) {
    if (event.target === updateComponentTestModal) closeUpdateComponentTestModal();
});

if (updateComponentTestForm) {
    updateComponentTestForm.onsubmit = async function (e) {
        e.preventDefault();
        updateComponentTestMsg.textContent = '';
        const compId = document.getElementById('updateComponentTestId').value;
        const name = document.getElementById('updateComponentTestName').value.trim();
        const desc = document.getElementById('updateComponentTestDesc').value.trim();
        const value = document.getElementById('updateComponentTestValue').value.trim();
        const notes = document.getElementById('updateComponentTestNotes').value.trim();
        if (!compId || !name || !value || !notes) {
            updateComponentTestMsg.textContent = 'Vui lòng điền đầy đủ các trường bắt buộc.';
            return;
        }
        try {
            const res = await fetch(`https://localhost:7009/api/ComponentTestResult/Update/${compId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    componentTestResultName: name,
                    ctrDescription: desc,
                    resultValue: value,
                    notes: notes
                })
            });
            if (!res.ok) throw new Error('Lỗi khi cập nhật thành phần xét nghiệm.');
            closeUpdateComponentTestModal();
            // Refresh test results
            if (typeof loadPatientData === 'function') loadPatientData();
        } catch (err) {
            updateComponentTestMsg.textContent = 'Lỗi khi cập nhật thành phần xét nghiệm.';
        }
    };
}

// --- Update Test Result Modal Logic ---
const updateTestResultModal = document.getElementById('updateTestResultModal');
const closeUpdateTestResultModalBtn = document.getElementById('closeUpdateTestResultModalBtn');
const cancelUpdateTestResultBtn = document.getElementById('cancelUpdateTestResultBtn');
const updateTestResultForm = document.getElementById('updateTestResultForm');
const updateTestResultMsg = document.getElementById('updateTestResultMsg');
const addUpdateComponentTestBtn = document.getElementById('addUpdateComponentTestBtn');

function closeUpdateTestResultModal() {
    updateTestResultModal.style.display = 'none';
    updateTestResultForm.reset();
    updateTestResultMsg.textContent = '';
    // Clear component tests container
    const container = document.getElementById('updateComponentTestsContainer');
    if (container) {
        container.innerHTML = '<h3>Thành phần xét nghiệm</h3>';
    }
}

if (closeUpdateTestResultModalBtn) closeUpdateTestResultModalBtn.onclick = closeUpdateTestResultModal;
if (cancelUpdateTestResultBtn) cancelUpdateTestResultBtn.onclick = closeUpdateTestResultModal;
window.addEventListener('click', function (event) {
    if (event.target === updateTestResultModal) closeUpdateTestResultModal();
});

// Load component test results for update modal from existing data
function loadComponentTestResultsFromData(componentResults) {
    try {
        // Clear existing component tests
        const container = document.getElementById('updateComponentTestsContainer');
        container.innerHTML = '<h3>Thành phần xét nghiệm</h3>';

        // Add existing component test results
        componentResults.forEach((comp, idx) => {
            const fieldset = document.createElement('div');
            fieldset.className = 'component-test-fieldset';
            fieldset.style = 'border:1px solid #eee; padding:12px; margin-bottom:12px; border-radius:8px; position:relative;';
            fieldset.innerHTML = `
                <input type="hidden" name="componentTestResultId" value="${comp.componentTestResultId}" />
                <div class="form-group">
                    <label>Tên thành phần <span style="color:red">*</span></label>
                    <input type="text" name="componentTestResultName" value="${comp.componentTestResultName || ''}" required />
                </div>
                <div class="form-group">
                    <label>Mô tả</label>
                    <input type="text" name="ctrDescription" value="${comp.ctrDescription || ''}" />
                </div>
                <div class="form-group">
                    <label>Giá trị kết quả <span style="color:red">*</span></label>
                    <input type="text" name="resultValue" value="${comp.resultValue || ''}" required />
                </div>
                <div class="form-group">
                    <label>Ghi chú <span style="color:red">*</span></label>
                    <textarea name="notes" rows="2" required>${comp.notes || ''}</textarea>
                </div>
                <button type="button" class="removeComponentBtn secondary-btn" style="position:absolute;top:8px;right:8px;">- Xóa</button>
            `;
            container.appendChild(fieldset);

            // Add remove button functionality
            fieldset.querySelector('.removeComponentBtn').onclick = function () {
                if (container.querySelectorAll('.component-test-fieldset').length > 1) {
                    fieldset.remove();
                }
            };
        });

        // Always add at least one empty component test fieldset if none exist
        if (componentResults.length === 0) {
            addUpdateComponentTestFieldset();
        }

    } catch (err) {
        console.error('Error in loadComponentTestResultsFromData:', err);
        // Add at least one empty fieldset if there's an error
        const container = document.getElementById('updateComponentTestsContainer');
        if (container && container.querySelectorAll('.component-test-fieldset').length === 0) {
            addUpdateComponentTestFieldset();
        }
    }
}

// Add new component test fieldset for update modal
function addUpdateComponentTestFieldset() {
    const container = document.getElementById('updateComponentTestsContainer');
    const idx = container.querySelectorAll('.component-test-fieldset').length;
    const fieldset = document.createElement('div');
    fieldset.className = 'component-test-fieldset';
    fieldset.style = 'border:1px solid #eee; padding:12px; margin-bottom:12px; border-radius:8px; position:relative;';
    fieldset.innerHTML = `
        <div class="form-group">
            <label>Tên thành phần <span style="color:red">*</span></label>
            <input type="text" name="componentTestResultName" required />
        </div>
        <div class="form-group">
            <label>Mô tả</label>
            <input type="text" name="ctrDescription" />
        </div>
        <div class="form-group">
            <label>Giá trị kết quả <span style="color:red">*</span></label>
            <input type="text" name="resultValue" required />
        </div>
        <div class="form-group">
            <label>Ghi chú <span style="color:red">*</span></label>
            <textarea name="notes" rows="2" required></textarea>
        </div>
        <button type="button" class="removeComponentBtn secondary-btn" style="position:absolute;top:8px;right:8px;">- Xóa</button>
    `;
    container.appendChild(fieldset);

    // Remove button logic
    fieldset.querySelector('.removeComponentBtn').onclick = function () {
        if (container.querySelectorAll('.component-test-fieldset').length > 1) {
            fieldset.remove();
        }
    };
}

if (addUpdateComponentTestBtn) {
    addUpdateComponentTestBtn.onclick = addUpdateComponentTestFieldset;
}

// Form submission with the new API
if (updateTestResultForm) {
    updateTestResultForm.onsubmit = async function (e) {
        e.preventDefault();
        updateTestResultMsg.textContent = '';

        const testResultId = document.getElementById('updateTestResultId').value;
        const testDate = document.getElementById('updateTestResultDate').value;
        const result = document.getElementById('updateTestResultSelect').value;
        const notes = document.getElementById('updateTestResultNotes').value.trim();

        if (!testResultId || !testDate || !result || !notes) {
            updateTestResultMsg.textContent = 'Vui lòng điền đầy đủ các trường bắt buộc.';
            return;
        }

        // Collect component test data
        const componentFieldsets = document.querySelectorAll('#updateComponentTestsContainer .component-test-fieldset');
        const componentTests = [];

        for (const fieldset of componentFieldsets) {
            const name = fieldset.querySelector('input[name="componentTestResultName"]')?.value.trim();
            const desc = fieldset.querySelector('input[name="ctrDescription"]')?.value.trim();
            const value = fieldset.querySelector('input[name="resultValue"]')?.value.trim();
            const compNotes = fieldset.querySelector('textarea[name="notes"]')?.value.trim();

            if (!name || !value || !compNotes) {
                updateTestResultMsg.textContent = 'Vui lòng điền đầy đủ thông tin thành phần xét nghiệm.';
                return;
            }

            componentTests.push({
                testResultId: 0, // Backend will handle this
                staffId: 0, // Backend will handle this
                componentTestResultName: name,
                ctrDescription: desc,
                resultValue: value,
                notes: compNotes
            });
        }

        // Build payload according to the API specification
        const payload = {
            testResult: {
                patientMedicalRecordId: window.pmrId, // Use the global pmrId
                testDate: testDate,
                result: result === 'true',
                notes: notes
            },
            componentTests: componentTests
        };

        try {
            const res = await fetch(`https://localhost:7009/api/TestResult/UpdateTestResultWithComponentTests/${testResultId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                let errorText = '';
                try { errorText = await res.text(); } catch { }
                updateTestResultMsg.textContent = 'Cập nhật kết quả xét nghiệm thất bại. ' + errorText;
                return;
            }

            alert('Cập nhật kết quả xét nghiệm thành công!');
            closeUpdateTestResultModal();
            // Refresh test results
            if (typeof loadPatientData === 'function') loadPatientData();

        } catch (err) {
            updateTestResultMsg.textContent = 'Lỗi khi cập nhật kết quả xét nghiệm: ' + err.message;
        }
    };
}

// Render payments
function renderPayments(payments) {
    const section = document.getElementById('paymentsSection');
    if (!section) return;
    if (!payments || payments.length === 0) {
        section.innerHTML = `<div class="empty-state"><i class="fas fa-money-bill-wave"></i><p>Không có giao dịch thanh toán nào.</p></div>`;
        return;
    }
    let html = `<table class="payments-table"><thead><tr><th>Ngày thanh toán</th><th>Số tiền</th><th>Phương thức</th><th>Trạng thái</th><th>Mô tả</th></tr></thead><tbody>`;
    payments.forEach(pay => {
        html += `<tr>
            <td>${pay.paymentDate ? new Date(pay.paymentDate).toLocaleString() : '-'}</td>
            <td>${pay.amount ? pay.amount.toLocaleString() + ' ₫' : '-'}</td>
            <td>${pay.paymentMethod || '-'}</td>
            <td><span class="payment-status payment-status-${pay.paymentStatus}">${paymentStatusMap[pay.paymentStatus] || pay.paymentStatus}</span></td>
            <td>${pay.description || '-'}</td>
        </tr>`;
    });
    html += `</tbody></table>`;
    section.innerHTML = html;
}

// Main function to load all patient data
async function loadPatientData() {
    const patientId = getPatientIdFromUrl();

    if (!patientId) {
        document.body.innerHTML = `
            <div class="error-state">
                <i class="fas fa-exclamation-triangle"></i>
                <p>No patient ID provided in URL.</p>
            </div>
        `;
        return;
    }

    try {
        // Fetch patient details
        const patient = await fetchPatientDetails(patientId);
        renderPatientProfile(patient);
        // Fetch all medical data (appointments, test results, arv regimens, payments)
        const medicalData = await fetchPatientMedicalDataByPatientId(patientId);
        // Store pmrId globally for modal logic
        if (medicalData && medicalData.pmrId != null) {
            window.pmrId = medicalData.pmrId;
        } else {
            window.pmrId = null;
        }
        // Fetch all ARV medications for the patient (if needed for regimens)
        let medications = [];
        if (medicalData && medicalData.arvRegimens) {
            medications = medicalData.arvRegimens.flatMap(r => r.arvMedications || []);
        }

        if (medicalData) {
            renderAppointments(medicalData.appointments || []);
            renderTestResults(medicalData.testResults || []);
            renderARVRegimens(medicalData.arvRegimens || [], medications);
            renderPayments(medicalData.payments || []);

            allRegimens = medicalData.arvRegimens || [];
allRegimenMedications = medications;
applyRegimenFilters();

allTestResults = medicalData.testResults || [];
applyTestResultFilters();
        } else {
            renderAppointments([]);
            renderTestResults([]);
            renderARVRegimens([], []);
            renderPayments([]);
        }
        // After data is loaded, show/hide staff button
        const btnContainer = document.getElementById('createTestResultContainer');
        if (btnContainer) {
            if (window.isStaff && window.pmrId != null) {
                btnContainer.style.display = '';
            } else {
                btnContainer.style.display = 'none';
            }

        }
    } catch (error) {
        console.error('Error loading patient data:', error);
        document.body.innerHTML = `
            <div class="error-state">
                <i class="fas fa-exclamation-triangle"></i>
<p>Error loading patient data. Please try again.</p>
            </div>
        `;
    }
}

// Initialize page when DOM is loaded
window.addEventListener('DOMContentLoaded', loadPatientData);

// Payment Creation Modal Logic
document.addEventListener('DOMContentLoaded', async function () {
    // Show payment creation button for doctors only
    if (window.isDoctor) {
        document.getElementById('createPaymentContainer').style.display = '';
    }

    // Load services for payment creation
    let allServices = [];
    try {
        allServices = await fetchAllServices();
        const serviceSelect = document.getElementById('paymentServiceSelect');
        if (serviceSelect && allServices.length > 0) {
            allServices.forEach(service => {
                // Only show available services
                if (service.isAvailable) {
                    const option = document.createElement('option');
                    option.value = service.serviceId;
                    option.textContent = `${service.serviceName} - ${service.price ? service.price.toLocaleString() : 'N/A'} VND`;
                    option.dataset.price = service.price || 0;
                    option.dataset.description = service.serviceDescription || '';
                    serviceSelect.appendChild(option);
                }
            });
        }
    } catch (error) {
        console.error('Error loading services:', error);
    }

    // Modal elements
    const openPaymentBtn = document.getElementById('openPaymentModalBtn');
    const paymentModal = document.getElementById('paymentModal');
    const closePaymentBtn = document.getElementById('closePaymentModalBtn');
    const cancelPaymentBtn = document.getElementById('cancelPaymentBtn');
    const paymentForm = document.getElementById('paymentForm');
    const paymentFormMsg = document.getElementById('paymentFormMsg');
    const serviceSelect = document.getElementById('paymentServiceSelect');
    const amountInput = document.getElementById('paymentAmount');
    const pmrIdInput = document.getElementById('paymentPmrId');
    const serviceDescription = document.getElementById('serviceDescription');
    const descriptionTextarea = document.getElementById('paymentDescription');

    // Update amount and description when service is selected
    if (serviceSelect && amountInput) {
        serviceSelect.addEventListener('change', function () {
            const selectedOption = this.options[this.selectedIndex];
            if (selectedOption && selectedOption.value) {
                // Update amount
                if (selectedOption.dataset.price) {
                    amountInput.value = selectedOption.dataset.price;
                }
                // Show service description
                if (selectedOption.dataset.description) {
                    serviceDescription.textContent = selectedOption.dataset.description;
                    serviceDescription.style.display = 'block';
                } else {
                    serviceDescription.style.display = 'none';
                }
                // Auto-fill payment description
                if (descriptionTextarea) {
                    const serviceName = selectedOption.textContent.split(' - ')[0];
                    descriptionTextarea.value = `Thanh toán cho dịch vụ: ${serviceName}`;
                }
            } else {
                // Hide service description when no service selected
                serviceDescription.style.display = 'none';
                if (descriptionTextarea) {
                    descriptionTextarea.value = '';
                }
            }
        });
    }

    // Modal open/close logic
    function openPaymentModal() {
        paymentFormMsg.textContent = '';
        paymentForm.reset();
        // Set default values
        document.getElementById('paymentAmount').value = '50000';
        document.getElementById('paymentCurrency').value = 'VND';

        // Set pmrId from global window.pmrId (set when page loads)
            if (patientId) {
                fetchPatientMedicalDataByPatientId(patientId).then(medicalData => {
                    if (medicalData && medicalData.pmrId) {
                        pmrIdInput.value = medicalData.pmrId;
                        window.pmrId = medicalData.pmrId; // Store globally for future use
                    }
                });
            }

        paymentModal.style.display = 'block';
    }

    function closePaymentModal() {
        paymentModal.style.display = 'none';
        paymentForm.reset();
        paymentFormMsg.textContent = '';
    }

    // Event listeners
    if (openPaymentBtn) openPaymentBtn.addEventListener('click', openPaymentModal);
    if (closePaymentBtn) closePaymentBtn.addEventListener('click', closePaymentModal);
    if (cancelPaymentBtn) cancelPaymentBtn.addEventListener('click', closePaymentModal);

    // Close modal on outside click
    window.addEventListener('click', function (event) {
        if (event.target === paymentModal) {
            closePaymentModal();
        }
    });

    // Form submission
    if (paymentForm) {
        paymentForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            paymentFormMsg.textContent = '';

            // Validate form
            const formData = new FormData(paymentForm);
            const pmrId = parseInt(formData.get('pmrId'));
            const srvId = parseInt(formData.get('serviceId'));
            const amount = parseFloat(formData.get('amount'));
            const currency = formData.get('currency');
            const paymentMethod = formData.get('paymentMethod');
            const description = formData.get('description');

            if (!pmrId || !srvId || !amount || !currency || !paymentMethod || !description) {
                paymentFormMsg.textContent = 'Vui lòng điền đầy đủ tất cả các trường.';
                return;
            }

            // Create payment object
            const paymentData = {
                pmrId: pmrId,
                srvId: srvId,
                amount: amount,
                currency: currency,
                paymentMethod: paymentMethod,
                description: description
            };

            try {
                await createPayment(paymentData);
                closePaymentModal();
                alert('Tạo thanh toán thành công!');
                // Reload patient data to refresh payment history
                loadPatientData();
            } catch (error) {
                paymentFormMsg.textContent = 'Lỗi khi tạo thanh toán. Vui lòng thử lại.';
            }
        });
    }
});

// Filter bars
function insertRegimenFilterBar() {
    const section = document.getElementById('arvRegimensSection');
    if (section && !document.getElementById('arvRegimenFilterBar')) {
        const filterDiv = document.createElement('div');
        filterDiv.id = 'arvRegimenFilterBar';
        filterDiv.className = 'filter-bar';
        filterDiv.innerHTML = `
            <label for="filterRegimenStatus">Trạng thái:</label>
            <select id="filterRegimenStatus">
                <option value="">Tất cả</option>
                <option value="1">Đã lên kế hoạch</option>
                <option value="2">Đang hoạt động</option>
                <option value="3">Tạm dừng</option>
                <option value="4">Đã hủy</option>
                <option value="5">Hoàn thành</option>
            </select>
            <label for="filterRegimenDate">Ngày tạo:</label>
            <input type="date" id="filterRegimenDate">
            <label for="sortRegimenDate">Sắp xếp ngày:</label>
            <select id="sortRegimenDate">
                <option value="desc">Mới nhất</option>
                <option value="asc">Cũ nhất</option>
            </select>
            <button id="clearRegimenFilters">Xóa lọc</button>
        `;
        section.parentNode.insertBefore(filterDiv, section);
    }
}

function insertTestResultFilterBar() {
    const section = document.getElementById('testResultsSection');
    if (section && !document.getElementById('testResultFilterBar')) {
        const filterDiv = document.createElement('div');
        filterDiv.id = 'testResultFilterBar';
        filterDiv.className = 'filter-bar';
        filterDiv.innerHTML = `
            <label for="filterTestResult">Kết quả:</label>
            <select id="filterTestResult">
                <option value="">Tất cả</option>
                <option value="true">Dương tính</option>
                <option value="false">Âm tính</option>
            </select>
            <label for="filterTestDate">Ngày xét nghiệm:</label>
            <input type="date" id="filterTestDate">
            <label for="sortTestDate">Sắp xếp ngày:</label>
            <select id="sortTestDate">
                <option value="desc">Mới nhất</option>
                <option value="asc">Cũ nhất</option>
            </select>
            <button id="clearTestFilters">Xóa lọc</button>
        `;
        section.parentNode.insertBefore(filterDiv, section);
    }
}

// Insert filter bars FIRST
insertRegimenFilterBar();
insertTestResultFilterBar();

// THEN set event listeners and call filter functions
const clearRegimenBtn = document.getElementById('clearRegimenFilters');
if (clearRegimenBtn) {
    clearRegimenBtn.onclick = function () {
        document.getElementById('filterRegimenStatus').value = '';
        document.getElementById('filterRegimenDate').value = '';
        document.getElementById('sortRegimenDate').value = 'desc';
        applyRegimenFilters();
    };
}
document.getElementById('sortRegimenDate').addEventListener('change', applyRegimenFilters);

document.getElementById('clearTestFilters').onclick = function () {
    document.getElementById('filterTestResult').value = '';
    document.getElementById('filterTestDate').value = '';
    document.getElementById('sortTestDate').value = 'desc';
    applyTestResultFilters();
};

// --- Filter event listeners ---
document.getElementById('filterRegimenStatus').addEventListener('change', applyRegimenFilters);
document.getElementById('filterRegimenDate').addEventListener('change', applyRegimenFilters);
document.getElementById('sortRegimenDate').addEventListener('change', applyRegimenFilters);

document.getElementById('filterTestResult').addEventListener('change', applyTestResultFilters);
document.getElementById('filterTestDate').addEventListener('change', applyTestResultFilters);
document.getElementById('sortTestDate').addEventListener('change', applyTestResultFilters);

applyRegimenFilters();
applyTestResultFilters();

// Filter functions
function applyRegimenFilters() {
    const status = document.getElementById('filterRegimenStatus')?.value;
    const date = document.getElementById('filterRegimenDate')?.value;
    const sortOrder = document.getElementById('sortRegimenDate')?.value;

    let filtered = allRegimens.filter(r =>
        (!status || String(r.regimenStatus) === status) &&
        (!date || (r.createdAt && r.createdAt.slice(0, 10) === date))
    );

    filtered.sort((a, b) => {
        const dateA = new Date(a.createdAt);
        const dateB = new Date(b.createdAt);
        return sortOrder === 'asc' ? dateA - dateB : dateB - dateA;
    });

    renderARVRegimens(filtered, allRegimenMedications);
}

function applyTestResultFilters() {
    const result = document.getElementById('filterTestResult')?.value;
    const date = document.getElementById('filterTestDate')?.value;
    const sortOrder = document.getElementById('sortTestDate')?.value;

    let filtered = allTestResults.filter(tr =>
        (!result || String(tr.result) === result) &&
        (!date || (tr.testDate && tr.testDate.slice(0, 10) === date))
    );

    filtered.sort((a, b) => {
        const dateA = new Date(a.testDate);
        const dateB = new Date(b.testDate);
        return sortOrder === 'asc' ? dateA - dateB : dateB - dateA;
    });

    renderTestResults(filtered);
}
function updateAddMedicationBtnState() {
    // No-op: implement logic if needed to enable/disable the add button
}
// Handle remove medication button
medicationsTableBody.onclick = function (e) {
    if (e.target.closest('.remove-med-btn')) {
        const idx = +e.target.closest('.remove-med-btn').dataset.idx;
        selectedTemplateMedications.splice(idx, 1);
        renderMedicationRows();
    }
};

function getVietnamToday() {
    const now = new Date();
    // Vietnam is UTC+7
    const vietnamOffset = 7 * 60; // phút
    const localOffset = now.getTimezoneOffset(); // phút
    const diff = vietnamOffset + localOffset;
    const vietnamDate = new Date(now.getTime() + diff * 60 * 1000);
    return vietnamDate.toISOString().slice(0, 10);
}