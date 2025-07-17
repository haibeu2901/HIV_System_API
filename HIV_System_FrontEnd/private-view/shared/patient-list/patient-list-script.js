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
