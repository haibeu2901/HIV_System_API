// Get token from localStorage
const token = localStorage.getItem('token');

// Appointment status mapping
const appointmentStatusMap = {
  1: "Pending",
  2: "Scheduled",
  3: "Rescheduled",
  4: "Cancelled",
  5: "Completed"
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
                    <td class="status status-${apmt.apmStatus}">${appointmentStatusMap[apmt.apmStatus] || 'Không rõ'}</td>
                    <td>
                        ${apmt.apmStatus === 1 ? `
                            <button class="button-accept" data-accept-id="${apmt.appointmentId}">Accept</button>
                            <button class="button-modify" data-modify-id="${apmt.appointmentId}">Modify</button>
                        ` : ''}
                        ${(apmt.apmStatus !== 4 && apmt.apmStatus !== 5) ? `
                            <button class="button-reject" data-reject-id="${apmt.appointmentId}">Reject</button>
                        ` : ''}
                        ${(apmt.apmStatus === 2 || apmt.apmStatus === 3) ? `
                            <button class="button-complete" data-complete-id="${apmt.appointmentId}">Complete</button>
                        ` : ''}
                    </td>
                </tr>
            `).join('')}
        </tbody>
    </table>
    <div id="appointment-message" style="margin-top:10px;"></div>`;
    section.innerHTML = html;

    // Accept button logic
    document.querySelectorAll('.button-accept').forEach(btn => {
        btn.onclick = async function() {
            const apmtId = this.getAttribute('data-accept-id');
            try {
                const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?id=${apmtId}&status=2`, {
                    method: 'PATCH',
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

    // Reject button logic
    document.querySelectorAll('.button-reject').forEach(btn => {
        btn.onclick = async function() {
            const apmtId = this.getAttribute('data-reject-id');
            if (!confirm('Are you sure you want to reject (cancel) this appointment?')) return;
            this.disabled = true;
            setMessage('Processing...', false);
            try {
                const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?id=${apmtId}&status=4`, {
                    method: 'PATCH',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                if (!res.ok) throw new Error('Failed to reject appointment');
                setMessage('Appointment rejected successfully!', true);
                let appointments = await fetchAppointments();
                renderAppointments(appointments);
            } catch (err) {
                setMessage('Error rejecting appointment.', false);
                this.disabled = false;
            }
        };
    });

    // Complete button logic
    document.querySelectorAll('.button-complete').forEach(btn => {
        btn.onclick = function() {
            const apmtId = this.getAttribute('data-complete-id');
            // Open modal and store appointment ID
            const completeModal = document.getElementById('completeAppointmentModal');
            const completeForm = document.getElementById('completeAppointmentForm');
            const completeNotes = document.getElementById('completeNotes');
            if (completeModal && completeForm && completeNotes) {
                completeModal.style.display = 'block';
                completeForm.setAttribute('data-apmt-id', apmtId);
                completeNotes.value = '';
            }
        };
    });
}

// Modal logic
window.addEventListener('DOMContentLoaded', async () => {
    let appointments = await fetchAppointments();
    console.log('Fetched appointments:', appointments); // Debug line
    renderAppointments(appointments);

    // Modal close/cancel (set up once)
    const closeCompleteModalBtn = document.getElementById('closeCompleteModal');
    if (closeCompleteModalBtn) closeCompleteModalBtn.onclick = closeCompleteModal;
    const cancelCompleteBtn = document.getElementById('cancelCompleteBtn');
    if (cancelCompleteBtn) cancelCompleteBtn.onclick = closeCompleteModal;
    function closeCompleteModal() {
        const completeModal = document.getElementById('completeAppointmentModal');
        const completeForm = document.getElementById('completeAppointmentForm');
        if (completeModal && completeForm) {
            completeModal.style.display = 'none';
            completeForm.reset();
        }
    }
    const completeAppointmentForm = document.getElementById('completeAppointmentForm');
    if (completeAppointmentForm) {
        completeAppointmentForm.onsubmit = async function(e) {
            e.preventDefault();
            const apmtId = this.getAttribute('data-apmt-id');
            const notes = document.getElementById('completeNotes') ? document.getElementById('completeNotes').value : '';
            const submitBtn = document.getElementById('submitCompleteBtn');
            if (submitBtn) submitBtn.disabled = true;
            setMessage('Processing...', false);
            try {
                const res = await fetch(`https://localhost:7009/api/Appointment/CompleteAppointment?appointmentId=${apmtId}`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ notes })
                });
                if (!res.ok) throw new Error('Failed to complete appointment');
                setMessage('Appointment marked as completed!', true);
                closeCompleteModal();
                let appointments = await fetchAppointments();
                renderAppointments(appointments);
            } catch (err) {
                setMessage('Error completing appointment.', false);
                if (submitBtn) submitBtn.disabled = false;
            }
        };
    }

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

    // Modal close/cancel
    document.getElementById('closeModifyModal').onclick = closeModifyModal;
    document.getElementById('cancelModifyBtn').onclick = closeModifyModal;
    function closeModifyModal() {
        document.getElementById('modifyAppointmentModal').style.display = 'none';
        document.getElementById('modifyAppointmentForm').reset();
        document.getElementById('modifyDateMessage').textContent = '';
        document.getElementById('modifyTime').innerHTML = '';
    }

    // Modal submit
    document.getElementById('modifyAppointmentForm').onsubmit = async function(e) {
        e.preventDefault();
        const apmtId = this.getAttribute('data-apmt-id');
        const date = document.getElementById('modifyDate').value;
        let time = document.getElementById('modifyTime').value;
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
        // Ensure time is in HH:mm:ss format
        if (time.length === 5) time += ':00';
        try {
            const res = await fetch(`https://localhost:7009/api/Appointment/UpdateAppointmentRequest?id=${apmtId}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    appointmentDate: date,
                    appointmentTime: time,
                    notes
                })
            });
            if (!res.ok) throw new Error('Failed to update appointment');
            setMessage('Appointment updated successfully!', true);
            closeModifyModal();
            let appointments = await fetchAppointments();
            renderAppointments(appointments);
        } catch (err) {
            setMessage('Error updating appointment.', false);
        }
    };
});

function setMessage(msg, success) {
    const msgDiv = document.getElementById('appointment-message');
    msgDiv.textContent = msg;
    msgDiv.style.color = success ? '#27ae60' : '#c0392b';
}
