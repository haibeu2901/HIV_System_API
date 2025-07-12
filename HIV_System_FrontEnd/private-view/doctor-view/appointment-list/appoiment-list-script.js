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
                <th>Hành động</th>
            </tr>
        </thead>
        <tbody>
            ${appointments.map((apmt, idx) => `
                <tr data-apmt-id="${apmt.appointmentId}" data-apmt-date="${apmt.apmtDate}" data-apmt-time="${apmt.apmTime}" data-apmt-notes="${apmt.notes || ''}">
                    <td>${idx + 1}</td>
                    <td>${apmt.patientName}</td>
                    <td>${apmt.apmtDate}</td>
                    <td>${apmt.apmTime.slice(0,5)}</td>
                    <td>${apmt.notes || ''}</td>
                    <td class="status status-${apmt.apmStatus}">${statusMap[apmt.apmStatus] || 'Không rõ'}</td>
                    <td>
                        ${apmt.apmStatus === 1 ? `
                            <button class="button-accept" data-accept-id="${apmt.appointmentId}">Accept</button>
                            <button class="button-modify" data-modify-id="${apmt.appointmentId}">Modify</button>
                        ` : ''}
                    </td>
                </tr>
            `).join('')}
        </tbody>
    </table>`;
    section.innerHTML = html;

    // Accept button logic
    document.querySelectorAll('.button-accept').forEach(btn => {
        btn.onclick = async function() {
            const apmtId = this.getAttribute('data-accept-id');
            try {
                const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?id=${apmtId}&status=2`, {
                    method: 'PUT',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                if (!res.ok) throw new Error('Failed to accept appointment');
                // Update UI
                let appointments = await fetchAppointments();
                renderAppointments(appointments);
            } catch (err) {
                alert('Error accepting appointment.');
            }
        };
    });

    // Modify button logic
    document.querySelectorAll('.button-modify').forEach(btn => {
        btn.onclick = async function() {
            const apmtId = this.getAttribute('data-modify-id');
            const row = this.closest('tr');
            // Prefill modal
            document.getElementById('modifyDate').value = row.getAttribute('data-apmt-date');
            document.getElementById('modifyNotes').value = row.getAttribute('data-apmt-notes');
            document.getElementById('modifyAppointmentModal').style.display = 'block';
            document.getElementById('modifyAppointmentForm').setAttribute('data-apmt-id', apmtId);
            // Fetch available times for doctor
            const accId = localStorage.getItem('accId');
            let timeSelect = document.getElementById('modifyTime');
            timeSelect.innerHTML = '';
            try {
                const wsRes = await fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetDoctorWorkSchedulesByDoctorId/${accId}`, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                if (wsRes.ok) {
                    const schedules = await wsRes.json();
                    // Flatten all available times
                    let times = [];
                    schedules.forEach(sch => {
                        if (sch.startTime && sch.endTime) {
                            // Assume times are in HH:mm:ss format
                            let start = sch.startTime.slice(0,5);
                            let end = sch.endTime.slice(0,5);
                            // Add every 30 min slot between start and end
                            let [h1, m1] = start.split(':').map(Number);
                            let [h2, m2] = end.split(':').map(Number);
                            let t1 = h1 * 60 + m1;
                            let t2 = h2 * 60 + m2;
                            for (let t = t1; t <= t2; t += 30) {
                                let h = Math.floor(t/60).toString().padStart(2,'0');
                                let m = (t%60).toString().padStart(2,'0');
                                times.push(`${h}:${m}`);
                            }
                        }
                    });
                    // Remove duplicates
                    times = [...new Set(times)];
                    times.forEach(time => {
                        let opt = document.createElement('option');
                        opt.value = time;
                        opt.textContent = time;
                        timeSelect.appendChild(opt);
                    });
                }
            } catch (err) {
                timeSelect.innerHTML = '<option value="">No available times</option>';
            }
        };
    });
}

// Modal logic
window.addEventListener('DOMContentLoaded', async () => {
    let appointments = await fetchAppointments();
    renderAppointments(appointments);

    // Modal close/cancel
    document.getElementById('closeModifyModal').onclick = closeModifyModal;
    document.getElementById('cancelModifyBtn').onclick = closeModifyModal;
    function closeModifyModal() {
        document.getElementById('modifyAppointmentModal').style.display = 'none';
        document.getElementById('modifyAppointmentForm').reset();
    }

    // Modal submit
    document.getElementById('modifyAppointmentForm').onsubmit = async function(e) {
        e.preventDefault();
        const apmtId = this.getAttribute('data-apmt-id');
        const date = document.getElementById('modifyDate').value;
        const time = document.getElementById('modifyTime').value;
        const notes = document.getElementById('modifyNotes').value;
        // Validate date (not in the past)
        const today = new Date();
        const selDate = new Date(date);
        today.setHours(0,0,0,0);
        selDate.setHours(0,0,0,0);
        if (selDate < today) {
            alert('Appointment date cannot be in the past.');
            return;
        }
        if (!time) {
            alert('Please select a valid time.');
            return;
        }
        try {
            const res = await fetch(`https://localhost:7009/api/Appointment/UpdateAppointmentRequest?id=${apmtId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    appointmentDate: date,
                    appointmentTime: time,
                    notes: notes
                })
            });
            if (!res.ok) throw new Error('Failed to update appointment');
            closeModifyModal();
            let appointments = await fetchAppointments();
            renderAppointments(appointments);
        } catch (err) {
            alert('Error updating appointment.');
        }
    };
});
