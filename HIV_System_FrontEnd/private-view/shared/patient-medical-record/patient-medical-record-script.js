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
const paymentStatusMap = {
  1: "Chờ xử lý",
  2: "Thành công",
  3: "Thất bại"
};

// Set window.isStaff and window.isDoctor globally
window.isStaff = false;
window.isDoctor = false;
if (window.roleUtils && window.roleUtils.getUserRole && window.roleUtils.ROLE_NAMES) {
  const roleId = window.roleUtils.getUserRole();
  const roleName = window.roleUtils.ROLE_NAMES[roleId];
  window.isStaff = (roleName === 'staff');
  window.isDoctor = (roleName === 'doctor');
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
        if (!response.ok) throw new Error('Lỗi thất bại lấy thông tin bệnh án bệnh nhân');
        return await response.json();
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
                <td>${appt.apmtDate}</td>
                <td>${appt.apmTime ? appt.apmTime.slice(0, 5) : '-'}</td>
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

    let html = '';
    testResults.forEach(testResult => {
        const resultClass = testResult.result ? 'test-result-positive' : 'test-result-negative';
        const resultText = testResult.result ? 'Positive' : 'Negative';

        html += `
            <div class="test-result-card">
                <div class="test-result-header">
                    <span class="test-result-date">Ngày xét nghiệm: ${testResult.testDate}</span>
                    <span class="test-result-overall ${resultClass}">${resultText}</span>
                </div>
        `;

        if (testResult.notes) {
            html += `<p><strong>Ghi chú:</strong> ${testResult.notes}</p>`;
        }

        if (testResult.componentTestResults && testResult.componentTestResults.length > 0) {
            html += `<div class="component-results">`;
            testResult.componentTestResults.forEach(comp => {
                // Add clickable class and data-id for staff
                const clickable = window.isStaff ? 'clickable-component-result' : '';
                const dataId = window.isStaff ? `data-id="${comp.componentTestResultId}"` : '';
                html += `
                    <div class="component-result ${clickable}" ${dataId} style="${window.isStaff ? 'cursor:pointer;' : ''}">
                        <div class="component-name">${comp.componentTestResultName}</div>
                        <div class="component-value">${comp.resultValue}</div>
                        <div class="component-notes">${comp.notes}</div>
                    </div>
                `;
            });
            html += `</div>`;
        }

        html += `</div>`;
    });

    section.innerHTML = html;

    // Add click event listeners for staff to open update modal
    if (window.isStaff) {
      section.querySelectorAll('.clickable-component-result').forEach(el => {
        el.addEventListener('click', async function() {
          const compId = this.getAttribute('data-id');
          if (!compId) return;
          // Fetch component test result data
          try {
            const res = await fetch(`https://localhost:7009/api/ComponentTestResult/GetById/${compId}`, {
              headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) throw new Error('Lỗi khi lấy dữ liệu thành phần xét nghiệm.');
            const data = await res.json();
            // Populate modal fields
            document.getElementById('updateComponentTestId').value = data.componentTestResultId;
            document.getElementById('updateComponentTestName').value = data.componentTestResultName || '';
            document.getElementById('updateComponentTestDesc').value = data.ctrDescription || '';
            document.getElementById('updateComponentTestValue').value = data.resultValue || '';
            document.getElementById('updateComponentTestNotes').value = data.notes || '';
            document.getElementById('updateComponentTestMsg').textContent = '';
            // Show modal
            document.getElementById('updateComponentTestModal').style.display = 'block';
          } catch (err) {
            alert('Không thể tải dữ liệu thành phần xét nghiệm.');
          }
        });
      });
    }
}

// Render ARV regimens (array) with medications
function renderARVRegimens(regimens, medications) {
    const section = document.getElementById('arvRegimensSection');
    
    if (!regimens || regimens.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-pills"></i>
                <p>No ARV regimens found for this patient.</p>
            </div>
        `;
        return;
    }

    // Check if any regimen is active
    const hasActiveRegimen = regimens.some(r => r.regimenStatus === 2);
    let html = '';
    regimens.forEach(regimen => {
        let statusClass = '';
        switch (regimen.regimenStatus) {
            case 1:
                statusClass = 'regimen-planned';
                break;
            case 2:
                statusClass = 'regimen-active';
                break;
            case 3:
                statusClass = 'regimen-paused';
                break;
            case 4:
                statusClass = 'regimen-failed';
                break;
            case 5:
                statusClass = 'regimen-completed';
                break;
            default:
                statusClass = 'regimen-inactive';
        }
        const statusText = regimenStatusMap[regimen.regimenStatus] || regimen.regimenStatus;
        const levelText = regimenLevelMap[regimen.regimenLevel] || regimen.regimenLevel;
        // Filter medications for this regimen
        const regimenMeds = (medications || []).filter(med => med.patientArvRegiId === regimen.patientArvRegiId);
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
                        <div class="regimen-detail-label">Created At</div>
                        <div class="regimen-detail-value">${new Date(regimen.createdAt).toLocaleDateString()}</div>
                    </div>
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">Tổng chi phí</div>
                        <div class="regimen-detail-value">${regimen.totalCost ? regimen.totalCost.toLocaleString('vi-VN') + ' VND' : 'Không xác định'}</div>
                    </div>
                </div>
                ${regimen.notes ? `
                    <div class="regimen-notes">
                        <strong>Notes:</strong> ${regimen.notes}
                    </div>
                ` : ''}
                <div class="regimen-medications">
                    <h4>Medications</h4>
                    ${regimenMeds.length > 0 ? `
                        <table class="medications-table">
                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>Dosage</th>
                                    <th>Quantity</th>
                                    <th>Price</th>
                                    <th>Manufacturer</th>
                                    <th>Description</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${regimenMeds.map(med => `
                                    <tr>
                                        <td>${med.medicationDetail.arvMedicationName}</td>
                                        <td>${med.medicationDetail.arvMedicationDosage}</td>
                                        <td>${med.quantity}</td>
                                        <td>${med.medicationDetail.arvMedicationPrice ? med.medicationDetail.arvMedicationPrice.toLocaleString() : 'N/A'}</td>
                                        <td>${med.medicationDetail.arvMedicationManufacturer}</td>
                                        <td>${med.medicationDetail.arvMedicationDescription}</td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    ` : `<div class='empty-state'><i class='fas fa-capsules'></i> No medications for this regimen.</div>`}
                </div>
                <div style="margin-top:1rem;text-align:right;">
                    ${(!window.isStaff && (regimen.regimenStatus !== 4 && regimen.regimenStatus !== 5)) ? `<button class="secondary-btn update-regimen-status-btn" data-id="${regimen.patientArvRegiId}" data-status="${regimen.regimenStatus}">Update Status</button>` : ''}
                </div>
            </div>
        `;
    });
    
    section.innerHTML = html;
    // Add event listeners for update status buttons
    document.querySelectorAll('.update-regimen-status-btn').forEach(btn => {
        btn.onclick = function() {
            const regimenId = this.getAttribute('data-id');
            const currentStatus = this.getAttribute('data-status');
            openUpdateRegimenStatusModal(regimenId, currentStatus);
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
closeUpdateRegimenStatusModalBtn.onclick = function() {
    updateRegimenStatusModal.style.display = 'none';
};
window.onclick = function(event) {
    if (event.target === updateRegimenStatusModal) {
        updateRegimenStatusModal.style.display = 'none';
    }
};

updateRegimenStatusForm.onsubmit = async function(e) {
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
openRegimenModalBtn.onclick = async function() {
    // Check for active regimen
    const section = document.getElementById('arvRegimensSection');
    if (section && section.innerHTML.includes('regimen-active')) {
        alert('Cannot create a new regimen while one is active.');
        return;
    }
    regimenModal.style.display = 'block';
    await loadMedicationDetails();
    resetRegimenForm();
};
closeRegimenModalBtn.onclick = cancelRegimenBtn.onclick = function() {
    regimenModal.style.display = 'none';
};
window.onclick = function(event) {
    if (event.target === regimenModal) {
        regimenModal.style.display = 'none';
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
const regimenForm = document.getElementById('regimenForm');

regimenLevel.onchange = async function() {
    regimenTemplate.innerHTML = '<option value="">Select template</option>';
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

regimenTemplate.onchange = function() {
    if (!this.value) return;
    const selected = this.options[this.selectedIndex].dataset.template;
    if (!selected) return;
    const template = JSON.parse(selected);
    // Fill form fields
    regimenNotes.value = template.description;
    // Set start date to today
    const today = new Date();
    regimenStartDate.value = today.toISOString().slice(0, 10);
    // Set end date to start date + duration days
    if (template.duration) {
        const endDate = new Date(today.getTime() + template.duration * 24 * 60 * 60 * 1000);
        if (document.getElementById('regimenEndDate')) {
            document.getElementById('regimenEndDate').value = endDate.toISOString().slice(0, 10);
        }
    }
    // Fill medications
    selectedTemplateMedications = template.medications.map(med => ({
        arvMedicationName: med.medicationName,
        dosage: med.dosage,
        quantity: med.quantity,
        manufacturer: ''
    }));
    renderMedicationRows();
};

function resetRegimenForm() {
    regimenForm.reset();
    medicationsTableBody.innerHTML = '';
    selectedTemplateMedications = [];
}

function renderMedicationRows() {
    medicationsTableBody.innerHTML = '';
    selectedTemplateMedications.forEach((med, idx) => {
        const medDetail = allMedicationDetails.find(m => m.arvMedicationName === med.arvMedicationName);
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>
                <select class="medication-name-select" data-idx="${idx}">
                    <option value="">Select</option>
                    ${allMedicationDetails.map(m => `<option value="${m.arvMedicationId}" ${m.arvMedicationName === med.arvMedicationName ? 'selected' : ''}>${m.arvMedicationName}</option>`).join('')}
                </select>
            </td>
            <td>${medDetail ? medDetail.arvMedicationDosage : med.dosage || ''}</td>
            <td><input type="number" min="1" value="${med.quantity || ''}" class="medication-qty-input" data-idx="${idx}" style="width:70px;"></td>
            <td>${medDetail ? medDetail.arvMedicationManufacturer : med.manufacturer || ''}</td>
            <td><button type="button" class="remove-med-btn" data-idx="${idx}"><i class="fas fa-trash"></i></button></td>
        `;
        medicationsTableBody.appendChild(row);
    });
    updateAddMedicationBtnState();
}

addMedicationBtn.onclick = function() {
    selectedTemplateMedications.push({ arvMedicationName: '', dosage: '', quantity: 1, manufacturer: '' });
    renderMedicationRows();
};

function updateAddMedicationBtnState() {
    addMedicationBtn.disabled = false;
}

// Handle medication name/quantity changes and remove
medicationsTableBody.onclick = function(e) {
    if (e.target.closest('.remove-med-btn')) {
        const idx = +e.target.closest('.remove-med-btn').dataset.idx;
        selectedTemplateMedications.splice(idx, 1);
        renderMedicationRows();
    }
};
medicationsTableBody.onchange = function(e) {
    if (e.target.classList.contains('medication-name-select')) {
        const idx = +e.target.dataset.idx;
        const medId = e.target.value;
        const medDetail = allMedicationDetails.find(m => String(m.arvMedicationId) === String(medId));
        if (medDetail) {
            selectedTemplateMedications[idx] = {
                arvMedicationName: medDetail.arvMedicationName,
                dosage: medDetail.arvMedicationDosage,
                quantity: selectedTemplateMedications[idx].quantity || 1,
                manufacturer: medDetail.arvMedicationManufacturer
            };
        } else {
            selectedTemplateMedications[idx] = { arvMedicationName: '', dosage: '', quantity: 1, manufacturer: '' };
        }
        renderMedicationRows();
    } else if (e.target.classList.contains('medication-qty-input')) {
        const idx = +e.target.dataset.idx;
        selectedTemplateMedications[idx].quantity = +e.target.value;
    }
};

// Regimen form submit
regimenForm.onsubmit = async function(e) {
    e.preventDefault();
    // Validation
    if (selectedTemplateMedications.length === 0) {
        alert('Please add at least one medication.');
        return;
    }
    const medNames = selectedTemplateMedications.map(m => m.arvMedicationName);
    if (new Set(medNames).size !== medNames.length) {
        alert('Duplicate medications are not allowed.');
        return;
    }
    if (selectedTemplateMedications.some(m => !m.arvMedicationName || !m.quantity)) {
        alert('Please fill all medication fields.');
        return;
    }
    // Check that every medication selection is valid
    if (selectedTemplateMedications.some(m => {
        const medDetail = allMedicationDetails.find(md => md.arvMedicationName === m.arvMedicationName);
        return !medDetail;
    })) {
        alert('Please select a valid medication for every row.');
        return;
    }
    // Prepare regimen payload
    if (!window.pmrId) {
        alert('Cannot find patient medical record.');
        return;
    }
    // Build medications array for API
    const medications = selectedTemplateMedications.map(med => {
        const medDetail = allMedicationDetails.find(md => md.arvMedicationName === med.arvMedicationName);
        return {
        patientArvRegId: 0,
            arvMedDetailId: medDetail.arvMedicationId, // Use arvMedicationId as arvMedDetailId
        quantity: med.quantity
        };
    });
    // Build regimen object
    const regimen = {
        patientMedRecordId: window.pmrId,
        notes: regimenNotes.value,
        regimenLevel: +regimenLevel.value,
        createdAt: new Date().toISOString(),
        startDate: regimenStartDate.value,
        endDate: document.getElementById('regimenEndDate') ? document.getElementById('regimenEndDate').value : null,
        regimenStatus: 1, // active
        totalCost: 0
    };
    const payload = { regimen, medications };
    // Detailed logging of regimen properties
    console.log('DEBUG: Regimen details:');
    Object.entries(regimen).forEach(([key, value]) => {
        console.log(`  ${key}:`, value);
    });
    console.log('DEBUG: Submitting ARV regimen payload:', payload);
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
                } catch {}
            }
            if (errorText) errorMsg += '\n' + errorText;
            alert(errorMsg);
            return;
        }
        alert('Regimen and medications created successfully!');
        regimenModal.style.display = 'none';
        // Refresh data
        loadPatientData();
    } catch (err) {
        alert('Error creating regimen.');
        console.error('Error creating regimen:', err);
    }
};

// --- Create Test Result Modal Logic ---
document.addEventListener('DOMContentLoaded', function() {
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

  // Helper: get today in yyyy-mm-dd
  function getToday() {
    const d = new Date();
    return d.toISOString().slice(0, 10);
  }

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
    fieldset.querySelector('.removeComponentBtn').onclick = function() {
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
    dateInput.value = getToday();
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
  window.onclick = function(event) {
    if (event.target === modal) closeModal();
  };

  // Add component test
  if (addComponentBtn) addComponentBtn.onclick = addComponentTestFieldset;

  // --- Form Submit ---
  form.onsubmit = async function(e) {
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
window.addEventListener('click', function(event) {
  if (event.target === updateComponentTestModal) closeUpdateComponentTestModal();
});

if (updateComponentTestForm) {
  updateComponentTestForm.onsubmit = async function(e) {
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
