// Get token from localStorage
const token = localStorage.getItem('token');

// Appointment status mapping
const statusMap = {
    1: 'Chờ xác nhận',
    2: 'Đã xác nhận',
    3: 'Đã xác nhận lại',
    4: 'Đã hủy'
};

async function fetchAppointments() {
    try {
        const response = await fetch('https://localhost:7009/api/Appointment/GetAllPersonalAppointments', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch appointments');
        return await response.json();
    } catch (error) {
        console.error('Error fetching appointments:', error);
        return [];
    }
}

function renderAppointments(appointments) {
    const section = document.getElementById('appointmentListSection');
    if (!appointments.length) {
        section.innerHTML = '<p>Không có lịch hẹn nào.</p>';
        return;
    }
    let html = `<table class="appointment-table">
        <thead>
            <tr>
                <th>STT</th>
                <th>Bệnh nhân</th>
                <th>Ngày</th>
                <th>Giờ</th>
                <th>Ghi chú</th>
                <th>Trạng thái</th>
            </tr>
        </thead>
        <tbody>
            ${appointments.map((apmt, idx) => `
                <tr>
                    <td>${idx + 1}</td>
                    <td>${apmt.patientName}</td>
                    <td>${apmt.apmtDate}</td>
                    <td>${apmt.apmTime.slice(0,5)}</td>
                    <td>${apmt.notes || ''}</td>
                    <td class="status status-${apmt.apmStatus}">${statusMap[apmt.apmStatus] || 'Không rõ'}</td>
                </tr>
            `).join('')}
        </tbody>
    </table>`;
    section.innerHTML = html;
}

window.addEventListener('DOMContentLoaded', async () => {
    let appointments = await fetchAppointments();
    renderAppointments(appointments);
});
