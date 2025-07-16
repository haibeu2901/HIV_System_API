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
        if (!response.ok) throw new Error('Failed to fetch patient details');
        return await response.json();
    } catch (error) {
        console.error('Error fetching patient details:', error);
        return null;
    }
}

// Fetch patient medical record ID
async function fetchPatientMRId(patientId) {
    try {
        const response = await fetch(`https://localhost:7009/api/Patient/GetPatientMRById/${patientId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch patient MR ID');
        return await response.json();
    } catch (error) {
        console.error('Error fetching patient MR ID:', error);
        return null;
    }
}

// Fetch all patient medical data (appointments, test results, arv regimens)
async function fetchPatientMedicalData(pmrId) {
    try {
        const response = await fetch(`https://localhost:7009/api/PatientMedicalRecord/GetPatientMedicalRecordById/${pmrId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch patient medical data');
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
        if (!response.ok) throw new Error('Failed to fetch ARV medications');
        return await response.json();
    } catch (error) {
        console.error('Error fetching ARV medications:', error);
        return [];
    }
}

// Render patient profile
function renderPatientProfile(patient) {
    if (!patient) {
        document.getElementById('patientName').textContent = 'Patient not found';
        return;
    }

    document.getElementById('patientName').textContent = patient.account.fullname;
    document.getElementById('patientGender').textContent = patient.account.gender ? 'Male' : 'Female';
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
                <p>No appointments found for this patient.</p>
            </div>
        `;
        return;
    }

    let html = `
        <table class="appointments-table">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Time</th>
                    <th>Doctor</th>
                    <th>Status</th>
                    <th>Notes</th>
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
                    <span class="test-result-date">Test Date: ${testResult.testDate}</span>
                    <span class="test-result-overall ${resultClass}">${resultText}</span>
                </div>
        `;

        if (testResult.notes) {
            html += `<p><strong>Notes:</strong> ${testResult.notes}</p>`;
        }

        if (testResult.componentTestResults && testResult.componentTestResults.length > 0) {
            html += `<div class="component-results">`;
            testResult.componentTestResults.forEach(comp => {
                html += `
                    <div class="component-result">
                        <div class="component-name">${comp.componentTestResultName}</div>
                        <div class="component-value">${comp.resultValue}</div>
                        ${comp.notes ? `<div class="component-notes">${comp.notes}</div>` : ''}
                    </div>
                `;
            });
            html += `</div>`;
        }

        html += `</div>`;
    });
    section.innerHTML = html;
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
                        <div class="regimen-detail-label">Total Cost</div>
                        <div class="regimen-detail-value">${regimen.totalCost ? `$${regimen.totalCost.toLocaleString()}` : 'N/A'}</div>
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
                    ${(regimen.regimenStatus !== 4 && regimen.regimenStatus !== 5) ? `<button class="secondary-btn update-regimen-status-btn" data-id="${regimen.patientArvRegiId}" data-status="${regimen.regimenStatus}">Update Status</button>` : ''}
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
const regimenTotalCost = document.getElementById('regimenTotalCost');
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
    regimenTotalCost.value = '';
    regimenStartDate.value = new Date().toISOString().slice(0, 10);
    // Fill medications
    selectedTemplateMedications = template.medications.map(med => ({
        arvMedicationName: med.medicationName,
        arvMedDetailId: med.arvMedicationDetailId,
        dosage: med.dosage,
        quantity: med.quantity,
        manufacturer: allMedicationDetails.find(m => m.arvMedicationName === med.medicationName)?.arvMedicationManufacturer || '',
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
                    ${allMedicationDetails.map(m => `<option value="${m.arvMedicationName}" ${m.arvMedicationName === med.arvMedicationName ? 'selected' : ''}>${m.arvMedicationName}</option>`).join('')}
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
    if (selectedTemplateMedications.length >= 3) return;
    selectedTemplateMedications.push({ arvMedicationName: '', arvMedDetailId: null, dosage: '', quantity: 1, manufacturer: '' });
    renderMedicationRows();
};

function updateAddMedicationBtnState() {
    addMedicationBtn.disabled = selectedTemplateMedications.length >= 3;
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
        const medName = e.target.value;
        const medDetail = allMedicationDetails.find(m => m.arvMedicationName === medName);
        if (medDetail) {
            selectedTemplateMedications[idx] = {
                arvMedicationName: medDetail.arvMedicationName,
                arvMedDetailId: medDetail.arvMedicationDetailId || medDetail.arvMedDetailId || medDetail.id || medDetail.arvMedicationDetailId,
                dosage: medDetail.arvMedicationDosage,
                quantity: selectedTemplateMedications[idx].quantity || 1,
                manufacturer: medDetail.arvMedicationManufacturer
            };
        } else {
            selectedTemplateMedications[idx] = { arvMedicationName: '', arvMedDetailId: null, dosage: '', quantity: 1, manufacturer: '' };
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
    if (selectedTemplateMedications.length === 0 || selectedTemplateMedications.length > 3) {
        alert('Please select 1-3 medications.');
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
    // Prepare regimen payload
    const patientId = getPatientIdFromUrl();
    const mrData = await fetchPatientMRId(patientId);
    if (!mrData || !mrData.pmrId) {
        alert('Cannot find patient medical record.');
        return;
    }
    // Build medications array for API
    const medications = selectedTemplateMedications.map(med => ({
        patientArvRegId: 0,
        arvMedDetailId: med.arvMedDetailId,
        quantity: med.quantity
    }));
    // Build regimen object
    const regimen = {
        patientMedRecordId: mrData.pmrId,
        notes: regimenNotes.value,
        regimenLevel: +regimenLevel.value,
        createdAt: new Date().toISOString(),
        startDate: regimenStartDate.value,
        endDate: null,
        regimenStatus: 1, // active
        totalCost: 0 // Let backend calculate
    };
    // Call new API
    try {
        const res = await fetch('https://localhost:7009/api/PatientArvRegimen/CreatePatientArvRegimenWithMedications', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
            body: JSON.stringify({ regimen, medications })
        });
        if (!res.ok) {
            alert('Failed to create regimen.');
            return;
        }
        alert('Regimen and medications created successfully!');
        regimenModal.style.display = 'none';
        // Refresh data
        loadPatientData();
    } catch (err) {
        alert('Error creating regimen.');
    }
};

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

        // Fetch patient MR ID
        const mrData = await fetchPatientMRId(patientId);
        
        if (mrData && mrData.pmrId) {
            // Fetch all medical data (appointments, test results, arv regimens)
            const medicalData = await fetchPatientMedicalData(mrData.pmrId);
            // Fetch all ARV medications for the patient
            const medications = await fetchPatientArvMedications(patientId);
            if (medicalData) {
                renderAppointments(medicalData.appointments || []);
                renderTestResults(medicalData.testResults || []);
                renderARVRegimens(medicalData.arvRegimens || [], medications);
            } else {
                renderAppointments([]);
                renderTestResults([]);
                renderARVRegimens([], []);
            }
        } else {
            renderAppointments([]);
            renderTestResults([]);
            renderARVRegimens([], []);
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
