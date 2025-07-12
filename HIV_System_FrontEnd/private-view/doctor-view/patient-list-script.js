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
    // Add event listeners for view buttons
    document.querySelectorAll('.view-details-btn').forEach(btn => {
        btn.onclick = async function() {
            const patientId = this.getAttribute('data-patient-id');
            try {
                // Step 1: Get pmrId for this patient
                const pmrIdRes = await fetch(`https://localhost:7009/api/Patient/GetPatientMRById/${patientId}`, {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                });
                if (!pmrIdRes.ok) throw new Error('Could not retrieve medical record ID for this patient.');
                const pmrIdData = await pmrIdRes.json();
                const pmrId = pmrIdData.pmrId;
                if (!pmrId) throw new Error('No medical record ID found for this patient.');

                // Step 2: Fetch patient medical record using pmrId
                let html = '';
                try {
                    const recordRes = await fetch(`https://localhost:7009/api/PatientMedicalRecord/GetPatientMedicalRecordById/${pmrId}`, {
                        headers: {
                            'Authorization': `Bearer ${token}`
                        }
                    });
                    if (!recordRes.ok) throw new Error('Failed to fetch patient medical record');
                    const record = await recordRes.json();
                    html += `<h2>Patient Medical Record</h2>`;
                    html += `<p><strong>Medical Record ID:</strong> ${record.pmrId}</p>`;
                    html += `<p><strong>Patient ID:</strong> ${record.ptnId}</p>`;
                    html += `<h3>Appointments</h3>`;
                    if (record.appointments && record.appointments.length > 0) {
                        html += `<ul>`;
                        record.appointments.forEach(appt => {
                            const statusLabel = appointmentStatusMap[appt.apmStatus] || appt.apmStatus;
                            html += `<li><strong>Date:</strong> ${appt.apmtDate} | <strong>Doctor:</strong> ${appt.doctorName} | <strong>Status:</strong> ${statusLabel}</li>`;
                        });
                        html += `</ul>`;
                    } else {
                        html += `<p>No appointments found.</p>`;
                    }
                } catch (err) {
                    html += `<p style='color:red;'>Error loading patient medical record.</p>`;
                }

                // Step 3: Fetch and display test result using pmrId
                try {
                    const testResultRes = await fetch(`https://localhost:7009/api/TestResult/GetById/${pmrId}`, {
                        headers: {
                            'Authorization': `Bearer ${token}`
                        }
                    });
                    if (testResultRes.ok) {
                        const testResult = await testResultRes.json();
                        html += `<h3>Test Result</h3>`;
                        html += `<p><strong>Test Date:</strong> ${testResult.testDate}</p>`;
                        html += `<p><strong>Result:</strong> ${testResult.result ? 'Positive' : 'Negative'}</p>`;
                        html += `<p><strong>Notes:</strong> ${testResult.notes || ''}</p>`;
                        if (testResult.componentTestResults && testResult.componentTestResults.length > 0) {
                            html += `<h4>Component Test Results</h4>`;
                            html += `<ul>`;
                            testResult.componentTestResults.forEach(comp => {
                                html += `<li><strong>${comp.componentTestResultName}:</strong> ${comp.resultValue}`;
                                if (comp.notes) html += ` <em>(${comp.notes})</em>`;
                                html += `</li>`;
                            });
                            html += `</ul>`;
                        }
                    } else {
                        html += `<p style='color:orange;'>No test result found for this medical record.</p>`;
                    }
                } catch (err) {
                    html += `<p style='color:red;'>Error loading test result.</p>`;
                }

                document.getElementById('patientDetailContent').innerHTML = html;
                document.getElementById('patientDetailModal').style.display = 'block';
            } catch (error) {
                document.getElementById('patientDetailContent').innerHTML = `<p style='color:red;'>${error.message || 'Error loading patient medical record.'}</p>`;
                document.getElementById('patientDetailModal').style.display = 'block';
            }
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
        if (event.target === modal) {
            modal.style.display = 'none';
        }
    };
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
    4: 'Đã hủy'
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
