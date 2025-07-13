// Get token from localStorage
const token = localStorage.getItem('token');

// Appointment status mapping
const appointmentStatusMap = {
    1: 'Chờ xác nhận',
    2: 'Đã xác nhận',
    3: 'Đã xác nhận lại',
    4: 'Đã hủy'
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

// Fetch patient medical record with appointments
async function fetchPatientMedicalRecord(pmrId) {
    try {
        const response = await fetch(`https://localhost:7009/api/PatientMedicalRecord/GetPatientMedicalRecordById/${pmrId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch patient medical record');
        return await response.json();
    } catch (error) {
        console.error('Error fetching patient medical record:', error);
        return null;
    }
}

// Fetch test results
async function fetchTestResults(pmrId) {
    try {
        const response = await fetch(`https://localhost:7009/api/TestResult/GetById/${pmrId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) return null; // No test results found
        return await response.json();
    } catch (error) {
        console.error('Error fetching test results:', error);
        return null;
    }
}

// Fetch ARV regimens
async function fetchARVRegimens(patientId) {
    try {
        const response = await fetch(`https://localhost:7009/api/PatientArvRegimen/GetPatientArvRegimensByPatientId?patientId=${patientId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch ARV regimens');
        return await response.json();
    } catch (error) {
        console.error('Error fetching ARV regimens:', error);
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

// Render test results
function renderTestResults(testResult) {
    const section = document.getElementById('testResultsSection');
    
    if (!testResult) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-flask"></i>
                <p>No test results found for this patient.</p>
            </div>
        `;
        return;
    }

    const resultClass = testResult.result ? 'test-result-positive' : 'test-result-negative';
    const resultText = testResult.result ? 'Positive' : 'Negative';

    let html = `
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
    section.innerHTML = html;
}

// Render ARV regimens
function renderARVRegimens(regimens) {
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

    let html = '';
    regimens.forEach(regimen => {
        const statusClass = regimen.regimenStatus === 2 ? 'regimen-active' : 'regimen-inactive';
        const statusText = regimen.regimenStatus === 2 ? 'Active' : 'Inactive';
        
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
                        <div class="regimen-detail-value">${regimen.regimenLevel}</div>
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
            </div>
        `;
    });
    
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

        // Fetch patient MR ID
        const mrData = await fetchPatientMRId(patientId);
        
        if (mrData && mrData.pmrId) {
            // Fetch medical record with appointments
            const medicalRecord = await fetchPatientMedicalRecord(mrData.pmrId);
            if (medicalRecord) {
                renderAppointments(medicalRecord.appointments || []);
            }

            // Fetch test results
            const testResult = await fetchTestResults(mrData.pmrId);
            renderTestResults(testResult);
        } else {
            renderAppointments([]);
            renderTestResults(null);
        }

        // Fetch ARV regimens
        const regimens = await fetchARVRegimens(patientId);
        renderARVRegimens(regimens);

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
